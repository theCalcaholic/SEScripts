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
		public class MANGetHangarDoorsProtocol : MANAgentProtocol
		{
			static List<string> HangarDoorGroups = new List<string>();

			public override string GetProtocolId()
			{ return "get-hangar-doors"; }

			public MANGetHangarDoorsProtocol(MANAgent agent) : base(agent) { }
			public override void Restart() { }

			public override void ReceiveMessage(MANAgentMessage msg)
			{
				if (msg.Status != MANAgentMessage.StatusCodes.OK)
				{
					base.ReceiveMessage(msg);
					return;
				}

				MANAgentMessage response;
				if (msg.TargetInterface == MANAgentMessage.Interfaces.UI)
				{
					string content = "<menu>";
					foreach (string groupName in HangarDoorGroups)
					{
						content += "<menuitem route='" + MakeRoute(Holder.Id, GetProtocolId(), groupName) + "'>" + groupName + "</menuitem>";
					}
					content += "</menu>";

					response = msg.MakeResponse(Holder.Id, MANAgentMessage.StatusCodes.OK, content);
				}
				else
					response = msg.MakeResponse(Holder.Id, MANAgentMessage.StatusCodes.OK, string.Join(",", HangarDoorGroups));

				response.SenderChatId = ChatId;
				Holder.SendMessage(ref response);
				Stop();
			}

			public override void Setup()
			{
				Holder.RegisterService(
					GetProtocolId(),
					(holder) => new MANGetHangarDoorsProtocol(holder),
					new Dictionary<string, string>
					{
					{"description", "List Hangars" },
					{"permissions", "ANY@ANY" },
					{"providesui", "true" }
					});
			}

			public static void AddHangarDoorGroup(string groupName)
			{
				HangarDoorGroups.Add(groupName);
			}

			public static void RemoveHangarDoorGroup(string groupName)
			{
				HangarDoorGroups.Remove(groupName);
			}
		}
	}
}