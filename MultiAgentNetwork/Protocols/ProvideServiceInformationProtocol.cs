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
		class MANProvideServiceInformationProtocol : MANAgentProtocol
		{
			public override string GetProtocolId()
			{ return "get-services"; }

			public MANProvideServiceInformationProtocol(MANAgent agent) : base(agent) { }

			public override void ReceiveMessage(MANAgentMessage msg)
			{
				if (msg.Status == MANAgentMessage.StatusCodes.OK)
				{
					List<MANPlatformService> services = new List<MANPlatformService>(Holder.Services.Values);
					MANPlatformAgent platform = Holder as MANPlatformAgent;
					if (platform != null)
					{
						foreach (List<MANPlatformService> platformServices in platform.PlatformServices.Values)
						{
							services.AddRange(platformServices);
						}
					}
					services = services.Uniques<MANPlatformService>();
					string content = "<platformInfo platformname='" + XMLParser.Sanitize(Holder.Prog.Me.CubeGrid.CustomName) + "'/>";
					content += "<services>";
					foreach (MANPlatformService service in services)
					{
						if ((msg.TargetInterface == MANAgentMessage.Interfaces.TEXT || service.ProvidesUI))//&& service.HasPermissions(msg.Sender))
						{
							content += service.ToXML();
						}
						else
						{
							if (!service.HasPermissions(msg.Sender))
							{
								//Logger.log("no permissions: " + service.Id);
							}
						}
					}
					content += "</services>";
					MANAgentMessage message = msg.MakeResponse(Holder.Id, MANAgentMessage.StatusCodes.OK, content);
					message.TargetInterface = MANAgentMessage.Interfaces.UI;
					message.SenderChatId = ChatId;
					Holder.ScheduleMessage(message);
					Stop();
				}
				else
				{
					base.ReceiveMessage(msg);
				}
			}


			public override void Restart() { }

			public override void Setup()
			{
				Holder.RegisterService(
					GetProtocolId(),
					(agent) =>
					{
						return new MANProvideServiceInformationProtocol(agent);
					},
					new Dictionary<string, string>
					{
					{"description", "List Services"}
					}
				);
			}
		}
	}
}