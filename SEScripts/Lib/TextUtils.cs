using System;
using System.Text;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using VRageMath;
using VRage.Game;
using VRage.Collections;
using Sandbox.ModAPI.Ingame;
using VRage.Game.Components;
using VRage.Game.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using Sandbox.Game.EntityComponents;
using SpaceEngineers.Game.ModAPI.Ingame;
using VRage.Game.ObjectBuilders.Definitions;

using SEScripts.Lib.LoggerNS;
using SEScripts.Lib.Profilers;

namespace SEScripts.Lib
{
    public static class TextUtils
    {
        public enum FONT { DEFAULT, MONOSPACE };
        public static bool DEBUG = true;
        private static FONT selectedFont = FONT.DEFAULT;

        private static Dictionary<char, int> LetterWidths = new Dictionary<char, int>{
            { ' ', 8 }, { '!', 8 }, { '"', 10}, {'#', 19}, {'$', 20}, {'%', 24}, {'&', 20}, {'\'', 6}, {'(', 9}, {')', 9}, {'*', 11}, {'+', 18}, {',', 9}, {'-', 10}, {'.', 9}, {'/', 14}, {'0', 19}, {'1', 9}, {'2', 19}, {'3', 17}, {'4', 19}, {'5', 19}, {'6', 19}, {'7', 16}, {'8', 19}, {'9', 19}, {':', 9}, {';', 9}, {'<', 18}, {'=', 18}, {'>', 18}, {'?', 16}, {'@', 25}, {'A', 21}, {'B', 21}, {'C', 19}, {'D', 21}, {'E', 18}, {'F', 17}, {'G', 20}, {'H', 20}, {'I', 8}, {'J', 16}, {'K', 17}, {'L', 15}, {'M', 26}, {'N', 21}, {'O', 21}, {'P', 20}, {'Q', 21}, {'R', 21}, {'S', 21}, {'T', 17}, {'U', 20}, {'V', 20}, {'W', 31}, {'X', 19}, {'Y', 20}, {'Z', 19}, {'[', 9}, {'\\', 12}, {']', 9}, {'^', 18}, {'_', 15}, {'`', 8}, {'a', 17}, {'b', 17}, {'c', 16}, {'d', 17}, {'e', 17}, {'f', 9}, {'g', 17}, {'h', 17}, {'i', 8}, {'j', 8}, {'k', 17}, {'l', 8}, {'m', 27}, {'n', 17}, {'o', 17}, {'p', 17}, {'q', 17}, {'r', 10}, {'s', 17}, {'t', 9}, {'u', 17}, {'v', 15}, {'w', 27}, {'x', 15}, {'y', 17}, {'z', 16}, {'{', 9}, {'|', 6}, {'}', 9}, {'~', 18}, {' ', 8}, {'¡', 8}, {'¢', 16}, {'£', 17}, {'¤', 19}, {'¥', 19}, {'¦', 6}, {'§', 20}, {'¨', 8}, {'©', 25}, {'ª', 10}, {'«', 15}, {'¬', 18}, {'­', 10}, {'®', 25}, {'¯', 8}, {'°', 12}, {'±', 18}, {'²', 11}, {'³', 11}, {'´', 8}, {'µ', 17}, {'¶', 18}, {'·', 9}, {'¸', 8}, {'¹', 11}, {'º', 10}, {'»', 15}, {'¼', 27}, {'½', 29}, {'¾', 28}, {'¿', 16}, {'À', 21}, {'Á', 21}, {'Â', 21}, {'Ã', 21}, {'Ä', 21}, {'Å', 21}, {'Æ', 31}, {'Ç', 19}, {'È', 18}, {'É', 18}, {'Ê', 18}, {'Ë', 18}, {'Ì', 8}, {'Í', 8}, {'Î', 8}, {'Ï', 8}, {'Ð', 21}, {'Ñ', 21}, {'Ò', 21}, {'Ó', 21}, {'Ô', 21}, {'Õ', 21}, {'Ö', 21}, {'×', 18}, {'Ø', 21}, {'Ù', 20}, {'Ú', 20}, {'Û', 20}, {'Ü', 20}, {'Ý', 17}, {'Þ', 20}, {'ß', 19}, {'à', 17}, {'á', 17}, {'â', 17}, {'ã', 17}, {'ä', 17}, {'å', 17}, {'æ', 28}, {'ç', 16}, {'è', 17}, {'é', 17}, {'ê', 17}, {'ë', 17}, {'ì', 8}, {'í', 8}, {'î', 8}, {'ï', 8}, {'ð', 17}, {'ñ', 17}, {'ò', 17}, {'ó', 17}, {'ô', 17}, {'õ', 17}, {'ö', 17}, {'÷', 18}, {'ø', 17}, {'ù', 17}, {'ú', 17}, {'û', 17}, {'ü', 17}, {'ý', 17}, {'þ', 17}, {'ÿ', 17}, {'Ā', 20}, {'ā', 17}, {'Ă', 21}, {'ă', 17}, {'Ą', 21}, {'ą', 17}, {'Ć', 19}, {'ć', 16}, {'Ĉ', 19}, {'ĉ', 16}, {'Ċ', 19}, {'ċ', 16}, {'Č', 19}, {'č', 16}, {'Ď', 21}, {'ď', 17}, {'Đ', 21}, {'đ', 17}, {'Ē', 18}, {'ē', 17}, {'Ĕ', 18}, {'ĕ', 17}, {'Ė', 18}, {'ė', 17}, {'Ę', 18}, {'ę', 17}, {'Ě', 18}, {'ě', 17}, {'Ĝ', 20}, {'ĝ', 17}, {'Ğ', 20}, {'ğ', 17}, {'Ġ', 20}, {'ġ', 17}, {'Ģ', 20}, {'ģ', 17}, {'Ĥ', 20}, {'ĥ', 17}, {'Ħ', 20}, {'ħ', 17}, {'Ĩ', 8}, {'ĩ', 8}, {'Ī', 8}, {'ī', 8}, {'Į', 8}, {'į', 8}, {'İ', 8}, {'ı', 8}, {'Ĳ', 24}, {'ĳ', 14}, {'Ĵ', 16}, {'ĵ', 8}, {'Ķ', 17}, {'ķ', 17}, {'Ĺ', 15}, {'ĺ', 8}, {'Ļ', 15}, {'ļ', 8}, {'Ľ', 15}, {'ľ', 8}, {'Ŀ', 15}, {'ŀ', 10}, {'Ł', 15}, {'ł', 8}, {'Ń', 21}, {'ń', 17}, {'Ņ', 21}, {'ņ', 17}, {'Ň', 21}, {'ň', 17}, {'ŉ', 17}, {'Ō', 21}, {'ō', 17}, {'Ŏ', 21}, {'ŏ', 17}, {'Ő', 21}, {'ő', 17}, {'Œ', 31}, {'œ', 28}, {'Ŕ', 21}, {'ŕ', 10}, {'Ŗ', 21}, {'ŗ', 10}, {'Ř', 21}, {'ř', 10}, {'Ś', 21}, {'ś', 17}, {'Ŝ', 21}, {'ŝ', 17}, {'Ş', 21}, {'ş', 17}, {'Š', 21}, {'š', 17}, {'Ţ', 17}, {'ţ', 9}, {'Ť', 17}, {'ť', 9}, {'Ŧ', 17}, {'ŧ', 9}, {'Ũ', 20}, {'ũ', 17}, {'Ū', 20}, {'ū', 17}, {'Ŭ', 20}, {'ŭ', 17}, {'Ů', 20}, {'ů', 17}, {'Ű', 20}, {'ű', 17}, {'Ų', 20}, {'ų', 17}, {'Ŵ', 31}, {'ŵ', 27}, {'Ŷ', 17}, {'ŷ', 17}, {'Ÿ', 17}, {'Ź', 19}, {'ź', 16}, {'Ż', 19}, {'ż', 16}, {'Ž', 19}, {'ž', 16}, {'ƒ', 19}, {'Ș', 21}, {'ș', 17}, {'Ț', 17}, {'ț', 9}, {'ˆ', 8}, {'ˇ', 8}, {'ˉ', 6}, {'˘', 8}, {'˙', 8}, {'˚', 8}, {'˛', 8}, {'˜', 8}, {'˝', 8}, {'Ё', 19}, {'Ѓ', 16}, {'Є', 18}, {'Ѕ', 21}, {'І', 8}, {'Ї', 8}, {'Ј', 16}, {'Љ', 28}, {'Њ', 21}, {'Ќ', 19}, {'Ў', 17}, {'Џ', 18}, {'А', 19}, {'Б', 19}, {'В', 19}, {'Г', 15}, {'Д', 19}, {'Е', 18}, {'Ж', 21}, {'З', 17}, {'И', 19}, {'Й', 19}, {'К', 17}, {'Л', 17}, {'М', 26}, {'Н', 18}, {'О', 20}, {'П', 19}, {'Р', 19}, {'С', 19}, {'Т', 19}, {'У', 19}, {'Ф', 20}, {'Х', 19}, {'Ц', 20}, {'Ч', 16}, {'Ш', 26}, {'Щ', 29}, {'Ъ', 20}, {'Ы', 24}, {'Ь', 19}, {'Э', 18}, {'Ю', 27}, {'Я', 20}, {'а', 16}, {'б', 17}, {'в', 16}, {'г', 15}, {'д', 17}, {'е', 17}, {'ж', 20}, {'з', 15}, {'и', 16}, {'й', 16}, {'к', 17}, {'л', 15}, {'м', 25}, {'н', 16}, {'о', 16}, {'п', 16}, {'р', 17}, {'с', 16}, {'т', 14}, {'у', 17}, {'ф', 21}, {'х', 15}, {'ц', 17}, {'ч', 15}, {'ш', 25}, {'щ', 27}, {'ъ', 16}, {'ы', 20}, {'ь', 16}, {'э', 14}, {'ю', 23}, {'я', 17}, {'ё', 17}, {'ђ', 17}, {'ѓ', 16}, {'є', 14}, {'ѕ', 16}, {'і', 8}, {'ї', 8}, {'ј', 7}, {'љ', 22}, {'њ', 25}, {'ћ', 17}, {'ќ', 16}, {'ў', 17}, {'џ', 17}, {'Ґ', 15}, {'ґ', 13}, {'–', 15}, {'—', 31}, {'‘', 6}, {'’', 6}, {'‚', 6}, {'“', 12}, {'”', 12}, {'„', 12}, {'†', 20}, {'‡', 20}, {'•', 15}, {'…', 31}, {'‰', 31}, {'‹', 8}, {'›', 8}, {'€', 19}, {'™', 30}, {'−', 18}, {'∙', 8}, {'□', 21}, {'', 40}, {'', 40}, {'', 40}, {'', 40}, {'', 41}, {'', 41}, {'', 32}, {'', 32}, {'', 40}, {'', 40}, {'', 34}, {'', 34}, {'', 40}, {'', 40}, {'', 40}, {'', 41}, {'', 32}, {'', 41}, {'', 32}, {'', 40}, {'', 40}, {'', 40}, {'', 40}, {'', 40}, {'', 40}, {'', 40}, {'', 40}
        };
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

