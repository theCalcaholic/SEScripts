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
		string AgentTimerName = "UITerminal Timer";

		MANUITerminalAgent LocalAgent;

		public Program()
		{
			Logger.DEBUG = false;
			try
			{
				using (var logger = new Logger("Program constructor()"))
				{
					List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();
					GridTerminalSystem.SearchBlocksOfName("GridMaster", blocks,
						(block) => (block as IMyProgrammableBlock != null));
					string platformName = blocks[0].CustomName;
					IMyTextPanel uITerminal = GridTerminalSystem.GetBlockWithName("UITerminal") as IMyTextPanel;
					LocalAgent = new MANUITerminalAgent(this, uITerminal);
					LocalAgent.SetTimer(GridTerminalSystem.GetBlockWithName(AgentTimerName) as IMyTimerBlock);
					LocalAgent.RegisterWith(platformName);
					logger.log("instructions: " + Runtime.CurrentInstructionCount + "/" + Runtime.MaxInstructionCount);
				}
				Me.CustomData = Logger.Output.Substring(Math.Max(0, Logger.Output.Length - 90000));
			}
			catch(Exception e)
			{
				throw new Exception("An exception occured: " + e.Message);
			}
			finally
			{
				Me.CustomData = Logger.Output.Substring(Math.Max(0, Logger.Output.Length - 90000));
			}
		}

		public void Main(string argument)
		{
			Logger.Clear();
			using (var logger = new Logger("Program.Main()"))
			{
				Echo(argument);
				logger.log("instructions: " + Runtime.CurrentInstructionCount + "/" + Runtime.MaxInstructionCount);
				List<string> parameters = XMLParser.ParamString2List(argument);

				if (parameters.Count > 0)
				{
					switch (parameters[0])
					{
						case "message":
							LocalAgent.ReceiveMessage(MANAgentMessage.FromXML(parameters[1]));
							break;
						case "refresh":
							LocalAgent.Refresh(ElapsedTime);
							break;
						default:
							try
							{
								LocalAgent.Call(parameters);
							}
							catch (Exception e)
							{
								throw new Exception(e.StackTrace);
							}
							break;
					}
				}
				logger.log("instructions: " + Runtime.CurrentInstructionCount + "/" + Runtime.MaxInstructionCount);
			}
			Me.CustomData = Logger.Output;

		}

		public void Save()
		{
			string data;
			LocalAgent.Save(out data);
			Storage = data;
		}

	}
}