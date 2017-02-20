class DataStorage : XML.DataStore
{
private Dictionary<string, Type> String2Type;
private Dictionary<Type, string> Type2String;
private Dictionary<string, string> StringEntries;
private Dictionary<string, int> IntegerEntries;
private Dictionary<string, float> FloatEntries;
private static DataStorage Instance;
private DataStorage() : base()
{
Logger.log("DataStore constructor()");
Logger.IncLvl();
String2Type = new Dictionary<string, Type> {
{"string", typeof(String) },
{"int", typeof(int) },
{"float", typeof(float) }
};
Type2String = new Dictionary<Type, string> {
{ typeof(String), "string" },
{ typeof(int), "int" },
{ typeof(float), "float" }
};
Type = "data";
Attributes = new Dictionary<string, string>();
StringEntries = new Dictionary<string, string>();
IntegerEntries = new Dictionary<string, int>();
FloatEntries = new Dictionary<string, float>();
Logger.DecLvl();
}
public static DataStorage GetInstance()
{
Logger.log("DataStore.GetInstance()");
Logger.IncLvl();
if (Instance == null)
{
Instance = new DataStorage();
}
Logger.DecLvl();
return Instance;
}
public void Save(out string data)
{
Logger.log("DataStore.Save(string)");
Logger.IncLvl();
UpdateAttributes();
DataStorage.SetUp();
data = "<data " + Parser.PackData(GetValues((node) => true)) + "/>";
Logger.DecLvl();
}
private void UpdateAttributes()
{
Logger.log("DataStore.UpdateAttributes()");
Logger.IncLvl();
foreach (KeyValuePair<string, string> entry in StringEntries)
{
Attributes[entry.Key] = "string:" + entry.Value;
}
foreach (KeyValuePair<string, int> entry in IntegerEntries)
{
Attributes[entry.Key] = "int:" + entry.Value.ToString();
}
foreach (KeyValuePair<string, float> entry in FloatEntries)
{
Attributes[entry.Key] = "float:" + entry.Value.ToString();
}
Logger.DecLvl();
}
public static DataStorage Load(string Storage)
{
Logger.log("DataStore.Load()");
Logger.IncLvl();
DataStorage.SetUp();
XML.XMLTree xml = XML.ParseXML(Storage);
if (xml != null)
{
DataStorage ds = xml.GetNode((node) => node.Type == "data") as DataStorage;
if (ds != null)
{
Instance = ds;
}
}
Logger.DecLvl();
return DataStorage.GetInstance();
}
public void Set<T>(string key, T value)
{
Logger.log("DataStore.Set<T>(string, T)");
Logger.IncLvl();
Type type = GetEntryType(key);
if (type != null && type != typeof(T))
{
throw new Exception("ERROR: An entry for key '" + key + "' does already exist, but is of type '" + type.ToString() + "'!");
}
if (typeof(T) == typeof(string))
{
StringEntries[key] = (string)(object)value;
}
else if (typeof(T) == typeof(int))
{
IntegerEntries[key] = (int)(object)value;
}
else if (typeof(T) == typeof(float))
{
FloatEntries[key] = (float)(object)value;
}
Logger.DecLvl();
}
public Type GetEntryType(string key)
{
Logger.log("DataStore.GetEntryType(string)");
Logger.IncLvl();
if (StringEntries.ContainsKey(key))
{
Logger.DecLvl();
return typeof(string);
}
else if (IntegerEntries.ContainsKey(key))
{
Logger.DecLvl();
return typeof(int);
}
else if (FloatEntries.ContainsKey(key))
{
Logger.DecLvl();
return typeof(float);
}
else
{
Logger.DecLvl();
return null;
}
}
public T Get<T>(string key)
{
Logger.log("DataStore.Get(string)");
Logger.IncLvl();
if (!Exists<T>(key))
{
throw new Exception("No entry found for key '" + key + "' of type '" + typeof(T).ToString() + "'!");
}
if (typeof(T) == typeof(string))
{
Logger.DecLvl();
return (T)(object)StringEntries[key];
}
else if (typeof(T) == typeof(int))
{
Logger.DecLvl();
return (T)(object)IntegerEntries[key];
}
else if (typeof(T) == typeof(float))
{
Logger.DecLvl();
return (T)(object)FloatEntries[key];
}
else
{
throw new Exception("Error: Invalid Type at DataStore.Get<Type>(string key)!");
}
}
public bool Exists(string key)
{
Logger.log("DataStore.Exists(string)");
return (Exists<string>(key) || Exists<int>(key) || Exists<float>(key));
}
public bool Exists<T>(string key)
{
Logger.log("Exists<T>(string)");
Logger.IncLvl();
if (typeof(T) == typeof(string))
{
Logger.DecLvl();
return StringEntries.ContainsKey(key);
}
else if (typeof(T) == typeof(int))
{
Logger.DecLvl();
return IntegerEntries.ContainsKey(key);
}
else if (typeof(T) == typeof(float))
{
Logger.DecLvl();
return FloatEntries.ContainsKey(key);
}
Logger.DecLvl();
return false;
}
public List<string> GetKeys()
{
Logger.log("DataStore.GetKeys()");
Logger.IncLvl();
List<string> keys = new List<string>(
StringEntries.Keys.Count +
IntegerEntries.Keys.Count +
FloatEntries.Keys.Count
);
keys.AddRange(StringEntries.Keys);
keys.AddRange(IntegerEntries.Keys);
keys.AddRange(FloatEntries.Keys);
Logger.DecLvl();
return keys;
}
public static void SetUp()
{
Logger.log("DataStore.SetUp()");
Logger.IncLvl();
if (!XML.NodeRegister.ContainsKey("data"))
{
XML.NodeRegister.Add("data", () => DataStorage.GetInstance());
}
Logger.DecLvl();
}
public override void SetAttribute(string key, string value)
{
Logger.log("DataStore.SetAttribute(string, string)");
Logger.IncLvl();
if (StringEntries == null || IntegerEntries == null || FloatEntries == null)
{
base.SetAttribute(key, value);
Logger.DecLvl();
return;
}
Type type = typeof(string);
string[] valueSplit = value.Split(':');
if (valueSplit.Length > 1 && String2Type.ContainsKey(valueSplit[0]))
{
type = String2Type[valueSplit[0]];
valueSplit[0] = "";
value = String.Join(":", valueSplit).Substring(1);
}
if (type == typeof(string))
{
Set(key, value);
}
else if (type == typeof(int))
{
int intValue;
if (Int32.TryParse(value, out intValue))
{
Set(key, intValue);
}
}
else if (type == typeof(float))
{
float floatValue;
if (Single.TryParse(value, out floatValue))
{
Set(key, floatValue);
}
}
Logger.DecLvl();
}
}

