using Microsoft.VisualStudio.TestTools.UnitTesting;
using SEScripts.XUI;
using System;
using System.Threading;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SEScripts.XUI.XML;
using SEScripts.Lib.LoggerNS;
using SEScripts.XUI.BoxRenderer;

namespace SEScripts.XUI.Tests
{
    [TestClass()]
    public class XMLWRAPPERTests
    {
        [TestInitialize()]
        public void Initialize()
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
        }
        [TestMethod()]
        public void CreateNodeTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void ParseXMLTest()
        {
            //Logger.debug = true;


            string xmlString = "" + //"<meta fontsize='0.8' fontcolor='00AA00'/>" +
                "<uicontrols alignSelf='center'>Main Menu</uicontrols>" +
                "<hl/>" +
                "<container alignChildren='center' width='100%'>" +
                    "<menu width='100%' alignChildren='center'>" +
                        "<menuitem width='300' route=''>Show<br/> Status</menuitem>" +
                        "<menuitem width='300'>Option 2</menuitem>" +
                    "</menu>" +
                    "some text" +
                    "<progressbar emptyString=' ' filledString='/' selectable width='50%' value='0.3'/>" +
                "</container>" +
                "<br />" +
                "<textinput inputBinding='value' maxLength='10'/>";
            //xmlString = "<hl/>";
            XMLTree tree = XMLWRAPPER.ParseXML(xmlString);
            Console.WriteLine("-------------------------------------------------------------");
            Console.WriteLine("Result:");
            Console.WriteLine("-------------------------------------------------------------");
            Console.WriteLine(tree.Render(600, 20));
            Console.WriteLine("-------------------------------------------------------------");
            Console.WriteLine("Log:");
            Console.WriteLine("-------------------------------------------------------------");
            Console.WriteLine(Logger.Output);
        }
    }
}