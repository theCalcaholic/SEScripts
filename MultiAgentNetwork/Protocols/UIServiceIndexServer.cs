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
		public class MANUIServiceIndexServer : MANAgentProtocol
		{
			static string XMLHeader = "<meta fontColor='000000' backgroundColor='999999' fontSize='1.5' $ATTRIBUTES$/><uicontrols>$TITLE$</uicontrols><hl/>";
			public override string GetProtocolId()
			{
				return "get-ui-services-index";
			}
			private int RefreshTime;
			private int ResetTime;

			public Dictionary<string, List<MANPlatformService>> Services
			{
				get
				{
					if (Holder.GetKnowledgeEntry("UISERVICES", this) == null)
					{
						Holder.SetKnowledgeEntry("UISERVICES", new Dictionary<string, List<MANPlatformService>>(), this);
					}
					return Holder.GetKnowledgeEntry("UISERVICES", this) as Dictionary<string, List<MANPlatformService>>;
				}
			}

			public Dictionary<string, string> Platforms
			{
				get
				{
					if (Holder.GetKnowledgeEntry("UIPLATFORMS", this) == null)
					{
						Holder.SetKnowledgeEntry("UIPLATFORMS", new Dictionary<string, string>(), this);
					}
					return Holder.GetKnowledgeEntry("UIPLATFORMS", this) as Dictionary<string, string>;
				}
			}

			MANAgentId UIReceiver;

			bool HomePageActive
			{
				get
				{
					object active = Holder.GetKnowledgeEntry("HomePageActive", this);
					return active != null && (bool)active;
				}
				set
				{
					Holder.SetKnowledgeEntry("HomePageActive", value, this);
				}
			}

			public MANUIServiceIndexServer(MANAgent holder) : base(holder)
			{
				RefreshTime = 1250;
				ResetTime = 10000;
			}

			public override void ReceiveMessage(MANAgentMessage msg)
			{
				if (msg.Status == MANAgentMessage.StatusCodes.OK)
				{
					ReceiveServices(msg);
				}
				else
				{
					base.ReceiveMessage(msg);
				}
			}

			private void ReceiveServices(MANAgentMessage msg)
			{
				XMLTree messageXML;
				try
				{
					messageXML = XML.ParseXML(msg.Content);
				}
				catch
				{
					return;
				}
				string platformId = msg.Sender.Platform;
				string platformName = messageXML.GetNode((node) => node.Type == "platforminfo")?.GetAttribute("platformname");
				List<MANPlatformService> services = new List<MANPlatformService>();
				Services[platformId] = new List<MANPlatformService>();
				Platforms[platformId] = XMLParser.UnescapeQuotes(platformName ?? platformId);
				MANPlatformService service;
				foreach (XMLTree serviceNode in messageXML.GetAllNodes((node) => node.Type == "service"))
				{
					service = MANPlatformService.FromXMLNode(serviceNode);
					if (service != null)
					{
						Services[platformId].Add(service);
					}
				}

				if (HomePageActive) LoadHomeScreen();
			}

			public override void Restart()
			{ }
			public override void NotifyEvent(string eventId)
			{
				switch (eventId)
				{
					case "register":
						RetrieveServices();
						break;
					case "refresh":
						int tuRefresh = (int)Holder.GetKnowledgeEntry("TimeUntilRefresh", this);
						tuRefresh -= Holder.ElapsedTime.Milliseconds;
						if (tuRefresh < 0)
						{
							tuRefresh = RefreshTime;
							RetrieveServices();
						}
						int tuReset = (int)Holder.GetKnowledgeEntry("TimeUntilReset", this);
						tuReset -= Holder.ElapsedTime.Milliseconds;
						if (tuReset < 0)
						{
							tuReset = ResetTime;
							if (!HomePageActive)
							{
								(Holder as MANUITerminalAgent)?.UI.ClearUIStack();
								LoadHomeScreen();
							}

						}
						Holder.ScheduleRefresh();
						Holder.SetKnowledgeEntry("TimeTillRefresh", tuRefresh, this);
						Holder.SetKnowledgeEntry("TimeTillReset", tuReset, this);

						break;
					default:
						base.NotifyEvent(eventId);
						break;
				}
			}

			public void SetUIReceiver(MANAgentId receiver)
			{
				UIReceiver = receiver;
			}

			private void RetrieveServices()
			{
				MANAgentMessage request = new MANAgentMessage(
					Holder.Id,
					new MANAgentId("ALL@ALL"),
					MANAgentMessage.StatusCodes.OK,
					"",
					"get-services",
					ChatId)
				{
					TargetInterface = MANAgentMessage.Interfaces.UI,
					SenderChatId = ChatId
				};
				Holder.SendMessage(ref request);
			}

			public string PageHome()
			{
				using (var logger = new Logger("UIServiceIndexServer.PageHome()"))
				{
					StringBuilder xml = new StringBuilder(XMLHeader).Replace("$TITLE$", "Platforms").Replace("$ATTRIBUTES$", "uiServiceIndexHome");
					xml.Append("<menu id='platformMenu'>");
					xml.Append(GetPlatformMenuitems());
					xml.Append("</menu>");
					logger.log("home page is:");
					logger.log(xml);
					return xml.ToString();
				}
			}
			private StringBuilder GetPlatformMenuitems()
			{
				StringBuilder xml = new StringBuilder();
				foreach (string key in Platforms.Keys)
				{
					if (Services[key].Count > 0)
						xml.Append("<menuItem id='" + key + "' route='fn:show-platform-services' platform='" + key + "'>" + Platforms[key] + "</menuitem>");
				}
				return xml;
			}

			public string PagePlatformServices(string platformId)
			{
				StringBuilder xml = new StringBuilder(XMLHeader).Replace("$TITLE$", Platforms[platformId] + " Services");
				xml.Append("<menu>");
				foreach (MANPlatformService service in Services[platformId])
				{
					xml.Append("<menuitem route='" + MakeRoute(service.Provider, service.Id, "") + "'>" + service.Description + "</menuitem>");
				}
				xml.Append("</menu>");
				return xml.ToString();
			}

			public void LoadHomeScreen()
			{
				MANUITerminalAgent UIAgent = Holder as MANUITerminalAgent;
				if (UIAgent != null)
				{
					XMLTree meta = UIAgent.UI.GetNode((node) => node.Type == "meta");
					bool isLoaded = (meta?.GetAttribute("uiserviceindexhome") != null);
					if (HomePageActive && !isLoaded)
						HomePageActive = false;
					else
					{
						XMLTree ui = XML.ParseXML(PageHome());
						if (isLoaded)
						{
							meta.SetAttribute("historydisabled", "true");
							//Logger.log("Get id of selected node");
							/*XML.XMLTree selected = UIAgent.UI.GetSelectedNode();
							//Logger.log("got selected node! (" + selected + ")");
							//Logger.log("selected node is " + (selected == null ? "null!" : "of type " + selected.Type));
							string selectedId = selected.GetAttribute("id");
							//Logger.log("got selected id!")

							//string selectedId = null;
							//Logger.log("id is: " + selectedId ?? "null" );
							if (selectedId != null)
							{
								//Logger.log("Get node to select in new UI");
								XML.XMLTree selectedNode = ui.GetNode((node) => (node.GetAttribute("id") == selectedId));
								//Logger.log("found it!");
								if (selectedNode != null && selectedNode.IsSelectable())
								{
									//Logger.log("select node...");
									while (!selectedNode.IsSelected())
									{
										ui.SelectNext();
									}
								}
							}
							//Logger.log("done");*/
						}
						UIAgent.LoadUI(ui);
						HomePageActive = true;
						UIAgent.UpdateScreen();
						Holder.ScheduleRefresh();
					}
				}
			}

			public override void Setup()
			{
				XUIRoute.RegisterRouteFunction("show-platform-services", (controller) =>
				{
					string platformId = controller.GetSelectedNode().GetAttribute("platform");
					if (platformId != null)
						controller.LoadUI(XML.ParseXML(PagePlatformServices(platformId)));
				});
				
				Holder.SetKnowledgeEntry("TimeUntilRefresh", RefreshTime, this);
				Holder.SetKnowledgeEntry("TimeUntilReset", ResetTime, this);

				MANAgentProtocol chat = new MANUIServiceIndexServer(Holder);
				Holder.AddChat(chat);
				Holder.OnEvent("register", chat);
				Holder.OnEvent("refresh", chat);
				Holder.SetKnowledgeEntry("HomePageActive", false, this);
			}
		}
	}
}