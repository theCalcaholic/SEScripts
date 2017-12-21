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
		class MANServiceRegistrationProtocol : MANAgentProtocol
		{
			public class Platform : MANAgentProtocol
			{
				public override string GetProtocolId()
				{ return "register-services"; }

				public Platform(MANAgent agent) : base(agent) { }

				public override void ReceiveMessage(MANAgentMessage msg)
				{
					List<XMLTree> services = XML.ParseXML(msg.Content).GetAllNodes((node) => (node.Type == "service"));
					MANPlatformAgent platform = Holder as MANPlatformAgent;
					MANAgentMessage response;
					if (platform == null)
					{
						response = msg.MakeResponse(
							Holder.Id,
							MANAgentMessage.StatusCodes.UNKNOWNERROR,
							"ERROR: Agent is no PlatformAgent - service registration not possible!"
							);
						response.SenderChatId = ChatId;
						Holder.SendMessage(ref response);
						Stop();
						return;
					}
					MANAgentId sender = msg.Sender;
					sender.Platform = platform.Id.Platform;
					MANPlatformService service;
					foreach (XMLTree serviceNode in services)
					{
						service = MANPlatformService.FromXMLNode(serviceNode);
						if (service != null)
						{

							if (!platform.PlatformServices.ContainsKey(service.Id))
								platform.PlatformServices[service.Id] = new List<MANPlatformService>();
							platform.PlatformServices[service.Id].Add(service);
						}
					}
					response = msg.MakeResponse(
						Holder.Id,
						MANAgentMessage.StatusCodes.OK,
						"services registered"
					);
					response.SenderChatId = ChatId;
					Holder.SendMessage(ref response);
					Stop();
				}

				public override void Restart() { }

				public override void Setup()
				{
					Holder.RegisterService(GetProtocolId(), (agent) =>
					{
						return new Platform(agent);
					});
				}
			}


			public override string GetProtocolId()
			{ return "complete-service-registration"; }

			public MANServiceRegistrationProtocol(MANAgent agent) : base(agent) { }

			public override void ReceiveMessage(MANAgentMessage msg)
			{
				if (msg.Status == MANAgentMessage.StatusCodes.OK && msg.Content == "services registered")
				{
					Holder.Id.Platform = msg.Sender.Name;
					Holder.Event("register");
					Stop();
				}
				else
					base.ReceiveMessage(msg);
			}

			public override void Restart() { }

			public override void Setup()
			{
				Holder.RegisterService(GetProtocolId(), (agent) =>
				{
					return new MANServiceRegistrationProtocol(agent);
				});
			}
		}
	}
}