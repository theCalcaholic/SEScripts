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
using SEScripts.ParseLib.XUI;

namespace SEScripts.MultiAgentNetwork.MAN.Protocols
{
    class PrintPlatformServicesProtocol : AgentProtocol
    {
        int State;
        public override string GetProtocolId()
        { return "print-platform-services"; }

        public PrintPlatformServicesProtocol(Agent agent) : base(agent)
        {
            Logger.log("Create new PrintPlatformServicesProtocol");
            State = 0;
        }

        public override void ReceiveMessage(AgentMessage msg)
        {
            Logger.log("PrintPlatformServicesProtocol.ReceiveMessage(message)");
            switch (State)
            {
                case 0:
                    Logger.log("Handling state 0");
                    if (Holder.Id.Platform == "local")
                    {
                        Logger.log("WARNING: PrintPlatformServicesProtocol started, but agent is not registered at any platform!");
                        Stop();
                        return;
                    }
                    else
                    {
                        AgentMessage newMsg = new AgentMessage(
                            Holder.Id,
                            new AgentId(Holder.Id.Platform + "@local"),
                            AgentMessage.StatusCodes.OK,
                            "",
                            "get-services",
                            ChatId
                        );
                        newMsg.SenderChatId = ChatId;
                        Holder.SendMessage(ref newMsg);
                        State = 1;
                    }
                    break;
                case 1:
                    Logger.log("Handling state 1");
                    if (msg.Status == AgentMessage.StatusCodes.OK)
                    {
                        List<XML.XMLTree> services = XML.ParseXML(msg.Content).GetAllNodes((node) => node.Type == "service");
                        Logger.log("Available Platform Services:");
                        foreach (XML.XMLTree service in services)
                        {
                            Logger.log("  " + service.GetAttribute("id"));
                        }
                    }
                    else
                    {
                        base.ReceiveMessage(msg);
                        Logger.log("An error occured in protocol PrintPlatformServicesProtocol: " + msg.Status.ToString());
                    }
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
            Holder.RegisterService(GetProtocolId(), (agent) => {
                return new PrintPlatformServicesProtocol(agent);
            });
        }
    }
    //EMBED SEScripts.MultiAgentNetwork.MAN.Protocols.AgentProtocol
    //EMBED SEScripts.MultiAgentNetwork.MAN.Agents.Agent
    //EMBED SEScripts.MultiAgentNetwork.MAN.Models.AgentMessage
    //EMBED SEScripts.MultiAgentNetwork.MAN.Models.AgentId
}
