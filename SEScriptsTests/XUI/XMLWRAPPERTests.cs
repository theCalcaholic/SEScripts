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
                    "<leftcontainer width='50%'>" +
                        "<menu>" +  
                            "<menuitem route='xml:'>Show Status</menuitem>" +
                            "<menuitem>Option 2</menuitem>" +
                        "</menu>" +
                        "<progressbar width='300' alignSelf='left' emptyString=' ' filledString='/' selectable value='0.3'/>" +
                    "</leftcontainer>" +
                    "<rightcontainer width='50%'>" +
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

            string axmlString = "<container flow='horizontal' width='100%'>..." +
                            "<leftcontainer width='50%'>" +
                                "<menu alignChildren='center'>" +
                                    "<menuitem route='xml:'>Show Status</menuitem>" +
                                    "<menuitem>Option 2</menuitem>" +
                                "</menu>" +
                            "</leftcontainer>..." +
                            "<rightcontainer width='50%'>" +
                            "<menu width='100%'>" +
                                "<menuitem route='xml:<uicontrols/><hl/>another page (yay!)'>Link</menuitem>" +
                                "<menuitem>this link is dead</menuitem>" +
                            "</menu>" +
                            "</rightcontainer>" +
                        "...</container>";

            //xmlString = "<container flow='horizontal'><containerleft>some random very long text</containerleft><containerright width='50%'>this text is also quite long</containerright></container>";
            TextUtils.SelectFont(TextUtils.FONT.DEFAULT);
            XMLTree tree = XMLWRAPPER.ParseXML(xmlString);
            TextUtils.SelectFont(TextUtils.FONT.MONOSPACE);
            Console.WriteLine("-------------------------------------------------------------");
            Console.WriteLine("Parsing Log:");
            Console.WriteLine("-------------------------------------------------------------");
            Console.WriteLine(Logger.Output);
            Logger.Clear();

            int maxWidth = 1000;
            int maxHeight = 60;

            IRenderBox renderBox = tree.GetRenderBox(maxWidth, maxHeight);
            Console.WriteLine("-------------------------------------------------------------");
            Console.WriteLine("Renderer retrieval Log:");
            Console.WriteLine("-------------------------------------------------------------");
            Console.WriteLine(Logger.Output);
            Logger.Clear();
            (renderBox as RenderBoxTree).Initialize(maxWidth, maxHeight);
            Console.WriteLine("-------------------------------------------------------------");
            Console.WriteLine("Renderer Initialization Log:");
            Console.WriteLine("-------------------------------------------------------------");
            Console.WriteLine(Logger.Output);
            Logger.Clear();
            StringBuilder result = new StringBuilder();
            foreach (StringBuilder line in renderBox.GetLines(maxWidth, maxHeight))
            {
                //logger.log("rendering line " + (i++), Logger.Mode.LOG);
                result.Append(line);
                result.Append("\n");
            }
            if (result.Length > 0)
                result.Remove(result.Length - 1, 1);
            
            Console.WriteLine("-------------------------------------------------------------");
            Console.WriteLine("Render Log:");
            Console.WriteLine("-------------------------------------------------------------");
            Console.WriteLine(Logger.Output);
            Console.WriteLine("-------------------------------------------------------------");
            Console.WriteLine("Result:");
            Console.WriteLine("-------------------------------------------------------------");
            Console.WriteLine(result);
        }
    }
}