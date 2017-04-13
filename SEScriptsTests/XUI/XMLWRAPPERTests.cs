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
using SEScripts.Lib;

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

            TextUtils.SelectFont(TextUtils.FONT.MONOSPACE);
            string xmlString = "<meta fontsize='1.0' fontcolor='00AA00' fontfamily='MONO'/>" +
                "<uicontrols alignSelf='center'>Main Menu</uicontrols>" +
                "<hl/>" +
                "<container flow='horizontal' width='100%'>" +
                    "<leftcontainer alignChildren='center' width='50%'>" +
                        "<menu alignChildren='center'>" +
                            "<menuitem width='300' route='xml:'>Show Status</menuitem>" +
                            "<menuitem width='300'>Option 2</menuitem>" +
                        "</menu>" +
                        "<progressbar width='300' alignSelf='left' emptyString=' ' filledString='/' selectable value='0.3'/>" +
                    "</leftcontainer>" +
                    "<rightcontainer minwidth='50%'>" +
                    "<menu width='100%' alignSelf='right'>" +
                        "<menuitem alignSelf='right' route='xml:<uicontrols/><hl/>another page (yay!)'>Link</menuitem>" +
                        "<menuitem alignChildren='right'>this link is dead</menuitem>" +
                    "</menu>" +
                    "</rightcontainer>" +
                "</container>" +
                "some text which is very long to test a new feature of the ui system. Text should now wrap automatically. Very cool indeed." +
                "<hl/>" +
                "<br />" +
                "<textinput inputBinding='value' maxLength='10'/>"
            ;
            //xmlString = "<container flow='horizontal'><containerleft>some random very long text</containerleft><containerright width='50%'>this text is also quite long</containerright></container>";
            TextUtils.SelectFont(TextUtils.FONT.DEFAULT);
            XMLTree tree = XMLWRAPPER.ParseXML(xmlString);
            TextUtils.SelectFont(TextUtils.FONT.MONOSPACE);
            Console.WriteLine("-------------------------------------------------------------");
            Console.WriteLine("Result:");
            Console.WriteLine("-------------------------------------------------------------");
            Console.WriteLine(tree.Render(658, 60));
            Console.WriteLine("-------------------------------------------------------------");
            Console.WriteLine("Log:");
            Console.WriteLine("-------------------------------------------------------------");
            Console.WriteLine(Logger.Output);
        }
    }
}