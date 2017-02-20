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
using SEScripts.MultiAgentNetwork.MAN.Agents;
using SEScripts.MultiAgentNetwork.MAN.Models;

namespace SEScripts.MultiAgentNetwork.MAN.Protocols
{
    class ProvideServiceInformationProtocol : AgentProtocol
    {
        public override string GetProtocolId()
        { return "get-services"; }

        public ProvideServiceInformationProtocol(Agent agent) : base(agent) { }

        public override void ReceiveMessage(AgentMessage msg)
        {
            if (msg.Status == AgentMessage.StatusCodes.OK)
            {
                List<PlatformService> services = new List<PlatformService>(Holder.Services.Values);
                PlatformAgent platform = Holder as PlatformAgent;
                if (platform != null)
                {
                    foreach (List<PlatformService> platformServices in platform.PlatformServices.Values)
                    {
                        services.AddRange(platformServices);
                    }
                }
                services = Util.Uniques<PlatformService>(services);
                string content = "<platformInfo platformname='" + Parser.Sanitize(Holder.Prog.Me.CubeGrid.CustomName) + "'/>";
                content += "<services>";
                foreach (PlatformService service in services)
                {
                    if((msg.TargetInterface == AgentMessage.Interfaces.TEXT || service.ProvidesUI) )//&& service.HasPermissions(msg.Sender))
                    {
                        content += service.ToXML();
                    }
                    else
                    {
                        if(!service.HasPermissions(msg.Sender))
                        {
                            Logger.log("no permissions: " + service.Id);
                        }
                    }
                }
                content += "</services>";
                AgentMessage message = msg.MakeResponse(Holder.Id, AgentMessage.StatusCodes.OK, content);
                message.TargetInterface = AgentMessage.Interfaces.UI;
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
                (agent) => {
                    return new ProvideServiceInformationProtocol(agent);
                },
                new Dictionary<string, string>
                {
                    {"description", "List Services"}
                }
            );
        }
    }
    //EMBED SEScripts.MultiAgentNetwork.MAN.Protocols.AgentProtocol
    //EMBED SEScripts.MultiAgentNetwork.MAN.Agents.PlatformAgent
    //EMBED SEScripts.MultiAgentNetwork.MAN.Agents.Agent
    //EMBED SEScripts.MultiAgentNetwork.MAN.Models.AgentMessage
    //EMBED SEScripts.MultiAgentNetwork.MAN.Models.PlatformService

}
