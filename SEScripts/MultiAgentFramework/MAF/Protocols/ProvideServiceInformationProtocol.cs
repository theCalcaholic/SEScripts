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
    class ProvideServiceInformationProtocol : AgentProtocol
    {
        public new static string Id
        {
            get { return "get-services"; }
        }

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
                string content = "<services>";
                foreach (PlatformService service in services)
                {
                    content += service.ToXML();
                }
                content += "</services>";
                AgentMessage message = new AgentMessage(
                    Holder.Id,
                    msg.Sender,
                    AgentMessage.StatusCodes.OK,
                    content,
                    "response",
                    ChatId
                );
                Holder.SendMessage(message);
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
                return new ProvideServiceInformationProtocol(agent);
            });
        }
    }
}
