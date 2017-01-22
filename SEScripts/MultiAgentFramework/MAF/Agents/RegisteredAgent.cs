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
using SEScripts.MultiAgentFramework.MAF.Protocols;
using SEScripts.MultiAgentFramework.MAF.Models;

namespace SEScripts.MultiAgentFramework.MAF.Agents
{
    public class RegisteredAgent : Agent
    {
        public RegisteredAgent(MyGridProgram program) : base(program)
        {
            if (Id.Platform != "local" && (GTS.GetBlockWithName(Id.Platform) as IMyProgrammableBlock) == null)
            {
                Id.Platform = "local";
            }

            ServiceRegistrationProtocol.RegisterServices(this);
            PrintPlatformServicesProtocol.RegisterServices(this);
        }

        public void RegisterWith(string platformName)
        {
            AgentId platform = new AgentId(platformName + "@local");
            ServiceRegistrationProtocol chat = new ServiceRegistrationProtocol(this);
            Chats[chat.ChatId] = chat;
            string content = "<services>";
            foreach (string service in Services.Keys)
            {
                content += "<service id='" + service + "'/>";
            }
            content += "</services>";
            AgentMessage message = new AgentMessage(
                this.Id,
                platform,
                AgentMessage.StatusCodes.OK,
                content,
                ServiceRegistrationProtocol.Platform.Id,
                chat.ChatId
            );
            SendMessage(message);
        }
    }

    //EMBED SEScripts.MultiAgentFramework.MAF.Agents.Agent
    //EMBED SEScripts.MultiAgentFramework.MAF.Protocols.ServiceRegistrationProtocol
    //EMBED SEScripts.MultiAgentFramework.MAF.Protocols.PrintPlatformServicesProtocol
}
