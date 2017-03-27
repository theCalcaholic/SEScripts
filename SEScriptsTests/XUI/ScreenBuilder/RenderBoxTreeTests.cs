using Microsoft.VisualStudio.TestTools.UnitTesting;
using SEScripts.XUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SEScripts.Lib;
using SEScripts.Lib.LoggerNS;
using SEScripts.XUI.BoxRenderer;

namespace SEScripts.XUI.Tests
{
    [TestClass()]
    public class RenderBoxTreeTests
    {
        [TestInitialize()]
        public void Initialize()
        {
            Logger.DEBUG = true;
            TextUtils.Reset();
        }

        [TestMethod()]
        public void MinWidthTest()
        {
            StringBuilder line1 = new StringBuilder("abcdefg");
            StringBuilder line2 = new StringBuilder("alask");
            RenderBoxTree tree = new RenderBoxTree();
            Assert.AreEqual(0, tree.MinWidth);

            tree.Add(line1);
            tree.Add(line2);

            int expectedWith = Math.Max(TextUtils.GetTextWidth(line1), TextUtils.GetTextWidth(line2));
            Assert.AreEqual(expectedWith, tree.MinWidth);
            
            tree.MinWidth = expectedWith - 20;
            Assert.AreEqual(expectedWith, tree.MinWidth);

            expectedWith += 20;

            tree.MinWidth = expectedWith;
            Assert.AreEqual(expectedWith, tree.MinWidth);

            tree.MinWidth = 0;
            tree.Flow = RenderBox.FlowDirection.HORIZONTAL;

            expectedWith = TextUtils.GetTextWidth(new StringBuilder(line1.ToString() + line2.ToString()));
            Assert.AreEqual(expectedWith, tree.MinWidth);

            tree.MinWidth = expectedWith - 20;
            Assert.AreEqual(expectedWith, tree.MinWidth);

            expectedWith += 20;

            tree.MinWidth = expectedWith;
            Assert.AreEqual(expectedWith, tree.MinWidth);

            line1 = new StringBuilder("abcdefg");
            line2 = new StringBuilder("alask");
            tree = new RenderBoxTree();
            tree.Add(line1);
            tree.Add(line2);

            Assert.AreEqual(
                Math.Max(
                    TextUtils.GetTextWidth(tree.GetLine(0)),
                    TextUtils.GetTextWidth(tree.GetLine(1))
                    ), 
                tree.MinWidth);

            tree.Flow = RenderBox.FlowDirection.HORIZONTAL;
            Assert.AreEqual(
                    TextUtils.GetTextWidth(new StringBuilder(
                        tree.GetLine(0).ToString())),
                    tree.MinWidth);


            string leftline1 = "abcde";
            string leftline2 = "f";
            string rightline1 = "ghi";
            string rightline2 = "jkl";
            tree = new RenderBoxTree();
            tree.Flow = RenderBox.FlowDirection.HORIZONTAL;
            RenderBoxTree leftBox = new RenderBoxTree();
            leftBox.Add(leftline1);
            leftBox.Add(leftline2);
            RenderBoxTree rightBox = new RenderBoxTree();
            rightBox.Add(rightline1);
            rightBox.Add(rightline2);

            tree.Add(leftBox);
            tree.Add(rightBox);


            Assert.AreEqual(
                Math.Max(
                    TextUtils.GetTextWidth(leftBox.GetLine(0)),
                    TextUtils.GetTextWidth(leftBox.GetLine(1))
                    ),
                leftBox.MinWidth);
            
            Assert.AreEqual(
                Math.Max(
                    TextUtils.GetTextWidth(rightBox.GetLine(0)),
                    TextUtils.GetTextWidth(rightBox.GetLine(1))
                    ),
                rightBox.MinWidth);

            Assert.AreEqual(
                Math.Max(
                    TextUtils.GetTextWidth(tree.GetLine(0)),
                    TextUtils.GetTextWidth(tree.GetLine(1))
                    ),
                tree.MinWidth);


            Assert.AreEqual(
                leftBox.MinWidth + rightBox.MinWidth + 1,
                tree.MinWidth);

        }

