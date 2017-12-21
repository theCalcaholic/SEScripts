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
		public class XMLLineBreak : XMLTextNode
		{
			public XMLLineBreak() : base("")
			{
				Type = "br";
			}

			public override IRenderBox GetRenderBox(int maxWidth, int maxHeight)
			{
				IRenderBox cache = new RenderBoxLeaf("\n");
				cache.type = Type;
				cache.MaxHeight = (GetParent() as XMLTree)?.GetAttribute("flow") == "horizontal" ? 1 : 0;
				cache.MaxWidth = 0;
				return cache;
			}
		}
	}
}
