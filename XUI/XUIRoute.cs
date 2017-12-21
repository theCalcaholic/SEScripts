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
	partial class Program
	{
		public class XUIRoute
		{
			static public Dictionary<string, Action<string, XUIController>> RouteHandlers = new Dictionary<string, Action<string, XUIController>>
			{
				{
					"revert", (def, controller) => { controller.RevertUI(); }
				},
				{
					"xml", (def, controller) =>
					{
						XMLTree ui = XML.ParseXML(XMLParser.UnescapeQuotes(def));
						controller.LoadUI(ui);
					}
				},
				{
					"fn", (def, controller) =>
					{
						if(UIFactories.ContainsKey(def))
						{
							UIFactories[def](controller);
						}
					}
				}
			};
			static Dictionary<string, Action<XUIController>> UIFactories =
				new Dictionary<string, Action<XUIController>>();

			string Definition;

			public XUIRoute(string definition)
			{
				Definition = definition;
			}

			public void Follow(XUIController controller)
			{
				string[] DefTypeAndValue = Definition.Split(new char[] { ':' }, 2);
				if (XUIRoute.RouteHandlers.ContainsKey(DefTypeAndValue[0].ToLower()))
				{
					XUIRoute.RouteHandlers[DefTypeAndValue[0].ToLower()](
						DefTypeAndValue.Length >= 2 ? DefTypeAndValue[1] : null, controller
					);
				}
			}

			static public void RegisterRouteFunction(string id, Action<XUIController> fn)
			{
				UIFactories[id] = fn;
			}
		}
	}
}