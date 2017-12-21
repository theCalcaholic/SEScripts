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
		public class XMLVerticalLine : XMLTree
		{
			public XMLVerticalLine() : base()
			{
				Type = "vl";
				SetAttribute("height", "100%");
				//SetAttribute("minwidth", "4");
			}

			/*protected override void RenderText(ref List<string> segments, int width, int availableWidth)
			{
				segments.Add(TextUtils.CreateStringOfLength("_", width, TextUtils.RoundMode.CEIL));
			}*/

			public override IRenderBox GetRenderBox(int containerWidth, int containerHeight)
			{
				//using (new Logger("VerticalLine.GetRenderBox()"))
				//{
				IRenderBox cache = new RenderBoxLeaf();
				cache.PadChar = '|';
				cache.type = Type;
				//cache.Add("_");
				UpdateRenderCacheProperties(cache, containerWidth, containerHeight);
				cache.MinWidth = TextUtils.GetCharWidth('|');
				return cache;
				//}
			}
		}

		//EMBED SEScripts.XUI.XML.XMLTree
	}
}