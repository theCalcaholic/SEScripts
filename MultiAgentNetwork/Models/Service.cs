using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System;
using VRage.Collections;
using VRage.Game.Components;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ObjectBuilders.Definitions;
using VRage.Game;
using VRageMath;

namespace IngameScript
{
	partial class Program : MyGridProgram
	{
		public class MANService : MANPlatformService
		{
			public Func<MANAgent, MANAgentProtocol> Create;

			public MANService(string id, string description, MANAgentId provider, Func<MANAgent, MANAgentProtocol> create) :
				this(id, description, provider, false, create)
			{ }

			public MANService(string id, string description, MANAgentId provider, bool providesUI, Func<MANAgent, MANAgentProtocol> create) :
				this(id, description, provider, new MANAgentId("ANY@ANY"), providesUI, create)
			{ }

			public MANService(string id, string description, MANAgentId provider, MANAgentId permissions, Func<MANAgent, MANAgentProtocol> create) :
				this(id, description, provider, permissions, false, create)
			{ }
			public MANService(string id, string description, MANAgentId provider, MANAgentId permissions, bool providesUI, Func<MANAgent, MANAgentProtocol> create)
				: base(id, description, provider, permissions, providesUI)
			{
				Create = create;
			}
		}
	}
}