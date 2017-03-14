using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;

using VRageMath;
using VRage.Game;
using VRage.Collections;
using Sandbox.ModAPI.Ingame;
using VRage.Game.Components;
using VRage.Game.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using Sandbox.Game.EntityComponents;
using SpaceEngineers.Game.ModAPI.Ingame;
using VRage.Game.ObjectBuilders.Definitions;

//using SEScripts.Lib;
using SEScripts.MultiAgentNetwork.MAN.Agents;
using SEScripts.MultiAgentNetwork.MAN.Models;
using SEScripts.ParseLib.XUI;

namespace SEScripts.MultiAgentNetwork.MAN.Protocols
{
    class ServiceRegistrationProtocol : AgentProtocol
    {
        public class Platform : AgentProtocol
        {
            public override string GetProtocolId()
            { return "register-services"; }

            public Platform(Agent agent) : base(agent) { }

            public override void ReceiveMessage(AgentMessage msg)
            {
                Logger.debug("ServiceRegistrationProtocol.ReceiveMessage(AgentMessage)");
                Logger.IncLvl();
                List<XML.XMLTree> services = XML.ParseXML(msg.Content).GetAllNodes((node) => (node.Type == "service"));
                PlatformAgent platform = Holder as PlatformAgent;
                AgentMessage response;
                if (platform == null)
                {
                    response = msg.MakeResponse(
                        Holder.Id,
                        AgentMessage.StatusCodes.UNKNOWNERROR,
                        "ERROR: Agent is no PlatformAgent - service registration not possible!"
                        );
                    response.SenderChatId = ChatId;
                    Holder.SendMessage(ref response);
                    Stop();
                    return;
                }
                AgentId sender = msg.Sender;
                sender.Platform = platform.Id.Platform;
                PlatformService service;
                foreach (XML.XMLTree serviceNode in services)
                {
                    service = PlatformService.FromXMLNode(serviceNode);
                    Logger.log("Register service '" + service.Id + "' of '" + sender + "'.");
                    if (service != null)
                    {

                        if (!platform.PlatformServices.ContainsKey(service.Id))
                        {
                            platform.PlatformServices[service.Id] = new List<PlatformService>();
                        }
                        platform.PlatformServices[service.Id].Add(service);
                    }
                }
                response = msg.MakeResponse(
                    Holder.Id,
                    AgentMessage.StatusCodes.OK,
                    "services registered"
                );
                response.SenderChatId = ChatId;
                Holder.SendMessage(ref response);
                Stop();
                Logger.DecLvl();
            }

            public override void Restart() { }

            public override void Setup()
            {
                Holder.RegisterService(GetProtocolId(), (agent) => {
                    return new Platform(agent);
                });
            }
        }


        public override string GetProtocolId()
        { return "complete-service-registration"; }

        public ServiceRegistrationProtocol(Agent agent) : base(agent) { }

        public override void ReceiveMessage(AgentMessage msg)
        {
            if (msg.Status == AgentMessage.StatusCodes.OK && msg.Content == "services registered")
            {
                Logger.log("Setting agent platform to '" + msg.Sender.Name + "'.");
                Holder.Id.Platform = msg.Sender.Name;
                Holder.Event("register");
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
            Holder.RegisterService(GetProtocolId(), (agent) => {
                return new ServiceRegistrationProtocol(agent);
            });
        }
    }

    //EMBED SEScripts.MultiAgentNetwork.MAN.Protocols.AgentProtocol
    //EMBED SEScripts.MultiAgentNetwork.MAN.Agents.PlatformAgent
    //EMBED SEScripts.MultiAgentNetwork.MAN.Agents.Agent
    //EMBED SEScripts.MultiAgentNetwork.MAN.Models.ServiceRegister
    //EMBED SEScripts.MultiAgentNetwork.MAN.Models.PlatformService
    //EMBED SEScripts.MultiAgentNetwork.MAN.Models.AgentMessage
    //EMBED SEScripts.MultiAgentNetwork.MAN.Models.AgentId
}
