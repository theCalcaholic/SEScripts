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
using SEScripts.MultiAgentNetwork.MAN.Agents;
using SEScripts.MultiAgentNetwork.MAN.Models;

namespace SEScripts.MultiAgentNetwork.MAN.Protocols
{
    public class PrintProtocol : AgentProtocol
    {
        public override string GetProtocolId()
        { return "print"; }

        public PrintProtocol(Agent agent) : base(agent) { }

        public override void ReceiveMessage(AgentMessage msg)
        {
            Logger.log("Message received:");
            Logger.IncLvl();
            Logger.log("from: " + msg.Sender);
            Logger.log("content: " + msg.Content);
            Logger.DecLvl();
            Stop();
        }

        public override void Restart() { }

        public override void Setup()
        {
            Holder.RegisterService(GetProtocolId(), (agent) => {
                return new PrintProtocol(agent);
            });
        }
    }

    //EMBED SEScripts.MultiAgentNetwork.MAN.Protocols.AgentProtocol
    //EMBED SEScripts.MultiAgentNetwork.MAN.Agents.Agent
    //EMBED SEScripts.MultiAgentNetwork.MAN.Models.AgentMessage
}