        public static int GetCharWidth(char c)
        {
            if(selectedFont == FONT.MONOSPACE)
            {
                return 24;
            }
            
            int v = 0;
            if (LetterWidths.ContainsKey(c)) return LetterWidths[c];
            return 8;
        }

        public static int GetTextWidth(string text)
        {
            return GetTextWidth(text, 0, text.Length);
        }
        public static int GetTextWidth(string text, int start, int length)
        {
            using (new SimpleProfiler("TextUtils.GetTextWidth(StringBuilder, int, int)"))
            {
                string[] lines = text.Substring(start, length).Split('\n');
                if (start + length > text.Length)
                {
                    throw new Exception("ERROR: stringbuilder slice exceeds the stringbuilders length!");
                }
                text = text.Replace("\r", "");
                int width = 0;
                int lineWidth = 0;
                int v;
                foreach(string line in lines)
                {
                    if (selectedFont == FONT.MONOSPACE)
                    {
                        lineWidth = (line.Length * 25) - 1 - (line.Count(c => c == '\n') * 25);
                    }
                    else
                    {
                        lineWidth = line.Select(c => LetterWidths.GetValueOrDefault(c, 6)).Sum() + line.Length - line.Count(c => c == '\n');
                        /*foreach (char c in line)
                        {
                            v = 6;
                            //switch (c)
                            //{
                            //    case' ': v=8;break;case'!': v=8;break;case'"': v=10;break;case'#': v=19;break;case'$': v=20;break;case'%': v=24;break;case'&': v=20;break;case'\'': v=6;break;case'(': v=9;break;case')': v=9;break;case'*': v=11;break;case'+': v=18;break;case',': v=9;break;case'-': v=10;break;case'.': v=9;break;case'/': v=14;break;case'0': v=19;break;case'1': v=9;break;case'2': v=19;break;case'3': v=17;break;case'4': v=19;break;case'5': v=19;break;case'6': v=19;break;case'7': v=16;break;case'8': v=19;break;case'9': v=19;break;case':': v=9;break;case';':v=9;break;case'<': v=18;break;case'=': v=18;break;case'>': v=18;break;case'?': v=16;break;case'@': v=25;break;case'A': v=21;break;case'B': v=21;break;case'C': v=19;break;case'D': v=21;break;case'E': v=18;break;case'F': v=17;break;case'G': v=20;break;case'H': v=20;break;case'I': v=8;break;case'J': v=16;break;case'K': v=17;break;case'L': v=15;break;case'M': v=26;break;case'N': v=21;break;case'O': v=21;break;case'P': v=20;break;case'Q': v=21;break;case'R': v=21;break;case'S': v=21;break;case'T': v=17;break;case'U': v=20;break;case'V': v=20;break;case'W': v=31;break;case'X': v=19;break;case'Y': v=20;break;case'Z': v=19;break;case'[': v=9;break;case'\\': v=12;break;case']': v=9;break;case'^': v=18;break;case'_': v=15;break;case'`': v=8;break;case'a': v=17;break;case'b': v=17;break;case'c': v=16;break;case'd': v=17;break;case'e': v=17;break;case'f': v=9;break;case'g': v=17;break;case'h': v=17;break;case'i': v=8;break;case'j': v=8;break;case'k': v=17;break;case'l': v=8;break;case'm': v=27;break;case'n': v=17;break;case'o': v=17;break;case'p': v=17;break;case'q': v=17;break;case'r': v=10;break;case's': v=17;break;case't': v=9;break;case'u': v=17;break;case'v': v=15;break;case'w': v=27;break;case'x': v=15;break;case'y': v=17;break;case'z': v=16;break;case'{': v=9;break;case'|': v=6;break;case'}': v=9;break;case'~': v=18;break;case' ': v=8;break;case'¡': v=8;break;case'¢': v=16;break;case'£': v=17;break;case'¤': v=19;break;case'¥': v=19;break;case'¦': v=6;break;case'§': v=20;break;case'¨': v=8;break;case'©': v=25;break;case'ª': v=10;break;case'«': v=15;break;case'¬': v=18;break;case'­': v=10;break;case'®': v=25;break;case'¯': v=8;break;case'°': v=12;break;case'±': v=18;break;case'²': v=11;break;case'³': v=11;break;case'´': v=8;break;case'µ': v=17;break;case'¶': v=18;break;case'·': v=9;break;case'¸': v=8;break;case'¹': v=11;break;case'º': v=10;break;case'»': v=15;break;case'¼': v=27;break;case'½': v=29;break;case'¾': v=28;break;case'¿': v=16;break;case'À': v=21;break;case'Á': v=21;break;case'Â': v=21;break;case'Ã': v=21;break;case'Ä': v=21;break;case'Å': v=21;break;case'Æ': v=31;break;case'Ç': v=19;break;case'È': v=18;break;case'É': v=18;break;case'Ê': v=18;break;case'Ë': v=18;break;case'Ì': v=8;break;case'Í': v=8;break;case'Î': v=8;break;case'Ï': v=8;break;case'Ð': v=21;break;case'Ñ': v=21;break;case'Ò': v=21;break;case'Ó': v=21;break;case'Ô': v=21;break;case'Õ': v=21;break;case'Ö': v=21;break;case'×': v=18;break;case'Ø': v=21;break;case'Ù': v=20;break;case'Ú': v=20;break;case'Û': v=20;break;case'Ü': v=20;break;case'Ý': v=17;break;case'Þ': v=20;break;case'ß': v=19;break;case'à': v=17;break;case'á': v=17;break;case'â': v=17;break;case'ã': v=17;break;case'ä': v=17;break;case'å': v=17;break;case'æ': v=28;break;case'ç': v=16;break;case'è': v=17;break;case'é': v=17;break;case'ê': v=17;break;case'ë': v=17;break;case'ì': v=8;break;case'í': v=8;break;case'î': v=8;break;case'ï': v=8;break;case'ð': v=17;break;case'ñ': v=17;break;case'ò': v=17;break;case'ó': v=17;break;case'ô': v=17;break;case'õ': v=17;break;case'ö': v=17;break;case'÷': v=18;break;case'ø': v=17;break;case'ù': v=17;break;case'ú': v=17;break;case'û': v=17;break;case'ü': v=17;break;case'ý': v=17;break;case'þ': v=17;break;case'ÿ': return 17;break;case'Ā': return 20;break;case'ā': v=17;break;case'Ă': v=21;break;case'ă': v=17;break;case'Ą': v=21;break;case'ą': v=17;break;case'Ć': v=19;break;case'ć': v=16;break;case'Ĉ': v=19;break;case'ĉ': v=16;break;case'Ċ': v=19;break;case'ċ': v=16;break;case'Č': v=19;break;case'č': v=16;break;case'Ď': v=21;break;case'ď': v=17;break;case'Đ': v=21;break;case'đ': v=17;break;case'Ē': v=18;break;case'ē': v=17;break;case'Ĕ': v=18;break;case'ĕ': v=17;break;case'Ė': v=18;break;case'ė': v=17;break;case'Ę': v=18;break;case'ę': v=17;break;case'Ě': v=18;break;case'ě': v=17;break;case'Ĝ': v=20;break;case'ĝ': v=17;break;case'Ğ': v=20;break;case'ğ': v=17;break;case'Ġ': v=20;break;case'ġ': v=17;break;case'Ģ': v=20;break;case'ģ': v=17;break;case'Ĥ': v=20;break;case'ĥ': v=17;break;case'Ħ': v=20;break;case'ħ': v=17;break;case'Ĩ': v=8;break;case'ĩ': v=8;break;case'Ī': v=8;break;case'ī': v=8;break;case'Į': v=8;break;case'į': v=8;break;case'İ': v=8;break;case'ı': v=8;break;case'Ĳ': v=24;break;case'ĳ': v=14;break;case'Ĵ': v=16;break;case'ĵ': v=8;break;case'Ķ': v=17;break;case'ķ': v=17;break;case'Ĺ': v=15;break;case'ĺ': v=8;break;case'Ļ': v=15;break;case'ļ': v=8;break;case'Ľ': v=15;break;case'ľ': v=8;break;case'Ŀ': v=15;break;case'ŀ': v=10;break;case'Ł': v=15;break;case'ł': v=8;break;case'Ń': v=21;break;case'ń': v=17;break;case'Ņ': v=21;break;case'ņ': v=17;break;case'Ň': v=21;break;case'ň': v=17;break;case'ŉ': v=17;break;case'Ō': v=21;break;case'ō': v=17;break;case'Ŏ': v=21;break;case'ŏ': v=17;break;case'Ő': v=21;break;case'ő': v=17;break;case'Œ': v=31;break;case'œ': v=28;break;case'Ŕ': v=21;break;case'ŕ': v=10;break;case'Ŗ': v=21;break;case'ŗ': v=10;break;case'Ř': v=21;break;case'ř': v=10;break;case'Ś': v=21;break;case'ś': v=17;break;case'Ŝ': v=21;break;case'ŝ': v=17;break;case'Ş': v=21;break;case'ş': v=17;break;case'Š': v=21;break;case'š': v=17;break;case'Ţ': v=17;break;case'ţ': v=9;break;case'Ť': v=17;break;case'ť': v=9;break;case'Ŧ': v=17;break;case'ŧ': v=9;break;case'Ũ': v=20;break;case'ũ': v=17;break;case'Ū': v=20;break;case'ū': v=17;break;case'Ŭ': v=20;break;case'ŭ': v=17;break;case'Ů': v=20;break;case'ů': v=17;break;case'Ű': v=20;break;case'ű': v=17;break;case'Ų': v=20;break;case'ų': v=17;break;case'Ŵ': v=31;break;case'ŵ': v=27;break;case'Ŷ': v=17;break;case'ŷ': v=17;break;case'Ÿ': return 17;break;case'Ź': return 19;break;case'ź': v=16;break;case'Ż': v=19;break;case'ż': v=16;break;case'Ž': v=19;break;case'ž': v=16;break;case'ƒ': v=19;break;case'Ș': v=21;break;case'ș': v=17;break;case'Ț': v=17;break;case'ț': v=9;break;case'ˆ': v=8;break;case'ˇ': v=8;break;case'ˉ': v=6;break;case'˘': v=8;break;case'˙': v=8;break;case'˚': v=8;break;case'˛': v=8;break;case'˜': v=8;break;case'˝': v=8;break;case'Ё': v=19;break;case'Ѓ': v=16;break;case'Є': v=18;break;case'Ѕ': v=21;break;case'І': v=8;break;case'Ї': v=8;break;case'Ј': v=16;break;case'Љ': v=28;break;case'Њ': v=21;break;case'Ќ': v=19;break;case'Ў': v=17;break;case'Џ': v=18;break;case'А': v=19;break;case'Б': v=19;break;case'В': v=19;break;case'Г': v=15;break;case'Д': v=19;break;case'Е': v=18;break;case'Ж': v=21;break;case'З': v=17;break;case'И': v=19;break;case'Й': v=19;break;case'К': v=17;break;case'Л': v=17;break;case'М': v=26;break;case'Н': v=18;break;case'О': v=20;break;case'П': v=19;break;case'Р': v=19;break;case'С': v=19;break;case'Т': v=19;break;case'У': v=19;break;case'Ф': v=20;break;case'Х': v=19;break;case'Ц': v=20;break;case'Ч': v=16;break;case'Ш': v=26;break;case'Щ': v=29;break;case'Ъ': v=20;break;case'Ы': v=24;break;case'Ь': v=19;break;case'Э': v=18;break;case'Ю': v=27;break;case'Я': v=20;break;case'а': v=16;break;case'б': v=17;break;case'в': v=16;break;case'г': v=15;break;case'д': v=17;break;case'е': v=17;break;case'ж': v=20;break;case'з': v=15;break;case'и': v=16;break;case'й': v=16;break;case'к': v=17;break;case'л': v=15;break;case'м': v=25;break;case'н': v=16;break;case'о': v=16;break;case'п': v=16;break;case'р': v=17;break;case'с': v=16;break;case'т': v=14;break;case'у': v=17;break;case'ф': v=21;break;case'х': v=15;break;case'ц': v=17;break;case'ч': v=15;break;case'ш': v=25;break;case'щ': v=27;break;case'ъ': v=16;break;case'ы': v=20;break;case'ь': v=16;break;case'э': v=14;break;case'ю': v=23;break;case'я': v=17;break;case'ё': v=17;break;case'ђ': v=17;break;case'ѓ': v=16;break;case'є': v=14;break;case'ѕ': v=16;break;case'і': v=8;break;case'ї': v=8;break;case'ј': v=7;break;case'љ': v=22;break;case'њ': v=25;break;case'ћ': v=17;break;case'ќ': v=16;break;case'ў': v=17;break;case'џ': v=17;break;case'Ґ': v=15;break;case'ґ': v=13;break;case'–': v=15;break;case'—': v=31;break;case'‘': v=6;break;case'’': v=6;break;case'‚': v=6;break;case'“': v=12;break;case'”': v=12;break;case'„': v=12;break;case'†': v=20;break;case'‡': v=20;break;case'•': v=15;break;case'…': v=31;break;case'‰': v=31;break;case'‹': v=8;break;case'›': v=8;break;case'€': v=19;break;case'™': v=30;break;case'−': v=18;break;case'∙': v=8;break;case'□': v=21;break;case'': v=40;break;case'': v=40;break;case'': v=40;break;case'': v=40;break;case'': v=41;break;case'': v=41;break;case'': v=32;break;case'': v=32;break;case'': v=40;break;case'': v=40;break;case'': v=34;break;case'': v=34;break;case'': v=40;break;case'': v=40;break;case'': v=40;break;case'': v=41;break;case'': v=32;break;case'': v=41;break;case'': v=32;break;case'': v=40;break;case'': v=40;break;case'': v=40;break;case'': v=40;break;case'': v=40;break;case'': v=40;break;case'': v=40;break;case'': v=40;break;
                            //    default:
                            //        v=6;
                            //        break;
                            //}
                            if (LetterWidths.ContainsKey(c))
                                v = LetterWidths[c];
                            lineWidth += v + 1;
                        }*/
                    }
                    width = Math.Max(lineWidth - 1, width);
                    lineWidth = 0;
                }
                return Math.Max(width, lineWidth - 1);
            }
        }

