using Microsoft.VisualStudio.TestTools.UnitTesting;
using SEScripts.XUI;
using System;
using System.Threading;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IngameScript;
using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System.Collections;
using VRage.Collections;
using VRage.Game.Components;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ObjectBuilders.Definitions;
using VRage.Game;
using VRageMath;

namespace SEScripts.XUI.Tests
{
	[TestClass()]
	public class XMLTests
	{

		[TestInitialize()]
		public void Initialize()
		{
			Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
		}

		[TestMethod()]
		public void ParseXMLTest()
		{
			//Logger.debug = true;
			Program.TextUtils.SelectFont(Program.TextUtils.FONT.MONOSPACE);
			string xmlString = "<meta fontsize='1.0' fontcolor='00AA00' fontfamily='Monospace'/>" +
				"<uicontrols alignSelf='center'>Main Menu</uicontrols>" +
				"<hl/>" +
				"<container flow='horizontal' width='100%'>" +
					"<leftcontainer alignChildren='center' width='50%'>" +
						"<menu>" +
							"<menuitem route='xml:'>Show Status</menuitem>" +
							"<menuitem>Option 2</menuitem>" +
						"</menu>" +
						"<progressbar width='300' emptyString=' ' filledString='/' selectable value='0.3'/>" +
					"</leftcontainer>" +
					"<rightcontainer width='50%'>" +
					"<menu alignChildren='right'>" +
						"<menuitem route='xml:<uicontrols/><hl/>another page (yay!)'>Link</menuitem>" +
						"<menuitem >this link is dead</menuitem>" +
					"</menu>" +
					"</rightcontainer>" +
				"</container>" +
				"some text which is very long to test a new feature of the ui system. Text should now wrap automatically. Very cool indeed." +
				"<hl/>" +
				"<br />" +
				"<textinput inputBinding='value' maxLength='10'/>";

			string axmlString = "<container flow='horizontal' width='100%'>..." +
							"<leftcontainer alignChildren='center' width='50%'>" +
								"<menu>" +
									"<menuitem route='xml:'>Show Status</menuitem>" +
									"<menuitem>Option 2</menuitem>" +
								"</menu>" +
							"</leftcontainer>..." +
							"<rightcontainer width='50%'>" +
							"<menu>" +
								"<menuitem route='xml:<uicontrols/><hl/>another page (yay!)'>Link</menuitem>" +
								"<menuitem>this link is dead</menuitem>" +
							"</menu>" +
							"</rightcontainer>" +
						"...</container>";

			//xmlString = "<container flow='horizontal'><containerleft>some random very long text</containerleft><containerright width='50%'>this text is also quite long</containerright></container>";
			//Program.TextUtils.SelectFont(Program.TextUtils.FONT.DEFAULT);
			Program.XMLTree tree = Program.XML.ParseXML(xmlString);
			Program.TextUtils.SelectFont(Program.TextUtils.FONT.MONOSPACE);
			Console.WriteLine("-------------------------------------------------------------");
			Console.WriteLine("Parsing Log:");
			Console.WriteLine("-------------------------------------------------------------");
			Console.WriteLine(Program.Logger.Output);
			Program.Logger.Clear();

			int maxWidth = 800;
			int maxHeight = 60;

			Program.IRenderBox renderBox = tree.GetRenderBox(maxWidth, maxHeight);
			Console.WriteLine("-------------------------------------------------------------");
			Console.WriteLine("Renderer retrieval Log:");
			Console.WriteLine("-------------------------------------------------------------");
			Console.WriteLine(Program.Logger.Output);
			Program.Logger.Clear();
			/*(renderBox as Program.RenderBoxTree).Initialize(maxWidth, maxHeight);
			Console.WriteLine("-------------------------------------------------------------");
			Console.WriteLine("Renderer Initialization Log:");
			Console.WriteLine("-------------------------------------------------------------");
			Console.WriteLine(Program.Logger.Output);
			Program.Logger.Clear();
			StringBuilder result = new StringBuilder();
			foreach (StringBuilder line in renderBox.GetLines(maxWidth, maxHeight))
			{
				//logger.log("rendering line " + (i++), Logger.Mode.LOG);
				result.Append(line);
				result.Append("\n");
			}
			if (result.Length > 0)
				result.Remove(result.Length - 1, 1);*/
			var result = renderBox.Render(maxWidth, maxHeight);
			Assert.AreEqual(
				"           Main Menu            \n" +
				"________________________________\n" +
				"   >>Show Status            Link\n" +
				"     Option 2          this link\n" +
				" [////           ]     is dead  \n" +
				"some text which is very long to \n" +
				"test a new feature of the ui    \n" +
				"system. Text should now wrap    \n" +
				"automatically. Very cool indeed.\n" +
				"________________________________\n" +
				"_                               ", result);

			Console.WriteLine("-------------------------------------------------------------");
			Console.WriteLine("Render Log:");
			Console.WriteLine("-------------------------------------------------------------");
			Console.WriteLine(Program.Logger.Output);
			Console.WriteLine("-------------------------------------------------------------");
			Console.WriteLine("Result:");
			Console.WriteLine("-------------------------------------------------------------");
			Console.WriteLine(result);
		}
	}
}