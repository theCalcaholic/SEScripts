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
		class XMLMetaNode : XMLHidden
		{
			public XMLMetaNode() : base()
			{
				Type = "meta";
			}

			public override Dictionary<string, string> GetValues(Func<XMLTree, bool> filter)
			{
				if (filter(this))
				{
					return Attributes;
				}
				else
				{
					return new Dictionary<string, string>();
				}
			}

			public override void SetAttribute(string key, string value)
			{
				long fontValue;
				if (key.ToLower() == "fontfamily" && long.TryParse(value, out fontValue))
				{
					if (fontValue == 1147350002)
						TextUtils.SelectFont(TextUtils.FONT.MONOSPACE);
					else
						TextUtils.SelectFont(TextUtils.FONT.DEFAULT);
				}
				base.SetAttribute(key, value);
			}
		}
	}
}