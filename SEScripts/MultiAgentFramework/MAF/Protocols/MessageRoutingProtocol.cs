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
    class MessageRoutingProtocol : AgentProtocol
    {
        static public bool IsPlatform = false;

        public new static string Id
        {
            get { return "route-message"; }
        }

        public MessageRoutingProtocol(Agent agent) : base(agent) { }

        public override void ReceiveMessage(AgentMessage msg) { }

        public override void Restart() { }

        public static new void RegisterServices(Agent holder)
        {
            holder.RegisterService(Id, (agent) => {
                return new MessageRoutingProtocol(agent);
            });
        }
    }

    //EMBED SEScripts.MultiAgentFramework.MAF.Protocols.AgentProtocol
}
