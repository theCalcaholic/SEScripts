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
            int desiredLength = TextUtils.GetTextWidth(" ") * 35 + 34;
            //Console.WriteLine("space width: " + TextUtils.GetLetterWidth(' '));
            StringBuilder theString = TextUtils.CreateStringOfLength(' ', desiredLength);
            Assert.AreEqual(35, theString.Length); // floor should be default round mode
            theString = TextUtils.CreateStringOfLength(' ', desiredLength, TextUtils.RoundMode.FLOOR);
            Assert.AreEqual(35, theString.Length);

            theString = TextUtils.CreateStringOfLength(' ', desiredLength + 1);
            Assert.AreEqual(35, theString.Length); // floor should be default round mode
            theString = TextUtils.CreateStringOfLength(' ', desiredLength + 1, TextUtils.RoundMode.FLOOR);
            Assert.AreEqual(35, theString.Length);

            theString = TextUtils.CreateStringOfLength(' ', desiredLength - 1);
            Assert.AreEqual(34, theString.Length); // floor should be default round mode
            theString = TextUtils.CreateStringOfLength(' ', desiredLength - 1, TextUtils.RoundMode.FLOOR);
            Assert.AreEqual(34, theString.Length);
        }

        [TestMethod()]
        public void CreateStringOfLengthWithRoundmodeCeilTest()
        {
            int desiredLength = TextUtils.GetTextWidth(" ") * 35 + 34;
            Console.WriteLine("desired text length: " + desiredLength);
            StringBuilder theString = TextUtils.CreateStringOfLength(' ', desiredLength, TextUtils.RoundMode.CEIL);
            Assert.AreEqual(desiredLength, TextUtils.GetTextWidth(theString.ToString()));
            
            theString = TextUtils.CreateStringOfLength(' ', desiredLength + 1, TextUtils.RoundMode.CEIL);
            Assert.AreEqual(desiredLength + TextUtils.GetTextWidth(" ") + 1, TextUtils.GetTextWidth(theString.ToString()));
        }

        [TestMethod()]
        public void PadTextTest()
        {
            int targetWidth = 201;
            StringBuilder sourceString = new StringBuilder("abcdef");
            StringBuilder paddedString = TextUtils.PadText(sourceString.ToString(), targetWidth, TextUtils.PadMode.LEFT);
            StringBuilder targetString = TextUtils.CreateStringOfLength(' ', targetWidth - TextUtils.GetTextWidth(sourceString.ToString()) - 1).Append(sourceString);
            Assert.AreEqual<string>(targetString.ToString(), paddedString.ToString());

            paddedString = TextUtils.PadText(sourceString.ToString(), targetWidth, TextUtils.PadMode.RIGHT);
            targetString = new StringBuilder(sourceString.ToString())
                .Append(TextUtils.CreateStringOfLength(' ', targetWidth - TextUtils.GetTextWidth(sourceString.ToString())));
            Assert.AreEqual<string>(targetString.ToString(), paddedString.ToString());

            paddedString = TextUtils.PadText(sourceString.ToString(), targetWidth, TextUtils.PadMode.BOTH);
            targetString = TextUtils.CreateStringOfLength(' ', (targetWidth - TextUtils.GetTextWidth(sourceString.ToString())) / 2)
                .Append(sourceString)
                .Append(TextUtils.CreateStringOfLength(' ', (targetWidth - TextUtils.GetTextWidth(sourceString.ToString())) / 2));
            Assert.AreEqual<string>(targetString.ToString(), paddedString.ToString());

            // test multiline string padding
            sourceString = new StringBuilder("abc\ndef");
            paddedString = TextUtils.PadText(sourceString.ToString(), targetWidth, TextUtils.PadMode.LEFT);
            targetString = TextUtils.CreateStringOfLength(' ', targetWidth - TextUtils.GetTextWidth(sourceString.ToString(), 0, 3) - 1)
                .Append("abc\n")
                .Append(TextUtils.CreateStringOfLength(' ', targetWidth - TextUtils.GetTextWidth(sourceString.ToString(), 4, 3) - 1))
                .Append("def");
            Assert.AreEqual<string>(targetString.ToString(), paddedString.ToString());

            targetWidth = 88;
            sourceString = new StringBuilder();
            paddedString = TextUtils.PadText(sourceString.ToString(), targetWidth, TextUtils.PadMode.RIGHT);
            targetString = TextUtils.CreateStringOfLength(' ', targetWidth);
            Console.WriteLine("|" + targetString + "|");
            Assert.AreEqual<string>(targetString.ToString(), paddedString.ToString());

            Assert.AreEqual<string>(
                TextUtils.PadText("", 88, TextUtils.PadMode.RIGHT).ToString(),
                TextUtils.PadText(" ", 88, TextUtils.PadMode.RIGHT).ToString()
                );

            StringBuilder sourceString1 = new StringBuilder();
            StringBuilder sourceString2 = new StringBuilder(" ");
            int length = 101;
            Assert.AreEqual<string>(
                TextUtils.PadText(sourceString1.ToString(), length, TextUtils.PadMode.RIGHT).ToString(),
                TextUtils.PadText(sourceString2.ToString(), length, TextUtils.PadMode.RIGHT).ToString()
                );
        }

        [TestMethod()]
        public void PadTextTestWithCustomConstituent()
        {
            int targetWidth = 201;
            StringBuilder sourceString = new StringBuilder("abcdef");
            StringBuilder paddedString = TextUtils.PadText(sourceString.ToString(), targetWidth, TextUtils.PadMode.LEFT, '.');
            StringBuilder targetString = TextUtils.CreateStringOfLength('.', targetWidth - TextUtils.GetTextWidth(sourceString.ToString()) - 1).Append(sourceString);
            Assert.AreEqual(targetString.ToString(), paddedString.ToString());

            paddedString = TextUtils.PadText(sourceString.ToString(), targetWidth, TextUtils.PadMode.RIGHT, '.');
            targetString = new StringBuilder(sourceString.ToString())
                .Append(TextUtils.CreateStringOfLength('.', targetWidth - TextUtils.GetTextWidth(sourceString.ToString())));
            Assert.AreEqual(targetString.ToString(), paddedString.ToString());

            paddedString = TextUtils.PadText(sourceString.ToString(), targetWidth, TextUtils.PadMode.BOTH, '.');
            targetString = TextUtils.CreateStringOfLength('.', (targetWidth - TextUtils.GetTextWidth(sourceString.ToString())) / 2)
                .Append(sourceString)
                .Append(TextUtils.CreateStringOfLength('.', (targetWidth - TextUtils.GetTextWidth(sourceString.ToString())) / 2));
            Assert.AreEqual(targetString.ToString(), paddedString.ToString());
        }
    }
}