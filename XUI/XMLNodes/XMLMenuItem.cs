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
		public class XMLMenuItem : XMLTree
		{
			XUIRoute TargetRoute;

			public XMLMenuItem() : this(null) { }

			public XMLMenuItem(XUIRoute targetRoute) : base()
			{
				Type = "menuitem";
				Selectable = true;
				SetRoute(targetRoute);
				PreventDefault("RIGHT/SUBMIT");
			}

			public override void SetAttribute(string key, string value)
			{
				switch (key)
				{
					case "route":
						SetRoute(new XUIRoute(value));

						break;
					default:
						base.SetAttribute(key, value);
						break;
				}

				////Logger.DecLvl();
			}

			public override void OnKeyPressed(string keyCode)
			{
				//Logger.debug(Type + ": OnKeyPressed():");
				//using (Logger logger = new Logger("MenuItem.OnKeyPressed(string)", Logger.Mode.LOG))
				//{
				switch (keyCode)
				{
					case "RIGHT/SUBMIT":
						if (TargetRoute != null)
						{
							//Logger.debug("Follow Target Route!");
							FollowRoute(TargetRoute);
						}
						else
						{
							//logger.log("target route is null!", Logger.Mode.WARNING);
							//Logger.debug("No route set!");
						}

						break;
				}

				base.OnKeyPressed(keyCode);
				//Logger.DecLvl();
				//}
			}

			public void SetRoute(XUIRoute route)
			{
				TargetRoute = route;
			}

		}

		//EMBED SEScripts.XUI.XML.XMLTree
		//EMBED SEScripts.XUI.XML.Route
	}
}