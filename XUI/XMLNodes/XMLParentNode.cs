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
		public interface XMLParentNode
		{
			bool HasUserInputBindings { get; set; }
			XMLParentNode GetParent();

			void UpdateSelectability(XMLTree child);

			void KeyPress(string keyCode);

			void FollowRoute(XUIRoute route);

			bool SelectNext();

			void DetachChild(XMLTree child);
		}

		//EMBED SEScripts.XUI.XML.XMLTree
	}
}