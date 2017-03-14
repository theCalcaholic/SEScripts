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

using SEScripts.Lib.LoggerNS;
using SEScripts.MultiAgentNetwork.MAN.Protocols;
using SEScripts.MultiAgentNetwork.MAN.Models;

namespace SEScripts.MultiAgentNetwork.MAN.Agents
{
    public class RegisteredAgent : Agent
    {
        public RegisteredAgent(MyGridProgram program) : base(program)
        {
            if (Id.Platform != "local" && (GTS.GetBlockWithName(Id.Platform) as IMyProgrammableBlock) == null)
            {
                Id.Platform = "local";
            }

            new ServiceRegistrationProtocol(this).Setup();
            new PrintPlatformServicesProtocol(this).Setup();
        }

        public void RegisterWith(string platformName)
        {
            Logger.log("RegisteredAgent.RegisterWith()");
            Logger.IncLvl();
            AgentId platform = new AgentId(platformName + "@local");
            ServiceRegistrationProtocol chat = new ServiceRegistrationProtocol(this);
            Chats[chat.ChatId] = chat;
            string content = "<services>";
            foreach (KeyValuePair<string, Service> service in Services)
            {

                if(service.Value.HasPermissions(new AgentId("**@local")))
                {
                    content += service.Value.ToXML();

                }
            }
            content += "</services>";
            AgentMessage message = new AgentMessage(
                this.Id,
                platform,
                AgentMessage.StatusCodes.OK,
                content,
                new ServiceRegistrationProtocol.Platform(this).GetProtocolId(),
                chat.ChatId
            );
            Logger.log("Registering:");
            Logger.log("msg: " + message.ToXML());
            SendMessage(ref message);
            Logger.DecLvl();
        }
    }

    //EMBED SEScripts.MultiAgentNetwork.MAN.Agents.Agent
    //EMBED SEScripts.MultiAgentNetwork.MAN.Protocols.ServiceRegistrationProtocol
    //EMBED SEScripts.MultiAgentNetwork.MAN.Protocols.PrintPlatformServicesProtocol
}
