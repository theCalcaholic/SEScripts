using Microsoft.VisualStudio.TestTools.UnitTesting;
using SEScripts.XUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SEScripts.Lib;
using SEScripts.Lib.LoggerNS;

namespace SEScripts.XUI.Tests
{
    [TestClass()]
    public class NodeBoxTreeTests
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
            NodeBoxTree tree = new NodeBoxTree();
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
            tree.Flow = NodeBox.FlowDirection.HORIZONTAL;

            expectedWith = TextUtils.GetTextWidth(new StringBuilder(line1.ToString() + line2.ToString()));
            Assert.AreEqual(expectedWith, tree.MinWidth);

            tree.MinWidth = expectedWith - 20;
            Assert.AreEqual(expectedWith, tree.MinWidth);

            expectedWith += 20;

            tree.MinWidth = expectedWith;
            Assert.AreEqual(expectedWith, tree.MinWidth);

            line1 = new StringBuilder("abcdefg");
            line2 = new StringBuilder("alask");
            tree = new NodeBoxTree();
            tree.Add(line1);
            tree.Add(line2);

            Assert.AreEqual(
                Math.Max(
                    TextUtils.GetTextWidth(tree.GetLine(0)),
                    TextUtils.GetTextWidth(tree.GetLine(1))
                    ), 
                tree.MinWidth);

            tree.Flow = NodeBox.FlowDirection.HORIZONTAL;
            Assert.AreEqual(
                    TextUtils.GetTextWidth(new StringBuilder(
                        tree.GetLine(0).ToString())),
                    tree.MinWidth);


