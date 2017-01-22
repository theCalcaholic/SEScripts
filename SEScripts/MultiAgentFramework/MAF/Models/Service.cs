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
using SEScripts.MultiAgentFramework.MAF.Protocols;

namespace SEScripts.MultiAgentFramework.MAF.Models
{
    public class Service : PlatformService
    {
        public Func<Agent, AgentProtocol> Create;

        public Service(string id, string description, AgentId provider, Func<Agent, AgentProtocol> create) :
            this(id, description, provider, false, create) { }
        public Service(string id, string description, AgentId provider, bool providesUI, Func<Agent, AgentProtocol> create) : base(id, description, provider, providesUI)
        {
            Create = create;
        }
    }

    //EMBED SEScripts.MultiAgentFramework.MAF.Models.PlatformService
}
