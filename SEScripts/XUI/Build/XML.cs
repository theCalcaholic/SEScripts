﻿namespace SEScripts.Merged
{

public static class Logger
{
private static StringBuilder Log = new StringBuilder();
public static string Output
{
get { return Log.ToString(); }
}
static IMyTextPanel DebugPanel;
static public bool DEBUG = false;
public static int offset = 0;
private static StringBuilder Prefix = new StringBuilder();

public static void log(string msg)
{
Log.Append(Prefix);
Log.Append(msg + "\n");
//Console.WriteLine(Prefix + msg);
//!UNCOMMENT P.Echo(Prefix + msg);
}
public static void debug(string msg)
{
if (DEBUG)
{
log(msg);
}
}
public static void IncLvl()
{
Prefix.Append("  ");
}
public static void DecLvl()
{
if( Prefix.Length >= 2)
Prefix.Remove(Prefix.Length - 2, 2);
}
}

public static class Parser
{
public static string PackData(Dictionary<string, string> data)
{
StringBuilder dataString = new StringBuilder();
foreach (string key in data.Keys)
{
dataString.Append(key + "=\"" + data[key] + "\" ");
}
return dataString.ToString();
}
public static string Sanitize(string xmlDefinition)
{
Logger.debug("Parser.Sanitize()");
return xmlDefinition.Replace("\"", "\\\"").Replace("'", "\\'");
}
public static string UnescapeQuotes(string xmlDefinition)
{
return xmlDefinition.Replace("\\\"", "\"").Replace("\\'", "'");
}
public static int GetNextUnescaped(char[] needles, string haystack)
{
return GetNextUnescaped(needles, haystack, 0);
}
public static int GetNextUnescaped(char[] needles, string haystack, int start)
{
return GetNextUnescaped(needles, haystack, start, haystack.Length - start);
}
public static int GetNextUnescaped(char[] needles, string haystack, int start, int count)
{
//Logger.debug("GetNextUnescaped():");
//Logger.IncLvl();
int end = start + count - 1;
int needlePos = haystack.IndexOfAny(needles, start, end - start + 1);
while (needlePos > 0 && haystack[needlePos - 1] == '\\')
{
needlePos = haystack.IndexOfAny(needles, needlePos + 1, end - needlePos);
}
//Logger.DecLvl();
return needlePos;
}
public static int GetNextOutsideQuotes(char needle, string haystack)
{
return GetNextOutsideQuotes(new char[] { needle }, haystack);
}
public static int GetNextOutsideQuotes(char needle, string haystack, bool ignoreEscapedQuotes)
{
return GetNextOutsideQuotes(new char[] { needle }, haystack, ignoreEscapedQuotes);
}
public static int GetNextOutsideQuotes(char[] needles, string haystack)
{
return GetNextOutsideQuotes(needles, haystack, true);
}
public static int GetNextOutsideQuotes(
char[] needles,
string haystack,
bool ignoreEscapedQuotes
)
{
//Logger.debug("GetNextOutsideQuotes():");
//Logger.IncLvl();
char[] quoteChars = new char[] { '\'', '"' };
int needlePos = -1;
int quoteEnd = -1;
int quoteStart;
//Logger.debug("needle: |" + new string(needles) + "|");
//Logger.debug("haystack: |" + haystack + "|");
while (needlePos == -1)
{
if (ignoreEscapedQuotes)
{
quoteStart = GetNextUnescaped(quoteChars, haystack, quoteEnd + 1);
}
else
{
quoteStart = haystack.IndexOfAny(quoteChars, quoteEnd + 1);
}
//Logger.debug("quoteStart position: " + quoteStart.ToString()
//    + ", quoteEnd position: " + quoteEnd.ToString());
if (quoteStart == -1)
{
//Logger.debug("searching for needle in:: " + haystack.Substring(quoteEnd + 1));
needlePos = GetNextUnescaped(needles, haystack, quoteEnd + 1);
}
else
{
//Logger.debug("found start quote: " + haystack.Substring(quoteStart));
//Logger.debug("searching for needle in: "
//    + haystack.Substring(quoteEnd + 1, quoteStart - quoteEnd));
needlePos = GetNextUnescaped(
needles,
haystack,
quoteEnd + 1,
quoteStart - quoteEnd - 1
);
if (needlePos != -1)
{
Logger.debug("found needle: " + haystack.Substring(needlePos));
}
if (ignoreEscapedQuotes)
{
quoteEnd = GetNextUnescaped(
new char[] { haystack[quoteStart] },
haystack,
quoteStart + 1
);
}
else
{
quoteEnd = haystack.IndexOf(haystack[quoteStart], quoteStart + 1);
}
//Logger.debug("found end quote: " + haystack.Substring(quoteEnd));
}
}
//Logger.debug("yay!");
//Logger.DecLvl();
return needlePos;
}
public static List<String> ParamString2List(string arg)
{
//Logger.debug("Parser.ParamString2List()");
//Logger.IncLvl();
arg = arg.Trim() + " ";
List<string> argList = new List<string>();
char[] quoteChars = new char[] { '\'', '"' };
int spacePos = -1;
while (spacePos != arg.Length - 1)
{
arg = arg.Substring(spacePos + 1);
spacePos = Parser.GetNextOutsideQuotes(new char[] { ' ', '\n' }, arg);
argList.Add(arg.Substring(0, spacePos).Trim(quoteChars));
}
//Logger.DecLvl();
return argList;
}
public static Dictionary<string, string> GetXMLAttributes(string attributeString)
{
//Logger.debug("GetXMLAttributes():");
//Logger.IncLvl();
//Logger.debug("attribute string is: <" + attributeString + ">");
Dictionary<string, string> attributes = new Dictionary<string, string>();
char[] quoteChars = new char[] { '\'', '"' };
List<string> attributeList = ParamString2List(attributeString);
int equalSign;
foreach (string attribute in attributeList)
{
equalSign = attribute.IndexOf('=');
if (equalSign == -1)
{
attributes[attribute.Substring(0).ToLower()] = "true";
}
else
{
attributes[attribute.Substring(0, equalSign).ToLower()] =
attribute.Substring(equalSign + 1).Trim(quoteChars);
}
}
//Logger.debug("attribute dict: {");
//Logger.IncLvl();
//foreach (string key in attributes.Keys)
//{
//Logger.debug(key + ": " + attributes[key]);
//}
//Logger.debug("}");
//Logger.DecLvl();
//Logger.DecLvl();
return attributes;
}
}

public static class TextUtils
{
public enum FONT { DEFAULT, MONOSPACE };
public static bool DEBUG = true;
private static FONT selectedFont = FONT.DEFAULT;
/*private static Dictionary<FONT, Dictionary<char, int>> LetterWidths = new Dictionary<FONT, Dictionary<char, int>>{
{
FONT.DEFAULT,
new Dictionary<char, int> {
{' ', 8 }, { '!', 8 }, { '"', 10}, {'#', 19}, {'$', 20}, {'%', 24}, {'&', 20}, {'\'', 6}, {'(', 9}, {')', 9}, {'*', 11}, {'+', 18}, {',', 9}, {'-', 10}, {'.', 9}, {'/', 14}, {'0', 19}, {'1', 9}, {'2', 19}, {'3', 17}, {'4', 19}, {'5', 19}, {'6', 19}, {'7', 16}, {'8', 19}, {'9', 19}, {':', 9}, {';', 9}, {'<', 18}, {'=', 18}, {'>', 18}, {'?', 16}, {'@', 25}, {'A', 21}, {'B', 21}, {'C', 19}, {'D', 21}, {'E', 18}, {'F', 17}, {'G', 20}, {'H', 20}, {'I', 8}, {'J', 16}, {'K', 17}, {'L', 15}, {'M', 26}, {'N', 21}, {'O', 21}, {'P', 20}, {'Q', 21}, {'R', 21}, {'S', 21}, {'T', 17}, {'U', 20}, {'V', 20}, {'W', 31}, {'X', 19}, {'Y', 20}, {'Z', 19}, {'[', 9}, {'\\', 12}, {']', 9}, {'^', 18}, {'_', 15}, {'`', 8}, {'a', 17}, {'b', 17}, {'c', 16}, {'d', 17}, {'e', 17}, {'f', 9}, {'g', 17}, {'h', 17}, {'i', 8}, {'j', 8}, {'k', 17}, {'l', 8}, {'m', 27}, {'n', 17}, {'o', 17}, {'p', 17}, {'q', 17}, {'r', 10}, {'s', 17}, {'t', 9}, {'u', 17}, {'v', 15}, {'w', 27}, {'x', 15}, {'y', 17}, {'z', 16}, {'{', 9}, {'|', 6}, {'}', 9}, {'~', 18}, {' ', 8}, {'¡', 8}, {'¢', 16}, {'£', 17}, {'¤', 19}, {'¥', 19}, {'¦', 6}, {'§', 20}, {'¨', 8}, {'©', 25}, {'ª', 10}, {'«', 15}, {'¬', 18}, {'­', 10}, {'®', 25}, {'¯', 8}, {'°', 12}, {'±', 18}, {'²', 11}, {'³', 11}, {'´', 8}, {'µ', 17}, {'¶', 18}, {'·', 9}, {'¸', 8}, {'¹', 11}, {'º', 10}, {'»', 15}, {'¼', 27}, {'½', 29}, {'¾', 28}, {'¿', 16}, {'À', 21}, {'Á', 21}, {'Â', 21}, {'Ã', 21}, {'Ä', 21}, {'Å', 21}, {'Æ', 31}, {'Ç', 19}, {'È', 18}, {'É', 18}, {'Ê', 18}, {'Ë', 18}, {'Ì', 8}, {'Í', 8}, {'Î', 8}, {'Ï', 8}, {'Ð', 21}, {'Ñ', 21}, {'Ò', 21}, {'Ó', 21}, {'Ô', 21}, {'Õ', 21}, {'Ö', 21}, {'×', 18}, {'Ø', 21}, {'Ù', 20}, {'Ú', 20}, {'Û', 20}, {'Ü', 20}, {'Ý', 17}, {'Þ', 20}, {'ß', 19}, {'à', 17}, {'á', 17}, {'â', 17}, {'ã', 17}, {'ä', 17}, {'å', 17}, {'æ', 28}, {'ç', 16}, {'è', 17}, {'é', 17}, {'ê', 17}, {'ë', 17}, {'ì', 8}, {'í', 8}, {'î', 8}, {'ï', 8}, {'ð', 17}, {'ñ', 17}, {'ò', 17}, {'ó', 17}, {'ô', 17}, {'õ', 17}, {'ö', 17}, {'÷', 18}, {'ø', 17}, {'ù', 17}, {'ú', 17}, {'û', 17}, {'ü', 17}, {'ý', 17}, {'þ', 17}, {'ÿ', 17}, {'Ā', 20}, {'ā', 17}, {'Ă', 21}, {'ă', 17}, {'Ą', 21}, {'ą', 17}, {'Ć', 19}, {'ć', 16}, {'Ĉ', 19}, {'ĉ', 16}, {'Ċ', 19}, {'ċ', 16}, {'Č', 19}, {'č', 16}, {'Ď', 21}, {'ď', 17}, {'Đ', 21}, {'đ', 17}, {'Ē', 18}, {'ē', 17}, {'Ĕ', 18}, {'ĕ', 17}, {'Ė', 18}, {'ė', 17}, {'Ę', 18}, {'ę', 17}, {'Ě', 18}, {'ě', 17}, {'Ĝ', 20}, {'ĝ', 17}, {'Ğ', 20}, {'ğ', 17}, {'Ġ', 20}, {'ġ', 17}, {'Ģ', 20}, {'ģ', 17}, {'Ĥ', 20}, {'ĥ', 17}, {'Ħ', 20}, {'ħ', 17}, {'Ĩ', 8}, {'ĩ', 8}, {'Ī', 8}, {'ī', 8}, {'Į', 8}, {'į', 8}, {'İ', 8}, {'ı', 8}, {'Ĳ', 24}, {'ĳ', 14}, {'Ĵ', 16}, {'ĵ', 8}, {'Ķ', 17}, {'ķ', 17}, {'Ĺ', 15}, {'ĺ', 8}, {'Ļ', 15}, {'ļ', 8}, {'Ľ', 15}, {'ľ', 8}, {'Ŀ', 15}, {'ŀ', 10}, {'Ł', 15}, {'ł', 8}, {'Ń', 21}, {'ń', 17}, {'Ņ', 21}, {'ņ', 17}, {'Ň', 21}, {'ň', 17}, {'ŉ', 17}, {'Ō', 21}, {'ō', 17}, {'Ŏ', 21}, {'ŏ', 17}, {'Ő', 21}, {'ő', 17}, {'Œ', 31}, {'œ', 28}, {'Ŕ', 21}, {'ŕ', 10}, {'Ŗ', 21}, {'ŗ', 10}, {'Ř', 21}, {'ř', 10}, {'Ś', 21}, {'ś', 17}, {'Ŝ', 21}, {'ŝ', 17}, {'Ş', 21}, {'ş', 17}, {'Š', 21}, {'š', 17}, {'Ţ', 17}, {'ţ', 9}, {'Ť', 17}, {'ť', 9}, {'Ŧ', 17}, {'ŧ', 9}, {'Ũ', 20}, {'ũ', 17}, {'Ū', 20}, {'ū', 17}, {'Ŭ', 20}, {'ŭ', 17}, {'Ů', 20}, {'ů', 17}, {'Ű', 20}, {'ű', 17}, {'Ų', 20}, {'ų', 17}, {'Ŵ', 31}, {'ŵ', 27}, {'Ŷ', 17}, {'ŷ', 17}, {'Ÿ', 17}, {'Ź', 19}, {'ź', 16}, {'Ż', 19}, {'ż', 16}, {'Ž', 19}, {'ž', 16}, {'ƒ', 19}, {'Ș', 21}, {'ș', 17}, {'Ț', 17}, {'ț', 9}, {'ˆ', 8}, {'ˇ', 8}, {'ˉ', 6}, {'˘', 8}, {'˙', 8}, {'˚', 8}, {'˛', 8}, {'˜', 8}, {'˝', 8}, {'Ё', 19}, {'Ѓ', 16}, {'Є', 18}, {'Ѕ', 21}, {'І', 8}, {'Ї', 8}, {'Ј', 16}, {'Љ', 28}, {'Њ', 21}, {'Ќ', 19}, {'Ў', 17}, {'Џ', 18}, {'А', 19}, {'Б', 19}, {'В', 19}, {'Г', 15}, {'Д', 19}, {'Е', 18}, {'Ж', 21}, {'З', 17}, {'И', 19}, {'Й', 19}, {'К', 17}, {'Л', 17}, {'М', 26}, {'Н', 18}, {'О', 20}, {'П', 19}, {'Р', 19}, {'С', 19}, {'Т', 19}, {'У', 19}, {'Ф', 20}, {'Х', 19}, {'Ц', 20}, {'Ч', 16}, {'Ш', 26}, {'Щ', 29}, {'Ъ', 20}, {'Ы', 24}, {'Ь', 19}, {'Э', 18}, {'Ю', 27}, {'Я', 20}, {'а', 16}, {'б', 17}, {'в', 16}, {'г', 15}, {'д', 17}, {'е', 17}, {'ж', 20}, {'з', 15}, {'и', 16}, {'й', 16}, {'к', 17}, {'л', 15}, {'м', 25}, {'н', 16}, {'о', 16}, {'п', 16}, {'р', 17}, {'с', 16}, {'т', 14}, {'у', 17}, {'ф', 21}, {'х', 15}, {'ц', 17}, {'ч', 15}, {'ш', 25}, {'щ', 27}, {'ъ', 16}, {'ы', 20}, {'ь', 16}, {'э', 14}, {'ю', 23}, {'я', 17}, {'ё', 17}, {'ђ', 17}, {'ѓ', 16}, {'є', 14}, {'ѕ', 16}, {'і', 8}, {'ї', 8}, {'ј', 7}, {'љ', 22}, {'њ', 25}, {'ћ', 17}, {'ќ', 16}, {'ў', 17}, {'џ', 17}, {'Ґ', 15}, {'ґ', 13}, {'–', 15}, {'—', 31}, {'‘', 6}, {'’', 6}, {'‚', 6}, {'“', 12}, {'”', 12}, {'„', 12}, {'†', 20}, {'‡', 20}, {'•', 15}, {'…', 31}, {'‰', 31}, {'‹', 8}, {'›', 8}, {'€', 19}, {'™', 30}, {'−', 18}, {'∙', 8}, {'□', 21}, {'', 40}, {'', 40}, {'', 40}, {'', 40}, {'', 41}, {'', 41}, {'', 32}, {'', 32}, {'', 40}, {'', 40}, {'', 34}, {'', 34}, {'', 40}, {'', 40}, {'', 40}, {'', 41}, {'', 32}, {'', 41}, {'', 32}, {'', 40}, {'', 40}, {'', 40}, {'', 40}, {'', 40}, {'', 40}, {'', 40}, {'', 40}
}
},
{
FONT.MONOSPACE,
new Dictionary<char, int> {
{' ', 24 }, { '!', 24 }, { '"', 24}, {'#', 24}, {'$', 24}, {'%', 24}, {'&', 24}, {'\'', 24}, {'(', 24}, {')', 24}, {'*', 24}, {'+', 24}, {',', 24}, {'-', 24}, {'.', 24}, {'/', 24}, {'0', 24}, {'1', 24}, {'2', 24}, {'3', 24}, {'4', 24}, {'5', 24}, {'6', 24}, {'7', 24}, {'8', 24}, {'9', 24}, {':', 24}, {';', 24}, {'<', 24}, {'=', 24}, {'>', 24}, {'?', 24}, {'@', 24}, {'A', 24}, {'B', 24}, {'C', 24}, {'D', 24}, {'E', 24}, {'F', 24}, {'G', 24}, {'H', 24}, {'I', 24}, {'J', 24}, {'K', 24}, {'L', 24}, {'M', 24}, {'N', 24}, {'O', 24}, {'P', 24}, {'Q', 24}, {'R', 24}, {'S', 24}, {'T', 24}, {'U', 24}, {'V', 24}, {'W', 24}, {'X', 24}, {'Y', 24}, {'Z', 24}, {'[', 24}, {'\\', 24}, {']', 24}, {'^', 24}, {'_', 24}, {'`', 24}, {'a', 24}, {'b', 24}, {'c', 24}, {'d', 24}, {'e', 24}, {'f', 24}, {'g', 24}, {'h', 24}, {'i', 24}, {'j', 24}, {'k', 24}, {'l', 24}, {'m', 24}, {'n', 24}, {'o', 24}, {'p', 24}, {'q', 24}, {'r', 24}, {'s', 24}, {'t', 24}, {'u', 24}, {'v', 24}, {'w', 24}, {'x', 24}, {'y', 24}, {'z', 24}, {'{', 24}, {'|', 24}, {'}', 24}, {'~', 24}, {' ', 24}, {'¡', 24}, {'¢', 24}, {'£', 24}, {'¤', 24}, {'¥', 24}, {'¦', 24}, {'§', 24}, {'¨', 24}, {'©', 24}, {'ª', 24}, {'«', 24}, {'¬', 24}, {'­', 24}, {'®', 24}, {'¯', 24}, {'°', 24}, {'±', 24}, {'²', 24}, {'³', 24}, {'´', 24}, {'µ', 24}, {'¶', 24}, {'·', 24}, {'¸', 24}, {'¹', 24}, {'º', 24}, {'»', 24}, {'¼', 24}, {'½', 24}, {'¾', 24}, {'¿', 24}, {'À', 24}, {'Á', 24}, {'Â', 24}, {'Ã', 24}, {'Ä', 24}, {'Å', 24}, {'Æ', 24}, {'Ç', 24}, {'È', 24}, {'É', 24}, {'Ê', 24}, {'Ë', 24}, {'Ì', 24}, {'Í', 24}, {'Î', 24}, {'Ï', 24}, {'Ð', 24}, {'Ñ', 24}, {'Ò', 24}, {'Ó', 24}, {'Ô', 24}, {'Õ', 24}, {'Ö', 24}, {'×', 24}, {'Ø', 24}, {'Ù', 24}, {'Ú', 24}, {'Û', 24}, {'Ü', 24}, {'Ý', 24}, {'Þ', 24}, {'ß', 24}, {'à', 24}, {'á', 24}, {'â', 24}, {'ã', 24}, {'ä', 24}, {'å', 24}, {'æ', 24}, {'ç', 24}, {'è', 24}, {'é', 24}, {'ê', 24}, {'ë', 24}, {'ì', 24}, {'í', 24}, {'î', 24}, {'ï', 24}, {'ð', 24}, {'ñ', 24}, {'ò', 24}, {'ó', 24}, {'ô', 24}, {'õ', 24}, {'ö', 24}, {'÷', 24}, {'ø', 24}, {'ù', 24}, {'ú', 24}, {'û', 24}, {'ü', 24}, {'ý', 24}, {'þ', 24}, {'ÿ', 24}, {'Ā', 24}, {'ā', 24}, {'Ă', 24}, {'ă', 24}, {'Ą', 24}, {'ą', 24}, {'Ć', 24}, {'ć', 24}, {'Ĉ', 24}, {'ĉ', 24}, {'Ċ', 24}, {'ċ', 24}, {'Č', 24}, {'č', 24}, {'Ď', 24}, {'ď', 24}, {'Đ', 24}, {'đ', 24}, {'Ē', 24}, {'ē', 24}, {'Ĕ', 24}, {'ĕ', 24}, {'Ė', 24}, {'ė', 24}, {'Ę', 24}, {'ę', 24}, {'Ě', 24}, {'ě', 24}, {'Ĝ', 24}, {'ĝ', 24}, {'Ğ', 24}, {'ğ', 24}, {'Ġ', 24}, {'ġ', 24}, {'Ģ', 24}, {'ģ', 24}, {'Ĥ', 24}, {'ĥ', 24}, {'Ħ', 24}, {'ħ', 24}, {'Ĩ', 24}, {'ĩ', 24}, {'Ī', 24}, {'ī', 24}, {'Į', 24}, {'į', 24}, {'İ', 24}, {'ı', 24}, {'Ĳ', 24}, {'ĳ', 24}, {'Ĵ', 24}, {'ĵ', 24}, {'Ķ', 24}, {'ķ', 24}, {'Ĺ', 24}, {'ĺ', 24}, {'Ļ', 24}, {'ļ', 24}, {'Ľ', 24}, {'ľ', 24}, {'Ŀ', 24}, {'ŀ', 24}, {'Ł', 24}, {'ł', 24}, {'Ń', 24}, {'ń', 24}, {'Ņ', 24}, {'ņ', 24}, {'Ň', 24}, {'ň', 24}, {'ŉ', 24}, {'Ō', 24}, {'ō', 24}, {'Ŏ', 24}, {'ŏ', 24}, {'Ő', 24}, {'ő', 24}, {'Œ', 24}, {'œ', 24}, {'Ŕ', 24}, {'ŕ', 24}, {'Ŗ', 24}, {'ŗ', 24}, {'Ř', 24}, {'ř', 24}, {'Ś', 24}, {'ś', 24}, {'Ŝ', 24}, {'ŝ', 24}, {'Ş', 24}, {'ş', 24}, {'Š', 24}, {'š', 24}, {'Ţ', 24}, {'ţ', 24}, {'Ť', 24}, {'ť', 24}, {'Ŧ', 24}, {'ŧ', 24}, {'Ũ', 24}, {'ũ', 24}, {'Ū', 24}, {'ū', 24}, {'Ŭ', 24}, {'ŭ', 24}, {'Ů', 24}, {'ů', 24}, {'Ű', 24}, {'ű', 24}, {'Ų', 24}, {'ų', 24}, {'Ŵ', 24}, {'ŵ', 24}, {'Ŷ', 24}, {'ŷ', 24}, {'Ÿ', 24}, {'Ź', 24}, {'ź', 24}, {'Ż', 24}, {'ż', 24}, {'Ž', 24}, {'ž', 24}, {'ƒ', 24}, {'Ș', 24}, {'ș', 24}, {'Ț', 24}, {'ț', 24}, {'ˆ', 24}, {'ˇ', 24}, {'ˉ', 24}, {'˘', 24}, {'˙', 24}, {'˚', 24}, {'˛', 24}, {'˜', 24}, {'˝', 24}, {'Ё', 24}, {'Ѓ', 24}, {'Є', 24}, {'Ѕ', 24}, {'І', 24}, {'Ї', 24}, {'Ј', 24}, {'Љ', 24}, {'Њ', 24}, {'Ќ', 24}, {'Ў', 24}, {'Џ', 24}, {'А', 24}, {'Б', 24}, {'В', 24}, {'Г', 24}, {'Д', 24}, {'Е', 24}, {'Ж', 24}, {'З', 24}, {'И', 24}, {'Й', 24}, {'К', 24}, {'Л', 24}, {'М', 24}, {'Н', 24}, {'О', 24}, {'П', 24}, {'Р', 24}, {'С', 24}, {'Т', 24}, {'У', 24}, {'Ф', 24}, {'Х', 24}, {'Ц', 24}, {'Ч', 24}, {'Ш', 24}, {'Щ', 24}, {'Ъ', 24}, {'Ы', 24}, {'Ь', 24}, {'Э', 24}, {'Ю', 24}, {'Я', 24}, {'а', 24}, {'б', 24}, {'в', 24}, {'г', 24}, {'д', 24}, {'е', 24}, {'ж', 24}, {'з', 24}, {'и', 24}, {'й', 24}, {'к', 24}, {'л', 24}, {'м', 24}, {'н', 24}, {'о', 24}, {'п', 24}, {'р', 24}, {'с', 24}, {'т', 24}, {'у', 24}, {'ф', 24}, {'х', 24}, {'ц', 24}, {'ч', 24}, {'ш', 24}, {'щ', 24}, {'ъ', 24}, {'ы', 24}, {'ь', 24}, {'э', 24}, {'ю', 24}, {'я', 24}, {'ё', 24}, {'ђ', 24}, {'ѓ', 24}, {'є', 24}, {'ѕ', 24}, {'і', 24}, {'ї', 24}, {'ј', 24}, {'љ', 24}, {'њ', 24}, {'ћ', 24}, {'ќ', 24}, {'ў', 24}, {'џ', 24}, {'Ґ', 24}, {'ґ', 24}, {'–', 24}, {'—', 24}, {'‘', 24}, {'’', 24}, {'‚', 24}, {'“', 24}, {'”', 24}, {'„', 24}, {'†', 24}, {'‡', 24}, {'•', 24}, {'…', 24}, {'‰', 24}, {'‹', 24}, {'›', 24}, {'€', 24}, {'™', 24}, {'−', 24}, {'∙', 24}, {'□', 24}, {'', 24}, {'', 24}, {'', 24}, {'', 24}, {'', 24}, {'', 24}, {'', 24}, {'', 24}, {'', 24}, {'', 24}, {'', 24}, {'', 24}, {'', 24}, {'', 24}, {'', 24}, {'', 24}, {'', 24}, {'', 24}, {'', 24}, {'', 24}, {'', 24}, {'', 24}, {'', 24}, {'', 24}, {'', 24}, {'', 24}, {'', 24}
}
}
};*/
public enum PadMode { LEFT, RIGHT, BOTH };
public enum RoundMode { FLOOR, CEIL };
public static void SelectFont(FONT f)
{
selectedFont = f;
}
public static void Reset()
{
selectedFont = FONT.DEFAULT;
}
public static int GetLetterWidth(char c)
{
switch (selectedFont)
{
case FONT.DEFAULT:
switch (c)
{
case ' ': return 8; case '!': return 8; case '"': return 10; case '#': return 19; case '$': return 20; case '%': return 24; case '&': return 20; case '\'': return 6; case '(': return 9; case ')': return 9; case '*': return 11; case '+': return 18; case ',': return 9; case '-': return 10; case '.': return 9; case '/': return 14; case '0': return 19; case '1': return 9; case '2': return 19; case '3': return 17; case '4': return 19; case '5': return 19; case '6': return 19; case '7': return 16; case '8': return 19; case '9': return 19; case ':': return 9; case ';': return 9; case '<': return 18; case '=': return 18; case '>': return 18; case '?': return 16; case '@': return 25; case 'A': return 21; case 'B': return 21; case 'C': return 19; case 'D': return 21; case 'E': return 18; case 'F': return 17; case 'G': return 20; case 'H': return 20; case 'I': return 8; case 'J': return 16; case 'K': return 17; case 'L': return 15; case 'M': return 26; case 'N': return 21; case 'O': return 21; case 'P': return 20; case 'Q': return 21; case 'R': return 21; case 'S': return 21; case 'T': return 17; case 'U': return 20; case 'V': return 20; case 'W': return 31; case 'X': return 19; case 'Y': return 20; case 'Z': return 19; case '[': return 9; case '\\': return 12; case ']': return 9; case '^': return 18; case '_': return 15; case '`': return 8; case 'a': return 17; case 'b': return 17; case 'c': return 16; case 'd': return 17; case 'e': return 17; case 'f': return 9; case 'g': return 17; case 'h': return 17; case 'i': return 8; case 'j': return 8; case 'k': return 17; case 'l': return 8; case 'm': return 27; case 'n': return 17; case 'o': return 17; case 'p': return 17; case 'q': return 17; case 'r': return 10; case 's': return 17; case 't': return 9; case 'u': return 17; case 'v': return 15; case 'w': return 27; case 'x': return 15; case 'y': return 17; case 'z': return 16; case '{': return 9; case '|': return 6; case '}': return 9; case '~': return 18; case ' ': return 8; case '¡': return 8; case '¢': return 16; case '£': return 17; case '¤': return 19; case '¥': return 19; case '¦': return 6; case '§': return 20; case '¨': return 8; case '©': return 25; case 'ª': return 10; case '«': return 15; case '¬': return 18; case '­': return 10; case '®': return 25; case '¯': return 8; case '°': return 12; case '±': return 18; case '²': return 11; case '³': return 11; case '´': return 8; case 'µ': return 17; case '¶': return 18; case '·': return 9; case '¸': return 8; case '¹': return 11; case 'º': return 10; case '»': return 15; case '¼': return 27; case '½': return 29; case '¾': return 28; case '¿': return 16; case 'À': return 21; case 'Á': return 21; case 'Â': return 21; case 'Ã': return 21; case 'Ä': return 21; case 'Å': return 21; case 'Æ': return 31; case 'Ç': return 19; case 'È': return 18; case 'É': return 18; case 'Ê': return 18; case 'Ë': return 18; case 'Ì': return 8; case 'Í': return 8; case 'Î': return 8; case 'Ï': return 8; case 'Ð': return 21; case 'Ñ': return 21; case 'Ò': return 21; case 'Ó': return 21; case 'Ô': return 21; case 'Õ': return 21; case 'Ö': return 21; case '×': return 18; case 'Ø': return 21; case 'Ù': return 20; case 'Ú': return 20; case 'Û': return 20; case 'Ü': return 20; case 'Ý': return 17; case 'Þ': return 20; case 'ß': return 19; case 'à': return 17; case 'á': return 17; case 'â': return 17; case 'ã': return 17; case 'ä': return 17; case 'å': return 17; case 'æ': return 28; case 'ç': return 16; case 'è': return 17; case 'é': return 17; case 'ê': return 17; case 'ë': return 17; case 'ì': return 8; case 'í': return 8; case 'î': return 8; case 'ï': return 8; case 'ð': return 17; case 'ñ': return 17; case 'ò': return 17; case 'ó': return 17; case 'ô': return 17; case 'õ': return 17; case 'ö': return 17; case '÷': return 18; case 'ø': return 17; case 'ù': return 17; case 'ú': return 17; case 'û': return 17; case 'ü': return 17; case 'ý': return 17; case 'þ': return 17; case 'ÿ': return 17; case 'Ā': return 20; case 'ā': return 17; case 'Ă': return 21; case 'ă': return 17; case 'Ą': return 21; case 'ą': return 17; case 'Ć': return 19; case 'ć': return 16; case 'Ĉ': return 19; case 'ĉ': return 16; case 'Ċ': return 19; case 'ċ': return 16; case 'Č': return 19; case 'č': return 16; case 'Ď': return 21; case 'ď': return 17; case 'Đ': return 21; case 'đ': return 17; case 'Ē': return 18; case 'ē': return 17; case 'Ĕ': return 18; case 'ĕ': return 17; case 'Ė': return 18; case 'ė': return 17; case 'Ę': return 18; case 'ę': return 17; case 'Ě': return 18; case 'ě': return 17; case 'Ĝ': return 20; case 'ĝ': return 17; case 'Ğ': return 20; case 'ğ': return 17; case 'Ġ': return 20; case 'ġ': return 17; case 'Ģ': return 20; case 'ģ': return 17; case 'Ĥ': return 20; case 'ĥ': return 17; case 'Ħ': return 20; case 'ħ': return 17; case 'Ĩ': return 8; case 'ĩ': return 8; case 'Ī': return 8; case 'ī': return 8; case 'Į': return 8; case 'į': return 8; case 'İ': return 8; case 'ı': return 8; case 'Ĳ': return 24; case 'ĳ': return 14; case 'Ĵ': return 16; case 'ĵ': return 8; case 'Ķ': return 17; case 'ķ': return 17; case 'Ĺ': return 15; case 'ĺ': return 8; case 'Ļ': return 15; case 'ļ': return 8; case 'Ľ': return 15; case 'ľ': return 8; case 'Ŀ': return 15; case 'ŀ': return 10; case 'Ł': return 15; case 'ł': return 8; case 'Ń': return 21; case 'ń': return 17; case 'Ņ': return 21; case 'ņ': return 17; case 'Ň': return 21; case 'ň': return 17; case 'ŉ': return 17; case 'Ō': return 21; case 'ō': return 17; case 'Ŏ': return 21; case 'ŏ': return 17; case 'Ő': return 21; case 'ő': return 17; case 'Œ': return 31; case 'œ': return 28; case 'Ŕ': return 21; case 'ŕ': return 10; case 'Ŗ': return 21; case 'ŗ': return 10; case 'Ř': return 21; case 'ř': return 10; case 'Ś': return 21; case 'ś': return 17; case 'Ŝ': return 21; case 'ŝ': return 17; case 'Ş': return 21; case 'ş': return 17; case 'Š': return 21; case 'š': return 17; case 'Ţ': return 17; case 'ţ': return 9; case 'Ť': return 17; case 'ť': return 9; case 'Ŧ': return 17; case 'ŧ': return 9; case 'Ũ': return 20; case 'ũ': return 17; case 'Ū': return 20; case 'ū': return 17; case 'Ŭ': return 20; case 'ŭ': return 17; case 'Ů': return 20; case 'ů': return 17; case 'Ű': return 20; case 'ű': return 17; case 'Ų': return 20; case 'ų': return 17; case 'Ŵ': return 31; case 'ŵ': return 27; case 'Ŷ': return 17; case 'ŷ': return 17; case 'Ÿ': return 17; case 'Ź': return 19; case 'ź': return 16; case 'Ż': return 19; case 'ż': return 16; case 'Ž': return 19; case 'ž': return 16; case 'ƒ': return 19; case 'Ș': return 21; case 'ș': return 17; case 'Ț': return 17; case 'ț': return 9; case 'ˆ': return 8; case 'ˇ': return 8; case 'ˉ': return 6; case '˘': return 8; case '˙': return 8; case '˚': return 8; case '˛': return 8; case '˜': return 8; case '˝': return 8; case 'Ё': return 19; case 'Ѓ': return 16; case 'Є': return 18; case 'Ѕ': return 21; case 'І': return 8; case 'Ї': return 8; case 'Ј': return 16; case 'Љ': return 28; case 'Њ': return 21; case 'Ќ': return 19; case 'Ў': return 17; case 'Џ': return 18; case 'А': return 19; case 'Б': return 19; case 'В': return 19; case 'Г': return 15; case 'Д': return 19; case 'Е': return 18; case 'Ж': return 21; case 'З': return 17; case 'И': return 19; case 'Й': return 19; case 'К': return 17; case 'Л': return 17; case 'М': return 26; case 'Н': return 18; case 'О': return 20; case 'П': return 19; case 'Р': return 19; case 'С': return 19; case 'Т': return 19; case 'У': return 19; case 'Ф': return 20; case 'Х': return 19; case 'Ц': return 20; case 'Ч': return 16; case 'Ш': return 26; case 'Щ': return 29; case 'Ъ': return 20; case 'Ы': return 24; case 'Ь': return 19; case 'Э': return 18; case 'Ю': return 27; case 'Я': return 20; case 'а': return 16; case 'б': return 17; case 'в': return 16; case 'г': return 15; case 'д': return 17; case 'е': return 17; case 'ж': return 20; case 'з': return 15; case 'и': return 16; case 'й': return 16; case 'к': return 17; case 'л': return 15; case 'м': return 25; case 'н': return 16; case 'о': return 16; case 'п': return 16; case 'р': return 17; case 'с': return 16; case 'т': return 14; case 'у': return 17; case 'ф': return 21; case 'х': return 15; case 'ц': return 17; case 'ч': return 15; case 'ш': return 25; case 'щ': return 27; case 'ъ': return 16; case 'ы': return 20; case 'ь': return 16; case 'э': return 14; case 'ю': return 23; case 'я': return 17; case 'ё': return 17; case 'ђ': return 17; case 'ѓ': return 16; case 'є': return 14; case 'ѕ': return 16; case 'і': return 8; case 'ї': return 8; case 'ј': return 7; case 'љ': return 22; case 'њ': return 25; case 'ћ': return 17; case 'ќ': return 16; case 'ў': return 17; case 'џ': return 17; case 'Ґ': return 15; case 'ґ': return 13; case '–': return 15; case '—': return 31; case '‘': return 6; case '’': return 6; case '‚': return 6; case '“': return 12; case '”': return 12; case '„': return 12; case '†': return 20; case '‡': return 20; case '•': return 15; case '…': return 31; case '‰': return 31; case '‹': return 8; case '›': return 8; case '€': return 19; case '™': return 30; case '−': return 18; case '∙': return 8; case '□': return 21; case '': return 40; case '': return 40; case '': return 40; case '': return 40; case '': return 41; case '': return 41; case '': return 32; case '': return 32; case '': return 40; case '': return 40; case '': return 34; case '': return 34; case '': return 40; case '': return 40; case '': return 40; case '': return 41; case '': return 32; case '': return 41; case '': return 32; case '': return 40; case '': return 40; case '': return 40; case '': return 40; case '': return 40; case '': return 40; case '': return 40; case '': return 40;
default:
return 6;
}
case FONT.MONOSPACE:
return 24;
default:
return 10;
}
}
public static int GetTextWidth(StringBuilder text)
{
return GetTextWidth(text, 0, text.Length);
}
public static int GetTextWidth(StringBuilder text, int start, int length)
{
Logger.debug("TextUtils.GetTextWidth()");
Logger.IncLvl();
if (start + length > text.Length)
{
throw new Exception("ERROR: stringbuilder slice exceeds the stringbuilders length!");
}
text = text.Replace("\r", "");
int width = 0;
int lineWidth = 0;
for (int i = start; i < start + length; i++)
{
if(text[i] == '\n')
{
width = Math.Max(width, lineWidth - 1);
lineWidth = 0;
}
else
{
lineWidth += GetLetterWidth(text[i]) + 1;
}
}
Logger.DecLvl();
return Math.Max(width, lineWidth - 1);
}
/*public static string RemoveLastTrailingNewline(string text)
{
Logger.debug("TextUtils.RemoveLastTrailingNewline");
Logger.IncLvl();
Logger.DecLvl();
return (text.Length > 1 && text[text.Length - 1] == '\n') ? text.Remove(text.Length - 1) : text;
}
public static string RemoveFirstTrailingNewline(string text)
{
Logger.debug("TextUtils.RemoveFirstTrailingNewline");
Logger.IncLvl();
Logger.DecLvl();
return (text.Length > 1 && text[0] == '\n') ? text.Remove(0) : text;
}*/
public static StringBuilder CreateStringOfLength(string constituent, int length)
{
return CreateStringOfLength(constituent, length, RoundMode.FLOOR);
}
public static StringBuilder CreateStringOfLength(string constituent, int length, RoundMode mode)
{
Logger.debug("TextUtils.CreateStringOfLength()");
Logger.IncLvl();
int lengthOfConstituent = GetTextWidth(new StringBuilder(constituent));
if (mode == RoundMode.CEIL)
{
Logger.debug("Ceil mode");
length += lengthOfConstituent;
}
StringBuilder result = new StringBuilder();
if (length < lengthOfConstituent)
{
Logger.DecLvl();
return new StringBuilder();
}
int i;
for (i = lengthOfConstituent - 1; i <= length - 1; i = i + 1 + lengthOfConstituent)
{
result.Append(constituent);
}
Logger.DecLvl();
return result;
}
public static StringBuilder PadText(StringBuilder text, int totalWidth, PadMode mode)
{
return PadText(text, totalWidth, mode, " ");
}
public static StringBuilder PadText(StringBuilder text, int totalWidth, PadMode mode, string padString)
{
Logger.debug("TextUtils.PadText()");
Logger.IncLvl();
StringBuilder result = new StringBuilder();
StringBuilder padding = new StringBuilder();
int width;
int lineStart;
int lineEnd = -1;
do
{
lineStart = lineEnd + 1;
lineEnd = -1;
for (int i = lineStart; i < text.Length; i++)
{
if (text[i] == '\n')
{
lineEnd = i;
break;
}
}
if (lineEnd == -1) lineEnd = text.Length;
width = GetTextWidth(text, lineStart, lineEnd - lineStart) + 1;
if(mode == PadMode.BOTH)
{
padding = CreateStringOfLength(padString, (totalWidth - width) / 2);
result.Append(padding);
SBExtensions.AppendSubstr(result, text, lineStart, lineEnd - lineStart);
result.Append(padding);
}
else
{
padding = CreateStringOfLength(padString, totalWidth - width);
if (mode == PadMode.LEFT)
{
result.Append(padding);
SBExtensions.AppendSubstr(result, text, lineStart, lineEnd - lineStart);
}
else
{
SBExtensions.AppendSubstr(result, text, lineStart, lineEnd - lineStart);
result.Append(padding);
}
}
result.Append("\n");
}
while (lineEnd < text.Length);
if(result.Length > 0)
result.Remove(result.Length - 1, 1);
Logger.DecLvl();
return result;
}
}

public static class SBExtensions
{
public static void AppendSubstr(StringBuilder me, StringBuilder append, int start, int count)
{
me.Capacity = me.Capacity + append.Length;
int loopEnd = Math.Min(append.Length, start + count);
for (int i = start; i < loopEnd; i++)
{
me.Append(append[i]);
}
}
}
public static class XML
{
public static Dictionary<string, Func<XML.XMLTree>> NodeRegister = new Dictionary<string, Func<XML.XMLTree>> {
{"root", () => { return new XML.RootNode(); } },
{"menu", () => { return new XML.Menu(); } },
{"menuitem", () => { return new XML.MenuItem(); } },
{"progressbar", () => { return new XML.ProgressBar(); } },
{"container", () => { return new XML.Container(); } },
{"hl", () => { return new XML.HorizontalLine(); } },
{"uicontrols", () => { return new UIControls(); } },
{"textinput", () => { return new TextInput(); } },
{"submitbutton", () => { return new SubmitButton(); } },
{"br", () => { return new Break(); } },
{"space", () => { return new Space(); } },
{"hidden", () => { return new Hidden(); } },
{"hiddendata", () => { return new Hidden(); } },
{"meta", () => { return new MetaNode(); } }
};
public static XMLTree CreateNode(string type)
{
Logger.debug("XML.CreateNode()");
Logger.IncLvl();
type = type.ToLower();
if (NodeRegister.ContainsKey(type))
{
Logger.DecLvl();
return NodeRegister[type]();
}
else
{
Logger.DecLvl();
return new Generic(type);
}
}
public static XMLTree ParseXML(string xmlString)
{
char[] spaceChars = { ' ', '\n' };
Logger.debug("ParseXML");
Logger.IncLvl();
RootNode root = new RootNode();
XMLTree currentNode = root;
string type;
//Logger.debug("Enter while loop");
while (xmlString.Length > 0)
{
if (xmlString[0] == '<')
{
//Logger.debug("found tag");
if (xmlString[1] == '/')
{
//Logger.debug("tag is end tag");
int spacePos = xmlString.IndexOfAny(spaceChars);
int bracketPos = xmlString.IndexOf('>');
int typeLength = (spacePos == -1 ? bracketPos : Math.Min(spacePos, bracketPos)) - 2;
type = xmlString.Substring(2, typeLength).ToLower();
if (type != currentNode.Type)
{
throw new Exception("Invalid end tag ('" + type + "(!= " + currentNode.Type + "') found (node has been ended but not started)!");
}
currentNode = currentNode.GetParent() as XMLTree;
xmlString = xmlString.Substring(bracketPos + 1);
}
else
{
//Logger.debug("tag is start tag");
int spacePos = xmlString.IndexOfAny(spaceChars);
int bracketPos = Parser.GetNextOutsideQuotes('>', xmlString);
int typeLength = (spacePos == -1 ? bracketPos : Math.Min(spacePos, bracketPos)) - 1;
type = xmlString.Substring(1, typeLength).ToLower().TrimEnd(new char[] { '/' });
XMLTree newNode = XML.CreateNode(type);
if (newNode == null)
{
int closingBracketPos = xmlString.IndexOf("<");
int textLength = closingBracketPos == -1 ? xmlString.Length : closingBracketPos;
newNode = new XML.TextNode(xmlString.Substring(0, textLength).Trim());
}
//Logger.debug("add new node of type '" + newNode.Type + "=" + type + "' to current node");
currentNode.AddChild(newNode);
//Logger.debug("added new node to current node");
if (spacePos != -1 && spacePos < bracketPos)
{
string attrString = xmlString.Substring(typeLength + 2, bracketPos - typeLength - 2);
attrString = attrString.TrimEnd(new char[] { '/' });
//Logger.debug("get xml attributes. attribute string: '" + attrString + "/" + xmlString + "'");
Dictionary<string, string> attributes =
Parser.GetXMLAttributes(attrString);
//Logger.debug("got xml attributes");
foreach (string key in attributes.Keys)
{
newNode.SetAttribute(key, attributes[key]);
}
}
if (newNode.Type == "textnode" || bracketPos == -1 || xmlString[bracketPos - 1] != '/')
{
currentNode = newNode;
}
xmlString = xmlString.Substring(bracketPos + 1);
}
}
else
{
int bracketPos = xmlString.IndexOf("<");
int textLength = bracketPos == -1 ? xmlString.Length : bracketPos;
XMLTree newNode = new XML.TextNode(xmlString.Substring(0, textLength).Trim());
if (true || newNode.GetRenderBox(0) != null)
{
currentNode.AddChild(newNode);
}
xmlString = bracketPos == -1 ? "" : xmlString.Substring(bracketPos);
}
}
//Logger.debug("parsing finished");
Logger.DecLvl();
return root;
}

public class RootNode : XMLTree
{
public RootNode() : base()
{
Type = "root";
PreventDefault("UP");
PreventDefault("DOWN");
}
public override string GetAttribute(string key)
{
XMLTree meta = GetNode((node) => { return node.Type == "meta"; });
string value;
if (meta != null)
{
value = meta.GetAttribute(key);
}
else
{
value = base.GetAttribute(key);
}
switch (key)
{
case "width":
if (value == null)
{
value = "100%";
}
break;
}
return value;
}
public override void SetAttribute(string key, string value)
{
XMLTree meta = GetNode((node) => { return node.Type == "meta"; });
if (meta != null)
{
meta.SetAttribute(key, value);
}
else
{
base.SetAttribute(key, value);
}
}
public override void UpdateSelectability(XMLTree child)
{
base.UpdateSelectability(child);
if (IsSelectable() && !IsSelected())
{
SelectFirst();
}
}
public override bool SelectNext()
{
if (IsSelectable() && !base.SelectNext())
{
return SelectNext();
}
return true;
}
public override bool SelectPrevious()
{
if (!base.SelectPrevious())
{
return SelectPrevious();
}
return true;
}
public override void OnKeyPressed(string keyCode)
{
switch (keyCode)
{
case "UP":
SelectPrevious();
break;
case "DOWN":
SelectNext();
break;
}
}
}

public abstract class XMLTree : XMLParentNode
{
public string Type;
private XMLParentNode Parent;
private List<string> PreventDefaults;
protected List<XMLTree> Children;
protected bool Selectable;
protected bool ChildrenAreSelectable;
private bool Selected;
protected int SelectedChild;
protected bool Activated;
protected Dictionary<string, string> Attributes;
private bool _hasUserInputBindings;
private RenderBox _renderCache;
public bool HasUserInputBindings
{
get { return _hasUserInputBindings; }
set
{
_hasUserInputBindings = value;
if(Parent != null && HasUserInputBindings)
{
Parent.HasUserInputBindings = true;
}
}
}
public int NumberOfChildren
{
get
{
return Children.Count;
}
}
protected bool RerenderRequired;
public virtual RenderBox GetRenderBox(int maxWidth)
{
Logger.debug("XMLTree.GetRenderCache(int)");
Logger.IncLvl();
/*if(_renderCache != null)
{
return _renderCache;
}*/
RenderBoxTree cache = new RenderBoxTree();
RenderBox childCache;
foreach (XMLTree child in Children)
{
childCache = child.GetRenderBox(maxWidth);
cache.Add(childCache);
}
UpdateRenderCacheProperties(cache, maxWidth);
//_renderCache = cache;
Logger.DecLvl();
return cache;
}
protected void UpdateRenderCacheProperties(RenderBox cache, int maxWidth)
{
Logger.debug("XMLTree.UpdateRenderCacheProperties(NodeBox, int)");
Logger.IncLvl();
cache.Flow = GetAttribute("flow") == "horizontal" ? RenderBox.FlowDirection.HORIZONTAL : RenderBox.FlowDirection.VERTICAL;
switch (GetAttribute("alignself"))
{
case "right":
cache.Align = RenderBox.TextAlign.RIGHT;
break;
case "center":
cache.Align = RenderBox.TextAlign.CENTER;
break;
default:
cache.Align = RenderBox.TextAlign.LEFT;
break;
}
int result;
cache.MinWidth = Math.Max(0, ResolveSize(GetAttribute("minwidth"), maxWidth));
cache.MaxWidth = ResolveSize(GetAttribute("maxwidth"), maxWidth);
cache.DesiredWidth = ResolveSize(GetAttribute("width"), maxWidth);
int forcedWidth = ResolveSize(GetAttribute("forcewidth"), maxWidth);
if(forcedWidth != -1)
{
cache.MinWidth = forcedWidth;
cache.MaxWidth = forcedWidth;
}
if (Int32.TryParse(GetAttribute("height"), out result))
cache.Height = result;
//cache.Height = CalculateWidth(GetAttribute("height"), -1);
Logger.DecLvl();
}
public static int ResolveSize(string widthString, int maxWidth)
{
Logger.debug("XMLTree.ResolvePercentage(string, int)");
Logger.IncLvl();
float fWidth;
if(widthString != null && widthString[widthString.Length - 1] == '%' && Single.TryParse(widthString.Substring(0, widthString.Length - 1), out fWidth))
{
if (maxWidth == -1)
return -1;
Logger.DecLvl();
return (int)(fWidth / 100f * Math.Max(0, maxWidth));
}
else
{
int iWidth = -1;
Logger.DecLvl();
if (Int32.TryParse(widthString, out iWidth))
return iWidth;
return -1;
}
}
public XMLTree()
{
Logger.debug("XMLTree constructor");
Logger.IncLvl();
HasUserInputBindings = false;
PreventDefaults = new List<string>();
Parent = null;
Children = new List<XMLTree>();
Selectable = false;
ChildrenAreSelectable = false;
Selected = false;
SelectedChild = -1;
Activated = false;
Attributes = new Dictionary<string, string>();
RerenderRequired = true;
Type = "NULL";
// set attribute defaults
SetAttribute("alignself", "left");
SetAttribute("aligntext", "left");
SetAttribute("selected", "false");
SetAttribute("selectable", "false");
SetAttribute("flow", "vertical");
Logger.DecLvl();
}
public bool IsSelectable()
{
Logger.debug(Type + ": IsSelectable():");
Logger.IncLvl();
Logger.DecLvl();
return Selectable || ChildrenAreSelectable;
}
public bool IsSelected()
{
Logger.debug(Type + ": IsSelected():");
Logger.IncLvl();
Logger.DecLvl();
return Selected;
}
public XMLTree GetSelectedSibling()
{
Logger.debug(Type + ": GetSelectedSibling():");
Logger.IncLvl();
if (!Selected)
{
Logger.DecLvl();
return null;
//throw new Exception(
//    "Node is not selected. You can only get the selected Node from one of it's parent nodes!");
}
if (SelectedChild == -1)
{
Logger.DecLvl();
return this;
}
else
{
Logger.DecLvl();
return Children[SelectedChild].GetSelectedSibling();
}
}
public virtual void AddChild(XMLTree child)
{
Logger.debug(Type + ": AddChild():");
Logger.IncLvl();
AddChildAt(Children.Count, child);
Logger.DecLvl();
}
public virtual void AddChildAt(int position, XMLTree child)
{
Logger.debug(Type + ":AddChildAt()");
Logger.IncLvl();
if( position > Children.Count )
{
throw new Exception("XMLTree.AddChildAt - Exception: position must be less than number of children!");
}
RerenderRequired = true;
Children.Insert(position, child);
child.SetParent(this as XMLParentNode);
UpdateSelectability(child);
Logger.DecLvl();
}
public void SetParent(XMLParentNode parent)
{
Logger.debug(Type + ": SetParent():");
Logger.IncLvl();
Parent = parent;
if(HasUserInputBindings && Parent != null)
{
Parent.HasUserInputBindings = true;
}
Logger.DecLvl();
}
public XMLParentNode GetParent()
{
Logger.debug(Type + ": GetParent():");
Logger.IncLvl();
Logger.DecLvl();
return Parent;
}
public XMLTree GetChild(int i)
{
Logger.debug(Type + ": GetChild():");
Logger.IncLvl();
Logger.DecLvl();
return i < Children.Count ? Children[i] : null;
}
public XMLTree GetNode(Func<XMLTree, bool> filter)
{
if (filter(this))
{
return this;
}
else
{
XMLTree child = GetChild(0);
XMLTree childResult;
for (int i = 1; child != null; i++)
{
childResult = child.GetNode(filter);
if (childResult != null)
{
return childResult;
}
child = GetChild(i);
}
}
return null;
}
public List<XMLTree> GetAllNodes(Func<XMLTree, bool> filter)
{
List<XMLTree> nodeList = new List<XMLTree>();
GetAllNodes(filter, ref nodeList);
return nodeList;
}
private void GetAllNodes(Func<XMLTree, bool> filter, ref List<XMLTree> nodeList)
{
if (filter(this))
{
nodeList.Add(this);
}
XMLTree child = GetChild(0);
for (int i = 1; child != null; i++)
{
child.GetAllNodes(filter, ref nodeList);
child = GetChild(i);
}
}
public virtual void UpdateSelectability(XMLTree child)
{
Logger.debug(Type + ": UpdateSelectability():");
Logger.IncLvl();
bool ChildrenWereSelectable = ChildrenAreSelectable;
ChildrenAreSelectable = ChildrenAreSelectable || child.IsSelectable();
if ((Selectable || ChildrenAreSelectable) != (Selectable || ChildrenWereSelectable))
{
RerenderRequired = true;
Logger.debug("update parent selectability");
if(Parent != null)
Parent.UpdateSelectability(this);
Logger.debug("parent selectability updated");
}
Logger.DecLvl();
}
public bool SelectFirst()
{
Logger.debug(Type + ": SelectFirst():");
Logger.IncLvl();
if (SelectedChild != -1)
{
Children[SelectedChild].Unselect();
}
SelectedChild = -1;
bool success = (Selectable || ChildrenAreSelectable) ? SelectNext() : false;
Logger.DecLvl();
return success;
}
public bool SelectLast()
{
Logger.debug(Type + ": SelectLast():");
Logger.IncLvl();
if (SelectedChild != -1)
{
Children[SelectedChild].Unselect();
}
SelectedChild = -1;
Logger.DecLvl();
return (Selectable || ChildrenAreSelectable) ? SelectPrevious() : false;
}
public void Unselect()
{
Logger.debug(Type + ": Unselect():");
Logger.IncLvl();
if (SelectedChild != -1)
{
Children[SelectedChild].Unselect();
}
Selected = false;
Activated = false;
Logger.DecLvl();
}
public virtual bool SelectNext()
{
Logger.debug(Type + ": SelectNext():");
Logger.IncLvl();
bool WasSelected = IsSelected();
if (SelectedChild == -1 || !Children[SelectedChild].SelectNext())
{
Logger.debug(Type + ": find next child to select...");
SelectedChild++;
while ((SelectedChild < Children.Count && (!Children[SelectedChild].SelectFirst())))
{
SelectedChild++;
}
if (SelectedChild == Children.Count)
{
SelectedChild = -1;
Selected = Selectable && !Selected;
}
else
{
Selected = true;
}
}
if (!Selected)
{
Unselect();
}
if (!WasSelected && IsSelected())
{
OnSelect();
RerenderRequired = true;
}
Logger.DecLvl();
return Selected;
}
public virtual bool SelectPrevious()
{
Logger.debug(Type + ": SelectPrevious():");
Logger.IncLvl();
bool WasSelected = IsSelected();
if (SelectedChild == -1) { SelectedChild = Children.Count; }
if (SelectedChild == Children.Count || !Children[SelectedChild].SelectPrevious())
{
SelectedChild--;
while (SelectedChild > -1 && !Children[SelectedChild].SelectLast())
{
SelectedChild--;
}
if (SelectedChild == -1)
{
Selected = Selectable && !Selected;
}
else
{
Selected = true;
}
}
if (!Selected)
{
Unselect();
}
if (!WasSelected && IsSelected())
{
OnSelect();
RerenderRequired = true;
}
Logger.DecLvl();
return Selected;
}
public virtual void OnSelect() { }
public virtual string GetAttribute(string key)
{
Logger.debug(Type + ": GetAttribute(" + key + "):");
Logger.IncLvl();
if (Attributes.ContainsKey(key))
{
Logger.DecLvl();
return Attributes[key];
}
else if( key == "flowdirection" && Attributes.ContainsKey("flow"))
{
return Attributes["flow"];
}
Logger.DecLvl();
return null;
}
public virtual void SetAttribute(string key, string value)
{
Logger.debug(Type + ": SetAttribute():");
Logger.IncLvl();
if (key == "selectable")
{
bool shouldBeSelectable = value == "true";
if (Selectable != shouldBeSelectable)
{
Selectable = shouldBeSelectable;
if (Parent != null)
{
Parent.UpdateSelectability(this);
}
}
}
else if (key == "activated")
{
bool shouldBeActivated = value == "true";
Activated = shouldBeActivated;
}
else if(key == "inputbinding")
{
HasUserInputBindings = true;
if(Parent != null)
{
Parent.HasUserInputBindings = true;
}
}
else if (key == "flow")
{
if(value == "horizontal")
{
//RenderCach.Flow = NodeBox.FlowDirection.HORIZONTAL;
}
else
{
//base.Flow = NodeBox.FlowDirection.VERTICAL;
}
RerenderRequired = true;
}
else if (key == "align")
{
switch(value)
{
case "right":
//base.Align = NodeBox.TextAlign.RIGHT;
break;
case "center":
//base.Align = NodeBox.TextAlign.CENTER;
break;
default:
//base.Align = NodeBox.TextAlign.LEFT;
break;
}
RerenderRequired = true;
}
else if (key == "width")
{
int width;
if(Int32.TryParse(value, out width))
{
//base.DesiredWidth = width;
}
}
Attributes[key] = value;
Logger.DecLvl();
}
public XMLParentNode RetrieveRoot()
{
XMLParentNode ancestor = this;
while (ancestor.GetParent() != null)
{
ancestor = ancestor.GetParent();
}
return ancestor;
}
public void KeyPress(string keyCode)
{
Logger.debug(Type + ": _KeyPress():");
Logger.IncLvl();
Logger.debug("button: " + keyCode);
OnKeyPressed(keyCode);
if (Parent != null && !PreventDefaults.Contains(keyCode))
{
Parent.KeyPress(keyCode);
}
Logger.DecLvl();
}
public virtual void OnKeyPressed(string keyCode)
{
Logger.debug(Type + ": OnKeyPressed()");
Logger.IncLvl();
switch (keyCode)
{
case "ACTIVATE":
ToggleActivation();
break;
default:
break;
}
Logger.DecLvl();
}
public virtual void ToggleActivation()
{
Logger.debug(Type + ": ToggleActivation()");
Logger.IncLvl();
Activated = !Activated;
Logger.DecLvl();
}
public void PreventDefault(string keyCode)
{
Logger.debug(Type + ": PreventDefault()");
Logger.IncLvl();
if (!PreventDefaults.Contains(keyCode))
{
PreventDefaults.Add(keyCode);
}
Logger.DecLvl();
}
public void AllowDefault(string keyCode)
{
Logger.debug(Type + ": AllowDefault()");
Logger.IncLvl();
if (PreventDefaults.Contains(keyCode))
{
PreventDefaults.Remove(keyCode);
}
Logger.DecLvl();
}
public void FollowRoute(Route route)
{
Logger.debug(Type + ": FollowRoute");
Logger.IncLvl();
if (Parent != null)
{
Parent.FollowRoute(route);
}
Logger.DecLvl();
}
public virtual Dictionary<string, string> GetValues(Func<XMLTree, bool> filter)
{
Logger.log(Type + ": GetValues()");
Logger.IncLvl();
Dictionary<string, string> dict = new Dictionary<string, string>();
string name = GetAttribute("name");
string value = GetAttribute("value");
if (name != null && value != null)
{
Logger.log($"Added entry {{{name}: {value}}}");
dict[name] = value;
}
Dictionary<string, string> childDict;
foreach (XMLTree child in Children)
{
childDict = child.GetValues(filter);
foreach (string key in childDict.Keys)
{
if (!dict.ContainsKey(key))
{
dict[key] = childDict[key];
}
}
}
Logger.DecLvl();
return dict;
}
/*public int GetWidth(int maxWidth)
{
Logger.debug(Type + ".GetWidth()");
Logger.IncLvl();
string attributeWidthValue = GetAttribute("width");
if (attributeWidthValue == null)
{
Logger.DecLvl();
return 0;
//return maxWidth;
}
else
{
if (attributeWidthValue[attributeWidthValue.Length - 1] == '%')
{
Logger.debug("is procent value (" + Single.Parse(attributeWidthValue.Substring(0, attributeWidthValue.Length - 1)).ToString() + ")");
Logger.DecLvl();
return (int)(Single.Parse(attributeWidthValue.Substring(0, attributeWidthValue.Length - 1)) / 100f * maxWidth);
}
else if (maxWidth == 0)
{
Logger.DecLvl();
return Int32.Parse(attributeWidthValue);
}
else
{
Logger.DecLvl();
return Math.Min(maxWidth, Int32.Parse(attributeWidthValue));
}
}
}
public string RenderOld(int availableWidth)
{
Logger.debug(Type + ".Render()");
Logger.IncLvl();
List<string> segments = new List<string>();
int width = GetWidth(availableWidth);
PreRender(ref segments, width, availableWidth);
RenderText(ref segments, width, availableWidth);
string renderString = PostRender(segments, width, availableWidth);
Logger.DecLvl();
return renderString;
}
public NodeBox Render(int availableWidth)
{
return Render(availableWidth, 0);
}
public NodeBox Render(int availableWidth, int availableHeight)
{
Logger.debug(Type + ".Render()");
Logger.IncLvl();
Logger.DecLvl();
return Cache;
}
protected virtual void PreRender(ref List<string> segments, int width, int availableWidth)
{
Logger.debug(Type + ".PreRender()");
Logger.IncLvl();
Logger.DecLvl();
}
protected virtual void RenderText(ref List<string> segments, int width, int availableWidth)
{
Logger.debug(Type + ".RenderText()");
Logger.IncLvl();
for (int i = 0; i < Children.Count; i++)
{
if (GetAttribute("flow") == "vertical")
{
string childString = RenderChild(Children[i], width);
if (childString != null)
{
if (i > 0 && Children[i - 1].Type == "textnode" && (Children[i].Type == "textnode" || Children[i].Type == "br"))
{
segments[segments.Count - 1] += childString;
}
else
{
segments.Add(childString);
}
}
else
{
}
}
else
{
string childString = RenderChild(Children[i], width);
if (childString != null)
{
availableWidth -= TextUtils.GetTextWidth(childString);
segments.Add(childString);
}
}
}
Logger.DecLvl();
}
protected virtual string PostRender(List<string> segments, int width, int availableWidth)
{
Logger.debug(Type + ".PostRender()");
Logger.IncLvl();
string renderString = "";
string flowdir = GetAttribute("flow");
string alignChildren = GetAttribute("alignchildren");
string alignSelf = GetAttribute("alignself");
int totalWidth = 0;
foreach (string segment in segments)
{
int lineWidth = TextUtils.GetTextWidth(segment);
if (lineWidth > totalWidth)
{
totalWidth = lineWidth;
}
}
totalWidth = Math.Min(availableWidth, Math.Max(width, totalWidth));
if (flowdir == "vertical")
{
for (int i = 0; i < segments.Count; i++)
{
switch (alignChildren)
{
case "right":
segments[i] = TextUtils.PadText(segments[i], totalWidth, TextUtils.PadMode.LEFT);
break;
case "center":
segments[i] = TextUtils.CenterText(segments[i], totalWidth);
break;
default:
segments[i] = TextUtils.PadText(segments[i], totalWidth, TextUtils.PadMode.RIGHT);
break;
}
}
renderString = String.Join("\n", segments.ToArray());
}
else
{
renderString = String.Join("", segments.ToArray());
}
if (availableWidth - totalWidth > 0)
{
if (alignSelf == "center")
{
Logger.log("Center element...");
renderString = TextUtils.CenterText(renderString, availableWidth);
}
else if (alignSelf == "right")
{
Logger.log("Aligning element right...");
renderString = TextUtils.PadText(renderString, availableWidth, TextUtils.PadMode.RIGHT);
}
}
Logger.DecLvl();
return renderString;
}

protected virtual string RenderChild(XMLTree child, int availableWidth)
{
Logger.log(Type + ".RenderChild()");
Logger.IncLvl();
Logger.DecLvl();
return child.Render(availableWidth);
}*/
public void DetachChild(XMLTree child)
{
Children.Remove(child);
RerenderRequired = true;
}
public void Detach()
{
if(GetParent() != null)
{
GetParent().DetachChild(this);
}
}
/*protected virtual void BuildRenderCache()
{
Logger.debug(Type + ".BuildRenderCache()");
Logger.IncLvl();
//base.Clear();
NodeBoxTree box = this;
foreach (XMLTree child in Children)
{
box.Add(child);
}
RerenderRequired = false;
Logger.DecLvl();
}*/
public virtual string Render(int maxWidth)
{
Logger.debug(Type + ".Render(int)");
Logger.IncLvl();
Logger.debug("RENDERING::PREPARE");
RenderBox cache = GetRenderBox(maxWidth);
Logger.debug("RENDERING::START");
string result = cache.Render(maxWidth);
Logger.DecLvl();
return result;
}
public string Render()
{
return Render(-1);
}
}

public interface XMLParentNode
{
bool HasUserInputBindings { get; set; }
XMLParentNode GetParent();
void UpdateSelectability(XMLTree child);
void KeyPress(string keyCode);
void FollowRoute(Route route);
bool SelectNext();
void DetachChild(XMLTree child);
}

public class TextNode : XMLTree
{
public string Content;
public TextNode(string content) : base()
{
Logger.debug("TextNode constructor()");
Logger.IncLvl();
Logger.debug("content: " + content);
Type = "textnode";
Content = content;
Content.Replace("\n", "");
Content = Content.Trim(new char[] { '\n', ' ', '\r' });
Logger.debug("final content: " + Content);
RerenderRequired = true;
Logger.DecLvl();
}
public override RenderBox GetRenderBox(int maxWidth)
{
Logger.debug("TextNode.GetRenderCache(int)");
Logger.IncLvl();
RenderBox cache = new RenderBoxLeaf(Content);
Logger.DecLvl();
return cache;
}
//protected override void RenderText(ref List<string> segments, int width, int availableWidth) { }
/*protected override string PostRender(List<string> segments, int width, int availableWidth)
{
return Content;
}*/
/*protected override void BuildRenderCache()
{
Logger.debug(Type + ".BuildRenderCache()");
Logger.IncLvl();
RerenderRequired = false;
Logger.DecLvl();
}*/
}

public class Route
{
static public Dictionary<string, Action<string, UIController>> RouteHandlers = new Dictionary<string, Action<string, UIController>>
{
{
"revert", (def, controller) => { controller.RevertUI(); }
},
{
"xml", (def, controller) =>
{
XMLTree ui = XML.ParseXML(Parser.UnescapeQuotes(def));
controller.LoadUI(ui);
}
},
{
"fn", (def, controller) =>
{
if(UIFactories.ContainsKey(def))
{
UIFactories[def](controller);
}
}
}
};
static Dictionary<string, Action<UIController>> UIFactories =
new Dictionary<string, Action<UIController>>();
string Definition;
public Route(string definition)
{
Logger.debug("Route constructor():");
Logger.IncLvl();
Definition = definition;
Logger.DecLvl();
}
public void Follow(UIController controller)
{
Logger.debug("Route.Follow()");
Logger.IncLvl();
string[] DefTypeAndValue = Definition.Split(new char[] { ':' }, 2);
if (Route.RouteHandlers.ContainsKey(DefTypeAndValue[0].ToLower()))
{
Route.RouteHandlers[DefTypeAndValue[0].ToLower()](
DefTypeAndValue.Length >= 2 ? DefTypeAndValue[1] : null, controller
);
}
Logger.DecLvl();
}
static public void RegisterRouteFunction(string id, Action<UIController> fn)
{
UIFactories[id] = fn;
}
}

public class UIController : XMLParentNode
{
XMLTree ui;
public Stack<XMLTree> UIStack;
public string Type;
bool UserInputActive;
IMyTerminalBlock UserInputSource;
TextInputMode UserInputMode;
List<XMLTree> UserInputBindings;
string InputDataCache;
public bool HasUserInputBindings
{
get { return UserInputActive && UserInputSource != null && UserInputBindings.Count > 0; }
set { }
}
public enum TextInputMode { PUBLIC_TEXT, CUSTOM_DATA }
public enum FONT
{
Debug, Red, Green, Blue, White, DarkBlue, UrlNormal, UrlHighlight, ErrorMessageBoxCaption, ErrorMessageBoxText,
InfoMessageBoxCaption, InfoMessageBoxText, ScreenCaption, GameCredits, LoadingScreen, BuildInfo, BuildInfoHighlight,
Monospace, MONO = Monospace, DEFAULT = Debug
}
Dictionary<FONT, long> Fonts = new Dictionary<FONT, long> {
{FONT.Debug, 151057691},
{FONT.Red, -795103743 },
{FONT.Green, -161094011 },
{FONT.Blue, 1920284339 },
{FONT.White, 48665683 },
{FONT.DarkBlue, 1919824171 },
{FONT.UrlNormal, 992097699 },
{FONT.UrlHighlight, -807552222 },
{FONT.ErrorMessageBoxCaption, 1458347610 },
{FONT.ErrorMessageBoxText, 895781166 },
{FONT.InfoMessageBoxCaption, 837834442 },
{FONT.InfoMessageBoxText, 1833612699 },
{FONT.ScreenCaption, 1216738022 },
{FONT.GameCredits, -1859174863 },
{FONT.LoadingScreen, 741958017 },
{FONT.BuildInfo, 1184185815 },
{FONT.BuildInfoHighlight, -270950170 },
{FONT.Monospace, 1147350002 }
};
public UIController(XMLTree rootNode)
{
Logger.debug("UIController constructor()");
Logger.IncLvl();
Type = "CTRL";
UIStack = new Stack<XMLTree>();
UserInputBindings = new List<XMLTree>();
UserInputActive = false;
InputDataCache = "";
ui = rootNode;
ui.SetParent(this);
if (GetSelectedNode() == null && ui.IsSelectable())
{
ui.SelectFirst();
}
CollectUserInputBindings();
Logger.DecLvl();
}
public static UIController FromXML(string xml)
{
Logger.debug("UIController FromXMLString()");
Logger.IncLvl();
XMLTree rootNode = XML.ParseXML(xml);
Logger.DecLvl();
return new UIController(rootNode);
}
public void ApplyScreenProperties(IMyTextPanel screen)
{
Logger.debug("UIController.ApplyScreenProperties()");
Logger.IncLvl();
if (ui.GetAttribute("fontcolor") != null)
{
string colorString = ui.GetAttribute("fontcolor");
colorString = "FF" +
colorString.Substring(colorString.Length - 2, 2)
+ colorString.Substring(colorString.Length - 4, 2)
+ colorString.Substring(colorString.Length - 6, 2);
Color fontColor = new Color(
uint.Parse(colorString, System.Globalization.NumberStyles.AllowHexSpecifier)
);
screen.SetValue<Color>("FontColor", fontColor);
}
if (ui.GetAttribute("fontsize") != null)
{
screen.SetValue<Single>("FontSize", Single.Parse(ui.GetAttribute("fontsize")));
}
if (ui.GetAttribute("backgroundcolor") != null)
{
string colorString = ui.GetAttribute("backgroundcolor");
colorString = "FF" +
colorString.Substring(colorString.Length - 2, 2)
+ colorString.Substring(colorString.Length - 4, 2)
+ colorString.Substring(colorString.Length - 6, 2);
Color fontColor = new Color(
uint.Parse(colorString, System.Globalization.NumberStyles.AllowHexSpecifier));
screen.SetValue<Color>("BackgroundColor", fontColor);
}
if(ui.GetAttribute("fontfamily") != null)
{
string font = ui.GetAttribute("font");
FONT fontName;
long fontValue;
if(Enum.TryParse<FONT>(font, out fontName))
{
screen.SetValue<long>("Font", Fonts[fontName]);
}
else if(long.TryParse(font, out fontValue))
{
screen.SetValue<long>("Font", fontValue);
}

}
Logger.DecLvl();
}
public void Call(List<string> parameters)
{
Logger.debug("UIController.Main()");
Logger.IncLvl();
switch (parameters[0])
{
case "key":
XMLTree selectedNode = GetSelectedNode();
if (selectedNode != null)
{
selectedNode.KeyPress(parameters[1].ToUpper());
}
break;
case "refresh":
string refresh = ui.GetAttribute("refresh");
if (refresh != null)
{
FollowRoute(new Route(refresh));
}
break;
case "revert":
RevertUI();
break;
default:
break;
}
Logger.DecLvl();
return;
}
public void LoadXML(string xmlString)
{
LoadUI(XML.ParseXML(xmlString));
}
public void LoadUI(XMLTree rootNode)
{
Logger.debug("UIController: LoadUI():");
Logger.IncLvl();
if (ui.GetAttribute("historydisabled") == null || ui.GetAttribute("historydisabled") != "true")
{
UIStack.Push(ui);
}
if (rootNode.GetAttribute("revert") != null && rootNode.GetAttribute("revert") == "true")
{
RevertUI();
}
else
{
ui = rootNode;
ui.SetParent(this);
}
UserInputBindings = new List<XMLTree>();
CollectUserInputBindings();
Logger.DecLvl();
}
public void ClearUIStack()
{
UIStack = new Stack<XMLTree>();
}
public void RevertUI()
{
Logger.log("UIController: RevertUI():");
Logger.IncLvl();
if (UIStack.Count == 0)
{
Logger.log("Error: Can't revert: UI stack is empty.");
Logger.DecLvl();
return;
}
ui = UIStack.Pop();
ui.SetParent(this);
Logger.DecLvl();
}
public string Render()
{
Logger.debug("UIController: Render():");
Logger.IncLvl();
Logger.DecLvl();
return ui.Render(0);
}
public void RenderTo(IMyTextPanel panel)
{
Logger.debug("UIController.RenderTo()");
Logger.IncLvl();
int panelWidth = 0;
string panelType = panel.BlockDefinition.SubtypeId;
Logger.debug("Type: " + panelType);
if (panelType == "LargeTextPanel" || panelType == "SmallTextPanel")
{
panelWidth = 658;
}
else if(panelType == "LargeLCDPanel" || panelType == "SmallLCDPanel")
{
panelWidth = 658;
}
else if(panelType == "SmallLCDPanelWide" || panelType == "LargeLCDPanelWide")
{
panelWidth = 1316;
}
else if(panelType == "LargeBlockCorner_LCD_1" || panelType == "LargeBlockCorner_LCD_2"
|| panelType == "SmallBlockCorner_LCD_1" || panelType == "SmallBlockCorner_LCD_2")
{ }
else if(panelType == "LargeBlockCorner_LCD_Flat_1" || panelType == "LargeBlockCorner_LCD_Flat_2"
|| panelType == "SmallBlockCorner_LCD_Flat_1" || panelType == "SmallBlockCorner_LCD_Flat_2")
{ }
int width = (int)(((float)panelWidth) / panel.GetValue<Single>("FontSize"));
if (panel.GetValue<long>("Font") == Fonts[FONT.MONO])
{
TextUtils.SelectFont(TextUtils.FONT.MONOSPACE);
}
else
{
TextUtils.SelectFont(TextUtils.FONT.DEFAULT);
}
Logger.log("Font configured...");
Logger.debug("font size: " + panel.GetValue<Single>("FontSize").ToString());
Logger.debug("resulting width: " + width.ToString());
string text = ui.Render(width);
Logger.debug("rendering <" + text);
panel.WritePublicText(text);
Logger.DecLvl();
}
public void KeyPress(string keyCode)
{
Logger.debug("UIController: KeyPress():");
Logger.IncLvl();
switch (keyCode)
{
case "LEFT/ABORT":
RevertUI();
break;
}
Logger.DecLvl();
}
public XMLTree GetSelectedNode()
{
Logger.debug("UIController: GetSelectedNode():");
Logger.IncLvl();
XMLTree sibling = ui.GetSelectedSibling();
Logger.DecLvl();
return sibling;
}
public XMLTree GetNode(Func<XMLTree, bool> filter)
{
Logger.debug("UIController: GetNode()");
Logger.IncLvl();
Logger.DecLvl();
return ui.GetNode(filter);
}
public List<XMLTree> GetAllNodes(Func<XMLTree, bool> filter)
{
Logger.debug("UIController: GetAllNodes()");
Logger.IncLvl();
Logger.DecLvl();
return ui.GetAllNodes(filter);
}
public void UpdateSelectability(XMLTree child) { }
public void FollowRoute(Route route)
{
Logger.debug("UIController: FollowRoute():");
Logger.IncLvl();
route.Follow(this);
Logger.DecLvl();
}
public XMLParentNode GetParent()
{
return null;
}
public Dictionary<string, string> GetValues()
{
Logger.debug("UIController.GetValues()");
Logger.IncLvl();
return GetValues((node) => true);
}
public Dictionary<string, string> GetValues(Func<XMLTree, bool> filter)
{
Logger.debug("UIController.GetValues()");
Logger.IncLvl();
if (ui == null)
{
Logger.DecLvl();
return null;
}
Logger.DecLvl();
return ui.GetValues(filter);
}
public string GetPackedValues(Func<XMLTree, bool> filter)
{
return Parser.PackData(GetValues(filter)).ToString();
}
public void DetachChild(XMLTree xml)
{
if(xml == ui)
{
ui = null;
}
}
public string GetPackedValues()
{
Logger.debug("UIController.GetPackedValues()");
Logger.IncLvl();
Logger.DecLvl();
return GetPackedValues(node => true);
}
public bool SelectNext()
{
return ui.SelectNext();
}
public void SetUserInputSource(IMyTerminalBlock sourceBlock, TextInputMode mode)
{
if(mode == TextInputMode.PUBLIC_TEXT && (sourceBlock as IMyTextPanel) == null)
{
throw new Exception("Only Text Panels can be used as user input if PUBLIC_TEXT mode is selected!");
}
UserInputSource = sourceBlock;
UserInputMode = mode;
}
public void EnableUserInput()
{
UserInputActive = true;
}
public void DisableUserInput()
{
UserInputActive = false;
}
public void RegisterInputBinding(XMLTree node)
{
UserInputBindings.Add(node);
}
public bool UpdateUserInput()
{
Logger.debug("UIController.RefreshUserInput()");
Logger.IncLvl();
if(!UserInputActive || UserInputSource == null)
{
return false;
}
// get input data
string inputData = null;
switch(UserInputMode)
{
case TextInputMode.CUSTOM_DATA:
inputData = UserInputSource?.CustomData;
break;
case TextInputMode.PUBLIC_TEXT:
inputData = (UserInputSource as IMyTextPanel)?.GetPublicText();
break;
}
bool inputHasChanged = true;
if( inputData == null || inputData == InputDataCache)
{
inputHasChanged = false;
}
Logger.debug("input has " + (inputHasChanged ? "" : "not ") + "changed");
Logger.debug("Iterating input bindings (" + UserInputBindings.Count + " bindings registered).");
// update ui input bindings
string binding;
string fieldValue;
foreach (XMLTree node in UserInputBindings)
{
binding = node.GetAttribute("inputbinding");
if(binding != null)
{
Logger.debug("binding found at " + node.Type + " node for field: " + binding);
fieldValue = node.GetAttribute(binding.ToLower());
Logger.debug("field is " + (fieldValue ?? "EMPTY") + ".");
if(!inputHasChanged && fieldValue != null && fieldValue != InputDataCache)
{
Logger.debug("applying field value: " + fieldValue);
inputData = fieldValue;
inputHasChanged = true;
}
else if(inputHasChanged)
{
Logger.debug("Updating field value to input: " + inputData);
node.SetAttribute(binding.ToLower(), inputData);
}
}
}
if(inputHasChanged)
{
InputDataCache = inputData;
}
// update user input device
switch (UserInputMode)
{
case TextInputMode.CUSTOM_DATA:
if(UserInputSource != null)
{
UserInputSource.CustomData = InputDataCache;
}
break;
case TextInputMode.PUBLIC_TEXT:
(UserInputSource as IMyTextPanel)?.WritePublicText(InputDataCache);
break;
}
return inputHasChanged;
}
private void CollectUserInputBindings()
{
Logger.debug("UIController.CollectUserInputBindings()");
XMLTree node;
Queue<XMLParentNode> nodes = new Queue<XMLParentNode>();
nodes.Enqueue(ui);
while(nodes.Count != 0)
{
node = nodes.Dequeue() as XMLTree;
if(!node.HasUserInputBindings)
{
Logger.debug("node has no userinputbindings");
}
if (node != null && node.HasUserInputBindings)
{
Logger.debug("Checking " + node.Type + " node...");
for (int i = 0; i < node.NumberOfChildren; i++)
{
nodes.Enqueue(node.GetChild(i));
}
if (node.GetAttribute("inputbinding") != null)
{
RegisterInputBinding(node);
}
}
}
}
}

public abstract class UIFactory
{
private int Count;
private int Max;
private List<UIController> UIs;
public UIFactory() : this(null) { }
public UIFactory(List<UIController> uiList)
{
Logger.debug("UIFactory constructor");
Logger.IncLvl();
if (uiList == null)
{
UIs = new List<UIController>();
}
UIs = uiList;
Logger.DecLvl();
}
public abstract XMLTree Render(UIController controller);
protected void UpdateUIs(XMLTree renderedUI)
{
foreach (UIController ui in UIs)
{
ui.LoadUI(renderedUI);
}
}
}

public class Generic : XMLTree
{
public Generic(string type) : base()
{
Type = type.ToLower();
}
}

public class Menu : XMLTree
{
RenderBox prefix;
RenderBox prefixSelected;
public Menu() : base()
{
Type = "menu";
prefix = new RenderBoxLeaf("     ");
prefixSelected = new RenderBoxLeaf(">> ");
int prefixWidth = Math.Max(prefix.MinWidth, prefixSelected.MinWidth);
prefix.MaxWidth = prefixWidth;
prefixSelected.MaxWidth = prefixWidth;
}
public override void AddChild(XMLTree child)
{
Logger.debug(Type + ": Add child():");
Logger.IncLvl();
if (child.Type != "menuitem" && child.IsSelectable())
{
Logger.DecLvl();
throw new Exception(
"ERROR: Only children of type <menupoint> or children that are not selectable are allowed!"
+ " (type was: <" + child.Type + ">)");
}
base.AddChild(child);
Logger.DecLvl();
}
/*protected override string RenderChild(XMLTree child, int width)
{
string renderString = "";
string prefix = "     ";
if (child.Type == "menuitem")
{
renderString += (child.IsSelected() ? ">> " : prefix);
}
renderString += base.RenderChild(child, width);
return renderString;
}*/
public override RenderBox GetRenderBox(int maxWidth)
{
Logger.debug("Menu.GetRenderCache(int)");
Logger.IncLvl();
RenderBoxTree cache = new RenderBoxTree();
RenderBoxTree menuPoint;
foreach (XMLTree child in Children)
{
menuPoint = new RenderBoxTree();
menuPoint.Flow = RenderBox.FlowDirection.HORIZONTAL;
if(child.IsSelected())
{
menuPoint.Add(prefixSelected);
}
else
{
menuPoint.Add(prefix);
}
menuPoint.Add(child.GetRenderBox(maxWidth));
cache.Add(menuPoint);
}
UpdateRenderCacheProperties(cache, maxWidth);
Logger.DecLvl();
return cache;
}
}

public class MenuItem : XMLTree
{
Route TargetRoute;
public MenuItem() : this(null) { }
public MenuItem(Route targetRoute) : base()
{
Type = "menuitem";
Selectable = true;
SetRoute(targetRoute);
PreventDefault("RIGHT/SUBMIT");
}
public override void SetAttribute(string key, string value)
{
Logger.debug(Type + ": SetAttribute():");
Logger.IncLvl();
switch (key)
{
case "route":
Logger.debug("prepare to set route...");
SetRoute(new Route(value));
if (TargetRoute == null)
{
Logger.debug("Failure!");
}
else
{
Logger.debug("Success!");
}
break;
default:
base.SetAttribute(key, value);
break;
}
Logger.DecLvl();
}
public override void OnKeyPressed(string keyCode)
{
Logger.debug(Type + ": OnKeyPressed():");
switch (keyCode)
{
case "RIGHT/SUBMIT":
if (TargetRoute != null)
{
Logger.debug("Follow Target Route!");
FollowRoute(TargetRoute);
}
else
{
Logger.debug("No route set!");
}
break;
}
base.OnKeyPressed(keyCode);
Logger.DecLvl();
}
public void SetRoute(Route route)
{
TargetRoute = route;
}

}

public class ProgressBar : XMLTree
{
RenderBox emptyBar;
RenderBox filledBar;
float StepSize
{
get
{
float stepSize;
if (!Single.TryParse(GetAttribute("stepsize"), out stepSize))
{
return 0.1f;
}
return stepSize;
}
set
{
string valueString = Math.Max(0.001f, Math.Min(0.009f, value)).ToString();
if (valueString.Length > 5)
{
valueString += valueString.Substring(0, 5);
}
SetAttribute("stepsize", valueString);
}
}
public float FillLevel
{
get
{
float fillLevel;
if (!Single.TryParse(GetAttribute("value"), out fillLevel))
{
return 0.0f;
}
if (fillLevel < 0 || fillLevel > 1)
return 0.0f;
return fillLevel;
}
set
{
string valueString = Math.Max(0f, Math.Min(1f, value)).ToString();
if (valueString.Length > 5)
{
valueString = valueString.Substring(0, 5);
}
SetAttribute("value", valueString);
}
}
public ProgressBar() : this(0f)
{
}
public ProgressBar(float fillLevel) : this(fillLevel, false)
{
}
public ProgressBar(float fillLevel, bool selectable) : base()
{
Type = "progressbar";
PreventDefault("LEFT/ABORT");
PreventDefault("RIGHT/SUBMIT");
SetAttribute("width", "500");
SetAttribute("filledstring", "|");
SetAttribute("emptystring", "'");
SetAttribute("value", fillLevel.ToString());
SetAttribute("stepsize", "0.05");
SetAttribute("selectable", selectable ? "true" : "false");
}
public void IncreaseFillLevel()
{
Logger.debug(Type + ".IncreaseFillLevel()");
Logger.IncLvl();
FillLevel += StepSize;
Logger.DecLvl();
}
public void DecreaseFillLevel()
{
Logger.debug(Type + ".DecreaseFillLevel()");
Logger.IncLvl();
FillLevel -= StepSize;
Logger.DecLvl();
}
public override void OnKeyPressed(string keyCode)
{
Logger.debug(Type + ": OnKeyPressed():");
Logger.IncLvl();
switch (keyCode)
{
case "LEFT/ABORT":
DecreaseFillLevel();
break;
case "RIGHT/SUBMIT":
IncreaseFillLevel();
break;
}
base.OnKeyPressed(keyCode);
Logger.DecLvl();
}
/*protected override void RenderText(ref List<string> segments, int width, int availableWidth)
{
Logger.debug(Type + ".RenderText()");
Logger.IncLvl();
string suffix = IsSelected() ? ">" : "  ";
string prefix = IsSelected() ? "<" : "  ";
string renderString = prefix + "[";
float fillLevel = FillLevel;
string fillString = GetAttribute("filledstring");
string emptyString = GetAttribute("emptystring");
int innerWidth = (width - 2 * TextUtils.GetTextWidth("[]"));
renderString += TextUtils.CreateStringOfLength(fillString, (int)(innerWidth * fillLevel));
renderString += TextUtils.CreateStringOfLength(emptyString, (int)(innerWidth * (1 - fillLevel)));
renderString += "]" + suffix;
segments.Add(renderString);
Logger.DecLvl();
}*/
public override RenderBox GetRenderBox(int maxWidth)
{
Logger.debug("ProgressBar.GetRenderCache(int)");
Logger.IncLvl();
RenderBoxTree cache = new RenderBoxTree();
int outerWidth = TextUtils.GetTextWidth(IsSelected() ? new StringBuilder("<[]>") : new StringBuilder(" [] ")) + 2;
RenderBox prefix = new RenderBoxLeaf(
(IsSelected() ? "<" : " ") + "[");
prefix.MaxWidth = prefix.MinWidth;
RenderBox suffix = new RenderBoxLeaf(
"]" + (IsSelected() ? ">" : " "));
suffix.MaxWidth = suffix.MinWidth;
cache.Add(prefix);
filledBar = new RenderBoxLeaf();
filledBar.PadString = GetAttribute("filledstring");
filledBar.Height = 1;
cache.Add(filledBar);
emptyBar = new RenderBoxLeaf();
emptyBar.Height = 1;
emptyBar.PadString = GetAttribute("emptystring");
cache.Add(emptyBar);
cache.Add(suffix);
int width = ResolveSize(GetAttribute("minwidth"), maxWidth);
if (width >= outerWidth)
{
filledBar.MinWidth = (int)((width - outerWidth) * FillLevel);
emptyBar.MinWidth = (int)((width - outerWidth) * (1 - FillLevel));
}
width = ResolveSize(GetAttribute("maxwidth"), maxWidth);
if (width >= outerWidth)
{
filledBar.MaxWidth = (int) ((width - outerWidth) * FillLevel);
emptyBar.MaxWidth = (int)((width - outerWidth) * (1 - FillLevel));
}
width = ResolveSize(GetAttribute("width"), maxWidth);
if (width >= outerWidth)
{
filledBar.DesiredWidth = (int)((width - outerWidth) * FillLevel);
emptyBar.DesiredWidth = (int)((width - outerWidth) * (1 - FillLevel));
}
width = ResolveSize(GetAttribute("forcewidth"), maxWidth);
if (width >= outerWidth)
{
int forcedWidth = (int)((width - outerWidth) * FillLevel);
filledBar.MaxWidth = forcedWidth;
filledBar.MinWidth = forcedWidth;
forcedWidth = (int)((width - outerWidth) * (1 - FillLevel));
emptyBar.MinWidth = forcedWidth;
emptyBar.MaxWidth = forcedWidth;
}
UpdateRenderCacheProperties(cache, maxWidth);
Logger.log("filledBar: ");
Logger.DEBUG = false;
Logger.log("  fillLevel: " + FillLevel);
Logger.log("  min width: " + filledBar.MinWidth);
Logger.log("  max width: " + filledBar.MaxWidth);
Logger.log("  desired width: " + filledBar.DesiredWidth);
Logger.log("  height: " + filledBar.Height);
Logger.DEBUG = true;
Logger.log("  actual width: " + filledBar.GetActualWidth(maxWidth));
cache.Flow = RenderBox.FlowDirection.HORIZONTAL;
//GetAttribute("flow") == "horizontal" ? NodeBox.FlowDirection.HORIZONTAL : NodeBox.FlowDirection.VERTICAL;
switch (GetAttribute("alignself"))
{
case "right":
cache.Align = RenderBox.TextAlign.RIGHT;
break;
case "center":
cache.Align = RenderBox.TextAlign.CENTER;
break;
default:
cache.Align = RenderBox.TextAlign.LEFT;
break;
}
Logger.DecLvl();
return cache;
}
}

public class Container : XMLTree
{
public Container() : base()
{
Type = "container";
}
}

public class HorizontalLine : XMLTree
{
public HorizontalLine() : base()
{
Type = "hl";
SetAttribute("width", "100%");
}
/*protected override void RenderText(ref List<string> segments, int width, int availableWidth)
{
segments.Add(TextUtils.CreateStringOfLength("_", width, TextUtils.RoundMode.CEIL));
}*/
public override RenderBox GetRenderBox(int maxWidth)
{
Logger.debug("HorizontalLine.GetRenderCache(int)");
Logger.IncLvl();
RenderBox cache = new RenderBoxLeaf();
//cache.Add("_");
cache.Height = 1;
cache.PadString = "_";
UpdateRenderCacheProperties(cache, maxWidth);
Logger.DecLvl();
return cache;
}
}

public class UIControls : XMLTree
{
UIController Controller;
public UIControls() : base()
{
Type = "uicontrols";
Controller = null;
SetAttribute("selectable", "false");
}
private void UpdateController()
{
Controller = RetrieveRoot() as UIController;
SetAttribute("selectable", (Controller != null && Controller.UIStack.Count > 0) ? "true" : "false");
if (IsSelectable())
{
PreventDefault("LEFT/ABORT");
PreventDefault("RIGHT/SUBMIT");
}
else
{
AllowDefault("LEFT/ABORT");
AllowDefault("RIGHT/SUBMIT");
}
GetParent().UpdateSelectability(this);
if (IsSelected() && !IsSelectable())
{
GetParent().SelectNext();
}
}
public override void OnKeyPressed(string keyCode)
{
if (Controller == null)
{
UpdateController();
}
switch (keyCode)
{
case "LEFT/ABORT":
case "RIGHT/SUBMIT":
if (Controller != null && Controller.UIStack.Count > 0)
{
Controller.RevertUI();
}
break;
}
}
/*protected override string PostRender(List<string> segments, int width, int availableWidth)
{
if (Controller == null)
{
UpdateController();
}
string prefix;
if (!IsSelectable())
{
prefix = "";
}
else
{
prefix = IsSelected() ? "<<" : TextUtils.CreateStringOfLength(" ", TextUtils.GetTextWidth("<<"));
}
string renderString = base.PostRender(segments, width, availableWidth);
int prefixSpacesCount = TextUtils.CreateStringOfLength(" ", TextUtils.GetTextWidth(prefix)).Length;
string tmpPrefix = "";
for (int i = 0; i < prefixSpacesCount; i++)
{
if ((renderString.Length - 1) < i || renderString[i] != ' ')
{
tmpPrefix += " ";
}
}
renderString = prefix + (tmpPrefix + renderString).Substring(prefixSpacesCount);
return renderString;
//renderString = prefix + renderString;
}*/
public override RenderBox GetRenderBox(int maxWidth)
{
Logger.debug("UIControls.GetRenderCache(int)");
Logger.IncLvl();
RenderBoxTree cache = new RenderBoxTree();
if (Controller == null)
{
UpdateController();
}
if(IsSelectable())
{
RenderBox childCache = new RenderBoxLeaf(IsSelected() ?
new StringBuilder("<<") :
TextUtils.CreateStringOfLength(" ", TextUtils.GetTextWidth(new StringBuilder("<<"))));
childCache.MaxWidth = childCache.MinWidth;
cache.Add(childCache);
}
RenderBoxTree contentCache = new RenderBoxTree();
contentCache.Flow = GetAttribute("flow") == "horizontal" ? RenderBox.FlowDirection.HORIZONTAL : RenderBox.FlowDirection.VERTICAL;
foreach (XMLTree child in Children)
{
contentCache.Add(child.GetRenderBox(maxWidth));
}
cache.Add(contentCache);
UpdateRenderCacheProperties(cache, maxWidth);
cache.Flow = RenderBox.FlowDirection.HORIZONTAL;
Logger.DecLvl();
return cache;
}
}

public class TextInput : XMLTree
{
int CursorPosition;
public TextInput()
{
Logger.log("TextInput constructor()");
Logger.IncLvl();
Type = "textinput";
Selectable = true;
CursorPosition = -1;
PreventDefault("LEFT/ABORT");
PreventDefault("RIGHT/SUBMIT");
SetAttribute("maxlength", "10");
SetAttribute("value", "");
SetAttribute("allowedchars", " a-z0-9");
Logger.DecLvl();
}
public override void OnKeyPressed(string keyCode)
{
switch (keyCode)
{
case "LEFT/ABORT":
DecreaseCursorPosition();
break;
case "RIGHT/SUBMIT":
IncreaseCursorPosition();
break;
case "UP":
DecreaseLetter();
break;
case "DOWN":
IncreaseLetter();
break;
default:
base.OnKeyPressed(keyCode);
break;
}
}
public override void SetAttribute(string key, string value)
{
if(key == "allowedchars")
{
if(!System.Text.RegularExpressions.Regex.IsMatch(value,
@"([^-\\]-[^-\\]|[^-\\]|\\-|\\\\)*"))
{
throw new Exception("Invalid format of allowed characters!");
}

}
base.SetAttribute(key, value);
}
private void IncreaseLetter()
{
Logger.log("TextInput.IncreaseLetter()");
Logger.IncLvl();
if (CursorPosition == -1)
{
return;
}
char[] value = GetAttribute("value").ToCharArray();
char letter = value[CursorPosition];
string[] charSets = GetAllowedCharSets();
for (int i = 0; i < charSets.Length; i++)
{
if ((charSets[i].Length == 1 && charSets[i][0] == value[CursorPosition])
|| (charSets[i].Length == 3 && charSets[i][2] == value[CursorPosition]))
{
Logger.log("letter outside class, setting to: " + charSets[i == 0 ? charSets.Length - 1 : i - 1][0] + ". (chars[" + ((i + 1) % charSets.Length) + "])");
value[CursorPosition] = charSets[(i + 1) % charSets.Length][0];
SetAttribute("value", new string(value));
Logger.DecLvl();
return;
}
}
Logger.log("letter inside class, setting to: " + (char)(((int)value[CursorPosition]) + 1));
value[CursorPosition] = (char)(((int)value[CursorPosition]) + 1);
SetAttribute("value", new string(value));
Logger.DecLvl();
}
private void DecreaseLetter()
{
Logger.log("TextInput.DecreaseLetter()");
Logger.IncLvl();
if (CursorPosition == -1)
{
return;
}
char[] value = GetAttribute("value").ToCharArray();
char[] chars = GetAttribute("allowedchars").ToCharArray();
string[] charSets = GetAllowedCharSets();
for(int i = 0; i < charSets.Length; i++)
{
if(charSets[i][0] == value[CursorPosition])
{
int index = (i == 0 ? charSets.Length - 1 : i - 1);
Logger.log("letter outside class, setting to: " + charSets[index][charSets[index].Length - 1] + ". (chars[" + (index) + "])");
value[CursorPosition] = charSets[index][charSets[index].Length - 1];
SetAttribute("value", new string(value));
return;
}
}
Logger.log("letter inside class, setting to: " + (char)(((int)value[CursorPosition]) - 1));
value[CursorPosition] = (char)(((int)value[CursorPosition]) - 1);
SetAttribute("value", new string(value));
Logger.DecLvl();
}
private string[] GetAllowedCharSets()
{
string charString = GetAttribute("allowedchars");
System.Text.RegularExpressions.MatchCollection matches =
System.Text.RegularExpressions.Regex.Matches(charString, @"[^-\\]-[^-\\]|[^-\\]|\\-|\\\\");
string[] charSets = new string[matches.Count];
int i = 0;
foreach (System.Text.RegularExpressions.Match match in matches)
{
string matchString = match.ToString();
if (matchString == "\\-")
{
charSets[i] = "-";
}
else if (matchString == "\\\\")
{
charSets[i] = "\\";
}
else
{
charSets[i] = matchString;
}
i++;
}
//P.Echo("Char sets found: " + string.Join(",", charSets));
return charSets;
}
private void IncreaseCursorPosition()
{
if (CursorPosition < Single.Parse(GetAttribute("maxlength")) - 1)
{
CursorPosition++;
}
else
{
CursorPosition = 0;
DecreaseCursorPosition();
KeyPress("DOWN");
}
if (CursorPosition != -1)
{
PreventDefault("UP");
PreventDefault("DOWN");
}
if (CursorPosition >= GetAttribute("value").Length)
{
string[] charSets = GetAllowedCharSets();
SetAttribute("value", GetAttribute("value") + charSets[0][0]);
}
}
private void DecreaseCursorPosition()
{
if (CursorPosition > -1)
{
CursorPosition--;
}
if (CursorPosition == -1)
{
AllowDefault("UP");
AllowDefault("DOWN");
}
}
/*protected override void RenderText(ref List<string> segments, int width, int availableWidth)
{
string value = GetAttribute("value");
if (CursorPosition != -1)
{
value = value.Substring(0, CursorPosition)
+ "|" + value.Substring(CursorPosition, 1) + "|"
+ value.Substring(CursorPosition + 1);
}
else if (value.Length == 0)
{
value = "_" + value;
}
segments.Add((IsSelected() ? new string(new char[] { (char)187 }) : "  ") + " " + value);
}*/
public override RenderBox GetRenderBox(int maxWidth)
{
Logger.debug("TextInput.GetRenderCache(int)");
Logger.IncLvl();
RenderBoxTree cache = new RenderBoxTree();
cache.Add((IsSelected() ? new string(new char[] { (char)187 }) : "  ") + " ");
cache.Flow = RenderBox.FlowDirection.HORIZONTAL;
string value = GetAttribute("value");
if(CursorPosition != -1)
{
cache.Add(value.Substring(0, CursorPosition));
cache.Add("|");
cache.Add(value.Substring(CursorPosition, 1));
cache.Add("|");
cache.Add(value.Substring(CursorPosition + 1));
}
else
{
if (value.Length == 0)
cache.Add("_");
cache.Add(value);
}
for(int i = 0; i < cache.Count; i++)
{
cache[i].MaxWidth = cache[i].MinWidth;
}
Logger.DecLvl();
return cache;
}
}

public abstract class DataStore : XMLTree
{
public DataStore() : base() { }
public override Dictionary<string, string> GetValues(Func<XMLTree, bool> filter)
{
Dictionary<string, string> dict = base.GetValues(filter);
if (!filter(this))
{
return dict;
}
foreach (KeyValuePair<string, string> data in Attributes)
{
if (!dict.ContainsKey(data.Key))
{
dict[data.Key] = data.Value;
}
}
return dict;
}
}


public class SubmitButton : MenuItem
{
public SubmitButton()
{
Type = "submitbutton";
SetAttribute("flowdirection", "horizontal");
}
/*protected override void PreRender(ref List<string> segments, int width, int availableWidth)
{
segments.Add(IsSelected() ? "[[  " : "[   ");
base.PreRender(ref segments, width, availableWidth);
}*/
/*protected override string PostRender(List<string> segments, int width, int availableWidth)
{
segments.Add(IsSelected() ? "  ]]" : "   ]");
return base.PostRender(segments, width, availableWidth);
}*/
public override RenderBox GetRenderBox(int maxWidth)
{
Logger.debug("SubmitButton.GetRenderCache(int)");
Logger.IncLvl();
RenderBoxTree cache = new RenderBoxTree();
RenderBoxLeaf childCache = new RenderBoxLeaf(IsSelected() ? "[[  " : "[   ");
childCache.MaxWidth = childCache.MinWidth;
cache.Add(childCache);
RenderBoxTree contentCache = new RenderBoxTree();
contentCache.Flow = GetAttribute("flow") == "horizontal" ? RenderBox.FlowDirection.HORIZONTAL : RenderBox.FlowDirection.VERTICAL;
foreach (XMLTree child in Children)
{
contentCache.Add(child.GetRenderBox(maxWidth));
}
cache.Add(contentCache);
childCache = new RenderBoxLeaf(IsSelected() ? "  ]]" : "   ]");
childCache.MaxWidth = childCache.MinWidth;
cache.Add(childCache);
UpdateRenderCacheProperties(cache, maxWidth);
cache.Flow = RenderBox.FlowDirection.HORIZONTAL;
Logger.DecLvl();
return cache;
}
}

public class Break : TextNode
{
public Break() : base("")
{
Type = "br";
}
//protected override void RenderText(ref List<string> segments, int width, int availableWidth) { }
/*protected override string PostRender(List<string> segments, int width, int availableWidth)
{
return "";
}*/
public override RenderBox GetRenderBox(int maxWidth)
{
Logger.debug("Break.GetRenderCache(int)");
Logger.IncLvl();
RenderBox cache = new RenderBoxLeaf();
cache.Height = 0;
Logger.DecLvl();
return cache;
}
}

public class Space : XMLTree
{
public Space() : base()
{
Logger.debug("Space constructor()");
Logger.IncLvl();
Type = "space";
SetAttribute("width", "0");
Logger.DecLvl();
}
/*protected override void RenderText(ref List<string> segments, int width, int availableWidth)
{
Logger.debug(Type + ".RenderText()");
Logger.IncLvl();
segments.Add(TextUtils.CreateStringOfLength(" ", width));
Logger.DecLvl();
}*/
public override RenderBox GetRenderBox(int maxWidth)
{
Logger.debug("GetRenderCache(int)");
Logger.IncLvl();
RenderBox cache = new RenderBoxLeaf();
cache.Height = 1;
int width = ResolveSize(GetAttribute("width"), maxWidth);
cache.MinWidth = width;
cache.MaxWidth = width;
Logger.DecLvl();
return cache;
}
}

public class Hidden : XMLTree
{
public Hidden() : base()
{
Type = "hidden";
}
/*protected override string PostRender(List<string> segments, int width, int availableWidth)
{
return null;
}*/
public override RenderBox GetRenderBox(int maxWidth)
{
Logger.debug("Hidden.GetRenderCache(int)");
Logger.IncLvl();
RenderBox cache = new RenderBoxTree();
cache.MaxWidth = 0;
cache.Height = 0;
Logger.DecLvl();
return cache;
}
}

public class HiddenData : DataStore
{
public HiddenData() : base()
{
Type = "hiddendata";
}
/*protected override string PostRender(List<string> segments, int width, int availableWidth)
{
return null;
}*/
public override RenderBox GetRenderBox(int maxWidth)
{
Logger.debug("HiddenData.GetRenderCache(int)");
Logger.IncLvl();
RenderBox cache = new RenderBoxTree();
cache.MaxWidth = 0;
cache.Height = 0;
Logger.DecLvl();
return cache;
}
}

class MetaNode : Hidden
{
public MetaNode() : base()
{
Type = "meta";
}
public override Dictionary<string, string> GetValues(Func<XMLTree, bool> filter)
{
if (filter(this))
{
return Attributes;
}
else
{
return new Dictionary<string, string>();
}
}
}
}

}