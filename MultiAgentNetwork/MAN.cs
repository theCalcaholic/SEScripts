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
		public class MAN
		{


			//WRAPPERBODY
			//EMBED SEScripts.MultiAgentNetwork.MAN.Agents.GridPlatformAgent
			//!EMBED SEScripts.MultiAgentNetwork.MAN.Agents.PlatformAgent
			//!EMBED SEScripts.MultiAgentNetwork.MAN.Agents.RegisteredAgent
			//EMBED SEScripts.MultiAgentNetwork.MAN.Agents.UITerminalAgent
			//EMBED SEScripts.MultiAgentNetwork.MAN.Agents.Agent
			//EMBED SEScripts.MultiAgentNetwork.MAN.Protocols.UIServerProtocol
		}

		//EMBED SEScripts.Lib.DataStorageMinificationPrepared
		//EMBED SEScripts.Lib.Util
	}
}