        /*public static string RemoveLastTrailingNewline(string text)
        {
            //Logger.debug("TextUtils.RemoveLastTrailingNewline");
            //Logger.IncLvl();
            //Logger.DecLvl();
            return (text.Length > 1 && text[text.Length - 1] == '\n') ? text.Remove(text.Length - 1) : text;
        }

        public static string RemoveFirstTrailingNewline(string text)
        {
            //Logger.debug("TextUtils.RemoveFirstTrailingNewline");
            //Logger.IncLvl();
            //Logger.DecLvl();
            return (text.Length > 1 && text[0] == '\n') ? text.Remove(0) : text;
        }*/

        public static StringBuilder CreateStringOfLength(char constituent, int length)
        {
            return CreateStringOfLength(constituent, length, RoundMode.FLOOR);
        }

        public static StringBuilder CreateStringOfLength(char constituent, int length, RoundMode mode)
        {
            using (new SimpleProfiler("TextUtils.CreateStringOfLength(string, int, RoundMode)"))
            {
                int lengthOfConstituent = GetCharWidth(constituent);
                if (mode == RoundMode.CEIL)
                {
                    //Logger.log("Ceil mode", Logger.Mode.DEBUG);
                    length += lengthOfConstituent;
                }
                StringBuilder result = new StringBuilder();
                if (length < lengthOfConstituent)
                {
                    return new StringBuilder();
                }
                int numberOfChars = (length + 1) / (lengthOfConstituent + 1);
                return new StringBuilder(new string(constituent, numberOfChars));
            }
        }

