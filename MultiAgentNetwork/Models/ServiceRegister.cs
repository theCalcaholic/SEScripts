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
	partial class Program : MyGridProgram
	{
		public class MANServiceRegister : Dictionary<string, List<MANPlatformService>>
		{
			public MANServiceRegister Merge(MANServiceRegister other)
			{
				MANServiceRegister merged = new MANServiceRegister();
				foreach (KeyValuePair<string, List<MANPlatformService>> item in this)
				{
					merged.Add(item.Key, item.Value);
				}
				foreach (KeyValuePair<string, List<MANPlatformService>> item in other)
				{
					if (merged.ContainsKey(item.Key))
					{
						merged[item.Key].AddList<MANPlatformService>(item.Value);
						merged[item.Key] = merged[item.Key].Uniques<MANPlatformService>();
					}
					else
					{
						merged[item.Key] = item.Value;
					}
				}
				return merged;
			}

			public string ToXML()
			{
				string xml = "<services>";
				foreach (KeyValuePair<string, List<MANPlatformService>> item in this)
				{
					foreach (MANPlatformService service in item.Value)
					{
						xml += service.ToXML();
					}
				}
				xml += "</services>";
				return xml;
			}
		}
	}
}