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
		class MANPrintPlatformServicesProtocol : MANAgentProtocol
		{
			int State;
			public override string GetProtocolId()
			{ return "print-platform-services"; }

			public MANPrintPlatformServicesProtocol(MANAgent agent) : base(agent)
			{
				State = 0;
			}

			public override void ReceiveMessage(MANAgentMessage msg)
			{
				switch (State)
				{
					case 0:
						if (Holder.Id.Platform == "local")
						{
							Stop();
							return;
						}
						else
						{
							MANAgentMessage newMsg = new MANAgentMessage(
								Holder.Id,
								new MANAgentId(Holder.Id.Platform + "@local"),
								MANAgentMessage.StatusCodes.OK,
								"",
								"get-services",
								ChatId
							)
							{
								SenderChatId = ChatId
							};
							Holder.SendMessage(ref newMsg);
							State = 1;
						}
						break;
					case 1:
						if (msg.Status == MANAgentMessage.StatusCodes.OK)
						{
							List<XMLTree> services = XML.ParseXML(msg.Content).GetAllNodes((node) => node.Type == "service");
						}
						else
							base.ReceiveMessage(msg);
						Stop();
						break;
				}
			}

			public override void Restart()
			{
				State = 0;
				ReceiveMessage(null);
			}

			public override void Setup()
			{
				Holder.RegisterService(GetProtocolId(), (agent) =>
				{
					return new MANPrintPlatformServicesProtocol(agent);
				});
			}
		}
	}
}