using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using SEScripts.Lib;
using SEScripts.XUI.BoxRenderer;

namespace SEScripts.XUI.Tests
{
    [TestClass()]
    public class WidthCalculationTest
    {
        [TestInitialize()]
        public void Initialize()
        {
            //Logger.debug = true;
            TextUtils.Reset();
        }


        [TestMethod()]
        public void RenderSingleElementStaticWidthTest()
        {
            RenderBoxLeaf box = new RenderBoxLeaf("test content abc")
            {
                DesiredWidth = 500
            };
            int maxWidth = 1000;
            int maxHeight = 10;

            box.Initialize(maxWidth, maxHeight);
            StringBuilder result = new StringBuilder();
            int count = 0;
            Console.WriteLine(box.GetLine(0, maxWidth, maxHeight));
            foreach (StringBuilder line in box.GetLines(maxWidth, maxHeight))
            {
                count++;
                Assert.AreEqual(500, TextUtils.GetTextWidth(line.ToString()));
            }
            Assert.AreEqual(1, count);
        }

        [TestMethod()]
        public void RenderChildElementRelativeWidthTest()
        {

        }

    }
}
