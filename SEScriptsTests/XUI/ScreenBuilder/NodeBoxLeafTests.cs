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
    public class NodeBoxLeafTests
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
            NodeBoxLeaf leaf = new NodeBoxLeaf(content);
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
            NodeBoxLeaf leaf = new NodeBoxLeaf();
            Assert.IsNotNull(leaf);
            Assert.AreEqual<string>("", leaf.GetLine(0).ToString());
            Assert.AreEqual(0, leaf.Height);
            Assert.AreEqual(-1, leaf.Width);
            Assert.AreEqual(-1, leaf.MaxWidth);
            Assert.AreEqual(0, leaf.MinWidth);
            Assert.AreEqual(-1, leaf.DesiredWidth);
            Assert.AreEqual(NodeBox.TextAlign.LEFT, leaf.Align);
            Assert.AreEqual(NodeBox.FlowDirection.VERTICAL, leaf.Flow);
        }

        [TestMethod()]
        public void NodeBoxLeafTest1()
        {
            StringBuilder content = new StringBuilder("test");
            NodeBoxLeaf leaf = new NodeBoxLeaf(content);
            Assert.IsNotNull(leaf);
            Assert.AreEqual<string>(content.ToString(), leaf.GetLine(0).ToString());
            Assert.AreEqual(1f, leaf.Height);
            //Assert.AreEqual(TextUtils.GetTextWidth(content), leaf.Width);
            Assert.AreEqual(-1f, leaf.MaxWidth);
            //Assert.AreEqual(leaf.Width, leaf.MinWidth);
            Assert.AreEqual(-1f, leaf.DesiredWidth);
            Assert.AreEqual(NodeBox.TextAlign.LEFT, leaf.Align);
            Assert.AreEqual(NodeBox.FlowDirection.VERTICAL, leaf.Flow);
        }

        [TestMethod()]
        public void AddTest()
        {
            string part1 = "hello ";
            string part2 = "world";
            NodeBoxLeaf leaf = new NodeBoxLeaf(part1);
            Assert.AreEqual(1, leaf.Height);
            leaf.Add(part2);
            Assert.AreEqual(1, leaf.Height);
            Assert.AreEqual<string>(part1 + part2, leaf.GetLine(0).ToString());
        }

        [TestMethod()]
        public void GetRenderedLineTest()
        {
            NodeBoxLeaf leaf = new NodeBoxLeaf("test line");
            Assert.AreEqual<string>(leaf.GetLine(0).ToString(), leaf.GetLine(0, 0).ToString());
            Assert.AreEqual<string>(leaf.GetLine(1).ToString(), leaf.GetLine(1, 0).ToString());
            Assert.AreEqual<string>(leaf.GetLine(0).ToString(), leaf.GetLine(0, 100).ToString());
            Assert.AreEqual<string>(leaf.GetLine(1).ToString(), leaf.GetLine(1, 100).ToString());
        }

        [TestMethod()]
        public void GetLineTest()
        {
            string line = "testline";
            NodeBoxLeaf leaf = new NodeBoxLeaf(line);

            Assert.AreEqual<string>(line, leaf.GetLine(0).ToString());
            Assert.AreEqual<string>("", leaf.GetLine(1).ToString());
            
            line = "testline\nsameline";
            leaf = new NodeBoxLeaf(line);

            Assert.AreEqual<string>(line.Replace("\n", ""), leaf.GetLine(0).ToString());
            Assert.AreEqual<string>("", leaf.GetLine(1).ToString());
        }

        [TestMethod()]
        public void ClearTest()
        {
            NodeBoxLeaf leaf = new NodeBoxLeaf("test");
            leaf.Clear();
            Assert.AreEqual<string>("", leaf.GetLine(0).ToString());
        }

        [TestMethod()]
        public void GetLinesTest()
        {
            string line = "first line";
            NodeBoxLeaf leaf = new NodeBoxLeaf(line);

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