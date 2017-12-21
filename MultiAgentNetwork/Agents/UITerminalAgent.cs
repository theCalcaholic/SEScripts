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
		public class MANUITerminalAgent : MANRegisteredAgent
		{
			public XUIController UI;
			public IMyTextPanel Screen;
			const string loadingPage = "<meta historyDisabled/><hl width='100%'/>Requesting Services...<hl width='100%'/>";

			public MANUITerminalAgent(MyGridProgram program, IMyTextPanel screen) : base(program)
			{
				Screen = screen;
				UI = XUIController.FromXML(loadingPage);
				new MANRequestRouteProtocol(this).Setup();
				MANUIServiceIndexServer indexServer = new MANUIServiceIndexServer(this);
				indexServer.Setup();
				indexServer.LoadHomeScreen();
			}

			public void LoadXML(string uiString)
			{
				UI.LoadXML(uiString);
			}

			public void LoadUI(XMLTree ui)
			{
				UI.LoadUI(ui);
				if (UI.HasUserInputBindings)
					ScheduleRefresh();
			}

			public void UpdateScreen()
			{
				UI.RenderTo(Screen);
				UI.ApplyScreenProperties(Screen);
			}

			public void Call(List<string> parameters)
			{
				UI.Call(parameters);
				UpdateScreen();
				if (UI.HasUserInputBindings)
					ScheduleRefresh();
			}

			public override void Refresh(TimeSpan elapsedTime)
			{
				base.Refresh(elapsedTime);
				UI.Call(new List<string> { "refresh" });
				if (UI.UpdateUserInput())
					UpdateScreen();
			}
		}
	}
}