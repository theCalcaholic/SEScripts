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
		public class MANGridPlatformAgent : MANPlatformAgent
		{
			List<IMyLaserAntenna> LaserAntennas;
			List<IMyRadioAntenna> RadioAntennas;
			public MANGridPlatformAgent(MyGridProgram program, List<IMyTerminalBlock> antennas) : base(program)
			{
				LaserAntennas = new List<IMyLaserAntenna>();
				RadioAntennas = new List<IMyRadioAntenna>();

				foreach (IMyTerminalBlock antenna in antennas)
				{
					IMyLaserAntenna laser = antenna as IMyLaserAntenna;
					IMyRadioAntenna radio = antenna as IMyRadioAntenna;
					if (laser != null)
					{
						LaserAntennas.Add(laser);
					}
					else if (radio != null)
					{
						RadioAntennas.Add(radio);
					}
				}
				Id = GenerateId(program.Me);
				program.Me.CustomName = Id.Name;
			}

			public override MANAgentId GenerateId(IMyProgrammableBlock me)
			{
				if (!Id.Name.Contains("GridMaster"))
				{
					me.CustomName = "GridMaster";
					return base.GenerateId(me);
				}
				return Id;
			}

			public override bool SendMessage(ref MANAgentMessage message)
			{
				bool baseSuccess = base.SendMessage(ref message);
				if (message.Receiver.Platform == "ALL" || !baseSuccess)
					return SendToGrid(message) || baseSuccess;
				return true;
			}

			public bool SendToGrid(MANAgentMessage message)
			{
				if (message.Receiver.Platform == Id.Platform || message.Receiver.Platform == "local")
					return false;

				if (message.Receiver.Platform == "ALL" || message.Receiver.Platform == "ANY")
				{
					//Logger.debug("Receiver platform set to local...");
					message.Receiver = new MANAgentId(message.Receiver.Name + "@local");
					//Logger.debug("Receiver id was set to: " + message.Receiver.ToString());
				}

				foreach (IMyRadioAntenna antenna in RadioAntennas)
				{
					if (antenna.TransmitMessage("message \"" + message.ToString() + "\"", MyTransmitTarget.Everyone))
					{
						//Logger.log("Sending was successful.");
						//Logger.DecLvl();
						return true;
					}
				}
				//Logger.log("Could not send message by antenna...");

				ScheduleMessage(message);
				//Logger.log("Message scheduled");
				if (ScheduledMessages.Count > 0)
				{
					//Logger.log("Schedule refresh");
					ScheduleRefresh();
				}
				//Logger.log("Sent to grid (finished)");
				return true;
			}
		}
	}
}