        [TestMethod()]
        public void NodeBoxTreeTest()
        {
            RenderBoxTree tree = new RenderBoxTree();
            Assert.IsNotNull(tree);
            Assert.AreEqual(0, tree.MinHeight);
            Assert.AreEqual(-1, tree.MaxHeight);
            Assert.AreEqual(-1, tree.DesiredHeight);
            Assert.AreEqual(-1, tree.MaxWidth);
            Assert.AreEqual(0, tree.MinWidth);
            Assert.AreEqual(-1, tree.DesiredWidth);
            Assert.AreEqual(RenderBox.TextAlign.LEFT, tree.Align);
            Assert.AreEqual(0, tree.Count);
            Assert.AreEqual(RenderBox.FlowDirection.VERTICAL, tree.Flow);
        }

        [TestMethod()]
        public void AddTest()
        {
            RenderBoxTree tree = new RenderBoxTree();
            Assert.AreEqual(0, tree.Count);
            string childContent = "hello world";
            RenderBoxLeaf child = new RenderBoxLeaf(childContent);
            tree.Add(child);
            Assert.AreEqual(1, tree.Count);
        }

        [TestMethod()]
        public void AddTest1()
        {
            RenderBoxTree tree = new RenderBoxTree();
            Assert.AreEqual(0, tree.Count);
            RenderBoxTree child = new RenderBoxTree();
            tree.Add(child);
            Assert.AreEqual(1, tree.Count);
        }

        [TestMethod()]
        public void GetLineWithFixedWidthTest()
        {
            StringBuilder line1 = new StringBuilder("testabctest");
            StringBuilder line2 = new StringBuilder("someothercontent");

            RenderBoxTree tree = new RenderBoxTree();
            RenderBoxLeaf leaf1 = new RenderBoxLeaf(line1);
            leaf1.MinWidth = TextUtils.GetTextWidth(line1);
            leaf1.MaxWidth = TextUtils.GetTextWidth(line1);
            RenderBoxLeaf leaf2 = new RenderBoxLeaf(line2);
            leaf2.MinWidth = TextUtils.GetTextWidth(line2);
            leaf2.MaxWidth = TextUtils.GetTextWidth(line2);

            tree.Add(leaf1);
            tree.Add(leaf2);
            tree.MinWidth = 0;
            tree.MaxWidth = 0;

            Assert.AreEqual<string>("", tree.GetLine(0).ToString());

            tree.MinWidth = Math.Max(TextUtils.GetTextWidth(line1), TextUtils.GetTextWidth(line2));
            tree.MaxWidth = Math.Max(TextUtils.GetTextWidth(line1), TextUtils.GetTextWidth(line2));
            Assert.AreEqual<string>(
                TextUtils.PadText(line1, TextUtils.GetTextWidth(line2), TextUtils.PadMode.RIGHT).ToString(),
                tree.GetLine(0).ToString());
            Assert.AreEqual<string>(line2.ToString(), tree.GetLine(1).ToString());
            Assert.AreEqual<string>(TextUtils.CreateStringOfLength(" ", tree.MaxWidth).ToString(), tree.GetLine(2).ToString());

            tree.Flow = RenderBox.FlowDirection.HORIZONTAL;
            tree.MinWidth = TextUtils.GetTextWidth(line1) + TextUtils.GetTextWidth(line2) + 1;
            tree.MaxWidth = TextUtils.GetTextWidth(line1) + TextUtils.GetTextWidth(line2) + 1;

            Assert.AreEqual<string>(line1.ToString() + line2, tree.GetLine(0).ToString());
            Console.WriteLine("testing empty line, horizontal");
            Assert.AreEqual<string>(TextUtils.CreateStringOfLength(" ", tree.MaxWidth).ToString(), tree.GetLine(1).ToString());

            string leftline1 = "abcde";
            string leftline2 = "f";
            string rightline1 = "ghi";
            string rightline2 = "jkl";
            tree = new RenderBoxTree();
            tree.Flow = RenderBox.FlowDirection.HORIZONTAL;
            RenderBoxTree leftBox = new RenderBoxTree();
            leftBox.Add(leftline1);
            leftBox.Add(leftline2);
            leftBox.MinWidth = Math.Max(TextUtils.GetTextWidth(new StringBuilder(leftline1)), TextUtils.GetTextWidth(new StringBuilder(leftline2)));
            leftBox.MaxWidth = Math.Max(TextUtils.GetTextWidth(new StringBuilder(leftline1)), TextUtils.GetTextWidth(new StringBuilder(leftline2)));
            RenderBoxTree rightBox = new RenderBoxTree();
            rightBox.Add(rightline1);
            rightBox.Add(rightline2);
            rightBox.MinWidth = Math.Max(TextUtils.GetTextWidth(new StringBuilder(rightline1)), TextUtils.GetTextWidth(new StringBuilder(rightline2)));
            rightBox.MaxWidth = Math.Max(TextUtils.GetTextWidth(new StringBuilder(rightline1)), TextUtils.GetTextWidth(new StringBuilder(rightline2)));

            tree.Add(leftBox);
            tree.Add(rightBox);
            tree.MinWidth = leftBox.MaxWidth + rightBox.MaxWidth + 1;
            tree.MaxWidth = leftBox.MaxWidth + rightBox.MaxWidth + 1;

            Assert.AreEqual<string>(leftline1 + rightline1, tree.GetLine(0).ToString());
            string expected =
                TextUtils.PadText(new StringBuilder(leftline2),
                Math.Max(
                    TextUtils.GetTextWidth(new StringBuilder(leftline1)),
                    TextUtils.GetTextWidth(new StringBuilder(leftline2))
                ), TextUtils.PadMode.RIGHT).ToString()
                + TextUtils.PadText(new StringBuilder(rightline2),
                    Math.Max(
                        TextUtils.GetTextWidth(new StringBuilder(rightline1)),
                        TextUtils.GetTextWidth(new StringBuilder(rightline2))
                    ), TextUtils.PadMode.RIGHT);
            Console.WriteLine("Test runs till here...");
            Assert.AreEqual<string>(expected,
                tree.GetLine(1).ToString());
            //Assert.AreEqual<string>(TextUtils.CreateStringOfLength(" ", tree.Width - 1).ToString(), tree.GetLine(2).ToString());
        }

