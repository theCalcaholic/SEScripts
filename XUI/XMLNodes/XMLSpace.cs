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
		public class XMLSpace : XMLTree
		{
			public XMLSpace() : base()
			{
				//Logger.debug("Space constructor()");
				//Logger.IncLvl();
				Type = "space";
				SetAttribute("width", "0");
				//Logger.DecLvl();
			}

			/*protected override void RenderText(ref List<string> segments, int width, int availableWidth)
			{
				//Logger.debug(Type + ".RenderText()");
				//Logger.IncLvl();
				segments.Add(TextUtils.CreateStringOfLength(" ", width));
				//Logger.DecLvl();
			}*/

			public override IRenderBox GetRenderBox(int containerWidth, int containerHeight)
			{
				//Logger.debug("GetRenderCache(int)");
				//Logger.IncLvl();
				IRenderBox cache = new RenderBoxLeaf();
				cache.type = Type;
				cache.MinHeight = 1;
				int width = ResolveSize(GetAttribute("width"), containerWidth) ?? 0;
				cache.MinWidth = width;
				cache.MaxWidth = width;
				//Logger.DecLvl();
				return cache;
			}
		}


		//EMBED SEScripts.XUI.XML.XMLTree
	}
}