using Microsoft.VisualStudio.TestTools.UnitTesting;
using SEScripts.XUI.XML;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEScripts.XUI.XML.Tests
{
    [TestClass()]
    public class GenericTests
    {
        [TestMethod()]
        public void GenericTest()
        {
            Generic tree = new Generic("generic");

            // check default settings

            Assert.IsInstanceOfType(tree, typeof(NodeBox));
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
            Assert.AreEqual<int>(1, ((NodeBoxTree) parent.RenderCache).Count);

            parent.AddChild(new Generic("child2"));

            Assert.AreEqual<int>(2, ((NodeBoxTree) parent.RenderCache).Count);
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

            Assert.AreEqual<int>(2, ((NodeBoxTree) parent.RenderCache).Count);
            Assert.AreEqual<String>("child2", parent.GetChild(0).Type);
            Assert.AreEqual<String>("child1", parent.GetChild(1).Type);
        }

        [TestMethod()]
        public void NumberOfChildrenTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void SetParentTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void GetParentTest()
        {
            Assert.Fail();
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
        public void UpdateSelectabilityTest()
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
        public void RenderTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void RenderTest1()
        {
            Assert.Fail();
        }
    }
}