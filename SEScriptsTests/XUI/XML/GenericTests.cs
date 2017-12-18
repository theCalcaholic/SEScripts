using Microsoft.VisualStudio.TestTools.UnitTesting;
using SEScripts.XUI.XML;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SEScripts.Lib;
using SEScripts.Lib.LoggerNS;
using SEScripts.XUI.BoxRenderer;

namespace SEScripts.XUI.XML.Tests
{
    [TestClass()]
    public class GenericTests
    {
        [TestInitialize()]
        public void Initialize()
        {
            //Logger.debug = true;
            TextUtils.Reset();
        }

        [TestMethod()]
        public void GenericTest()
        {
            Generic tree = new Generic("generic");

            // check default settings

            Assert.IsInstanceOfType(tree, typeof(IRenderBox));
            Assert.IsFalse(tree.HasUserInputBindings);
            Assert.IsNull(tree.GetParent());
            Assert.IsNull(tree.GetChild(0));
            Assert.IsFalse(tree.IsSelectable());
            Assert.AreEqual<String>("left", tree.GetAttribute("alignself"));
            Assert.AreEqual<String>("left", tree.GetAttribute("aligntext"));
            Assert.AreEqual<String>("false", tree.GetAttribute("selected"));
            Assert.AreEqual<String>("false", tree.GetAttribute("selectable"));
            Assert.AreEqual<String>("vertical", tree.GetAttribute("flow"));


            Assert.AreEqual<String>("generic", tree.Type);
        }

        [TestMethod()]
        public void IsSelectableTest()
        {
            Generic tree = new Generic("test");

            Assert.IsFalse(tree.IsSelectable());
            tree.SetAttribute("selectable", "true");
            Assert.IsTrue(tree.IsSelectable());

            tree = new Generic("test");

            Assert.IsFalse(tree.IsSelectable());

            Generic child = new Generic("child");
            child.SetAttribute("selectable", "true");
            tree.AddChild(child);

            Assert.IsTrue(tree.IsSelectable());
        }

        [TestMethod()]
        public void GetSelectedSiblingTest()
        {
            Generic parent = new Generic("parent");
            Generic child1 = new Generic("child1");
            Generic child2 = new Generic("child2");
            child1.SetAttribute("selectable", "true");
            child2.SetAttribute("selectable", "true");

            parent.AddChild(child1);
            parent.AddChild(child2);
            parent.SelectFirst();

            Assert.AreEqual<String>("child1", parent.GetSelectedSibling().Type);
        }

        [TestMethod()]
        public void AddChildTest()
        {
            Generic parent = new Generic("parent");

            Assert.AreEqual<XMLTree>(null, parent.GetChild(0));

            parent.AddChild(new Generic("child1"));

            Assert.AreEqual<String>("child1", parent.GetChild(0).Type);
            Assert.AreEqual<int>(1, ((RenderBoxTree) parent.GetRenderBox(-1, -1)).Count);

            parent.AddChild(new Generic("child2"));

            Assert.AreEqual<int>(2, ((RenderBoxTree) parent.GetRenderBox(-1, -1)).Count);
            Assert.AreEqual<String>("child1", parent.GetChild(0).Type);
            Assert.AreEqual<String>("child2", parent.GetChild(1).Type);
        }

        [TestMethod()]
        public void AddChildAtTest()
        {
            Generic parent = new Generic("parent");

            Assert.AreEqual<XMLTree>(null, parent.GetChild(0));

            parent.AddChild(new Generic("child1"));

            parent.AddChildAt(0, new Generic("child2"));

            Assert.AreEqual<int>(2, ((RenderBoxTree) parent.GetRenderBox(-1, -1)).Count);
            Assert.AreEqual<String>("child2", parent.GetChild(0).Type);
            Assert.AreEqual<String>("child1", parent.GetChild(1).Type);
        }

        [TestMethod()]
        public void SetParentTest()
        {
            Generic child = new Generic("child");
            Generic parent = new Generic("parent");

            Assert.IsNull(child.GetParent());
            child.SetParent(parent);
            Assert.AreSame(parent, child.GetParent());
        }

