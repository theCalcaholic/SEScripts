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
		public class XMLHorizontalLine : XMLTree
		{
			public XMLHorizontalLine() : base()
			{
				Type = "hl";
				SetAttribute("width", "100%");
				SetAttribute("minheight", "1");
				SetAttribute("maxheight", "1");
			}

			public override IRenderBox GetRenderBox(int containerWidth, int containerHeight)
			{
				IRenderBox cache = new RenderBoxTree();
				UpdateRenderCacheProperties(cache, containerWidth, containerHeight);
				cache.type = Type;
				cache.PadChar = '_';
				cache.MinWidth = containerWidth;
				cache.MaxWidth = containerWidth;
				cache.MinHeight = 1;
				cache.MaxHeight = 1;
				return cache;
			}
		}

		//EMBED SEScripts.XUI.XML.XMLTree
	}
}