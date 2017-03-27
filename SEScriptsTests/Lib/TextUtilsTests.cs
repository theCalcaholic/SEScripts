using Microsoft.VisualStudio.TestTools.UnitTesting;
using SEScripts.Lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEScripts.Lib.Tests
{
    [TestClass()]
    public class TextUtilsTests
    {

        [TestMethod()]
        public void GetLetterWidthTest()
        {
            var defaultchars = new Dictionary<char, int> {
                    {' ', 8 }, { '!', 8 }, { '"', 10}, {'#', 19}, {'$', 20}, {'%', 24}, {'&', 20}, {'\'', 6}, {'(', 9}, {')', 9}, {'*', 11}, {'+', 18}, {',', 9}, {'-', 10}, {'.', 9}, {'/', 14}, {'0', 19}, {'1', 9}, {'2', 19}, {'3', 17}, {'4', 19}, {'5', 19}, {'6', 19}, {'7', 16}, {'8', 19}, {'9', 19}, {':', 9}, {';', 9}, {'<', 18}, {'=', 18}, {'>', 18}, {'?', 16}, {'@', 25}, {'A', 21}, {'B', 21}, {'C', 19}, {'D', 21}, {'E', 18}, {'F', 17}, {'G', 20}, {'H', 20}, {'I', 8}, {'J', 16}, {'K', 17}, {'L', 15}, {'M', 26}, {'N', 21}, {'O', 21}, {'P', 20}, {'Q', 21}, {'R', 21}, {'S', 21}, {'T', 17}, {'U', 20}, {'V', 20}, {'W', 31}, {'X', 19}, {'Y', 20}, {'Z', 19}, {'[', 9}, {'\\', 12}, {']', 9}, {'^', 18}, {'_', 15}, {'`', 8}, {'a', 17}, {'b', 17}, {'c', 16}, {'d', 17}, {'e', 17}, {'f', 9}, {'g', 17}, {'h', 17}, {'i', 8}, {'j', 8}, {'k', 17}, {'l', 8}, {'m', 27}, {'n', 17}, {'o', 17}, {'p', 17}, {'q', 17}, {'r', 10}, {'s', 17}, {'t', 9}, {'u', 17}, {'v', 15}, {'w', 27}, {'x', 15}, {'y', 17}, {'z', 16}, {'{', 9}, {'|', 6}, {'}', 9}, {'~', 18}, {' ', 8}, {'¡', 8}, {'¢', 16}, {'£', 17}, {'¤', 19}, {'¥', 19}, {'¦', 6}, {'§', 20}, {'¨', 8}, {'©', 25}, {'ª', 10}, {'«', 15}, {'¬', 18}, {'­', 10}, {'®', 25}, {'¯', 8}, {'°', 12}, {'±', 18}, {'²', 11}, {'³', 11}, {'´', 8}, {'µ', 17}, {'¶', 18}, {'·', 9}, {'¸', 8}, {'¹', 11}, {'º', 10}, {'»', 15}, {'¼', 27}, {'½', 29}, {'¾', 28}, {'¿', 16}, {'À', 21}, {'Á', 21}, {'Â', 21}, {'Ã', 21}, {'Ä', 21}, {'Å', 21}, {'Æ', 31}, {'Ç', 19}, {'È', 18}, {'É', 18}, {'Ê', 18}, {'Ë', 18}, {'Ì', 8}, {'Í', 8}, {'Î', 8}, {'Ï', 8}, {'Ð', 21}, {'Ñ', 21}, {'Ò', 21}, {'Ó', 21}, {'Ô', 21}, {'Õ', 21}, {'Ö', 21}, {'×', 18}, {'Ø', 21}, {'Ù', 20}, {'Ú', 20}, {'Û', 20}, {'Ü', 20}, {'Ý', 17}, {'Þ', 20}, {'ß', 19}, {'à', 17}, {'á', 17}, {'â', 17}, {'ã', 17}, {'ä', 17}, {'å', 17}, {'æ', 28}, {'ç', 16}, {'è', 17}, {'é', 17}, {'ê', 17}, {'ë', 17}, {'ì', 8}, {'í', 8}, {'î', 8}, {'ï', 8}, {'ð', 17}, {'ñ', 17}, {'ò', 17}, {'ó', 17}, {'ô', 17}, {'õ', 17}, {'ö', 17}, {'÷', 18}, {'ø', 17}, {'ù', 17}, {'ú', 17}, {'û', 17}, {'ü', 17}, {'ý', 17}, {'þ', 17}, {'ÿ', 17}, {'Ā', 20}, {'ā', 17}, {'Ă', 21}, {'ă', 17}, {'Ą', 21}, {'ą', 17}, {'Ć', 19}, {'ć', 16}, {'Ĉ', 19}, {'ĉ', 16}, {'Ċ', 19}, {'ċ', 16}, {'Č', 19}, {'č', 16}, {'Ď', 21}, {'ď', 17}, {'Đ', 21}, {'đ', 17}, {'Ē', 18}, {'ē', 17}, {'Ĕ', 18}, {'ĕ', 17}, {'Ė', 18}, {'ė', 17}, {'Ę', 18}, {'ę', 17}, {'Ě', 18}, {'ě', 17}, {'Ĝ', 20}, {'ĝ', 17}, {'Ğ', 20}, {'ğ', 17}, {'Ġ', 20}, {'ġ', 17}, {'Ģ', 20}, {'ģ', 17}, {'Ĥ', 20}, {'ĥ', 17}, {'Ħ', 20}, {'ħ', 17}, {'Ĩ', 8}, {'ĩ', 8}, {'Ī', 8}, {'ī', 8}, {'Į', 8}, {'į', 8}, {'İ', 8}, {'ı', 8}, {'Ĳ', 24}, {'ĳ', 14}, {'Ĵ', 16}, {'ĵ', 8}, {'Ķ', 17}, {'ķ', 17}, {'Ĺ', 15}, {'ĺ', 8}, {'Ļ', 15}, {'ļ', 8}, {'Ľ', 15}, {'ľ', 8}, {'Ŀ', 15}, {'ŀ', 10}, {'Ł', 15}, {'ł', 8}, {'Ń', 21}, {'ń', 17}, {'Ņ', 21}, {'ņ', 17}, {'Ň', 21}, {'ň', 17}, {'ŉ', 17}, {'Ō', 21}, {'ō', 17}, {'Ŏ', 21}, {'ŏ', 17}, {'Ő', 21}, {'ő', 17}, {'Œ', 31}, {'œ', 28}, {'Ŕ', 21}, {'ŕ', 10}, {'Ŗ', 21}, {'ŗ', 10}, {'Ř', 21}, {'ř', 10}, {'Ś', 21}, {'ś', 17}, {'Ŝ', 21}, {'ŝ', 17}, {'Ş', 21}, {'ş', 17}, {'Š', 21}, {'š', 17}, {'Ţ', 17}, {'ţ', 9}, {'Ť', 17}, {'ť', 9}, {'Ŧ', 17}, {'ŧ', 9}, {'Ũ', 20}, {'ũ', 17}, {'Ū', 20}, {'ū', 17}, {'Ŭ', 20}, {'ŭ', 17}, {'Ů', 20}, {'ů', 17}, {'Ű', 20}, {'ű', 17}, {'Ų', 20}, {'ų', 17}, {'Ŵ', 31}, {'ŵ', 27}, {'Ŷ', 17}, {'ŷ', 17}, {'Ÿ', 17}, {'Ź', 19}, {'ź', 16}, {'Ż', 19}, {'ż', 16}, {'Ž', 19}, {'ž', 16}, {'ƒ', 19}, {'Ș', 21}, {'ș', 17}, {'Ț', 17}, {'ț', 9}, {'ˆ', 8}, {'ˇ', 8}, {'ˉ', 6}, {'˘', 8}, {'˙', 8}, {'˚', 8}, {'˛', 8}, {'˜', 8}, {'˝', 8}, {'Ё', 19}, {'Ѓ', 16}, {'Є', 18}, {'Ѕ', 21}, {'І', 8}, {'Ї', 8}, {'Ј', 16}, {'Љ', 28}, {'Њ', 21}, {'Ќ', 19}, {'Ў', 17}, {'Џ', 18}, {'А', 19}, {'Б', 19}, {'В', 19}, {'Г', 15}, {'Д', 19}, {'Е', 18}, {'Ж', 21}, {'З', 17}, {'И', 19}, {'Й', 19}, {'К', 17}, {'Л', 17}, {'М', 26}, {'Н', 18}, {'О', 20}, {'П', 19}, {'Р', 19}, {'С', 19}, {'Т', 19}, {'У', 19}, {'Ф', 20}, {'Х', 19}, {'Ц', 20}, {'Ч', 16}, {'Ш', 26}, {'Щ', 29}, {'Ъ', 20}, {'Ы', 24}, {'Ь', 19}, {'Э', 18}, {'Ю', 27}, {'Я', 20}, {'а', 16}, {'б', 17}, {'в', 16}, {'г', 15}, {'д', 17}, {'е', 17}, {'ж', 20}, {'з', 15}, {'и', 16}, {'й', 16}, {'к', 17}, {'л', 15}, {'м', 25}, {'н', 16}, {'о', 16}, {'п', 16}, {'р', 17}, {'с', 16}, {'т', 14}, {'у', 17}, {'ф', 21}, {'х', 15}, {'ц', 17}, {'ч', 15}, {'ш', 25}, {'щ', 27}, {'ъ', 16}, {'ы', 20}, {'ь', 16}, {'э', 14}, {'ю', 23}, {'я', 17}, {'ё', 17}, {'ђ', 17}, {'ѓ', 16}, {'є', 14}, {'ѕ', 16}, {'і', 8}, {'ї', 8}, {'ј', 7}, {'љ', 22}, {'њ', 25}, {'ћ', 17}, {'ќ', 16}, {'ў', 17}, {'џ', 17}, {'Ґ', 15}, {'ґ', 13}, {'–', 15}, {'—', 31}, {'‘', 6}, {'’', 6}, {'‚', 6}, {'“', 12}, {'”', 12}, {'„', 12}, {'†', 20}, {'‡', 20}, {'•', 15}, {'…', 31}, {'‰', 31}, {'‹', 8}, {'›', 8}, {'€', 19}, {'™', 30}, {'−', 18}, {'∙', 8}, {'□', 21}, {'', 40}, {'', 40}, {'', 40}, {'', 40}, {'', 41}, {'', 41}, {'', 32}, {'', 32}, {'', 40}, {'', 40}, {'', 34}, {'', 34}, {'', 40}, {'', 40}, {'', 40}, {'', 41}, {'', 32}, {'', 41}, {'', 32}, {'', 40}, {'', 40}, {'', 40}, {'', 40}, {'', 40}, {'', 40}, {'', 40}, {'', 40}
                };
            TextUtils.SelectFont(TextUtils.FONT.DEFAULT);
            foreach( KeyValuePair<char, int> entry in defaultchars )
            {
                Assert.AreEqual(entry.Value, TextUtils.GetLetterWidth(entry.Key));
            }
            TextUtils.SelectFont(TextUtils.FONT.MONOSPACE);
            foreach (KeyValuePair<char, int> entry in defaultchars )
            {
                Assert.AreEqual(24, TextUtils.GetLetterWidth(entry.Key));
            }
        }

        [TestMethod()]
        public void GetTextWidthTest()
        {
            StringBuilder text = new StringBuilder("aaabbbccc");
            int expectedWidth = 3 * TextUtils.GetLetterWidth('a') + 3 * TextUtils.GetLetterWidth('b') + 3 * TextUtils.GetLetterWidth('c') + 8;
            Assert.AreEqual(expectedWidth, TextUtils.GetTextWidth(text));

            text = new StringBuilder("aaa\nbbb");
            expectedWidth = Math.Max(3 * TextUtils.GetLetterWidth('a') + 2, 3 * TextUtils.GetLetterWidth('b') + 2);
            Assert.AreEqual(expectedWidth, TextUtils.GetTextWidth(text));

            text = new StringBuilder();
            expectedWidth = 0;
            Assert.AreEqual(expectedWidth, TextUtils.GetTextWidth(text));
        }

        [TestMethod()]
        public void GetTextWidthSpecifyingStartAndLengthTest()
        {
            StringBuilder text = new StringBuilder("aaabbbccc");
            int expectedWidth = 2 * TextUtils.GetLetterWidth('a') + 3 * TextUtils.GetLetterWidth('b') + 1 * TextUtils.GetLetterWidth('c') + 5;
            Assert.AreEqual(expectedWidth, TextUtils.GetTextWidth(text, 1, 6));

            text = new StringBuilder("aaa\nbbb");
            expectedWidth = Math.Max(2 * TextUtils.GetLetterWidth('a') + 1, TextUtils.GetLetterWidth('b'));
            Assert.AreEqual(expectedWidth, TextUtils.GetTextWidth(text, 1, 4));
        }

        /*[TestMethod()]
        public void RemoveLastTrailingNewlineTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void RemoveFirstTrailingNewlineTest()
        {
            Assert.Fail();
        }*/

        /*[TestMethod()]
        public void CenterTextTest()
        {
            Assert.Fail();
        }*/

        [TestMethod()]
        public void CreateStringOfLengthWithRoundmodeFloorTest()
        {
            int desiredLength = TextUtils.GetLetterWidth(' ') * 35 + 34;
            Console.WriteLine("space width: " + TextUtils.GetLetterWidth(' '));
            StringBuilder theString = TextUtils.CreateStringOfLength(" ", desiredLength);
            Assert.AreEqual(35, theString.Length); // floor should be default round mode
            theString = TextUtils.CreateStringOfLength(" ", desiredLength, TextUtils.RoundMode.FLOOR);
            Assert.AreEqual(35, theString.Length);

            theString = TextUtils.CreateStringOfLength(" ", desiredLength + 1);
            Assert.AreEqual(35, theString.Length); // floor should be default round mode
            theString = TextUtils.CreateStringOfLength(" ", desiredLength + 1, TextUtils.RoundMode.FLOOR);
            Assert.AreEqual(35, theString.Length);

            theString = TextUtils.CreateStringOfLength(" ", desiredLength - 1);
            Assert.AreEqual(34, theString.Length); // floor should be default round mode
            theString = TextUtils.CreateStringOfLength(" ", desiredLength - 1, TextUtils.RoundMode.FLOOR);
            Assert.AreEqual(34, theString.Length);
        }

        [TestMethod()]
        public void CreateStringOfLengthWithRoundmodeCeilTest()
        {
            int desiredLength = TextUtils.GetLetterWidth(' ') * 35 + 34;
            StringBuilder theString = TextUtils.CreateStringOfLength(" ", desiredLength, TextUtils.RoundMode.CEIL);
            Assert.AreEqual(desiredLength, TextUtils.GetTextWidth(theString));
            
            theString = TextUtils.CreateStringOfLength(" ", desiredLength + 1, TextUtils.RoundMode.CEIL);
            Assert.AreEqual(desiredLength + TextUtils.GetLetterWidth(' ') + 1, TextUtils.GetTextWidth(theString));
        }

        [TestMethod()]
        public void PadTextTest()
        {
            int targetWidth = 201;
            StringBuilder sourceString = new StringBuilder("abcdef");
            StringBuilder paddedString = TextUtils.PadText(sourceString, targetWidth, TextUtils.PadMode.LEFT);
            StringBuilder targetString = TextUtils.CreateStringOfLength(" ", targetWidth - TextUtils.GetTextWidth(sourceString) - 1).Append(sourceString);
            Assert.AreEqual<string>(targetString.ToString(), paddedString.ToString());

            paddedString = TextUtils.PadText(sourceString, targetWidth, TextUtils.PadMode.RIGHT);
            targetString = new StringBuilder(sourceString.ToString())
                .Append(TextUtils.CreateStringOfLength(" ", targetWidth - TextUtils.GetTextWidth(sourceString)));
            Assert.AreEqual<string>(targetString.ToString(), paddedString.ToString());

            paddedString = TextUtils.PadText(sourceString, targetWidth, TextUtils.PadMode.BOTH);
            targetString = TextUtils.CreateStringOfLength(" ", (targetWidth - TextUtils.GetTextWidth(sourceString)) / 2)
                .Append(sourceString)
                .Append(TextUtils.CreateStringOfLength(" ", (targetWidth - TextUtils.GetTextWidth(sourceString)) / 2));
            Assert.AreEqual<string>(targetString.ToString(), paddedString.ToString());

            // test multiline string padding
            sourceString = new StringBuilder("abc\ndef");
            paddedString = TextUtils.PadText(sourceString, targetWidth, TextUtils.PadMode.LEFT);
            targetString = TextUtils.CreateStringOfLength(" ", targetWidth - TextUtils.GetTextWidth(sourceString, 0, 3) - 1)
                .Append("abc\n")
                .Append(TextUtils.CreateStringOfLength(" ", targetWidth - TextUtils.GetTextWidth(sourceString, 4, 3) - 1))
                .Append("def");
            Assert.AreEqual<string>(targetString.ToString(), paddedString.ToString());

            targetWidth = 88;
            sourceString = new StringBuilder();
            paddedString = TextUtils.PadText(sourceString, targetWidth, TextUtils.PadMode.RIGHT);
            targetString = TextUtils.CreateStringOfLength(" ", targetWidth);
            Console.WriteLine("|" + targetString + "|");
            Assert.AreEqual<string>(targetString.ToString(), paddedString.ToString());

            Assert.AreEqual<string>(
                TextUtils.PadText(new StringBuilder(), 88, TextUtils.PadMode.RIGHT).ToString(),
                TextUtils.PadText(new StringBuilder(" "), 88, TextUtils.PadMode.RIGHT).ToString()
                );

            StringBuilder sourceString1 = new StringBuilder();
            StringBuilder sourceString2 = new StringBuilder(" ");
            int length = 101;
            Assert.AreEqual<string>(
                TextUtils.PadText(sourceString1, length, TextUtils.PadMode.RIGHT).ToString(),
                TextUtils.PadText(sourceString2, length, TextUtils.PadMode.RIGHT).ToString()
                );
        }

        [TestMethod()]
        public void PadTextTestWithCustomConstituent()
        {
            int targetWidth = 201;
            StringBuilder sourceString = new StringBuilder("abcdef");
            StringBuilder paddedString = TextUtils.PadText(sourceString, targetWidth, TextUtils.PadMode.LEFT, "...");
            StringBuilder targetString = TextUtils.CreateStringOfLength("...", targetWidth - TextUtils.GetTextWidth(sourceString) - 1).Append(sourceString);
            Assert.AreEqual(targetString.ToString(), paddedString.ToString());

            paddedString = TextUtils.PadText(sourceString, targetWidth, TextUtils.PadMode.RIGHT, "...");
            targetString = new StringBuilder(sourceString.ToString())
                .Append(TextUtils.CreateStringOfLength("...", targetWidth - TextUtils.GetTextWidth(sourceString)));
            Assert.AreEqual(targetString.ToString(), paddedString.ToString());

            paddedString = TextUtils.PadText(sourceString, targetWidth, TextUtils.PadMode.BOTH, "...");
            targetString = TextUtils.CreateStringOfLength("...", (targetWidth - TextUtils.GetTextWidth(sourceString)) / 2)
                .Append(sourceString)
                .Append(TextUtils.CreateStringOfLength("...", (targetWidth - TextUtils.GetTextWidth(sourceString)) / 2));
            Assert.AreEqual(targetString.ToString(), paddedString.ToString());
        }
    }
}