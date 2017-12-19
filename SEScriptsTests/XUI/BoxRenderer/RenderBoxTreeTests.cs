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
            //Logger.debug = true;
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
            int expectedWidth = Math.Max(TextUtils.GetTextWidth(line1.ToString()), TextUtils.GetTextWidth(line2.ToString()));
            Assert.AreEqual(expectedWidth, tree.MinWidth);
            
            tree.MinWidth = expectedWidth - 20;
            Assert.AreEqual(expectedWidth, tree.MinWidth);

            expectedWidth += 20;

            tree.MinWidth = expectedWidth;
            Assert.AreEqual(expectedWidth, tree.MinWidth);

            tree.MinWidth = 0;
            tree.Flow = IRenderBox.FlowDirection.HORIZONTAL;

            expectedWidth = TextUtils.GetTextWidth(line1.ToString() + line2.ToString());
            Assert.AreEqual(expectedWidth, tree.MinWidth);

            tree.MinWidth = expectedWidth - 20;
            Assert.AreEqual(expectedWidth, tree.MinWidth);

            expectedWidth += 20;

            tree.MinWidth = expectedWidth;
            Assert.AreEqual(expectedWidth, tree.MinWidth);

            line1 = new StringBuilder("abcdefg");
            line2 = new StringBuilder("alask");
            tree = new RenderBoxTree();
            tree.Add(line1);
            tree.Add(line2);

            Assert.AreEqual(
                Math.Max(
                    TextUtils.GetTextWidth(tree.GetLine(0).ToString()),
                    TextUtils.GetTextWidth(tree.GetLine(1).ToString())
                    ), 
                tree.MinWidth);

            tree.Flow = IRenderBox.FlowDirection.HORIZONTAL;
            Assert.AreEqual(
                    TextUtils.GetTextWidth(
                        tree.GetLine(0).ToString()),
                    tree.MinWidth);


            string leftline1 = "abcde";
            string leftline2 = "f";
            string rightline1 = "ghi";
            string rightline2 = "jkl";
            tree = new RenderBoxTree();
            tree.Flow = IRenderBox.FlowDirection.HORIZONTAL;
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
                    TextUtils.GetTextWidth(leftBox.GetLine(0).ToString()),
                    TextUtils.GetTextWidth(leftBox.GetLine(1).ToString())
                    ),
                leftBox.MinWidth);
            
            Assert.AreEqual(
                Math.Max(
                    TextUtils.GetTextWidth(rightBox.GetLine(0).ToString()),
                    TextUtils.GetTextWidth(rightBox.GetLine(1).ToString())
                    ),
                rightBox.MinWidth);

            Assert.AreEqual(
                Math.Max(
                    TextUtils.GetTextWidth(tree.GetLine(0).ToString()),
                    TextUtils.GetTextWidth(tree.GetLine(1).ToString())
                    ),
                tree.MinWidth);


            Assert.AreEqual(
                leftBox.MinWidth + rightBox.MinWidth + 1,
                tree.MinWidth);

        }

        [TestMethod()]
        public void GetActualWidthTest()
        {
            string content1 = "abc";
            string content2 = "adsjafe";
            string content3 = "aaa";
            RenderBoxTree left = new RenderBoxTree();
            RenderBoxTree right = new RenderBoxTree();
            left.Add(content1);
            left.Add(content2);
            right.Add(content3);
            RenderBoxTree root = new RenderBoxTree();
            root.Add(left); root.Add(right);

            int minwidth;
            int maxwidth;
            int desiredwidth;

            // minwidth < maxwidth, no desired width
            maxwidth = root.MinWidth + (TextUtils.GetCharWidth(' ') * 20) + 20;
            minwidth = root.MinWidth;
            Assert.AreEqual(minwidth, root.GetActualWidth(maxwidth));

            // minwidth > maxwidth, no desired width
            maxwidth = root.MinWidth - TextUtils.GetCharWidth(content3[content3.Length - 1]) + 1;
            Assert.AreEqual(maxwidth, root.GetActualWidth(maxwidth));

            // minwidth < desiredwidth < maxwidth
            maxwidth = root.MinWidth + (TextUtils.GetCharWidth(' ') * 20) + 20;
            desiredwidth = root.MinWidth + TextUtils.GetCharWidth(' ') * 10 + 10;
            root.DesiredWidth = desiredwidth;
            Assert.AreEqual(desiredwidth, root.GetActualWidth(maxwidth));
        }

        [TestMethod()]
        public void MinHeightTest()
        {
            string line1 = "abcdefg";
            string line2 = "alask";
            RenderBoxTree tree = new RenderBoxTree();
            Assert.AreEqual(0, tree.MinHeight);

            tree.Add(line1);
            tree.Add(line2);
            int expectedHeight = 2;
            Assert.AreEqual(expectedHeight, tree.MinHeight);

            tree.MinHeight = expectedHeight - 1;
            Assert.AreEqual(expectedHeight, tree.MinHeight);

            expectedHeight += 1;

            tree.MinHeight = expectedHeight;
            Assert.AreEqual(expectedHeight, tree.MinHeight);

            tree.MinHeight = 0;
            tree.Flow = IRenderBox.FlowDirection.HORIZONTAL;

            expectedHeight = 1;
            Assert.AreEqual(expectedHeight, tree.MinHeight);

            tree.MinHeight = 0;
            Assert.AreEqual(expectedHeight, tree.MinHeight);

            expectedHeight = 1;

            tree.MinHeight = expectedHeight;
            Assert.AreEqual(expectedHeight, tree.MinHeight);

            line1 = "abcdefg";
            line2 = "alask";
            tree = new RenderBoxTree();
            tree.Add(line1);
            tree.Add(line2);

            Assert.AreEqual(
                2,
                tree.MinHeight);

            tree.Flow = IRenderBox.FlowDirection.HORIZONTAL;
            Assert.AreEqual(
                    1,
                    tree.MinHeight);


            string leftline1 = "abcde";
            string leftline2 = "f";
            string rightline1 = "ghi";
            tree = new RenderBoxTree();
            tree.Flow = IRenderBox.FlowDirection.HORIZONTAL;
            RenderBoxTree leftBox = new RenderBoxTree();
            leftBox.Add(leftline1);
            leftBox.Add(leftline2);
            RenderBoxTree rightBox = new RenderBoxTree();
            rightBox.Add(rightline1);

            tree.Add(leftBox);
            tree.Add(rightBox);


            Assert.AreEqual(
                2,
                leftBox.MinHeight);

            Assert.AreEqual(
                1,
                rightBox.MinHeight);

            Assert.AreEqual(
                2,
                tree.MinHeight);

        }

        [TestMethod()]
        public void GetActualHeightTest()
        {
            string content1 = "abc";
            string content2 = "adsjafe";
            string content3 = "aaa";
            RenderBoxTree left = new RenderBoxTree();
            RenderBoxTree right = new RenderBoxTree();
            left.Add(content1);
            left.Add(content2);
            right.Add(content3);
            RenderBoxTree root = new RenderBoxTree();
            root.Add(left); root.Add(right);

            int minheight;
            int maxheight;
            int desiredheight;

            // minheight < maxheight, no desiredheight
            maxheight = root.MinHeight + 3;
            Assert.AreEqual(root.MinHeight, root.GetActualHeight(maxheight));

            // minheight > maxheight, no desiredheight
            minheight = root.MinHeight + 3;
            maxheight = root.MinHeight + 1;
            root.MinHeight = minheight;
            Assert.AreEqual(maxheight, root.GetActualHeight(maxheight));

            // minwidth < desiredwidth < maxwidth
            root.MinHeight = 0;
            maxheight = root.MinHeight + 5;
            desiredheight = root.MinHeight + 3;
            root.DesiredHeight = desiredheight;
            Assert.AreEqual(desiredheight, root.GetActualHeight(maxheight));
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
            Assert.AreEqual(IRenderBox.TextAlign.LEFT, tree.Align);
            Assert.AreEqual(0, tree.Count);
            Assert.AreEqual(IRenderBox.FlowDirection.VERTICAL, tree.Flow);
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
            leaf1.MinWidth = TextUtils.GetTextWidth(line1.ToString());
            leaf1.MaxWidth = TextUtils.GetTextWidth(line1.ToString());
            RenderBoxLeaf leaf2 = new RenderBoxLeaf(line2.ToString());
            leaf2.MinWidth = TextUtils.GetTextWidth(line2.ToString());
            leaf2.MaxWidth = TextUtils.GetTextWidth(line2.ToString());

            tree.Add(leaf1);
            tree.Add(leaf2);
            tree.MinWidth = 0;
            tree.MaxWidth = 0;

            Assert.AreEqual<string>("", tree.GetLine(0).ToString());

            tree.MinWidth = Math.Max(TextUtils.GetTextWidth(line1.ToString()), TextUtils.GetTextWidth(line2.ToString()));
            tree.MaxWidth = Math.Max(TextUtils.GetTextWidth(line1.ToString()), TextUtils.GetTextWidth(line2.ToString()));
            Assert.AreEqual<string>(
                TextUtils.PadText(line1.ToString(), TextUtils.GetTextWidth(line2.ToString()), TextUtils.PadMode.RIGHT).ToString(),
                tree.GetLine(0).ToString());
            Assert.AreEqual<string>(line2.ToString(), tree.GetLine(1).ToString());
            Assert.AreEqual<string>(TextUtils.CreateStringOfLength(' ', tree.MaxWidth).ToString(), tree.GetLine(2).ToString());

            tree.Flow = IRenderBox.FlowDirection.HORIZONTAL;
            tree.MinWidth = TextUtils.GetTextWidth(line1.ToString()) + TextUtils.GetTextWidth(line2.ToString()) + 1;
            tree.MaxWidth = TextUtils.GetTextWidth(line1.ToString()) + TextUtils.GetTextWidth(line2.ToString()) + 1;

            Assert.AreEqual<string>(line1.ToString() + line2, tree.GetLine(0).ToString());
            Console.WriteLine("testing empty line, horizontal");
            Assert.AreEqual<string>(TextUtils.CreateStringOfLength(' ', tree.MaxWidth).ToString(), tree.GetLine(1).ToString());

            string leftline1 = "abcde";
            string leftline2 = "f";
            string rightline1 = "ghi";
            string rightline2 = "jkl";
            tree = new RenderBoxTree();
            tree.Flow = IRenderBox.FlowDirection.HORIZONTAL;
            RenderBoxTree leftBox = new RenderBoxTree();
            leftBox.Add(leftline1);
            leftBox.Add(leftline2);
            leftBox.MinWidth = Math.Max(TextUtils.GetTextWidth(leftline1), TextUtils.GetTextWidth(leftline2));
            leftBox.MaxWidth = Math.Max(TextUtils.GetTextWidth(leftline1), TextUtils.GetTextWidth(leftline2));
            RenderBoxTree rightBox = new RenderBoxTree();
            rightBox.Add(rightline1);
            rightBox.Add(rightline2);
            rightBox.MinWidth = Math.Max(TextUtils.GetTextWidth(rightline1), TextUtils.GetTextWidth(rightline2));
            rightBox.MaxWidth = Math.Max(TextUtils.GetTextWidth(rightline1), TextUtils.GetTextWidth(rightline2));

            tree.Add(leftBox);
            tree.Add(rightBox);
            tree.MinWidth = leftBox.MaxWidth + rightBox.MaxWidth + 1;
            tree.MaxWidth = leftBox.MaxWidth + rightBox.MaxWidth + 1;

            Assert.AreEqual<string>(leftline1 + rightline1, tree.GetLine(0).ToString());
            string expected =
                TextUtils.PadText(leftline2,
                Math.Max(
                    TextUtils.GetTextWidth(leftline1),
                    TextUtils.GetTextWidth(leftline2)
                ), TextUtils.PadMode.RIGHT).ToString()
                + TextUtils.PadText(rightline2,
                    Math.Max(
                        TextUtils.GetTextWidth(rightline1),
                        TextUtils.GetTextWidth(rightline2)
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
                TextUtils.PadText(line1.ToString(), TextUtils.GetTextWidth(line2.ToString()), TextUtils.PadMode.RIGHT).ToString(),
                tree.GetLine(0).ToString());
            Console.WriteLine("Test second line vertical no fixed width.");
            Assert.AreEqual<string>(line2.ToString(), tree.GetLine(1).ToString());
            Assert.AreEqual<string>(TextUtils.CreateStringOfLength(' ', tree.MinWidth).ToString(), tree.GetLine(2).ToString());

            tree.Flow = IRenderBox.FlowDirection.HORIZONTAL;

            Assert.AreEqual<string>(line1.ToString() + line2, tree.GetLine(0).ToString());
            Console.WriteLine("testing empty line, horizontal");
            Assert.AreEqual<string>(TextUtils.CreateStringOfLength(' ', tree.MinWidth).ToString(), tree.GetLine(1).ToString());

            string leftline1 = "abcde";
            string leftline2 = "f";
            string rightline1 = "ghi";
            string rightline2 = "jkl";
            tree = new RenderBoxTree();
            tree.Flow = IRenderBox.FlowDirection.HORIZONTAL;
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
                TextUtils.PadText(leftline2,
                Math.Max(
                    TextUtils.GetTextWidth(leftline1),
                    TextUtils.GetTextWidth(leftline2)
                ), TextUtils.PadMode.RIGHT).ToString()
                + TextUtils.PadText(rightline2,
                    Math.Max(
                        TextUtils.GetTextWidth(rightline1),
                        TextUtils.GetTextWidth(rightline2)
                    ), TextUtils.PadMode.RIGHT);
            Assert.AreEqual<string>(
                expected,
                tree.GetLine(1).ToString());

            Console.WriteLine("\nTest runs til here");
            Assert.AreEqual<string>(
                TextUtils.PadText(" ", tree.MinWidth, TextUtils.PadMode.RIGHT).ToString(),
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
                TextUtils.PadText(line1.ToString(), TextUtils.GetTextWidth(line2.ToString()), TextUtils.PadMode.RIGHT).ToString(),
                tree.GetLine(0).ToString());
            Assert.AreEqual<string>(line2.ToString(), tree.GetLine(1).ToString());

            int customMinWidth = tree.MinWidth + 100;
            tree.MinWidth = customMinWidth;

            Assert.AreEqual<string>(
                TextUtils.PadText(line1.ToString(), customMinWidth, TextUtils.PadMode.RIGHT).ToString(),
                tree.GetLine(0).ToString());
            Assert.AreEqual<string>(
                TextUtils.PadText(line2.ToString(), customMinWidth, TextUtils.PadMode.RIGHT).ToString(),
                tree.GetLine(1).ToString());
            Assert.AreEqual<string>(TextUtils.CreateStringOfLength(' ', customMinWidth).ToString(), tree.GetLine(2).ToString());

            tree.Flow = IRenderBox.FlowDirection.HORIZONTAL;
            tree.MinWidth = tree.MinWidth - 50;

            Console.WriteLine("Test runs until here.");
            Assert.AreEqual<string>(line1.ToString() + line2, tree.GetLine(0).ToString());
            Assert.AreEqual<string>(
                TextUtils.CreateStringOfLength(' ', TextUtils.GetTextWidth(line1.ToString() + line2.ToString())).ToString(),
                tree.GetLine(1).ToString());

            customMinWidth = tree.MinWidth + 100;
            tree.MinWidth = customMinWidth;

            Assert.AreEqual<string>(TextUtils.PadText(line1.ToString() + line2, customMinWidth, TextUtils.PadMode.RIGHT).ToString(),
                tree.GetLine(0).ToString());
            Assert.AreEqual<string>(TextUtils.PadText(" ", customMinWidth, TextUtils.PadMode.RIGHT).ToString(), tree.GetLine(1).ToString());

            string leftline1 = "abcde";
            string leftline2 = "f";
            string rightline1 = "ghi";
            string rightline2 = "jkl";
            tree = new RenderBoxTree();
            tree.Flow = IRenderBox.FlowDirection.HORIZONTAL;
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
                TextUtils.PadText(leftline1 + rightline1, customMinWidth, TextUtils.PadMode.RIGHT).ToString(),
                tree.GetLine(0).ToString());
            Assert.AreEqual<string>(
                TextUtils.PadText(leftline2, leftBox.MinWidth, TextUtils.PadMode.RIGHT).ToString()
                + TextUtils.PadText(rightline2, Math.Max(rightBox.MinWidth, tree.MinWidth - leftBox.MinWidth), TextUtils.PadMode.RIGHT),
                tree.GetLine(1).ToString());
            Assert.AreEqual<string>(TextUtils.CreateStringOfLength(' ', customMinWidth).ToString(), tree.GetLine(2).ToString());
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
            leftLeaf1.MinWidth = TextUtils.GetTextWidth(left1);
            leftLeaf1.MaxWidth = TextUtils.GetTextWidth(left1);
            RenderBoxLeaf leftLeaf2 = new RenderBoxLeaf(left2);
            leftLeaf2.MinWidth = TextUtils.GetTextWidth(left2);
            leftLeaf2.MaxWidth = TextUtils.GetTextWidth(left2);
            RenderBoxLeaf rightLeaf1 = new RenderBoxLeaf(right1);
            rightLeaf1.MinWidth = TextUtils.GetTextWidth(right1);
            rightLeaf1.MaxWidth = TextUtils.GetTextWidth(right1);
            RenderBoxLeaf rightLeaf2 = new RenderBoxLeaf(right2);
            rightLeaf2.MinWidth = TextUtils.GetTextWidth(right2);
            rightLeaf2.MaxWidth = TextUtils.GetTextWidth(right2);
            RenderBoxLeaf rightLeaf3 = new RenderBoxLeaf(right3);
            rightLeaf3.MinWidth = TextUtils.GetTextWidth(right3);
            rightLeaf3.MaxWidth = TextUtils.GetTextWidth(right3);

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
            rightBox.Align = IRenderBox.TextAlign.RIGHT;

            frame.Add(leftBox);
            frame.Add(rightBox);
            frame.MinWidth = leftBox.MaxWidth + 1 + rightBox.MaxWidth;
            frame.MaxWidth = leftBox.MaxWidth + 1 + rightBox.MaxWidth;
            frame.Flow = IRenderBox.FlowDirection.HORIZONTAL;
            
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