        [TestMethod()]
        public void GetLineWithDynamicWidthTest()
        {
            StringBuilder line1 = new StringBuilder("testabctest");
            StringBuilder line2 = new StringBuilder("someothercontent");

            RenderBoxTree tree = new RenderBoxTree();
            RenderBoxLeaf leaf1 = new RenderBoxLeaf(line1);
            RenderBoxLeaf leaf2 = new RenderBoxLeaf(line2);

            tree.Add(leaf1);
            tree.Add(leaf2);

            Console.WriteLine("Test first line vertical no fixed width.");
            Assert.AreEqual<string>(
                TextUtils.PadText(line1, TextUtils.GetTextWidth(line2), TextUtils.PadMode.RIGHT).ToString(),
                tree.GetLine(0).ToString());
            Console.WriteLine("Test second line vertical no fixed width.");
            Assert.AreEqual<string>(line2.ToString(), tree.GetLine(1).ToString());
            Assert.AreEqual<string>(TextUtils.CreateStringOfLength(" ", tree.MinWidth).ToString(), tree.GetLine(2).ToString());

            tree.Flow = RenderBox.FlowDirection.HORIZONTAL;

            Assert.AreEqual<string>(line1.ToString() + line2, tree.GetLine(0).ToString());
            Console.WriteLine("testing empty line, horizontal");
            Assert.AreEqual<string>(TextUtils.CreateStringOfLength(" ", tree.MinWidth).ToString(), tree.GetLine(1).ToString());

            string leftline1 = "abcde";
            string leftline2 = "f";
            string rightline1 = "ghi";
            string rightline2 = "jkl";
            tree = new RenderBoxTree();
            tree.Flow = RenderBox.FlowDirection.HORIZONTAL;
            RenderBoxTree leftBox = new RenderBoxTree();
            leftBox.Add(leftline1);
            leftBox.Add(leftline2);
            RenderBoxTree rightBox = new RenderBoxTree();
            rightBox.Add(rightline1);
            rightBox.Add(rightline2);

            tree.Add(leftBox);
            tree.Add(rightBox);

            Assert.AreEqual<string>(leftline1 + rightline1, tree.GetLine(0).ToString());

            string expected =
                TextUtils.PadText(new StringBuilder(leftline2),
                Math.Max(
                    TextUtils.GetTextWidth(new StringBuilder(leftline1)),
                    TextUtils.GetTextWidth(new StringBuilder(leftline2))
                ), TextUtils.PadMode.RIGHT).ToString()
                + TextUtils.PadText(new StringBuilder(rightline2),
                    Math.Max(
                        TextUtils.GetTextWidth(new StringBuilder(rightline1)),
                        TextUtils.GetTextWidth(new StringBuilder(rightline2))
                    ), TextUtils.PadMode.RIGHT);
            Assert.AreEqual<string>(
                expected,
                tree.GetLine(1).ToString());

            Console.WriteLine("\nTest runs til here");
            Assert.AreEqual<string>(
                TextUtils.PadText(new StringBuilder(" "), tree.MinWidth, TextUtils.PadMode.RIGHT).ToString(),
                tree.GetLine(2).ToString());
        }

