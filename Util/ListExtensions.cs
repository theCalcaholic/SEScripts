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
	public static class ListExtensions
	{
		public static List<T> Uniques<T>(this List<T> list)
		{
			List<T> listOut = new List<T>();
			bool duplicate;
			foreach (T itemIn in list)
			{
				duplicate = false;
				foreach (T itemOut in listOut)
				{
					if (itemOut.Equals(itemIn))
					{
						duplicate = true;
					}
				}
				if (!duplicate)
				{
					listOut.Add(itemIn);
				}
			}
			return listOut;
		}
	}
}