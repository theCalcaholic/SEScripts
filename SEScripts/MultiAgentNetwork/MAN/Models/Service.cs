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
using SEScripts.MultiAgentNetwork.MAN.Protocols;

namespace SEScripts.MultiAgentNetwork.MAN.Models
{
    public class Service : PlatformService
    {
        public Func<Agent, AgentProtocol> Create;

        public Service(string id, string description, AgentId provider, Func<Agent, AgentProtocol> create) :
            this(id, description, provider, false, create) { }

        public Service(string id, string description, AgentId provider, bool providesUI, Func<Agent, AgentProtocol> create) :
            this(id, description, provider, new AgentId("ANY@ANY"), providesUI, create) { }

        public Service(string id, string description, AgentId provider, AgentId permissions, Func<Agent, AgentProtocol> create) :
            this(id, description, provider, permissions, false, create) { }
        public Service(string id, string description, AgentId provider, AgentId permissions, bool providesUI, Func<Agent, AgentProtocol> create) 
            : base(id, description, provider, permissions, providesUI)
        {
            Create = create;
        }
    }

    //EMBED SEScripts.MultiAgentNetwork.MAN.Models.PlatformService
    //EMBED SEScripts.MultiAgentNetwork.MAN.Agents.Agent
    //EMBED SEScripts.MultiAgentNetwork.MAN.Protocol.AgentProtocol
}
