using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System;
using VRage.Collections;
using VRage.Game.Components;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ObjectBuilders.Definitions;
using VRage.Game;
using VRageMath;

namespace IngameScript
{
	partial class Program : MyGridProgram
	{
		// This file contains your actual script.
		//
		// You can either keep all your code here, or you can create separate
		// code files to make your program easier to navigate while coding.
		//
		// In order to add a new utility class, right-click on your project, 
		// select 'New' then 'Add Item...'. Now find the 'Space Engineers'
		// category under 'Visual C# Items' on the left hand side, and select
		// 'Utility Class' in the main area. Name it in the box below, and
		// press OK. This utility class will be merged in with your code when
		// deploying your final script.
		//
		// You can also simply create a new utility class manually, you don't
		// have to use the template if you don't want to. Just do so the first
		// time to see what a utility class looks like.

		public Program()
		{
			// The constructor, called only once every session and
			// always before any other method is called. Use it to
			// initialize your script. 
			//     
			// The constructor is optional and can be removed if not
			// needed.
		}

		public void Save()
		{
			// Called when the program needs to save its state. Use
			// this method to save your state to the Storage field
			// or some other means. 
			// 
			// This method is optional and can be removed if not
			// needed.
		}

		public void Main(string argument)
		{
			// The main entry point of the script, invoked every time
			// one of the programmable block's Run actions are invoked.
			// 
			// The method itself is required, but the argument above
			// can be removed if not needed.

			string result;
			using (var logger = new Logger("Program.Main()", Logger.Mode.LOG))
			{
				//TextUtils.SelectFont(TextUtils.FONT.MONOSPACE);
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

				//xmlString = "<container flow='horizontal'><containerleft>some random very long text</containerleft><containerright width='50%'>this text is also quite long</containerright></container>";
				//Program.TextUtils.SelectFont(Program.TextUtils.FONT.DEFAULT);
				//XMLTree tree = XML.ParseXML(xmlString);
				XUIController ctrlr = XUIController.FromXML(xmlString);
				//TextUtils.SelectFont(TextUtils.FONT.MONOSPACE);

				//int maxWidth = 800;
				//int maxHeight = 60;

				//IRenderBox renderBox = tree.GetRenderBox(maxWidth, maxHeight);
				//result = renderBox.Render(maxWidth, maxHeight);
				//result = ctrlr.Render(maxWidth, maxHeight);
				var panels = new List<IMyTextPanel>();
				GridTerminalSystem.GetBlocksOfType(panels);
				//panels[0].WritePublicText(result);
				ctrlr.RenderTo(panels[0]);
				//ctrlr.ApplyScreenProperties(panels[0]);
			}
			Me.CustomData = Logger.Output;
			//Me.CustomData += "\n\n" + result;
			Logger.Clear();
		}
	}
}