public static class Logger{public static string History="";static IMyTextPanel DebugPanel;static public bool DEBUG=false;static int offset=0;public static void log(string a){var b="";for(int d=0;d<offset;d++)b+="  ";P.Echo(b+a);}public static void debug(string a){if(!DEBUG)return;log(a);}public static void IncLvl(){offset+=2;}public static void DecLvl(){offset=offset-2;}}public static class Parser{public static string PackData(Dictionary<string,string>a){var b="";foreach(string key in a.Keys)b+=key+"=\""+a[key]+"\" ";return b;}public static string Sanitize(string a){return a.Replace("\"","\\\"").Replace("'","\\'");}public static string UnescapeQuotes(string a){return a.Replace("\\\"","\"").Replace("\\'","'");}public static int GetNextUnescaped(char[]a,string b){return GetNextUnescaped(a,b,0);}public static int GetNextUnescaped(char[]a,string b,int d){return GetNextUnescaped(a,b,d,b.Length-d);}public static int GetNextUnescaped(char[]a,string b,int d,int e){Logger.debug("GetNextUnescaped():");Logger.IncLvl();int f=d+e-1;int g=b.IndexOfAny(a,d,f-d+1);while(g>0&&b[g-1]=='\\')g=b.IndexOfAny(a,g+1,f-g);Logger.DecLvl();return g;}public static int GetNextOutsideQuotes(char a,string b){return GetNextOutsideQuotes(new char[]{a},b);}public static int GetNextOutsideQuotes(char a,string b,bool d){return GetNextOutsideQuotes(new char[]{a},b,d);}public static int GetNextOutsideQuotes(char[]a,string b){return GetNextOutsideQuotes(a,b,true);}public static int GetNextOutsideQuotes(char[]a,string b,bool d){Logger.debug("GetNextOutsideQuotes():");Logger.IncLvl();char[]e=new char[]{'\'','"'};int f=-1;int g=-1;int h;Logger.debug("needle: |"+new string(a)+"|");Logger.debug("haystack: |"+b+"|");while(f==-1){if(d)h=GetNextUnescaped(e,b,g+1);else h=b.IndexOfAny(e,g+1);Logger.debug("quoteStart position: "+h.ToString()+", quoteEnd position: "+g.ToString());if(h==-1){Logger.debug("searching for needle in:: "+b.Substring(g+1));f=GetNextUnescaped(a,b,g+1);}else{Logger.debug("found start quote: "+b.Substring(h));Logger.debug("searching for needle in: "+b.Substring(g+1,h-g));f=GetNextUnescaped(a,b,g+1,h-g-1);if(f!=-1)Logger.debug("found needle: "+b.Substring(f));if(d)g=GetNextUnescaped(new char[]{b[h]},b,h+1);else g=b.IndexOf(b[h],h+1);Logger.debug("found end quote: "+b.Substring(g));}}Logger.debug("yay!");Logger.DecLvl();return f;}public static List<String>ParamString2List(string a){Logger.debug("Parser.ParamString2List()");Logger.IncLvl();a=a.Trim()+" ";var b=new List<string>();char[]d=new char[]{'\'','"'};int e=-1;while(e!=a.Length-1){a=a.Substring(e+1);e=Parser.GetNextOutsideQuotes(new char[]{' ','\n'},a);b.Add(a.Substring(0,e).Trim(d));}Logger.DecLvl();return b;}public static Dictionary<string,string>GetXMLAttributes(string a){Logger.debug("GetXMLAttributes():");Logger.IncLvl();Logger.debug("attribute string is: <"+a+">");var b=new Dictionary<string,string>();char[]d=new char[]{'\'','"'};List<string>e=ParamString2List(a);int f;foreach(string attribute in e){f=attribute.IndexOf('=');if(f==-1)b[attribute.Substring(0).ToLower()]="true";else b[attribute.Substring(0,f).ToLower()]=attribute.Substring(f+1).Trim(d);}Logger.debug("attribute dict: {");Logger.IncLvl();foreach(string key in b.Keys)Logger.debug(key+": "+b[key]);Logger.debug("}");Logger.DecLvl();Logger.DecLvl();return b;}}public static class TextUtils{public enum FONT{DEFAULT,MONOSPACE,}public static bool DEBUG=true;static FONT selectedFont=FONT.DEFAULT;static Dictionary<FONT,Dictionary<char,int>>LetterWidths=new Dictionary<FONT,Dictionary<char,int>>{{FONT.DEFAULT,new Dictionary<char,int>{{' ',8},{'!',8},{'"',10},{'#',19},{'$',20},{'%',24},{'&',20},{'\'',6},{'(',9},{')',9},{'*',11},{'+',18},{',',9},{'-',10},{'.',9},{'/',14},{'0',19},{'1',9},{'2',19},{'3',17},{'4',19},{'5',19},{'6',19},{'7',16},{'8',19},{'9',19},{':',9},{';',9},{'<',18},{'=',18},{'>',18},{'?',16},{'@',25},{'A',21},{'B',21},{'C',19},{'D',21},{'E',18},{'F',17},{'G',20},{'H',20},{'I',8},{'J',16},{'K',17},{'L',15},{'M',26},{'N',21},{'O',21},{'P',20},{'Q',21},{'R',21},{'S',21},{'T',17},{'U',20},{'V',20},{'W',31},{'X',19},{'Y',20},{'Z',19},{'[',9},{'\\',12},{']',9},{'^',18},{'_',15},{'`',8},{'a',17},{'b',17},{'c',16},{'d',17},{'e',17},{'f',9},{'g',17},{'h',17},{'i',8},{'j',8},{'k',17},{'l',8},{'m',27},{'n',17},{'o',17},{'p',17},{'q',17},{'r',10},{'s',17},{'t',9},{'u',17},{'v',15},{'w',27},{'x',15},{'y',17},{'z',16},{'{',9},{'|',6},{'}',9},{'~',18},{' ',8},{'¡',8},{'¢',16},{'£',17},{'¤',19},{'¥',19},{'¦',6},{'§',20},{'¨',8},{'©',25},{'ª',10},{'«',15},{'¬',18},{'­',10},{'®',25},{'¯',8},{'°',12},{'±',18},{'²',11},{'³',11},{'´',8},{'µ',17},{'¶',18},{'·',9},{'¸',8},{'¹',11},{'º',10},{'»',15},{'¼',27},{'½',29},{'¾',28},{'¿',16},{'À',21},{'Á',21},{'Â',21},{'Ã',21},{'Ä',21},{'Å',21},{'Æ',31},{'Ç',19},{'È',18},{'É',18},{'Ê',18},{'Ë',18},{'Ì',8},{'Í',8},{'Î',8},{'Ï',8},{'Ð',21},{'Ñ',21},{'Ò',21},{'Ó',21},{'Ô',21},{'Õ',21},{'Ö',21},{'×',18},{'Ø',21},{'Ù',20},{'Ú',20},{'Û',20},{'Ü',20},{'Ý',17},{'Þ',20},{'ß',19},{'à',17},{'á',17},{'â',17},{'ã',17},{'ä',17},{'å',17},{'æ',28},{'ç',16},{'è',17},{'é',17},{'ê',17},{'ë',17},{'ì',8},{'í',8},{'î',8},{'ï',8},{'ð',17},{'ñ',17},{'ò',17},{'ó',17},{'ô',17},{'õ',17},{'ö',17},{'÷',18},{'ø',17},{'ù',17},{'ú',17},{'û',17},{'ü',17},{'ý',17},{'þ',17},{'ÿ',17},{'Ā',20},{'ā',17},{'Ă',21},{'ă',17},{'Ą',21},{'ą',17},{'Ć',19},{'ć',16},{'Ĉ',19},{'ĉ',16},{'Ċ',19},{'ċ',16},{'Č',19},{'č',16},{'Ď',21},{'ď',17},{'Đ',21},{'đ',17},{'Ē',18},{'ē',17},{'Ĕ',18},{'ĕ',17},{'Ė',18},{'ė',17},{'Ę',18},{'ę',17},{'Ě',18},{'ě',17},{'Ĝ',20},{'ĝ',17},{'Ğ',20},{'ğ',17},{'Ġ',20},{'ġ',17},{'Ģ',20},{'ģ',17},{'Ĥ',20},{'ĥ',17},{'Ħ',20},{'ħ',17},{'Ĩ',8},{'ĩ',8},{'Ī',8},{'ī',8},{'Į',8},{'į',8},{'İ',8},{'ı',8},{'Ĳ',24},{'ĳ',14},{'Ĵ',16},{'ĵ',8},{'Ķ',17},{'ķ',17},{'Ĺ',15},{'ĺ',8},{'Ļ',15},{'ļ',8},{'Ľ',15},{'ľ',8},{'Ŀ',15},{'ŀ',10},{'Ł',15},{'ł',8},{'Ń',21},{'ń',17},{'Ņ',21},{'ņ',17},{'Ň',21},{'ň',17},{'ŉ',17},{'Ō',21},{'ō',17},{'Ŏ',21},{'ŏ',17},{'Ő',21},{'ő',17},{'Œ',31},{'œ',28},{'Ŕ',21},{'ŕ',10},{'Ŗ',21},{'ŗ',10},{'Ř',21},{'ř',10},{'Ś',21},{'ś',17},{'Ŝ',21},{'ŝ',17},{'Ş',21},{'ş',17},{'Š',21},{'š',17},{'Ţ',17},{'ţ',9},{'Ť',17},{'ť',9},{'Ŧ',17},{'ŧ',9},{'Ũ',20},{'ũ',17},{'Ū',20},{'ū',17},{'Ŭ',20},{'ŭ',17},{'Ů',20},{'ů',17},{'Ű',20},{'ű',17},{'Ų',20},{'ų',17},{'Ŵ',31},{'ŵ',27},{'Ŷ',17},{'ŷ',17},{'Ÿ',17},{'Ź',19},{'ź',16},{'Ż',19},{'ż',16},{'Ž',19},{'ž',16},{'ƒ',19},{'Ș',21},{'ș',17},{'Ț',17},{'ț',9},{'ˆ',8},{'ˇ',8},{'ˉ',6},{'˘',8},{'˙',8},{'˚',8},{'˛',8},{'˜',8},{'˝',8},{'Ё',19},{'Ѓ',16},{'Є',18},{'Ѕ',21},{'І',8},{'Ї',8},{'Ј',16},{'Љ',28},{'Њ',21},{'Ќ',19},{'Ў',17},{'Џ',18},{'А',19},{'Б',19},{'В',19},{'Г',15},{'Д',19},{'Е',18},{'Ж',21},{'З',17},{'И',19},{'Й',19},{'К',17},{'Л',17},{'М',26},{'Н',18},{'О',20},{'П',19},{'Р',19},{'С',19},{'Т',19},{'У',19},{'Ф',20},{'Х',19},{'Ц',20},{'Ч',16},{'Ш',26},{'Щ',29},{'Ъ',20},{'Ы',24},{'Ь',19},{'Э',18},{'Ю',27},{'Я',20},{'а',16},{'б',17},{'в',16},{'г',15},{'д',17},{'е',17},{'ж',20},{'з',15},{'и',16},{'й',16},{'к',17},{'л',15},{'м',25},{'н',16},{'о',16},{'п',16},{'р',17},{'с',16},{'т',14},{'у',17},{'ф',21},{'х',15},{'ц',17},{'ч',15},{'ш',25},{'щ',27},{'ъ',16},{'ы',20},{'ь',16},{'э',14},{'ю',23},{'я',17},{'ё',17},{'ђ',17},{'ѓ',16},{'є',14},{'ѕ',16},{'і',8},{'ї',8},{'ј',7},{'љ',22},{'њ',25},{'ћ',17},{'ќ',16},{'ў',17},{'џ',17},{'Ґ',15},{'ґ',13},{'–',15},{'—',31},{'‘',6},{'’',6},{'‚',6},{'“',12},{'”',12},{'„',12},{'†',20},{'‡',20},{'•',15},{'…',31},{'‰',31},{'‹',8},{'›',8},{'€',19},{'™',30},{'−',18},{'∙',8},{'□',21},{'',40},{'',40},{'',40},{'',40},{'',41},{'',41},{'',32},{'',32},{'',40},{'',40},{'',34},{'',34},{'',40},{'',40},{'',40},{'',41},{'',32},{'',41},{'',32},{'',40},{'',40},{'',40},{'',40},{'',40},{'',40},{'',40},{'',40}}},{FONT.MONOSPACE,new Dictionary<char,int>{{' ',24},{'!',24},{'"',24},{'#',24},{'$',24},{'%',24},{'&',24},{'\'',24},{'(',24},{')',24},{'*',24},{'+',24},{',',24},{'-',24},{'.',24},{'/',24},{'0',24},{'1',24},{'2',24},{'3',24},{'4',24},{'5',24},{'6',24},{'7',24},{'8',24},{'9',24},{':',24},{';',24},{'<',24},{'=',24},{'>',24},{'?',24},{'@',24},{'A',24},{'B',24},{'C',24},{'D',24},{'E',24},{'F',24},{'G',24},{'H',24},{'I',24},{'J',24},{'K',24},{'L',24},{'M',24},{'N',24},{'O',24},{'P',24},{'Q',24},{'R',24},{'S',24},{'T',24},{'U',24},{'V',24},{'W',24},{'X',24},{'Y',24},{'Z',24},{'[',24},{'\\',24},{']',24},{'^',24},{'_',24},{'`',24},{'a',24},{'b',24},{'c',24},{'d',24},{'e',24},{'f',24},{'g',24},{'h',24},{'i',24},{'j',24},{'k',24},{'l',24},{'m',24},{'n',24},{'o',24},{'p',24},{'q',24},{'r',24},{'s',24},{'t',24},{'u',24},{'v',24},{'w',24},{'x',24},{'y',24},{'z',24},{'{',24},{'|',24},{'}',24},{'~',24},{' ',24},{'¡',24},{'¢',24},{'£',24},{'¤',24},{'¥',24},{'¦',24},{'§',24},{'¨',24},{'©',24},{'ª',24},{'«',24},{'¬',24},{'­',24},{'®',24},{'¯',24},{'°',24},{'±',24},{'²',24},{'³',24},{'´',24},{'µ',24},{'¶',24},{'·',24},{'¸',24},{'¹',24},{'º',24},{'»',24},{'¼',24},{'½',24},{'¾',24},{'¿',24},{'À',24},{'Á',24},{'Â',24},{'Ã',24},{'Ä',24},{'Å',24},{'Æ',24},{'Ç',24},{'È',24},{'É',24},{'Ê',24},{'Ë',24},{'Ì',24},{'Í',24},{'Î',24},{'Ï',24},{'Ð',24},{'Ñ',24},{'Ò',24},{'Ó',24},{'Ô',24},{'Õ',24},{'Ö',24},{'×',24},{'Ø',24},{'Ù',24},{'Ú',24},{'Û',24},{'Ü',24},{'Ý',24},{'Þ',24},{'ß',24},{'à',24},{'á',24},{'â',24},{'ã',24},{'ä',24},{'å',24},{'æ',24},{'ç',24},{'è',24},{'é',24},{'ê',24},{'ë',24},{'ì',24},{'í',24},{'î',24},{'ï',24},{'ð',24},{'ñ',24},{'ò',24},{'ó',24},{'ô',24},{'õ',24},{'ö',24},{'÷',24},{'ø',24},{'ù',24},{'ú',24},{'û',24},{'ü',24},{'ý',24},{'þ',24},{'ÿ',24},{'Ā',24},{'ā',24},{'Ă',24},{'ă',24},{'Ą',24},{'ą',24},{'Ć',24},{'ć',24},{'Ĉ',24},{'ĉ',24},{'Ċ',24},{'ċ',24},{'Č',24},{'č',24},{'Ď',24},{'ď',24},{'Đ',24},{'đ',24},{'Ē',24},{'ē',24},{'Ĕ',24},{'ĕ',24},{'Ė',24},{'ė',24},{'Ę',24},{'ę',24},{'Ě',24},{'ě',24},{'Ĝ',24},{'ĝ',24},{'Ğ',24},{'ğ',24},{'Ġ',24},{'ġ',24},{'Ģ',24},{'ģ',24},{'Ĥ',24},{'ĥ',24},{'Ħ',24},{'ħ',24},{'Ĩ',24},{'ĩ',24},{'Ī',24},{'ī',24},{'Į',24},{'į',24},{'İ',24},{'ı',24},{'Ĳ',24},{'ĳ',24},{'Ĵ',24},{'ĵ',24},{'Ķ',24},{'ķ',24},{'Ĺ',24},{'ĺ',24},{'Ļ',24},{'ļ',24},{'Ľ',24},{'ľ',24},{'Ŀ',24},{'ŀ',24},{'Ł',24},{'ł',24},{'Ń',24},{'ń',24},{'Ņ',24},{'ņ',24},{'Ň',24},{'ň',24},{'ŉ',24},{'Ō',24},{'ō',24},{'Ŏ',24},{'ŏ',24},{'Ő',24},{'ő',24},{'Œ',24},{'œ',24},{'Ŕ',24},{'ŕ',24},{'Ŗ',24},{'ŗ',24},{'Ř',24},{'ř',24},{'Ś',24},{'ś',24},{'Ŝ',24},{'ŝ',24},{'Ş',24},{'ş',24},{'Š',24},{'š',24},{'Ţ',24},{'ţ',24},{'Ť',24},{'ť',24},{'Ŧ',24},{'ŧ',24},{'Ũ',24},{'ũ',24},{'Ū',24},{'ū',24},{'Ŭ',24},{'ŭ',24},{'Ů',24},{'ů',24},{'Ű',24},{'ű',24},{'Ų',24},{'ų',24},{'Ŵ',24},{'ŵ',24},{'Ŷ',24},{'ŷ',24},{'Ÿ',24},{'Ź',24},{'ź',24},{'Ż',24},{'ż',24},{'Ž',24},{'ž',24},{'ƒ',24},{'Ș',24},{'ș',24},{'Ț',24},{'ț',24},{'ˆ',24},{'ˇ',24},{'ˉ',24},{'˘',24},{'˙',24},{'˚',24},{'˛',24},{'˜',24},{'˝',24},{'Ё',24},{'Ѓ',24},{'Є',24},{'Ѕ',24},{'І',24},{'Ї',24},{'Ј',24},{'Љ',24},{'Њ',24},{'Ќ',24},{'Ў',24},{'Џ',24},{'А',24},{'Б',24},{'В',24},{'Г',24},{'Д',24},{'Е',24},{'Ж',24},{'З',24},{'И',24},{'Й',24},{'К',24},{'Л',24},{'М',24},{'Н',24},{'О',24},{'П',24},{'Р',24},{'С',24},{'Т',24},{'У',24},{'Ф',24},{'Х',24},{'Ц',24},{'Ч',24},{'Ш',24},{'Щ',24},{'Ъ',24},{'Ы',24},{'Ь',24},{'Э',24},{'Ю',24},{'Я',24},{'а',24},{'б',24},{'в',24},{'г',24},{'д',24},{'е',24},{'ж',24},{'з',24},{'и',24},{'й',24},{'к',24},{'л',24},{'м',24},{'н',24},{'о',24},{'п',24},{'р',24},{'с',24},{'т',24},{'у',24},{'ф',24},{'х',24},{'ц',24},{'ч',24},{'ш',24},{'щ',24},{'ъ',24},{'ы',24},{'ь',24},{'э',24},{'ю',24},{'я',24},{'ё',24},{'ђ',24},{'ѓ',24},{'є',24},{'ѕ',24},{'і',24},{'ї',24},{'ј',24},{'љ',24},{'њ',24},{'ћ',24},{'ќ',24},{'ў',24},{'џ',24},{'Ґ',24},{'ґ',24},{'–',24},{'—',24},{'‘',24},{'’',24},{'‚',24},{'“',24},{'”',24},{'„',24},{'†',24},{'‡',24},{'•',24},{'…',24},{'‰',24},{'‹',24},{'›',24},{'€',24},{'™',24},{'−',24},{'∙',24},{'□',24},{'',24},{'',24},{'',24},{'',24},{'',24},{'',24},{'',24},{'',24},{'',24},{'',24},{'',24},{'',24},{'',24},{'',24},{'',24},{'',24},{'',24},{'',24},{'',24},{'',24},{'',24},{'',24},{'',24},{'',24},{'',24},{'',24},{'',24}}}};public enum PadMode{LEFT,RIGHT,}public enum RoundMode{FLOOR,CEIL,}public static void SelectFont(FONT a){selectedFont=a;}public static int GetTextWidth(string a){if(DEBUG)Logger.debug("TextUtils.GetTextWidth()");Logger.IncLvl();int b=0;a=a.Replace("\r","");string[]d=a.Split('\n');foreach(string line in d)b=Math.Max(b,GetLineWidth(line.ToCharArray()));Logger.DecLvl();return b;}static int GetLineWidth(char[]a){if(DEBUG)Logger.debug("TextUtils.GetLineWidth()");Logger.IncLvl();int b=0;if(a.Length==0)return b;foreach(char c in a)b+=LetterWidths[selectedFont][c]+1;Logger.DecLvl();return b-1;}public static string RemoveLastTrailingNewline(string a){if(DEBUG)Logger.debug("TextUtils.RemoveLastTrailingNewline");Logger.IncLvl();Logger.DecLvl();return(a.Length>1&&a[a.Length-1]=='\n')?a.Remove(a.Length-1):a;}public static string RemoveFirstTrailingNewline(string a){if(DEBUG)Logger.debug("TextUtils.RemoveFirstTrailingNewline");Logger.IncLvl();Logger.DecLvl();return(a.Length>1&&a[0]=='\n')?a.Remove(0):a;}public static string CenterText(string a,int b){if(DEBUG)Logger.debug("TextUtils.CenterText()");Logger.IncLvl();if(DEBUG){Logger.debug("text is "+a);Logger.debug("width is "+b.ToString());}var d="";string[]e=a.Split('\n');int f;foreach(string line in e){f=GetLineWidth(line.ToCharArray());d+=CreateStringOfLength(" ",(b-f)/2)+line+CreateStringOfLength(" ",(b-f)/2)+"\n";}d=RemoveLastTrailingNewline(d);Logger.DecLvl();return d;}public static string CreateStringOfLength(string a,int b){return CreateStringOfLength(a,b,RoundMode.FLOOR);}public static string CreateStringOfLength(string a,int b,RoundMode d){if(DEBUG)Logger.debug("TextUtils.CreateStringOfLength()");Logger.IncLvl();int e=GetLineWidth(a.ToCharArray());if(d==RoundMode.CEIL)b+=e;var f="";if(b<e){Logger.DecLvl();return "";}for(int g=-1;g<b;g=g+e+1)f+=a;Logger.DecLvl();return f;}public static string PadString(string a,int b,PadMode d,string e){if(DEBUG)Logger.debug("TextUtils.PadString()");Logger.IncLvl();if(d==PadMode.LEFT){Logger.DecLvl();return CreateStringOfLength(e,b-GetLineWidth(a.ToCharArray()))+a;}else if(d==PadMode.RIGHT){Logger.DecLvl();return a+CreateStringOfLength(e,b-GetLineWidth(a.ToCharArray()));}Logger.DecLvl();return a;}public static string PadText(string a,int b,PadMode d){return PadText(a,b,d," ");}public static string PadText(string a,int b,PadMode d,string e){if(DEBUG)Logger.debug("TextUtils.PadText()");Logger.IncLvl();string[]f=a.Split('\n');var g="";foreach(string line in f)g+=PadString(line,b,d,e)+"\n";Logger.DecLvl();return g.Trim(new char[]{'\n'});}}public static class XML{public static Dictionary<string,Func<XML.XMLTree>>NodeRegister=new Dictionary<string,Func<XML.XMLTree>>{{"root",()=>{return new XML.RootNode();}},{"menu",()=>{return new XML.Menu();}},{"menuitem",()=>{return new XML.MenuItem();}},{"progressbar",()=>{return new XML.ProgressBar();}},{"container",()=>{return new XML.Container();}},{"hl",()=>{return new XML.HorizontalLine();}},{"uicontrols",()=>{return new UIControls();}},{"textinput",()=>{return new TextInput();}},{"submitbutton",()=>{return new SubmitButton();}},{"br",()=>{return new Break();}},{"space",()=>{return new Space();}},{"hidden",()=>{return new Hidden();}},{"hiddendata",()=>{return new Hidden();}},{"meta",()=>{return new o();}}};public static XMLTree CreateNode(string a){Logger.debug("XML.CreateNode()");Logger.IncLvl();a=a.ToLower();if(NodeRegister.ContainsKey(a)){Logger.DecLvl();return NodeRegister[a]();}else{Logger.DecLvl();return new Generic(a);}}public static XMLTree ParseXML(string a){char[]b={' ','\n'};Logger.debug("ParseXML");Logger.IncLvl();var d=new RootNode();XMLTree e=d;string f;Logger.debug("Enter while loop");while(a.Length>0)if(a[0]=='<'){Logger.debug("found tag");if(a[1]=='/'){Logger.debug("tag is end tag");int g=a.IndexOfAny(b);int h=a.IndexOf('>');int i=(g==-1?h:Math.Min(g,h))-2;f=a.Substring(2,i).ToLower();if(f!=e.Type){throw new Exception("Invalid end tag ('"+f+"(!= "+e.Type+"') found (node has been ended but not started)!");}e=e.GetParent()as XMLTree;a=a.Substring(h+1);}else{Logger.debug("tag is start tag");int g=a.IndexOfAny(b);int h=Parser.GetNextOutsideQuotes('>',a);int i=(g==-1?h:Math.Min(g,h))-1;f=a.Substring(1,i).ToLower().TrimEnd(new char[]{'/'});XMLTree j=XML.CreateNode(f);if(j==null){int k=a.IndexOf("<");int l=k==-1?a.Length:k;j=new XML.TextNode(a.Substring(0,l).Trim());}Logger.debug("add new node of type '"+j.Type+"="+f+"' to current node");e.AddChild(j);Logger.debug("added new node to current node");if(g!=-1&&g<h){string m=a.Substring(i+2,h-i-2);m=m.TrimEnd(new char[]{'/'});Logger.debug("get xml attributes. attribute string: '"+m+"/"+a+"'");Dictionary<string,string>n=Parser.GetXMLAttributes(m);Logger.debug("got xml attributes");foreach(string key in n.Keys){j.SetAttribute(key,n[key]);}}if(j.Type=="textnode"||h==-1||a[h-1]!='/'){e=j;}a=a.Substring(h+1);}}else{int h=a.IndexOf("<");int l=h==-1?a.Length:h;XMLTree j=new XML.TextNode(a.Substring(0,l).Trim());if(j.Render(0)!=null){e.AddChild(j);}a=h==-1?"":a.Substring(h);}Logger.debug("parsing finished");Logger.DecLvl();return d;}public class RootNode:XMLTree{public RootNode():base(){Type="root";PreventDefault("UP");PreventDefault("DOWN");}public override string GetAttribute(string a){XMLTree b=GetNode(d=>{return d.Type=="meta";});string e;if(b!=null)e=b.GetAttribute(a);else e=base.GetAttribute(a);switch(a){case "width":if(e==null)e="100%";break;}return e;}public override void UpdateSelectability(XMLTree a){base.UpdateSelectability(a);if(IsSelectable()&&!IsSelected())SelectFirst();}public override bool SelectNext(){if(IsSelectable()&&!base.SelectNext())return SelectNext();return true;}public override bool SelectPrevious(){if(!base.SelectPrevious())return SelectPrevious();return true;}public override void OnKeyPressed(string a){switch(a){case "UP":SelectPrevious();break;case "DOWN":SelectNext();break;}}}public abstract class XMLTree:XMLParentNode{public string Type;XMLParentNode Parent;List<string>PreventDefaults;protected List<XMLTree>Children;protected bool Selectable;protected bool ChildrenAreSelectable;bool Selected;protected int SelectedChild;protected bool Activated;protected Dictionary<string,string>Attributes;public XMLTree(){PreventDefaults=new List<string>();Parent=null;Children=new List<XMLTree>();Selectable=false;ChildrenAreSelectable=false;Selected=false;SelectedChild=-1;Activated=false;Attributes=new Dictionary<string,string>();Type="NULL";SetAttribute("alignself","left");SetAttribute("aligntext","left");SetAttribute("selected","false");SetAttribute("selectable","false");SetAttribute("flowdirection","vertical");}public bool IsSelectable(){Logger.debug(Type+": IsSelectable():");Logger.IncLvl();Logger.DecLvl();return Selectable||ChildrenAreSelectable;}public bool IsSelected(){Logger.debug(Type+": IsSelected():");Logger.IncLvl();Logger.DecLvl();return Selected;}public XMLTree GetSelectedSibling(){Logger.debug(Type+": GetSelectedSibling():");Logger.IncLvl();if(!Selected){Logger.DecLvl();return null;}if(SelectedChild==-1){Logger.DecLvl();return this;}else{Logger.DecLvl();return Children[SelectedChild].GetSelectedSibling();}}public virtual void AddChild(XMLTree a){Logger.debug(Type+": AddChild():");Logger.IncLvl();Children.Add(a);a.SetParent(this as XMLParentNode);UpdateSelectability(a);Logger.DecLvl();}public void SetParent(XMLParentNode a){Logger.debug(Type+": SetParent():");Logger.IncLvl();Parent=a;Logger.DecLvl();}public XMLParentNode GetParent(){Logger.debug(Type+": GetParent():");Logger.IncLvl();Logger.DecLvl();return Parent;}public XMLTree GetChild(int a){Logger.debug(Type+": GetChild():");Logger.IncLvl();Logger.DecLvl();return a<Children.Count?Children[a]:null;}public XMLTree GetNode(Func<XMLTree,bool>a){if(a(this))return this;else{XMLTree b=GetChild(0);XMLTree d;for(int e=1;b!=null;e++){d=b.GetNode(a);if(d!=null)return d;b=GetChild(e);}}return null;}public List<XMLTree>GetAllNodes(Func<XMLTree,bool>a){var b=new List<XMLTree>();GetAllNodes(a,ref b);return b;}void GetAllNodes(Func<XMLTree,bool>a,ref List<XMLTree>b){if(a(this))b.Add(this);XMLTree d=GetChild(0);for(int e=1;d!=null;e++){d.GetAllNodes(a,ref b);d=GetChild(e);}}public virtual void UpdateSelectability(XMLTree a){Logger.debug(Type+": UpdateSelectability():");Logger.IncLvl();var b=ChildrenAreSelectable;ChildrenAreSelectable=ChildrenAreSelectable||a.IsSelectable();if(Parent!=null&&(Selectable||ChildrenAreSelectable)!=(Selectable||b))Parent.UpdateSelectability(this);Logger.DecLvl();}public bool SelectFirst(){Logger.debug(Type+": SelectFirst():");Logger.IncLvl();if(SelectedChild!=-1)Children[SelectedChild].Unselect();SelectedChild=-1;var a=(Selectable||ChildrenAreSelectable)?SelectNext():false;Logger.DecLvl();return a;}public bool SelectLast(){Logger.debug(Type+": SelectLast():");Logger.IncLvl();if(SelectedChild!=-1)Children[SelectedChild].Unselect();SelectedChild=-1;Logger.DecLvl();return(Selectable||ChildrenAreSelectable)?SelectPrevious():false;}public void Unselect(){Logger.debug(Type+": Unselect():");Logger.IncLvl();if(SelectedChild!=-1)Children[SelectedChild].Unselect();Selected=false;Activated=false;Logger.DecLvl();}public virtual bool SelectNext(){Logger.debug(Type+": SelectNext():");Logger.IncLvl();var a=IsSelected();if(SelectedChild==-1||!Children[SelectedChild].SelectNext()){Logger.debug(Type+": find next child to select...");SelectedChild++;while((SelectedChild<Children.Count&&(!Children[SelectedChild].SelectFirst())))SelectedChild++;if(SelectedChild==Children.Count){SelectedChild=-1;Selected=Selectable&&!Selected;}else Selected=true;}if(!Selected)Unselect();if(!a&&IsSelected())OnSelect();Logger.DecLvl();return Selected;}public virtual bool SelectPrevious(){Logger.debug(Type+": SelectPrevious():");Logger.IncLvl();var a=IsSelected();if(SelectedChild==-1)SelectedChild=Children.Count;if(SelectedChild==Children.Count||!Children[SelectedChild].SelectPrevious()){SelectedChild--;while(SelectedChild>-1&&!Children[SelectedChild].SelectLast())SelectedChild--;if(SelectedChild==-1)Selected=Selectable&&!Selected;else Selected=true;}if(!Selected)Unselect();if(!a&&IsSelected())OnSelect();Logger.DecLvl();return Selected;}public virtual void OnSelect(){}public virtual string GetAttribute(string a){Logger.debug(Type+": GetAttribute("+a+"):");Logger.IncLvl();if(Attributes.ContainsKey(a)){Logger.DecLvl();return Attributes[a];}Logger.DecLvl();return null;}public virtual void SetAttribute(string a,string b){Logger.debug(Type+": SetAttribute():");Logger.IncLvl();if(a=="selectable"){var d=b=="true";if(Selectable!=d){Selectable=d;if(Parent!=null)Parent.UpdateSelectability(this);}}if(a=="activated"){var e=b=="true";Activated=e;}Attributes[a]=b;Logger.DecLvl();}public void KeyPress(string a){Logger.debug(Type+": _KeyPress():");Logger.IncLvl();Logger.debug("button: "+a);OnKeyPressed(a);if(Parent!=null&&!PreventDefaults.Contains(a))Parent.KeyPress(a);Logger.DecLvl();}public virtual void OnKeyPressed(string a){Logger.debug(Type+": OnKeyPressed()");Logger.IncLvl();switch(a){case "ACTIVATE":ToggleActivation();break;default:break;}Logger.DecLvl();}public virtual void ToggleActivation(){Logger.debug(Type+": ToggleActivation()");Logger.IncLvl();Activated=!Activated;Logger.DecLvl();}public void PreventDefault(string a){Logger.debug(Type+": PreventDefault()");Logger.IncLvl();if(!PreventDefaults.Contains(a))PreventDefaults.Add(a);Logger.DecLvl();}public void AllowDefault(string a){Logger.debug(Type+": AllowDefault()");Logger.IncLvl();if(PreventDefaults.Contains(a))PreventDefaults.Remove(a);Logger.DecLvl();}public void FollowRoute(Route a){Logger.debug(Type+": FollowRoute");Logger.IncLvl();if(Parent!=null)Parent.FollowRoute(a);Logger.DecLvl();}public virtual Dictionary<string,string>GetValues(Func<XMLTree,bool>a){Logger.debug(Type+": GetValues()");Logger.IncLvl();var b=new Dictionary<string,string>();var d=GetAttribute("name");var e=GetAttribute("value");if(d!=null&&e!=null)b[d]=e;Dictionary<string,string>f;foreach(XMLTree child in Children){f=child.GetValues(a);foreach(string key in f.Keys)if(!b.ContainsKey(key)){b[key]=f[key];}}Logger.DecLvl();return b;}public int GetWidth(int a){Logger.debug(Type+".GetWidth()");Logger.IncLvl();var b=GetAttribute("width");if(b==null){Logger.DecLvl();return 0;}else if(b[b.Length-1]=='%'){Logger.debug("is procent value ("+Single.Parse(b.Substring(0,b.Length-1)).ToString()+")");Logger.DecLvl();return(int)(Single.Parse(b.Substring(0,b.Length-1))/100f*a);}else if(a==0){Logger.DecLvl();return Int32.Parse(b);}else{Logger.DecLvl();return Math.Min(a,Int32.Parse(b));}}public string Render(int a){Logger.debug(Type+".Render()");Logger.IncLvl();var b=new List<string>();int d=GetWidth(a);PreRender(ref b,d,a);RenderText(ref b,d,a);var e=PostRender(b,d,a);Logger.DecLvl();return e;}protected virtual void PreRender(ref List<string>a,int b,int d){Logger.debug(Type+".PreRender()");Logger.IncLvl();Logger.DecLvl();}protected virtual void RenderText(ref List<string>a,int b,int d){Logger.debug(Type+".RenderText()");Logger.IncLvl();for(int e=0;e<Children.Count;e++)if(GetAttribute("flowdirection")=="vertical"){string f=RenderChild(Children[e],b);if(f!=null){if(e>0&&Children[e-1].Type=="textnode"&&(Children[e].Type=="textnode"||Children[e].Type=="br")){a[a.Count-1]+=f;}else{a.Add(f);}}else{}}else{string f=RenderChild(Children[e],b);if(f!=null){d-=TextUtils.GetTextWidth(f);a.Add(f);}}Logger.DecLvl();}protected virtual string PostRender(List<string>a,int b,int d){Logger.debug(Type+".PostRender()");Logger.IncLvl();var e="";var f=GetAttribute("flowdirection");var g=GetAttribute("alignchildren");var h=GetAttribute("alignself");int i=0;foreach(string segment in a){int j=TextUtils.GetTextWidth(segment);if(j>i)i=j;}i=Math.Min(d,Math.Max(b,i));if(f=="vertical"){for(int k=0;k<a.Count;k++)switch(g){case "right":a[k]=TextUtils.PadText(a[k],i,TextUtils.PadMode.LEFT);break;case "center":a[k]=TextUtils.CenterText(a[k],i);break;default:a[k]=TextUtils.PadText(a[k],i,TextUtils.PadMode.RIGHT);break;}e=String.Join("\n",a.ToArray());}else e=String.Join("",a.ToArray());if(d-i>0)if(h=="center"){Logger.log("Center element...");e=TextUtils.CenterText(e,d);}else if(h=="right"){Logger.log("Aligning element right...");e=TextUtils.PadText(e,d,TextUtils.PadMode.RIGHT);}Logger.DecLvl();return e;}protected virtual string RenderChild(XMLTree a,int b){Logger.log(Type+".RenderChild()");Logger.IncLvl();Logger.DecLvl();return a.Render(b);}}public interface XMLParentNode{XMLParentNode GetParent();void UpdateSelectability(XMLTree a);void KeyPress(string a);void FollowRoute(Route a);bool SelectNext();}public class TextNode:XMLTree{public string Content;public TextNode(string a):base(){Type="textnode";Content=a.Replace("\n","");Content=Content.Trim(new char[]{'\n',' ','\r'});if(Content=="")Content=null;}protected override void RenderText(ref List<string>a,int b,int d){}protected override string PostRender(List<string>a,int b,int d){return Content;}}public class Route{string Definition;static Dictionary<string,Action<UIController>>UIFactories=new Dictionary<string,Action<UIController>>();public Route(string a){Logger.debug("Route constructor():");Logger.IncLvl();Definition=a;Logger.debug("xml string is: "+Definition.Substring(4));Logger.DecLvl();}public void Follow(UIController a){Logger.debug("Route: GetUI():");Logger.IncLvl();XMLTree b=null;if(Definition=="revert")a.RevertUI();else if(Definition.Substring(0,4)=="xml:"){b=ParseXML(Parser.UnescapeQuotes(Definition.Substring(4)));a.LoadUI(b);}else if(Definition.Substring(0,3)=="fn:"&&UIFactories.ContainsKey(Definition.Substring(3)))UIFactories[Definition.Substring(3)](a);Logger.DecLvl();}static public void RegisterRouteFunction(string a,Action<UIController>b){UIFactories[a]=b;}}public class UIController:XMLParentNode{XMLTree ui;public Stack<XMLTree>UIStack;public string Type;public UIController(XMLTree a){Logger.debug("UIController constructor()");Logger.IncLvl();Type="CTRL";UIStack=new Stack<XMLTree>();ui=a;ui.SetParent(this);if(GetSelectedNode()==null&&ui.IsSelectable())ui.SelectFirst();Logger.DecLvl();}public static UIController FromXML(string a){Logger.debug("UIController FromXMLString()");Logger.IncLvl();XMLTree b=XML.ParseXML(a);Logger.DecLvl();return new UIController(b);}public void ApplyScreenProperties(IMyTextPanel a){Logger.debug("UIController.ApplyScreenProperties()");Logger.IncLvl();if(ui.GetAttribute("fontcolor")!=null){var b=ui.GetAttribute("fontcolor");b="FF"+b.Substring(b.Length-2,2)+b.Substring(b.Length-4,2)+b.Substring(b.Length-6,2);var d=new Color(uint.Parse(b,System.Globalization.NumberStyles.AllowHexSpecifier));a.SetValue<Color>("FontColor",d);}if(ui.GetAttribute("fontsize")!=null)a.SetValue<Single>("FontSize",Single.Parse(ui.GetAttribute("fontsize")));if(ui.GetAttribute("backgroundcolor")!=null){var b=ui.GetAttribute("backgroundcolor");b="FF"+b.Substring(b.Length-2,2)+b.Substring(b.Length-4,2)+b.Substring(b.Length-6,2);var d=new Color(uint.Parse(b,System.Globalization.NumberStyles.AllowHexSpecifier));a.SetValue<Color>("BackgroundColor",d);}Logger.DecLvl();}public void Call(List<string>a){Logger.debug("UIController.Main()");Logger.IncLvl();switch(a[0]){case "key":XMLTree b=GetSelectedNode();if(b!=null)b.KeyPress(a[1].ToUpper());break;case "refresh":var d=ui.GetAttribute("refresh");if(d!=null)FollowRoute(new Route(d));break;case "revert":RevertUI();break;default:break;}Logger.DecLvl();return;}public void LoadXML(string a){LoadUI(XML.ParseXML(a));}public void LoadUI(XMLTree a){Logger.debug("UIController: LoadUI():");Logger.IncLvl();if(ui.GetAttribute("historydisabled")==null||ui.GetAttribute("historydisabled")!="true")UIStack.Push(ui);if(a.GetAttribute("revert")!=null&&a.GetAttribute("revert")=="true")RevertUI();else{ui=a;ui.SetParent(this);}Logger.DecLvl();}public void ClearUIStack(){UIStack=new Stack<XMLTree>();}public void RevertUI(){Logger.log("UIController: RevertUI():");Logger.IncLvl();if(UIStack.Count==0){Logger.log("Error: Can't revert: UI stack is empty.");Logger.DecLvl();return;}ui=UIStack.Pop();ui.SetParent(this);Logger.DecLvl();}public string Render(){Logger.debug("UIController: Render():");Logger.IncLvl();Logger.DecLvl();return ui.Render(0);}public void RenderTo(IMyTextPanel a){Logger.debug("UIController.RenderTo()");Logger.IncLvl();int b=0;var d=a.DetailedInfo.Split('\n')[0];Logger.debug("Type: "+d);switch(d){case "Type: Text Panel":b=658;break;case "Type: LCD Panel":b=658;break;case "Wide LCD Panel":b=1316;break;}int e=(int)(((float)b)/a.GetValue<Single>("FontSize"));Logger.debug("font size: "+a.GetValue<Single>("FontSize").ToString());Logger.debug("resulting width: "+e.ToString());var f=ui.Render(e);a.WritePublicText(f);Logger.DecLvl();}public void KeyPress(string a){Logger.debug("UIController: KeyPress():");Logger.IncLvl();switch(a){case "LEFT/ABORT":RevertUI();break;}Logger.DecLvl();}public XMLTree GetSelectedNode(){Logger.debug("UIController: GetSelectedNode():");Logger.IncLvl();XMLTree a=ui.GetSelectedSibling();Logger.DecLvl();return a;}public XMLTree GetNode(Func<XMLTree,bool>a){Logger.debug("UIController: GetNode()");Logger.IncLvl();Logger.DecLvl();return ui.GetNode(a);}public List<XMLTree>GetAllNodes(Func<XMLTree,bool>a){Logger.debug("UIController: GetAllNodes()");Logger.IncLvl();Logger.DecLvl();return ui.GetAllNodes(a);}public void UpdateSelectability(XMLTree a){}public void FollowRoute(Route a){Logger.debug("UIController: FollowRoute():");Logger.IncLvl();a.Follow(this);Logger.DecLvl();}public XMLParentNode GetParent(){return null;}public Dictionary<string,string>GetValues(){Logger.debug("UIController.GetValues()");Logger.IncLvl();return GetValues(a=>true);}public Dictionary<string,string>GetValues(Func<XMLTree,bool>a){Logger.debug("UIController.GetValues()");Logger.IncLvl();if(ui==null){Logger.DecLvl();return null;}Logger.DecLvl();return ui.GetValues(a);}public string GetPackedValues(Func<XMLTree,bool>a){return Parser.PackData(GetValues(a));}public string GetPackedValues(){Logger.debug("UIController.GetPackedValues()");Logger.IncLvl();Logger.DecLvl();return GetPackedValues(a=>true);}public bool SelectNext(){return ui.SelectNext();}}public abstract class UIFactory{int Count;int Max;List<UIController>UIs;public UIFactory():this(null){}public UIFactory(List<UIController>a){Logger.debug("UIFactory constructor");Logger.IncLvl();if(a==null)UIs=new List<UIController>();UIs=a;Logger.DecLvl();}public abstract XMLTree Render(UIController a);protected void UpdateUIs(XMLTree a){foreach(UIController ui in UIs)ui.LoadUI(a);}}public class Generic:XMLTree{public Generic(string a):base(){Type=a;}}public class Menu:XMLTree{public Menu():base(){Type="menu";}public override void AddChild(XMLTree a){Logger.debug(Type+": Add child():");Logger.IncLvl();if(a.Type!="menuitem"&&a.IsSelectable()){Logger.DecLvl();throw new Exception("ERROR: Only children of type <menupoint> or children that are not selectable are allowed!"+" (type was: <"+a.Type+">)");}base.AddChild(a);Logger.DecLvl();}protected override string RenderChild(XMLTree a,int b){var d="";var e="     ";if(a.Type=="menuitem")d+=(a.IsSelected()?">> ":e);d+=base.RenderChild(a,b);return d;}}public class MenuItem:XMLTree{Route TargetRoute;public MenuItem():this(null){}public MenuItem(Route a):base(){Type="menuitem";Selectable=true;SetRoute(a);PreventDefault("RIGHT/SUBMIT");}public override void SetAttribute(string a,string b){Logger.debug(Type+": SetAttribute():");Logger.IncLvl();switch(a){case "route":Logger.debug("prepare to set route...");SetRoute(new Route(b));if(TargetRoute==null)Logger.debug("Failure!");else Logger.debug("Success!");break;default:base.SetAttribute(a,b);break;}Logger.DecLvl();}public override void OnKeyPressed(string a){Logger.debug(Type+": OnKeyPressed():");switch(a){case "RIGHT/SUBMIT":if(TargetRoute!=null){Logger.debug("Follow Target Route!");FollowRoute(TargetRoute);}else Logger.debug("No route set!");break;}base.OnKeyPressed(a);Logger.DecLvl();}public void SetRoute(Route a){TargetRoute=a;}protected override void RenderText(ref List<string>a,int b,int d){}}public class ProgressBar:XMLTree{float StepSize{get{float a;if(!Single.TryParse(GetAttribute("stepsize"),out a))return 0.1f;return a;}set{var b=Math.Max(0.001f,Math.Min(0.009f,value)).ToString();if(b.Length>5)b+=b.Substring(0,5);SetAttribute("stepsize",b);}}public float FillLevel{get{float a;if(!Single.TryParse(GetAttribute("value"),out a))return 0.0f;return a;}set{var b=Math.Max(0f,Math.Min(1f,value)).ToString();if(b.Length>5)b=b.Substring(0,5);SetAttribute("value",b);}}public ProgressBar():this(0f){}public ProgressBar(float a):this(a,false){}public ProgressBar(float a,bool b):base(){Type="progressbar";PreventDefault("LEFT/ABORT");PreventDefault("RIGHT/SUBMIT");SetAttribute("width","500");SetAttribute("filledstring","|");SetAttribute("emptystring","'");SetAttribute("value",a.ToString());SetAttribute("stepsize","0.05");SetAttribute("selectable",b?"true":"false");}public void IncreaseFillLevel(){Logger.debug(Type+".IncreaseFillLevel()");Logger.IncLvl();FillLevel+=StepSize;Logger.DecLvl();}public void DecreaseFillLevel(){Logger.debug(Type+".DecreaseFillLevel()");Logger.IncLvl();FillLevel-=StepSize;Logger.DecLvl();}public override void OnKeyPressed(string a){Logger.debug(Type+": OnKeyPressed():");Logger.IncLvl();switch(a){case "LEFT/ABORT":DecreaseFillLevel();break;case "RIGHT/SUBMIT":IncreaseFillLevel();break;}base.OnKeyPressed(a);Logger.DecLvl();}protected override void RenderText(ref List<string>a,int b,int d){Logger.debug(Type+".RenderText()");Logger.IncLvl();var e=IsSelected()?">":"  ";var f=IsSelected()?"<":"  ";var g=f+"[";float h=FillLevel;var i=GetAttribute("filledstring");var j=GetAttribute("emptystring");int k=(b-2*TextUtils.GetTextWidth("[]"));g+=TextUtils.CreateStringOfLength(i,(int)(k*h));g+=TextUtils.CreateStringOfLength(j,(int)(k*(1-h)));g+="]"+e;a.Add(g);Logger.DecLvl();}}public class Container:XMLTree{public Container():base(){Type="container";}}public class HorizontalLine:XMLTree{public HorizontalLine():base(){Type="hl";SetAttribute("width","100%");}protected override void RenderText(ref List<string>a,int b,int d){a.Add(TextUtils.CreateStringOfLength("_",b,TextUtils.RoundMode.CEIL));}}public class UIControls:XMLTree{UIController Controller;public UIControls():base(){Type="uicontrols";Controller=null;SetAttribute("selectable","false");}void RetrieveController(){XMLParentNode a=this;while(a.GetParent()!=null)a=a.GetParent();Controller=a as UIController;SetAttribute("selectable",(Controller!=null&&Controller.UIStack.Count>0)?"true":"false");if(IsSelectable()){PreventDefault("LEFT/ABORT");PreventDefault("RIGHT/SUBMIT");}else{AllowDefault("LEFT/ABORT");AllowDefault("RIGHT/SUBMIT");}GetParent().UpdateSelectability(this);if(IsSelected()&&!IsSelectable())GetParent().SelectNext();}public override void OnKeyPressed(string a){if(Controller==null)RetrieveController();switch(a){case "LEFT/ABORT":case "RIGHT/SUBMIT":if(Controller!=null&&Controller.UIStack.Count>0)Controller.RevertUI();break;}}protected override string PostRender(List<string>a,int b,int d){if(Controller==null)RetrieveController();string e;if(!IsSelectable())e="";else e=IsSelected()?"<<":TextUtils.CreateStringOfLength(" ",TextUtils.GetTextWidth("<<"));var f=base.PostRender(a,b,d);int g=TextUtils.CreateStringOfLength(" ",TextUtils.GetTextWidth(e)).Length;var h="";for(int i=0;i<g;i++)if((f.Length-1)<i||f[i]!=' '){h+=" ";}f=e+(h+f).Substring(g);return f;}}public class TextInput:XMLTree{int CursorPosition;public TextInput(){Type="textinput";Selectable=true;CursorPosition=-1;PreventDefault("LEFT/ABORT");PreventDefault("RIGHT/SUBMIT");SetAttribute("maxlength","10");SetAttribute("value","");}public override void OnKeyPressed(string a){switch(a){case "LEFT/ABORT":DecreaseCursorPosition();break;case "RIGHT/SUBMIT":IncreaseCursorPosition();break;case "UP":IncreaseLetter();break;case "DOWN":DecreaseLetter();break;default:base.OnKeyPressed(a);break;}}void IncreaseLetter(){if(CursorPosition==-1)return;char[]a=GetAttribute("value").ToCharArray();var b=a[CursorPosition];switch(b){case ' ':a[CursorPosition]='a';break;case 'z':a[CursorPosition]='A';break;case 'Z':a[CursorPosition]='0';break;case '9':a[CursorPosition]=' ';break;default:a[CursorPosition]=(char)(((int)a[CursorPosition])+1);break;}SetAttribute("value",new string(a));}void DecreaseLetter(){if(CursorPosition==-1)return;char[]a=GetAttribute("value").ToCharArray();var b=a[CursorPosition];switch(b){case ' ':a[CursorPosition]='9';break;case '0':a[CursorPosition]='Z';break;case 'a':a[CursorPosition]=' ';break;case 'A':a[CursorPosition]='z';break;default:a[CursorPosition]=(char)(((int)a[CursorPosition])-1);break;}SetAttribute("value",new string(a));}void IncreaseCursorPosition(){if(CursorPosition<Single.Parse(GetAttribute("maxlength"))-1)CursorPosition++;else{CursorPosition=0;DecreaseCursorPosition();KeyPress("DOWN");}if(CursorPosition!=-1){PreventDefault("UP");PreventDefault("DOWN");}if(CursorPosition>=GetAttribute("value").Length)SetAttribute("value",GetAttribute("value")+" ");}void DecreaseCursorPosition(){if(CursorPosition>-1)CursorPosition--;if(CursorPosition==-1){AllowDefault("UP");AllowDefault("DOWN");}}protected override void RenderText(ref List<string>a,int b,int d){var e=GetAttribute("value");if(CursorPosition!=-1)e=e.Substring(0,CursorPosition)+"|"+e.Substring(CursorPosition,1)+"|"+e.Substring(CursorPosition+1);else if(e.Length==0)e="_"+e;a.Add((IsSelected()?new string(new char[]{(char)187}):"  ")+" "+e);}}public abstract class DataStore:XMLTree{public DataStore():base(){}public override Dictionary<string,string>GetValues(Func<XMLTree,bool>a){Dictionary<string,string>b=base.GetValues(a);if(!a(this))return b;foreach(KeyValuePair<string,string>data in Attributes)if(!b.ContainsKey(data.Key)){b[data.Key]=data.Value;}return b;}}public class SubmitButton:MenuItem{public SubmitButton(){Type="submitbutton";SetAttribute("flowdirection","horizontal");}protected override void PreRender(ref List<string>a,int b,int d){a.Add(IsSelected()?"[[  ":"[   ");base.PreRender(ref a,b,d);}protected override string PostRender(List<string>a,int b,int d){a.Add(IsSelected()?"  ]]":"   ]");return base.PostRender(a,b,d);}}public class Break:TextNode{public Break():base(""){Type="br";}protected override void RenderText(ref List<string>a,int b,int d){}protected override string PostRender(List<string>a,int b,int d){return "";}}public class Space:XMLTree{public Space():base(){Logger.debug("Space constructor()");Logger.IncLvl();Type="space";SetAttribute("width","0");Logger.DecLvl();}protected override void RenderText(ref List<string>a,int b,int d){Logger.debug(Type+".RenderText()");Logger.IncLvl();a.Add(TextUtils.CreateStringOfLength(" ",b));Logger.DecLvl();}}public class Hidden:XMLTree{public Hidden():base(){Type="hidden";}protected override string PostRender(List<string>a,int b,int d){return null;}}public class HiddenData:DataStore{public HiddenData():base(){Type="hiddendata";}protected override string PostRender(List<string>a,int b,int d){return null;}}class o:Hidden{public o():base(){Type="meta";}public override Dictionary<string,string>GetValues(Func<XMLTree,bool>a){if(a(this))return Attributes;else return new Dictionary<string,string>();}}}
