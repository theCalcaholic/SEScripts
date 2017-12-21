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
		public class MANRegisteredAgent : MANAgent
		{
			public MANRegisteredAgent(MyGridProgram program) : base(program)
			{
				using (var logger = new Logger("MANRegisteredAgent constructor"))
				{
					logger.log("checking platform id...");
					if (Id.Platform != "local" && (GTS.GetBlockWithName(Id.Platform) as IMyProgrammableBlock) == null)
					{
						Id.Platform = "local";
					}
					logger.log("setting up service registration protocol...");
					new MANServiceRegistrationProtocol(this).Setup();
					logger.log("setting up print platform services protocol...");
					new MANPrintPlatformServicesProtocol(this).Setup();
					logger.log("done");
				}
			}

			public void RegisterWith(string platformName)
			{
				MANAgentId platform = new MANAgentId(platformName + "@local");
				MANServiceRegistrationProtocol chat = new MANServiceRegistrationProtocol(this);
				Chats[chat.ChatId] = chat;
				string content = "<services>";
				foreach (KeyValuePair<string, MANService> service in Services)
				{

					if (service.Value.ProvidesUI || service.Value.HasPermissions(new MANAgentId("**@local")))
						content += service.Value.ToXML();
				}
				content += "</services>";
				MANAgentMessage message = new MANAgentMessage(
					this.Id,
					platform,
					MANAgentMessage.StatusCodes.OK,
					content,
					new MANServiceRegistrationProtocol.Platform(this).GetProtocolId(),
					chat.ChatId
				);
				SendMessage(ref message);
			}
		}
	}
}