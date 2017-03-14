using System;
using System.Text;
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

namespace SEScripts.Lib
{
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
                Console.WriteLine("Ceil mode");
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
                    result.AppendStringBuilder(padding);
                    result.AppendSubstring(text, lineStart, lineEnd - lineStart);
                    result.AppendStringBuilder(padding);
                }
                else
                {
                    padding = CreateStringOfLength(padString, totalWidth - width);
                    if (mode == PadMode.LEFT)
                    {
                        result.AppendStringBuilder(padding);
                        result.AppendSubstring(text, lineStart, lineEnd - lineStart);
                    }
                    else
                    {
                        result.AppendSubstring(text, lineStart, lineEnd - lineStart);
                        result.AppendStringBuilder(padding);
                    }
                }
                result.Append("\n");
            }
            while (lineEnd < text.Length);

            result.TrimEnd(1);
            Logger.DecLvl();
            return result;
        }
    }

    //EMBED SEScripts.Lib.Logger
}
