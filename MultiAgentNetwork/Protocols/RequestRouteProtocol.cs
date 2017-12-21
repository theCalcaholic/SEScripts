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
		class MANRequestRouteProtocol : MANAgentProtocol
		{
			String RouteDefinition;
			const string LoadingPage = "<meta refresh historyDisabled backgroundColor='000000' fontColor='CC0000'/><br/><br/><hl/>Loading...<hl/>";
			const string DefaultColors = "<meta historyDisabled fontColor='FFFFFF' backgroundColor='000000'/>";
			const string Header = "<uicontrols>Browser</uicontrols><hl/>";
			public override string GetProtocolId()
			{ return "request-route"; }

			private XUIController UI;

			public MANRequestRouteProtocol(MANAgent agent) : base(agent)
			{
				//Logger.debug("RequestRouteProtocol constructor()");
			}

			public override void Restart()
			{
				if (RouteDefinition != null)
				{
					RequestRoute(RouteDefinition);
				}
			}

			public override void ReceiveMessage(MANAgentMessage msg)
			{
				if (msg.Status != MANAgentMessage.StatusCodes.OK)
				{
					base.ReceiveMessage(msg);
					return;
				}
				if (msg.GetAttribute("uiupdates") != null || msg.GetAttribute("uiupdates") != "false")
				{
					ListenForUIUpdate(true);
				}

				MANUITerminalAgent UIAgent = Holder as MANUITerminalAgent;
				if (UIAgent != null)
				{
					try
					{
						XMLTree xml = XML.ParseXML(msg.Content);
						UIAgent.LoadUI(xml);
						UIAgent.UpdateScreen();

					}
					catch
					{
						//Logger.log("WARNING: Invalid UI received from " + msg.Sender);
					}
					Stop();
				}
				else
				{
					//Logger.log("WARNING: Agent is not a UITerminalAgent. Can not display UI.");
					base.ReceiveMessage(msg);
					Stop();
					return;
				}
			}

			public bool RequestRoute(string routeString)
			{
				using (Logger logger = new Logger("RequestRouteProtocol.RequestRoute()", Logger.Mode.LOG))
				{
					string mafRoutePattern = @"(?<provider>[\w{}\s_\-#]+@[\w{}\s_\-#]+)::(?<service>[\w\-]+)(\((?<argument>[^)]*)\)){0,1}";
					System.Text.RegularExpressions.Regex mafRouteRegex = new System.Text.RegularExpressions.Regex(mafRoutePattern);
					System.Text.RegularExpressions.Match mafRouteMatch = mafRouteRegex.Match(routeString);
					if (!mafRouteMatch.Success)
					{
						throw new Exception("WARNING: Route not understood: <<" + routeString + ">>");
						//Logger.log("WARNING: Route not understood: <<" + routeString + ">>");
						//Logger.DecLvl();
						Stop();
						return false;
					}
					ListenForUIUpdate(false);

					MANAgentId id = new MANAgentId(mafRouteMatch.Groups["provider"].Value);
					string service = mafRouteMatch.Groups["service"].Value;
					string content = XMLParser.UnescapeQuotes(mafRouteMatch.Groups["argument"].Value);

					MANUITerminalAgent UIAgent = Holder as MANUITerminalAgent;
					content += " " + (UIAgent?.UI.GetPackedValues() ?? "");
					UIAgent?.LoadXML(LoadingPage);
					UIAgent?.UpdateScreen();

					RouteDefinition = routeString;
					MANAgentMessage request = new MANAgentMessage(
						Holder.Id,
						id,
						MANAgentMessage.StatusCodes.OK,
						content,
						service,
						ChatId
						)
					{
						TargetInterface = MANAgentMessage.Interfaces.UI,
						SenderChatId = ChatId
					};
					Holder.ScheduleMessage(request);
					logger.log("requesting route: " + request.ToXML(), Logger.Mode.LOG);
					return true;
				}
			}

			public void ListenForUIUpdate(bool on)
			{
				if (on)
				{
					Holder.RegisterService("update-ui", (agent) => new MANRequestRouteProtocol(agent));
				}
				else
				{
					Holder.Services.Remove("update-ui");
				}
			}

			public override void Setup()
			{
				XUIRoute.RouteHandlers.Add("man", (def, controller) =>
				{
					using (Logger logger = new Logger("Handle man route", Logger.Mode.LOG))
					{
						MANRequestRouteProtocol request = new MANRequestRouteProtocol(Holder);
						Holder.AddChat(request);
						request.UI = controller;
						request.RequestRoute(def);
					}
				});
			}

		}
	}
}