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
    public class RenderBoxLeafTests
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
            StringBuilder content = new StringBuilder("abc");
            RenderBoxLeaf leaf = new RenderBoxLeaf(content);
            Assert.AreEqual(TextUtils.GetTextWidth(content.ToString()), leaf.MinWidth);

            leaf.MinWidth = leaf.MinWidth - 10;
            Assert.AreEqual(TextUtils.GetTextWidth(content.ToString()), leaf.MinWidth);

            int newMinWidth = (leaf.MinWidth + 20);
            leaf.MinWidth = newMinWidth;
            Assert.AreEqual(TextUtils.GetTextWidth(content.ToString()) + 20, leaf.MinWidth);
        }

        [TestMethod()]
        public void GetActualWidthTest()
        {
            string content = "abc";
            RenderBoxLeaf leaf = new RenderBoxLeaf(content);
            int minwidth;
            int maxwidth;
            int desiredwidth;

            // minwidth < maxwidth, no desired width
            maxwidth = leaf.MinWidth + (TextUtils.GetCharWidth(' ') * 20) + 20;
            Assert.AreEqual(maxwidth, leaf.GetActualWidth(maxwidth));

            // minwidth > maxwidth, no desired width
            maxwidth = leaf.MinWidth - TextUtils.GetCharWidth(content[content.Length - 1]) + 1;
            Assert.AreEqual(maxwidth, leaf.GetActualWidth(maxwidth));

            // minwidth < desiredwidth < maxwidth
            maxwidth = leaf.MinWidth + (TextUtils.GetCharWidth(' ') * 20) + 20;
            desiredwidth = leaf.MinWidth + TextUtils.GetCharWidth(' ') * 10 + 10;
            leaf.DesiredWidth = desiredwidth;
            Assert.AreEqual(desiredwidth, leaf.GetActualWidth(maxwidth));
        }

        [TestMethod()]
        public void MinHeightTest()
        {
            string content = "abc";
            RenderBoxLeaf leaf = new RenderBoxLeaf(content);
            Assert.AreEqual(1, leaf.MinHeight);

            leaf.MinHeight = 0;
            Assert.AreEqual(1, leaf.MinHeight);
            Assert.AreEqual(content, leaf.GetLine(0).ToString());

            leaf.MinHeight = 5;
            Assert.AreEqual(5, leaf.MinHeight);
        }

        [TestMethod()]
        public void GetActualHeightTest()
        {
            string content = "abc";
            RenderBoxLeaf leaf = new RenderBoxLeaf(content);
            int minheight;
            int maxheight;
            int desiredheight;

            // minheight < maxheight, no desiredheight
            maxheight = leaf.MinHeight + 3;
            Assert.AreEqual(leaf.MinHeight, leaf.GetActualHeight(maxheight));

            // minheight > maxheight, no desiredheight
            minheight = leaf.MinHeight + 3;
            maxheight =  leaf.MinHeight + 1;
            leaf.MinHeight = minheight;
            Assert.AreEqual(maxheight, leaf.GetActualHeight(maxheight));

            // minheight < desiredheight < maxheight
            leaf.MinHeight = 0;
            maxheight = leaf.MinHeight + 5;
            desiredheight = leaf.MinHeight + 3;
            leaf.DesiredHeight = desiredheight;
            Assert.AreEqual(desiredheight, leaf.GetActualHeight(maxheight));

            // no maxheight
            leaf.DesiredHeight = -1;
            leaf.MaxHeight = -1;
            leaf.MinHeight = 0;

            Assert.AreEqual(leaf.MinHeight, leaf.GetActualHeight(-1));
        }

        [TestMethod()]
        public void NodeBoxLeafTest()
        {
            RenderBoxLeaf leaf = new RenderBoxLeaf();
            Assert.IsNotNull(leaf);
            Assert.AreEqual<string>("", leaf.GetLine(0).ToString());
            Assert.AreEqual(0, leaf.MinHeight);
            Assert.AreEqual(-1, leaf.MaxHeight);
            Assert.AreEqual(-1, leaf.DesiredHeight);
            Assert.AreEqual(-1, leaf.MaxWidth);
            Assert.AreEqual(0, leaf.MinWidth);
            Assert.AreEqual(-1, leaf.DesiredWidth);
            Assert.AreEqual(IRenderBox.TextAlign.LEFT, leaf.Align);
            Assert.AreEqual(IRenderBox.FlowDirection.VERTICAL, leaf.Flow);
        }

        [TestMethod()]
        public void NodeBoxLeafTest1()
        {
            StringBuilder content = new StringBuilder("test");
            RenderBoxLeaf leaf = new RenderBoxLeaf(content);
            Assert.IsNotNull(leaf);
            Assert.AreEqual<string>(content.ToString(), leaf.GetLine(0).ToString());
            Assert.AreEqual(1f, leaf.MinHeight);
            //Assert.AreEqual(TextUtils.GetTextWidth(content), leaf.Width);
            Assert.AreEqual(-1f, leaf.MaxWidth);
            //Assert.AreEqual(leaf.Width, leaf.MinWidth);
            Assert.AreEqual(-1f, leaf.DesiredWidth);
            Assert.AreEqual(IRenderBox.TextAlign.LEFT, leaf.Align);
            Assert.AreEqual(IRenderBox.FlowDirection.VERTICAL, leaf.Flow);
        }

        [TestMethod()]
        public void AddTest()
        {
            string part1 = "hello ";
            string part2 = "world";
            RenderBoxLeaf leaf = new RenderBoxLeaf(part1);
            Assert.AreEqual(1, leaf.MinHeight);
            leaf.Add(part2);
            Assert.AreEqual(1, leaf.MinHeight);
            Assert.AreEqual<string>(part1 + part2, leaf.GetLine(0).ToString());
        }

        [TestMethod()]
        public void GetLineTest()
        {
            string line = "testline";
            RenderBoxLeaf leaf = new RenderBoxLeaf(line);

            Assert.AreEqual<string>(line, leaf.GetLine(0).ToString());
            Assert.AreEqual<string>(
                TextUtils.CreateStringOfLength(' ', TextUtils.GetTextWidth(line), TextUtils.RoundMode.FLOOR).ToString(),
                leaf.GetLine(1).ToString());
            
            line = "testline\nsameline";
            leaf = new RenderBoxLeaf(line);

            Assert.AreEqual<string>(line, leaf.GetLine(0).ToString());
            Assert.AreEqual<string>(
                TextUtils.CreateStringOfLength(' ', TextUtils.GetTextWidth(line), TextUtils.RoundMode.FLOOR).ToString(),
                leaf.GetLine(1).ToString());
        }

        [TestMethod()]
        public void ClearTest()
        {
            RenderBoxLeaf leaf = new RenderBoxLeaf("test");
            leaf.Clear();
            Assert.AreEqual<string>("", leaf.GetLine(0).ToString());
        }

        [TestMethod()]
        public void GetLinesTest()
        {
            string line = "first line";
            RenderBoxLeaf leaf = new RenderBoxLeaf(line);
            
            int numberOfLines = 0;
            foreach(StringBuilder l in leaf.GetLines())
            {
                Assert.AreEqual<string>(leaf.GetLine(0).ToString(), l.ToString());
                numberOfLines++;
            }
            Assert.AreEqual(1, numberOfLines);

            numberOfLines = 0;
            foreach (StringBuilder l in leaf.GetLines(600, 30))
            {
                if(numberOfLines == 0)
                    Assert.AreEqual<string>(leaf.GetLine(0, 600, 30).ToString(), l.ToString());
                else
                    Assert.AreEqual<string>(leaf.GetLine(1, 600, 30).ToString(), l.ToString());
                numberOfLines++;
            }
            Assert.AreEqual(1, numberOfLines);
        }
    }
}