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
		static IMyTextPanel DebugPanel;
		MANGridPlatformAgent LocalAgent;
		static MyGridProgram P;

		public Program()
		{
			Logger.DEBUG = true;
			P = this;
			List<IMyTerminalBlock> antennas = new List<IMyTerminalBlock>();
			GridTerminalSystem.GetBlocksOfType<IMyRadioAntenna>(antennas);
			LocalAgent = new MANGridPlatformAgent(this, new List<IMyTerminalBlock> { antennas[0] });
			LocalAgent.SetTimer(GridTerminalSystem.GetBlockWithName("Platform Timer") as IMyTimerBlock);
			LocalAgent.RefreshInterval = 2000;
			Echo(LocalAgent.Id.ToString());


			MANUIServerProtocol.CreateApplication(
				LocalAgent,
				"refresh",
				"Refresh Services",
				new Dictionary<string, Func<MANUIServerProtocol, MANAgentMessage, Dictionary<string, string>, string>> {
			{"", (session, request, data) =>
				{
					return "<menu>"+
						"<menuitem route='man:ALL@ALL::get-services()'>Refresh</menuitem>"+
					"</menu>";
				}
			}
				}
			);
		}

		public void Main(string argument)
		{
			using (var logger = new Logger("Program.Main()"))
			{
				if (argument == "refresh")
				{
					LocalAgent.Refresh(ElapsedTime);
					return;
				}
				logger.log("Main( " + argument + ")");
				List<string> parameters = XMLParser.ParamString2List(argument);

				if (parameters.Count > 0)
				{
					switch (parameters[0])
					{
						case "message":
							LocalAgent.ReceiveMessage(MANAgentMessage.FromXML(parameters[1]));
							break;
						case "register-buffer":
							LocalAgent.RegisterBuffers(new List<string> { "platform1 buffer" });
							break;
						case "refresh":
							LocalAgent.Refresh(ElapsedTime);
							break;
					}
				}
			}
			P.Me.CustomData = Logger.Output;
			Logger.Clear();
		}

		public void Save()
		{
			string data;
			LocalAgent.Save(out data);
			Storage = data;
		}
	}
}