        public static StringBuilder PadText(string text, int totalWidth, PadMode mode)
        {
            return PadText(text, totalWidth, mode, ' ');
        }

        public static StringBuilder PadText(string text, int totalWidth, PadMode mode, char padChar)
        {
            using (new SimpleProfiler("TextUtils.PadText(StringBuilder, int, PadMode, string)"))
            {
                string[] lines = text.Split('\n');
                StringBuilder result = new StringBuilder();
                StringBuilder padding = new StringBuilder();
                int width;
                int lineStart;
                int lineEnd = -1;
                foreach (string line in lines)
                {
                    width = GetTextWidth(line) + 1;

                    if (mode == PadMode.BOTH)
                    {
                        padding = CreateStringOfLength(padChar, (totalWidth - width) / 2);
                        result.Append(padding);
                        result.Append(line);
                        result.Append(padding);
                    }
                    else
                    {
                        padding = CreateStringOfLength(padChar, totalWidth - width);
                        if (mode == PadMode.LEFT)
                        {
                            result.Append(padding);
                            result.Append(line);
                        }
                        else
                        {
                            result.Append(line);
                            result.Append(padding);
                        }
                    }
                    result.Append("\n");
                }
                /*do
                {
                    lineStart = lineEnd + 1;
                    lineEnd = text.find;
                    StringBuilder s;
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

                    if (mode == PadMode.BOTH)
                    {
                        padding = CreateStringOfLength(padChar, (totalWidth - width) / 2);
                        result.Append(padding);
                        SBExtensions.AppendSubstr(result, text, lineStart, lineEnd - lineStart);
                        result.Append(padding);
                    }
                    else
                    {
                        padding = CreateStringOfLength(padChar, totalWidth - width);
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
                while (lineEnd < text.Length);*/

                if (result.Length > 0)
                    result.Remove(result.Length - 1, 1);
                return result;
            }
        }
    }

    //EMBED SEScripts.Lib.Logger
    //EMBED SEScripts.Lib.SBExtensions
}
