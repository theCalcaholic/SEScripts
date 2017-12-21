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
		public class MANUIServerProtocol : MANAgentProtocol
		{
			public override string GetProtocolId()
			{ return "serve-ui-page"; }


			public string ApplicationId;
			public Dictionary<string, string> ResponseData;
			public IMyGridTerminalSystem GTS
			{
				get
				{
					return Holder.GTS;
				}
			}

			public Dictionary<string, Func<MANUIServerProtocol, MANAgentMessage, Dictionary<string, string>, string>> Pages
			{
				get
				{
					return GetPageGenerators().Where((page) => page.Key.StartsWith(ApplicationId + "_")).ToDictionary(page => page.Key, page => page.Value);
				}

				set
				{
					foreach (KeyValuePair<string, Func<MANUIServerProtocol, MANAgentMessage, Dictionary<string, string>, string>> page in value)
					{
						SetPageGenerator(page.Key, page.Value, false);
					}
				}
			}


			public MANUIServerProtocol(MANAgent holder) : base(holder)
			{
				if (!GetPageGenerators().ContainsKey("404"))
				{
					GetPageGenerators()["404"] = (agent, msg, data) => "Page not found!<hl/><uicontrols>BACK</uicontrols>";
				}
				ApplicationId = "";
			}

			public void SetPageGenerator(string id, Func<MANUIServerProtocol, MANAgentMessage, Dictionary<string, string>, string> pageGenerator)
			{
				SetPageGenerator(id, pageGenerator, false);
			}

			private void SetPageGenerator(string id, Func<MANUIServerProtocol, MANAgentMessage, Dictionary<string, string>, string> pageGenerator, bool global)
			{
				string pageId = global ? id : ApplicationId + "_" + id;
				GetPageGenerators()[pageId] = pageGenerator;
			}

			public Func<MANUIServerProtocol, MANAgentMessage, Dictionary<string, string>, string> GetPageGenerator(string id)
			{
				return GetPageGenerator(id, false);
			}

			public string GetPage(string id, MANAgentMessage msg, Dictionary<string, string> data)
			{
				return GetPageGenerator(id)(this, msg, data);
			}

			private Func<MANUIServerProtocol, MANAgentMessage, Dictionary<string, string>, string> GetPageGenerator(string id, bool global)
			{
				string pageId = global ? id : ApplicationId + "_" + id;
				return GetPageGenerators()?.GetValueOrDefault(pageId, GetPageGenerators()["404"] ?? ((a, m, b) => "404"));
			}

			private Dictionary<string, Func<MANUIServerProtocol, MANAgentMessage, Dictionary<string, string>, string>> GetPageGenerators()
			{
				if (Holder.GetKnowledgeEntry("UIPAGES", this) == null)
					Holder.SetKnowledgeEntry("UIPAGES", new Dictionary<string, Func<MANUIServerProtocol, MANAgentMessage, Dictionary<string, string>, string>>(), this);
				return Holder.GetKnowledgeEntry("UIPAGES", this) as Dictionary<string, Func<MANUIServerProtocol, MANAgentMessage, Dictionary<string, string>, string>>;
			}

			public override void ReceiveMessage(MANAgentMessage msg)
			{
				if (msg.Status == MANAgentMessage.StatusCodes.OK)
				{
					Dictionary<string, string> data = XMLParser.GetXMLAttributes(msg.Content);
					ResponseData = new Dictionary<string, string>();
					string page = GetPage(
						data.GetValueOrDefault<string, string>("page") ?? "",
						msg,
						data);

					MANAgentMessage response = msg.MakeResponse(
						Holder.Id,
						MANAgentMessage.StatusCodes.OK,
						page);
					response.SenderChatId = ChatId;
					foreach (KeyValuePair<string, string> entry in ResponseData)
					{
						if (response.GetAttribute(entry.Key) == null)
						{
							response.SetAttribute(entry.Key, entry.Value);
						}
					}
					Holder.SendMessage(ref response);
					Stop();
				}
				else
				{
					base.ReceiveMessage(msg);
				}
			}

			public static bool CreateApplication(MANAgent holder, string id, string title, Dictionary<string, Func<MANUIServerProtocol, MANAgentMessage, Dictionary<string, string>, string>> pages)
			{
				MANUIServerProtocol setup = new MANUIServerProtocol(holder);
				setup.SelectApplication(id);
				setup.Pages = pages;
				if (holder.Services.ContainsKey(id))
					return false;

				holder.RegisterService(
					id,
					(agent) =>
					{
						MANUIServerProtocol uiServer = new MANUIServerProtocol(agent);
						uiServer.SelectApplication(id);
						return uiServer;
					},
					new Dictionary<string, string>
					{
						{"description", title},
						{"providesui", "true"}
					}
				);
				return true;
			}

			static public Func<MANUIServerProtocol, MANAgentMessage, Dictionary<string, string>, string> SimplePage(string id, string uiContent)
			{
				return (session, msg, data) => uiContent;
			}


			public override void Restart() { }


			public void SelectApplication(string id)
			{
				ApplicationId = id;
			}


			public string MakeApplicationRoute(string page, string argument)
			{
				return MakeRoute(Holder.Id, ApplicationId, "page='" + page + "' " + argument);
			}

		}

	}
}