        [TestMethod()]
        public void GetLineWithFixedMinWidthTest()
        {
            StringBuilder line1 = new StringBuilder("testabctest");
            StringBuilder line2 = new StringBuilder("someothercontent");

            RenderBoxTree tree = new RenderBoxTree();
            RenderBoxLeaf leaf1 = new RenderBoxLeaf(line1);
            RenderBoxLeaf leaf2 = new RenderBoxLeaf(line2);

            tree.Add(leaf1);
            tree.Add(leaf2);
            tree.MinWidth = tree.MinWidth - 50;
            
            Assert.AreEqual<string>(
                TextUtils.PadText(line1, TextUtils.GetTextWidth(line2), TextUtils.PadMode.RIGHT).ToString(),
                tree.GetLine(0).ToString());
            Assert.AreEqual<string>(line2.ToString(), tree.GetLine(1).ToString());

            int customMinWidth = tree.MinWidth + 100;
            tree.MinWidth = customMinWidth;

            Assert.AreEqual<string>(
                TextUtils.PadText(line1, customMinWidth, TextUtils.PadMode.RIGHT).ToString(),
                tree.GetLine(0).ToString());
            Assert.AreEqual<string>(
                TextUtils.PadText(line2, customMinWidth, TextUtils.PadMode.RIGHT).ToString(),
                tree.GetLine(1).ToString());
            Assert.AreEqual<string>(TextUtils.CreateStringOfLength(" ", customMinWidth).ToString(), tree.GetLine(2).ToString());

            tree.Flow = RenderBox.FlowDirection.HORIZONTAL;
            tree.MinWidth = tree.MinWidth - 50;

            Console.WriteLine("Test runs until here.");
            Assert.AreEqual<string>(line1.ToString() + line2, tree.GetLine(0).ToString());
            Assert.AreEqual<string>(
                TextUtils.CreateStringOfLength(" ", TextUtils.GetTextWidth(new StringBuilder(line1.ToString() + line2.ToString()))).ToString(),
                tree.GetLine(1).ToString());

            customMinWidth = tree.MinWidth + 100;
            tree.MinWidth = customMinWidth;

            Assert.AreEqual<string>(TextUtils.PadText(new StringBuilder(line1.ToString() + line2), customMinWidth, TextUtils.PadMode.RIGHT).ToString(),
                tree.GetLine(0).ToString());
            Assert.AreEqual<string>(TextUtils.PadText(new StringBuilder(), customMinWidth, TextUtils.PadMode.RIGHT).ToString(), tree.GetLine(1).ToString());

            string leftline1 = "abcde";
            string leftline2 = "f";
            string rightline1 = "ghi";
            string rightline2 = "jkl";
            tree = new RenderBoxTree();
            tree.Flow = RenderBox.FlowDirection.HORIZONTAL;
            RenderBoxTree leftBox = new RenderBoxTree();
            leftBox.Add(leftline1);
            leftBox.Add(leftline2);
            RenderBoxTree rightBox = new RenderBoxTree();
            rightBox.Add(rightline1);
            rightBox.Add(rightline2);

            tree.Add(leftBox);
            tree.Add(rightBox);

            customMinWidth = tree.MinWidth + 100;
            tree.MinWidth = customMinWidth;

            Assert.AreEqual<string>(
                TextUtils.PadText(new StringBuilder(leftline1 + rightline1), customMinWidth, TextUtils.PadMode.RIGHT).ToString(),
                tree.GetLine(0).ToString());
            Assert.AreEqual<string>(
                TextUtils.PadText(new StringBuilder(leftline2), leftBox.MinWidth, TextUtils.PadMode.RIGHT).ToString()
                + TextUtils.PadText(new StringBuilder(rightline2), Math.Max(rightBox.MinWidth, tree.MinWidth - leftBox.MinWidth), TextUtils.PadMode.RIGHT),
                tree.GetLine(1).ToString());
            Assert.AreEqual<string>(TextUtils.CreateStringOfLength(" ", customMinWidth).ToString(), tree.GetLine(2).ToString());
        }

