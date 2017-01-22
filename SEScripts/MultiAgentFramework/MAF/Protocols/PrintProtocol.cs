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
    public class PrintProtocol : AgentProtocol
    {
        public new static string Id
        {
            get { return "print"; }
        }

        public PrintProtocol(Agent agent) : base(agent) { }

        public override void ReceiveMessage(AgentMessage msg)
        {
            Logger.log("Message received:");
            Logger.IncLvl();
            Logger.log("from: " + msg.Sender.Id);
            Logger.log("content: " + msg.Content);
            Logger.DecLvl();
            Stop();
        }

        public override void Restart() { }

        public static new void RegisterServices(Agent holder)
        {
            holder.RegisterService(Id, (agent) => {
                return new PrintProtocol(agent);
            });
        }
    }

    //EMBED SEScripts.MultiAgentFramework.MAF.Protocols.AgentProtocol
}
