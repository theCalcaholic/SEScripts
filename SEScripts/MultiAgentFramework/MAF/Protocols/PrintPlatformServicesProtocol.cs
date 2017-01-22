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

namespace SEScripts.MultiAgentFramework.MAF.Protocols
{
    class PrintPlatformServicesProtocol : AgentProtocol
    {
        int State;
        public new static string Id
        {
            get { return "print-platform-services"; }
        }

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
                        Holder.SendMessage(new AgentMessage(
                            Holder.Id,
                            new AgentId(Holder.Id.Platform + "@local"),
                            AgentMessage.StatusCodes.OK,
                            "",
                            "get-services",
                            ChatId
                        ));
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

        public static new void RegisterServices(Agent holder)
        {
            holder.RegisterService(Id, (agent) => {
                return new PrintPlatformServicesProtocol(agent);
            });
        }
    }
}