        [TestMethod()]
        public void FTest()
        {
            string left1 = "abc";
            string left2 = "d";
            string right1 = "ef";
            string right2 = "ghi";
            string right3 = "jklm";

            RenderBoxTree frame = new RenderBoxTree();
            TextUtils.SelectFont(TextUtils.FONT.MONOSPACE);

            RenderBoxLeaf leftLeaf1 = new RenderBoxLeaf(left1);
            leftLeaf1.MinWidth = TextUtils.GetTextWidth(new StringBuilder(left1));
            leftLeaf1.MaxWidth = TextUtils.GetTextWidth(new StringBuilder(left1));
            RenderBoxLeaf leftLeaf2 = new RenderBoxLeaf(left2);
            leftLeaf2.MinWidth = TextUtils.GetTextWidth(new StringBuilder(left2));
            leftLeaf2.MaxWidth = TextUtils.GetTextWidth(new StringBuilder(left2));
            RenderBoxLeaf rightLeaf1 = new RenderBoxLeaf(right1);
            rightLeaf1.MinWidth = TextUtils.GetTextWidth(new StringBuilder(right1));
            rightLeaf1.MaxWidth = TextUtils.GetTextWidth(new StringBuilder(right1));
            RenderBoxLeaf rightLeaf2 = new RenderBoxLeaf(right2);
            rightLeaf2.MinWidth = TextUtils.GetTextWidth(new StringBuilder(right2));
            rightLeaf2.MaxWidth = TextUtils.GetTextWidth(new StringBuilder(right2));
            RenderBoxLeaf rightLeaf3 = new RenderBoxLeaf(right3);
            rightLeaf3.MinWidth = TextUtils.GetTextWidth(new StringBuilder(right3));
            rightLeaf3.MaxWidth = TextUtils.GetTextWidth(new StringBuilder(right3));

            RenderBoxTree leftBox = new RenderBoxTree();
            RenderBoxTree rightBox = new RenderBoxTree();

            leftBox.Add(leftLeaf1);
            leftBox.Add(leftLeaf2);
            leftBox.MinWidth = Math.Max(leftLeaf1.MaxWidth, leftLeaf2.MaxWidth);
            leftBox.MaxWidth = Math.Max(leftLeaf1.MaxWidth, leftLeaf2.MaxWidth);
            rightBox.Add(rightLeaf1);
            rightBox.Add(rightLeaf2);
            rightBox.Add(rightLeaf3);
            rightBox.MinWidth = Math.Max(rightLeaf1.MaxWidth, Math.Max(rightLeaf2.MaxWidth, rightLeaf3.MaxWidth));
            rightBox.MaxWidth = Math.Max(rightLeaf1.MaxWidth, Math.Max(rightLeaf2.MaxWidth, rightLeaf3.MaxWidth));
            rightBox.Align = RenderBox.TextAlign.RIGHT;

            frame.Add(leftBox);
            frame.Add(rightBox);
            frame.MinWidth = leftBox.MaxWidth + 1 + rightBox.MaxWidth;
            frame.MaxWidth = leftBox.MaxWidth + 1 + rightBox.MaxWidth;
            frame.Flow = RenderBox.FlowDirection.HORIZONTAL;
            
            foreach(StringBuilder line in frame.GetLines())
            {
                Console.WriteLine(line);
            }
        }

        [TestMethod()]
        public void ClearTest()
        {
            RenderBoxTree tree = new RenderBoxTree();
            tree.Add("test");
            tree.Add(new RenderBoxTree());
            Assert.AreEqual(2, tree.Count);
            tree.Clear();
            Assert.AreEqual(0, tree.Count);
        }
        
    }
}