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
		public abstract class XMLDataStore : XMLTree
		{
			public XMLDataStore() : base() { }

			public override Dictionary<string, string> GetValues(Func<XMLTree, bool> filter)
			{
				Dictionary<string, string> dict = base.GetValues(filter);
				if (!filter(this))
				{
					return dict;
				}
				foreach (KeyValuePair<string, string> data in Attributes)
				{
					if (!dict.ContainsKey(data.Key))
					{
						dict[data.Key] = data.Value;
					}
				}
				return dict;
			}
		}


		//EMBED SEScripts.XUI.XML.XMLTree
	}
}