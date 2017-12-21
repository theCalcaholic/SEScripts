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
		public abstract class XUIFactory
		{
			private int Count;
			private int Max;
			private List<XUIController> UIs;

			public XUIFactory() : this(null) { }

			public XUIFactory(List<XUIController> uiList)
			{
				if (uiList == null)
				{
					UIs = new List<XUIController>();
				}
				UIs = uiList;
			}

			public abstract XMLTree Render(XUIController controller);

			protected void UpdateUIs(XMLTree renderedUI)
			{
				foreach (XUIController ui in UIs)
				{
					ui.LoadUI(renderedUI);
				}
			}
		}
	}
}