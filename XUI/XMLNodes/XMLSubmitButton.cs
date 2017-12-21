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
		public class XMLSubmitButton : XMLMenuItem
		{
			public XMLSubmitButton()
			{
				Type = "submitbutton";
				SetAttribute("flowdirection", "horizontal");
			}

			public override IRenderBox GetRenderBox(int containerWidth, int containerHeight)
			{
				RenderBoxTree cache = new RenderBoxTree();
				cache.type = Type;
				RenderBoxLeaf childCache = new RenderBoxLeaf(IsSelected() ? "[[  " : "[   ");
				childCache.MaxWidth = childCache.MinWidth;
				cache.Add(childCache);
				RenderBoxTree contentCache = new RenderBoxTree();
				contentCache.Flow = GetAttribute("flow") == "horizontal" ? IRenderBox.FlowDirection.HORIZONTAL : IRenderBox.FlowDirection.VERTICAL;
				UpdateRenderCacheProperties(cache, containerWidth, containerHeight);
				foreach (XMLTree child in Children)
				{
					contentCache.Add(child.GetRenderBox(Math.Max(cache._DesiredWidth, cache._DesiredWidth), Math.Max(cache._DesiredHeight, cache._MinHeight)));
				}
				cache.Add(contentCache);
				childCache = new RenderBoxLeaf(IsSelected() ? "  ]]" : "   ]");
				childCache.MaxWidth = childCache.MinWidth;
				cache.Add(childCache);


				cache.Flow = IRenderBox.FlowDirection.HORIZONTAL;
				
				return cache;
			}
		}
		
	}
}