        [TestMethod()]
        public void GetChildTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void GetNodeTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void GetAllNodesTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void SelectFirstTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void SelectLastTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void UnselectTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void SelectNextTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void SelectPreviousTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void OnSelectTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void GetAttributeTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void SetAttributeTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void RetrieveRootTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void KeyPressTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void OnKeyPressedTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void ToggleActivationTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void PreventDefaultTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void AllowDefaultTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void FollowRouteTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void GetValuesTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void DetachChildTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void DetachTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void RenderTestWithForcedWidth()
        {
            StringBuilder line1 = new StringBuilder("testabctest");
            StringBuilder line2 = new StringBuilder("someothercontent");

            XMLTree tree = new Generic("rootnode");
            XMLTree leaf1 = new TextNode(line1.ToString());
            leaf1.SetAttribute("forcewidth", TextUtils.GetTextWidth(line1.ToString()).ToString());
            TextNode leaf2 = new TextNode(line2.ToString());
            leaf2.SetAttribute("forcewidth", TextUtils.GetTextWidth(line2.ToString()).ToString());

            tree.AddChild(leaf1);
            tree.AddChild(leaf2);
            tree.SetAttribute("forcewidth", 0.ToString());

            Assert.AreEqual<string>("\n", tree.Render());

            int width = Math.Max(TextUtils.GetTextWidth(line1.ToString()), TextUtils.GetTextWidth(line2.ToString()));
            tree.SetAttribute("forcewidth", width.ToString());
            string expected = TextUtils.PadText(line1.ToString(), width, TextUtils.PadMode.RIGHT).ToString() + "\n"
                + TextUtils.PadText(line2.ToString(), width, TextUtils.PadMode.RIGHT).ToString();
            Assert.AreEqual<string>(expected, tree.Render());

            width = TextUtils.GetTextWidth(line1.ToString()) + TextUtils.GetTextWidth(line2.ToString()) + 1;
            tree.SetAttribute("flow", "horizontal");
            tree.SetAttribute("forcewidth", width.ToString());

            Assert.AreEqual<string>(line1.ToString() + line2, tree.Render());
            Console.WriteLine("testing empty line, horizontal");

            string leftline1 = "abcde";
            string leftline2 = "f";
            string rightline1 = "ghi";
            string rightline2 = "jkl";
            tree = new Generic("rootnode");
            tree.SetAttribute("flow", "horizontal");
            XMLTree leftBox = new Generic("generic");
            leftBox.AddChild(new TextNode(leftline1));
            leftBox.AddChild(new TextNode(leftline2));
            int leftWidth = Math.Max(TextUtils.GetTextWidth(leftline1), TextUtils.GetTextWidth(leftline2));
            leftBox.SetAttribute("forcewidth", leftWidth.ToString());
            XMLTree rightBox = new Generic("generic");
            rightBox.AddChild(new TextNode(rightline1));
            rightBox.AddChild(new TextNode(rightline2));
            int rightWidth = Math.Max(TextUtils.GetTextWidth(rightline1), TextUtils.GetTextWidth(rightline2));
            rightBox.SetAttribute("forcewidth", rightWidth.ToString());
            width = leftWidth + rightWidth + 1;
            tree.AddChild(leftBox);
            tree.AddChild(rightBox);
            tree.SetAttribute("forcewidth", width.ToString());
            
            expected =
                leftline1 + rightline1 + "\n"
                + TextUtils.PadText(leftline2,
                Math.Max(
                    TextUtils.GetTextWidth(leftline1),
                    TextUtils.GetTextWidth(leftline2)
                ), TextUtils.PadMode.RIGHT).ToString()
                + TextUtils.PadText(rightline2,
                    Math.Max(
                        TextUtils.GetTextWidth(rightline1),
                        TextUtils.GetTextWidth(rightline2)
                    ), TextUtils.PadMode.RIGHT);
            Assert.AreEqual<string>(expected,
                tree.Render());
            //Assert.AreEqual<string>(TextUtils.CreateStringOfLength(" ", tree.Width - 1).ToString(), tree.GetLine(2).ToString());
        }

