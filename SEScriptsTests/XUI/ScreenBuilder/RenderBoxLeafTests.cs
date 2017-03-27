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
            Logger.DEBUG = true;
            TextUtils.Reset();
        }

        [TestMethod()]
        public void MinWidthTest()
        {
            StringBuilder content = new StringBuilder("abc");
            RenderBoxLeaf leaf = new RenderBoxLeaf(content);
            Assert.AreEqual(TextUtils.GetTextWidth(content), leaf.MinWidth);

            leaf.MinWidth = leaf.MinWidth - 10;
            Assert.AreEqual(TextUtils.GetTextWidth(content), leaf.MinWidth);

            int newMinWidth = (leaf.MinWidth + 20);
            leaf.MinWidth = newMinWidth;
            Assert.AreEqual(TextUtils.GetTextWidth(content) + 20, leaf.MinWidth);
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
            Assert.AreEqual(RenderBox.TextAlign.LEFT, leaf.Align);
            Assert.AreEqual(RenderBox.FlowDirection.VERTICAL, leaf.Flow);
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
            Assert.AreEqual(RenderBox.TextAlign.LEFT, leaf.Align);
            Assert.AreEqual(RenderBox.FlowDirection.VERTICAL, leaf.Flow);
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
        public void GetRenderedLineTest()
        {
            RenderBoxLeaf leaf = new RenderBoxLeaf("test line");
            Assert.AreEqual<string>(leaf.GetLine(0).ToString(), leaf.GetLine(0, 0, -1).ToString());
            Assert.AreEqual<string>(leaf.GetLine(1).ToString(), leaf.GetLine(1, 0, -1).ToString());
            Assert.AreEqual<string>(leaf.GetLine(0).ToString(), leaf.GetLine(0, 100, -1).ToString());
            Assert.AreEqual<string>(leaf.GetLine(1).ToString(), leaf.GetLine(1, 100, -1).ToString());
        }

        [TestMethod()]
        public void GetLineTest()
        {
            string line = "testline";
            RenderBoxLeaf leaf = new RenderBoxLeaf(line);

            Assert.AreEqual<string>(line, leaf.GetLine(0).ToString());
            Assert.AreEqual<string>("", leaf.GetLine(1).ToString());
            
            line = "testline\nsameline";
            leaf = new RenderBoxLeaf(line);

            Assert.AreEqual<string>(line.Replace("\n", ""), leaf.GetLine(0).ToString());
            Assert.AreEqual<string>("", leaf.GetLine(1).ToString());
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
        }
    }
}