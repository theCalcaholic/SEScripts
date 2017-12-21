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
		public class MANPrintProtocol : MANAgentProtocol
		{
			public override string GetProtocolId()
			{ return "print"; }

			public MANPrintProtocol(MANAgent agent) : base(agent) { }

			public override void ReceiveMessage(MANAgentMessage msg)
			{
				Stop();
			}

			public override void Restart() { }

			public override void Setup()
			{
				Holder.RegisterService(GetProtocolId(), (agent) =>
				{
					return new MANPrintProtocol(agent);
				});
			}
		}
	}
}