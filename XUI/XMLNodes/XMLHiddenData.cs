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
		public class XMLHiddenData : XMLDataStore
		{
			public XMLHiddenData() : base()
			{
				Type = "hiddendata";
			}
			public override IRenderBox GetRenderBox(int maxWidth, int maxHeight)
			{
				IRenderBox cache = new RenderBoxTree();
				cache.type = Type;
				cache.MaxWidth = 0;
				cache.MaxHeight = 0;
				return cache;
			}
		}
	}
}