            string leftline1 = "abcde";
            string leftline2 = "f";
            string rightline1 = "ghi";
            string rightline2 = "jkl";
            tree = new NodeBoxTree();
            tree.Flow = NodeBox.FlowDirection.HORIZONTAL;
            NodeBoxTree leftBox = new NodeBoxTree();
            leftBox.Add(leftline1);
            leftBox.Add(leftline2);
            NodeBoxTree rightBox = new NodeBoxTree();
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
            NodeBoxTree tree = new NodeBoxTree();
            Assert.IsNotNull(tree);
            Assert.AreEqual(0, tree.Height);
            Assert.AreEqual(-1, tree.ForcedWidth);
            Assert.AreEqual(-1, tree.MaxWidth);
            Assert.AreEqual(0, tree.MinWidth);
            Assert.AreEqual(-1, tree.DesiredWidth);
            Assert.AreEqual(NodeBox.TextAlign.LEFT, tree.Align);
            Assert.AreEqual(0, tree.Count);
            Assert.AreEqual(NodeBox.FlowDirection.VERTICAL, tree.Flow);
        }

        [TestMethod()]
        public void AddTest()
        {
            NodeBoxTree tree = new NodeBoxTree();
            Assert.AreEqual(0, tree.Count);
            string childContent = "hello world";
            NodeBoxLeaf child = new NodeBoxLeaf(childContent);
            tree.Add(child);
            Assert.AreEqual(1, tree.Count);
        }

        [TestMethod()]
        public void AddTest1()
        {
            NodeBoxTree tree = new NodeBoxTree();
            Assert.AreEqual(0, tree.Count);
            NodeBoxTree child = new NodeBoxTree();
            tree.Add(child);
            Assert.AreEqual(1, tree.Count);
        }

        [TestMethod()]
        public void GetLineWithFixedWidthTest()
        {
            StringBuilder line1 = new StringBuilder("testabctest");
            StringBuilder line2 = new StringBuilder("someothercontent");

            NodeBoxTree tree = new NodeBoxTree();
            NodeBoxLeaf leaf1 = new NodeBoxLeaf(line1);
            leaf1.ForcedWidth = TextUtils.GetTextWidth(line1);
            NodeBoxLeaf leaf2 = new NodeBoxLeaf(line2);
            leaf2.ForcedWidth = TextUtils.GetTextWidth(line2);

            tree.Add(leaf1);
            tree.Add(leaf2);
            tree.ForcedWidth = 0;

            Assert.AreEqual<string>("", tree.GetLine(0).ToString());

            tree.ForcedWidth = Math.Max(TextUtils.GetTextWidth(line1), TextUtils.GetTextWidth(line2));
            Assert.AreEqual<string>(
                TextUtils.PadText(line1, TextUtils.GetTextWidth(line2), TextUtils.PadMode.RIGHT).ToString(),
                tree.GetLine(0).ToString());
            Assert.AreEqual<string>(line2.ToString(), tree.GetLine(1).ToString());
            Assert.AreEqual<string>(TextUtils.CreateStringOfLength(" ", tree.ForcedWidth).ToString(), tree.GetLine(2).ToString());

            tree.Flow = NodeBox.FlowDirection.HORIZONTAL;
            tree.ForcedWidth = TextUtils.GetTextWidth(line1) + TextUtils.GetTextWidth(line2) + 1;

            Assert.AreEqual<string>(line1.ToString() + line2, tree.GetLine(0).ToString());
            Console.WriteLine("testing empty line, horizontal");
            Assert.AreEqual<string>(TextUtils.CreateStringOfLength(" ", tree.ForcedWidth).ToString(), tree.GetLine(1).ToString());

            string leftline1 = "abcde";
            string leftline2 = "f";
            string rightline1 = "ghi";
            string rightline2 = "jkl";
            tree = new NodeBoxTree();
            tree.Flow = NodeBox.FlowDirection.HORIZONTAL;
            NodeBoxTree leftBox = new NodeBoxTree();
            leftBox.Add(leftline1);
            leftBox.Add(leftline2);
            leftBox.ForcedWidth = Math.Max(TextUtils.GetTextWidth(new StringBuilder(leftline1)), TextUtils.GetTextWidth(new StringBuilder(leftline2)));
            NodeBoxTree rightBox = new NodeBoxTree();
            rightBox.Add(rightline1);
            rightBox.Add(rightline2);
            rightBox.ForcedWidth = Math.Max(TextUtils.GetTextWidth(new StringBuilder(rightline1)), TextUtils.GetTextWidth(new StringBuilder(rightline2)));

            tree.Add(leftBox);
            tree.Add(rightBox);
            tree.ForcedWidth = leftBox.ForcedWidth + rightBox.ForcedWidth + 1;

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

            NodeBoxTree tree = new NodeBoxTree();
            NodeBoxLeaf leaf1 = new NodeBoxLeaf(line1);
            NodeBoxLeaf leaf2 = new NodeBoxLeaf(line2);

            tree.Add(leaf1);
            tree.Add(leaf2);

            Console.WriteLine("Test first line vertical no fixed width.");
            Assert.AreEqual<string>(
                TextUtils.PadText(line1, TextUtils.GetTextWidth(line2), TextUtils.PadMode.RIGHT).ToString(),
                tree.GetLine(0).ToString());
            Console.WriteLine("Test second line vertical no fixed width.");
            Assert.AreEqual<string>(line2.ToString(), tree.GetLine(1).ToString());
            Assert.AreEqual<string>(TextUtils.CreateStringOfLength(" ", tree.MinWidth).ToString(), tree.GetLine(2).ToString());

            tree.Flow = NodeBox.FlowDirection.HORIZONTAL;

            Assert.AreEqual<string>(line1.ToString() + line2, tree.GetLine(0).ToString());
            Console.WriteLine("testing empty line, horizontal");
            Assert.AreEqual<string>(TextUtils.CreateStringOfLength(" ", tree.MinWidth).ToString(), tree.GetLine(1).ToString());

            string leftline1 = "abcde";
            string leftline2 = "f";
            string rightline1 = "ghi";
            string rightline2 = "jkl";
            tree = new NodeBoxTree();
            tree.Flow = NodeBox.FlowDirection.HORIZONTAL;
            NodeBoxTree leftBox = new NodeBoxTree();
            leftBox.Add(leftline1);
            leftBox.Add(leftline2);
            NodeBoxTree rightBox = new NodeBoxTree();
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

            NodeBoxTree tree = new NodeBoxTree();
            NodeBoxLeaf leaf1 = new NodeBoxLeaf(line1);
            NodeBoxLeaf leaf2 = new NodeBoxLeaf(line2);

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

            tree.Flow = NodeBox.FlowDirection.HORIZONTAL;
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
            tree = new NodeBoxTree();
            tree.Flow = NodeBox.FlowDirection.HORIZONTAL;
            NodeBoxTree leftBox = new NodeBoxTree();
            leftBox.Add(leftline1);
            leftBox.Add(leftline2);
            NodeBoxTree rightBox = new NodeBoxTree();
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

            NodeBoxTree frame = new NodeBoxTree();
            TextUtils.SelectFont(TextUtils.FONT.MONOSPACE);

            NodeBoxLeaf leftLeaf1 = new NodeBoxLeaf(left1);
            leftLeaf1.ForcedWidth = TextUtils.GetTextWidth(new StringBuilder(left1));
            NodeBoxLeaf leftLeaf2 = new NodeBoxLeaf(left2);
            leftLeaf2.ForcedWidth = TextUtils.GetTextWidth(new StringBuilder(left2));
            NodeBoxLeaf rightLeaf1 = new NodeBoxLeaf(right1);
            rightLeaf1.ForcedWidth = TextUtils.GetTextWidth(new StringBuilder(right1));
            NodeBoxLeaf rightLeaf2 = new NodeBoxLeaf(right2);
            rightLeaf2.ForcedWidth = TextUtils.GetTextWidth(new StringBuilder(right2));
            NodeBoxLeaf rightLeaf3 = new NodeBoxLeaf(right3);
            rightLeaf3.ForcedWidth = TextUtils.GetTextWidth(new StringBuilder(right3));

            NodeBoxTree leftBox = new NodeBoxTree();
            NodeBoxTree rightBox = new NodeBoxTree();

            leftBox.Add(leftLeaf1);
            leftBox.Add(leftLeaf2);
            leftBox.ForcedWidth = Math.Max(leftLeaf1.ForcedWidth, leftLeaf2.ForcedWidth);
            rightBox.Add(rightLeaf1);
            rightBox.Add(rightLeaf2);
            rightBox.Add(rightLeaf3);
            rightBox.ForcedWidth = Math.Max(rightLeaf1.ForcedWidth, Math.Max(rightLeaf2.ForcedWidth, rightLeaf3.ForcedWidth));
            rightBox.Align = NodeBox.TextAlign.RIGHT;

            frame.Add(leftBox);
            frame.Add(rightBox);
            frame.ForcedWidth = leftBox.ForcedWidth + 1 + rightBox.ForcedWidth;
            frame.Flow = NodeBox.FlowDirection.HORIZONTAL;
            
            foreach(StringBuilder line in frame.GetLines())
            {
                Console.WriteLine(line);
            }
        }

        [TestMethod()]
        public void ClearTest()
        {
            NodeBoxTree tree = new NodeBoxTree();
            tree.Add("test");
            tree.Add(new NodeBoxTree());
            Assert.AreEqual(2, tree.Count);
            tree.Clear();
            Assert.AreEqual(0, tree.Count);
        }
        
    }
}