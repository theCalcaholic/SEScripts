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

using SEScripts.Lib;
using SEScripts.MultiAgentFramework.MAF.Agents;
using SEScripts.MultiAgentFramework.MAF.Models;

namespace SEScripts.MultiAgentFramework.MAF.Protocols
{
    class ServiceRegistrationProtocol : AgentProtocol
    {
        public class Platform : AgentProtocol
        {
            public new static string Id
            {
                get { return "register-services"; }
            }

            public Platform(Agent agent) : base(agent) { }

            public override void ReceiveMessage(AgentMessage msg)
            {
                Logger.debug("ServiceRegistrationProtocol.ReceiveMessage(AgentMessage)");
                Logger.IncLvl();
                List<XML.XMLTree> services = XML.ParseXML(msg.Content).GetAllNodes((node) => (node.Type == "service"));
                PlatformAgent platform = Holder as PlatformAgent;
                if (platform == null)
                {
                    Holder.SendMessage(
                        msg.MakeResponse(
                            Holder.Id,
                            AgentMessage.StatusCodes.UNKNOWNERROR,
                            "ERROR: Agent is no PlatformAgent - service registration not possible!"
                        )
                    );
                    Stop();
                    return;
                }
                foreach (XML.XMLTree service in services)
                {
                    string serviceId = service.GetAttribute("id");
                    string serviceDesc = service.GetAttribute("description") ?? "";
                    AgentId sender = msg.Sender;
                    sender.Platform = platform.Id.Platform;
                    Logger.log("Register service '" + serviceId + "' of '" + sender.Id + "'.");
                    if (serviceId != null)
                    {
                        if (!platform.PlatformServices.ContainsKey(serviceId))
                        {
                            platform.PlatformServices[serviceId] = new List<PlatformService>();
                        }
                        platform.PlatformServices[serviceId].Add(new PlatformService(serviceId, serviceDesc, sender));
                    }
                }
                Holder.SendMessage(msg.MakeResponse(
                    Holder.Id,
                    AgentMessage.StatusCodes.OK,
                    "services registered"
                ));
                Stop();
                Logger.DecLvl();
            }

            public override void Restart() { }

            public static new void RegisterServices(Agent holder)
            {
                holder.RegisterService(Id, (agent) => {
                    return new Platform(agent);
                });
            }
        }


        public new static string Id
        {
            get { return "complete-service-registration"; }
        }

        public ServiceRegistrationProtocol(Agent agent) : base(agent) { }

        public override void ReceiveMessage(AgentMessage msg)
        {
            if (msg.Status == AgentMessage.StatusCodes.OK && msg.Content == "services registered")
            {
                Logger.log("Setting agent platform to '" + msg.Sender.Name + "'.");
                Holder.Id.Platform = msg.Sender.Name;
                Stop();
            }
            else
            {
                base.ReceiveMessage(msg);
            }
        }

        public override void Restart() { }

        public static new void RegisterServices(Agent holder)
        {
            holder.RegisterService(Id, (agent) => {
                return new ServiceRegistrationProtocol(agent);
            });
        }
    }

    //EMBED SEScripts.MultiAgentFramework.MAF.Protocols.AgentProtocol
    //EMBED SEScripts.MultiAgentFramework.MAF.Agents.PlatformAgent
    //EMBED SEScripts.MultiAgentFramework.MAF.Models.ServiceRegister
    //EMBED SEScripts.MultiAgentFramework.MAF.Models.PlatformService
}