        [TestMethod()]
        public void RenderTestWithDynamicWidth()
        {
            StringBuilder line1 = new StringBuilder("testabctest");
            StringBuilder line2 = new StringBuilder("someothercontent");

            XMLTree tree = new Generic("rootnode");
            XMLTree leaf1 = new TextNode(line1.ToString());
            XMLTree leaf2 = new TextNode(line2.ToString());

            tree.AddChild(leaf1);
            tree.AddChild(leaf2);

            Console.WriteLine("Test first line vertical no fixed width.");
            int width = Math.Max(TextUtils.GetTextWidth(line1.ToString()), TextUtils.GetTextWidth(line2.ToString()));
            string expected =
                TextUtils.PadText(line1.ToString(), width, TextUtils.PadMode.RIGHT).ToString() + "\n"
                + TextUtils.PadText(line2.ToString(), width, TextUtils.PadMode.RIGHT).ToString();
            Assert.AreEqual<string>(expected, tree.Render());

            tree.SetAttribute("flow", "horizontal");

            Assert.AreEqual<string>(line1.ToString() + line2, tree.Render());
            Console.WriteLine("testing empty line, horizontal");

            string leftline1 = "abcde";
            string leftline2 = "f";
            string rightline1 = "ghi";
            string rightline2 = "jkl";
            tree = new Generic("rootnode");
            tree.SetAttribute("flow", "horizontal");
            XMLTree leftBox = new Generic("generic");
            leftBox.AddChild(new TextNode(leftline1));
            leftBox.AddChild(new TextNode(leftline2));
            XMLTree rightBox = new Generic("generic");
            rightBox.AddChild(new TextNode(rightline1));
            rightBox.AddChild(new TextNode(rightline2));

            tree.AddChild(leftBox);
            tree.AddChild(rightBox);

            expected =
                leftline1 + rightline1 + "\n"
                + TextUtils.PadText(leftline2,
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
                tree.Render());
        }

        [TestMethod()]
        public void RenderTestWithFixedMinWidth()
        {
            StringBuilder line1 = new StringBuilder("testabctest");
            StringBuilder line2 = new StringBuilder("someothercontent");

            XMLTree tree = new Generic("noderoot");
            XMLTree leaf1 = new TextNode(line1.ToString());
            XMLTree leaf2 = new TextNode(line2.ToString());

            tree.AddChild(leaf1);
            tree.AddChild(leaf2);
            int minWidth = tree.GetRenderBox(-1, -1).MinWidth;
            tree.SetAttribute("minwidth", (minWidth - 50).ToString());

            string expected = TextUtils.PadText(line1.ToString(), TextUtils.GetTextWidth(line2.ToString()), TextUtils.PadMode.RIGHT).ToString() + "\n"
                + line2.ToString();
            Assert.AreEqual<string>(
                expected,
                tree.Render());
            
            minWidth = tree.GetRenderBox(-1, -1).MinWidth + 100;
            tree.SetAttribute("minwidth", minWidth.ToString());

            expected = TextUtils.PadText(line1.ToString(), minWidth, TextUtils.PadMode.RIGHT).ToString() + "\n"
                + TextUtils.PadText(line2.ToString(), minWidth, TextUtils.PadMode.RIGHT).ToString();
            Assert.AreEqual<string>(expected, tree.Render());

            tree.SetAttribute("flow", "horizontal");
            minWidth = tree.GetRenderBox(-1, -1).MinWidth - 50;
            tree.SetAttribute("minwidth", minWidth.ToString());
            expected = line1.ToString() + line2.ToString();
            
            Assert.AreEqual<string>(expected, tree.Render());

            minWidth = tree.GetRenderBox(-1, -1).MinWidth + 100;
            tree.SetAttribute("minwidth", minWidth.ToString());
            expected = TextUtils.PadText(line1.ToString() + line2, minWidth, TextUtils.PadMode.RIGHT).ToString();
            Assert.AreEqual<string>(expected, tree.Render());

            string leftline1 = "abcde";
            string leftline2 = "f";
            string rightline1 = "ghi";
            string rightline2 = "jkl";
            tree = new Generic("rootnode");
            tree.SetAttribute("flow", "horizontal");
            XMLTree leftBox = new Generic("generic");
            leftBox.AddChild(new TextNode(leftline1));
            leftBox.AddChild(new TextNode(leftline2));
            XMLTree rightBox = new Generic("generic");
            rightBox.AddChild(new TextNode(rightline1));
            rightBox.AddChild(new TextNode(rightline2));

            tree.AddChild(leftBox);
            tree.AddChild(rightBox);

            minWidth = tree.GetRenderBox(-1, -1).MinWidth + 100;
            int leftMinWidth = leftBox.GetRenderBox(-1, -1).MinWidth;
            int rightMinWidth = rightBox.GetRenderBox(-1, -1).MinWidth;

            tree.SetAttribute("minwidth", minWidth.ToString());
            expected = TextUtils.PadText(leftline1 + rightline1, minWidth, TextUtils.PadMode.RIGHT).ToString() + "\n"
                + TextUtils.PadText(leftline2, leftMinWidth, TextUtils.PadMode.RIGHT).ToString()
                + TextUtils.PadText(rightline2, Math.Max(rightMinWidth, tree.GetRenderBox(-1, -1).MinWidth - leftMinWidth), TextUtils.PadMode.RIGHT);
            Assert.AreEqual<string>(
                expected,
                tree.Render());

        }
    }
}