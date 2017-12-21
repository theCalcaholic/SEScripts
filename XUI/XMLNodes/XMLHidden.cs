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
		public class XMLHidden : XMLTree
		{
			public XMLHidden() : base()
			{
				Type = "hidden";
			}
			/*protected override string PostRender(List<string> segments, int width, int availableWidth)
			{
				return null;
			}*/

			public override IRenderBox GetRenderBox(int maxWidth, int maxHeight)
			{
				//using (new Logger("Hidden.GetRenderCache(int)", Logger.Mode.LOG))
				//{
				IRenderBox cache = new RenderBoxTree();
				cache.type = Type;
				cache.MaxWidth = 0;
				cache.MaxHeight = 0;
				return cache;
				//}
			}
		}


		//EMBED SEScripts.XUI.XML.DataStore
	}
}