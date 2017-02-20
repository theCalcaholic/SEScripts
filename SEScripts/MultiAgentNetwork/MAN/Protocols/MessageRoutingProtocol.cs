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
    class MessageRoutingProtocol : AgentProtocol
    {
        static public bool IsPlatform = false;

        public override string GetProtocolId()
        { return "route-message"; }

        public MessageRoutingProtocol(Agent agent) : base(agent) { }

        public override void ReceiveMessage(AgentMessage msg) { }

        public override void Restart() { }

        public override void Setup()
        {
            Holder.RegisterService(GetProtocolId(), (agent) => {
                return new MessageRoutingProtocol(agent);
            });
        }
    }

    //EMBED SEScripts.MultiAgentNetwork.MAN.Protocols.AgentProtocol
    //EMBED SEScripts.MultiAgentNetwork.MAN.Agents.Agent
    //EMBED SEScripts.MultiAgentNetwork.MAN.Models.AgentMessage
}
