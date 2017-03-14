using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

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
using SEScripts.Lib.LoggerNS;
using SEScripts.MultiAgentNetwork.MAN.Agents;
using SEScripts.MultiAgentNetwork.MAN.Models;

namespace SEScripts.MultiAgentNetwork.MAN.Protocols
{
    public class UIServerProtocol : AgentProtocol
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

        public Dictionary<string, Func<UIServerProtocol, AgentMessage, Dictionary<string, string>, string>> Pages
        {
            get
            {
                return GetPageGenerators().Where((page) => page.Key.StartsWith(ApplicationId + "_")).ToDictionary(page => page.Key, page => page.Value);
            }

            set
            {
                foreach(KeyValuePair<string, Func<UIServerProtocol, AgentMessage, Dictionary<string, string>, string>> page in value)
                {
                    SetPageGenerator(page.Key, page.Value, false);
                }
            }
        }


        public UIServerProtocol(Agent holder) : base(holder)
        {
            Logger.debug("UIServerProtocol constructor");
            Logger.IncLvl();
            if(!GetPageGenerators().ContainsKey("404"))
            {
                GetPageGenerators()["404"] = (agent, msg, data) => "Page not found!<hl/><uicontrols>BACK</uicontrols>";
            }
            ApplicationId = "";
            Logger.DecLvl();
        }

        public void SetPageGenerator(string id, Func<UIServerProtocol, AgentMessage, Dictionary<string, string>, string> pageGenerator)
        {
            SetPageGenerator(id, pageGenerator, false);
        }

        private void SetPageGenerator(string id, Func<UIServerProtocol, AgentMessage, Dictionary<string, string>, string> pageGenerator, bool global)
        {
            Logger.debug("UIServerProtocol.SetPageGenerator()");
            Logger.IncLvl();
            string pageId = global ? id : ApplicationId + "_" + id;
            GetPageGenerators()[pageId] = pageGenerator;
            Logger.DecLvl();
        }

        public Func<UIServerProtocol, AgentMessage, Dictionary<string, string>, string> GetPageGenerator(string id)
        {
            return GetPageGenerator(id, false);
        }

        public string GetPage(string id, AgentMessage msg, Dictionary<string, string> data)
        {
            return GetPageGenerator(id)(this, msg, data);
        }

        private Func<UIServerProtocol, AgentMessage, Dictionary<string, string>, string> GetPageGenerator(string id, bool global)
        {
            Logger.debug("UIServerProtocol.GetPageGenerator()");
            Logger.IncLvl();
            string pageId = global ? id : ApplicationId + "_" + id;
            Logger.DecLvl();
            return GetPageGenerators()?.GetValueOrDefault(pageId, GetPageGenerators()["404"] ?? ((a,m,b) => "404") );
        }

        private Dictionary<string, Func<UIServerProtocol, AgentMessage, Dictionary<string, string>, string>> GetPageGenerators()
        {
            Logger.debug("UIServerProtocol.GetPageGenerators()");
            Logger.IncLvl();
            if (Holder.GetKnowledgeEntry("UIPAGES", this) == null)
            {
                Holder.SetKnowledgeEntry("UIPAGES", new Dictionary<string, Func<UIServerProtocol, AgentMessage, Dictionary<string, string>, string>>(), this);
            }
            Logger.DecLvl();
            return Holder.GetKnowledgeEntry("UIPAGES", this) as Dictionary<string, Func<UIServerProtocol, AgentMessage, Dictionary<string, string>, string>>;
        }

        public override void ReceiveMessage(AgentMessage msg)
        {
            Logger.debug("UIServerProtocol.ReceiveMessage()");
            Logger.IncLvl();
            if (msg.Status == AgentMessage.StatusCodes.OK)
            {
                Dictionary<string, string> data = Parser.GetXMLAttributes(msg.Content);
                ResponseData = new Dictionary<string, string>();
                string page = GetPage(
                    data.GetValueOrDefault<string, string>("page") ?? "",
                    msg,
                    data);

                AgentMessage response = msg.MakeResponse(
                    Holder.Id,
                    AgentMessage.StatusCodes.OK,
                    page);
                response.SenderChatId = ChatId;
                foreach(KeyValuePair<string, string> entry in ResponseData)
                {
                    if(response.GetAttribute(entry.Key) == null)
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
            Logger.DecLvl();
        }

        public static bool CreateApplication(Agent holder, string id, string title, Dictionary<string, Func<UIServerProtocol, AgentMessage, Dictionary<string, string>, string>> pages)
        {
            Logger.debug("UIServerProtocol.CreateApplication()");
            Logger.IncLvl();
            UIServerProtocol setup = new UIServerProtocol(holder);
            setup.SelectApplication(id);
            setup.Pages = pages;
            if(holder.Services.ContainsKey(id))
            {
                Logger.DecLvl();
                return false;
            }

            holder.RegisterService(
                id,
                (agent) =>
                {
                    UIServerProtocol uiServer = new UIServerProtocol(agent);
                    uiServer.SelectApplication(id);
                    return uiServer;
                },
                new Dictionary<string, string>
                {
                    {"description", title},
                    {"providesui", "true"}
                }
            );
            Logger.DecLvl();
            return true;
        }

        static public Func<UIServerProtocol, AgentMessage, Dictionary<string, string>, string> SimplePage(string id, string uiContent)
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
    //EMBED SEScripts.MultiAgentNetwork.MAN.Protocols.AgentProtocol
    //EMBED SEScripts.MultiAgentNetwork.MAN.Agents.Agent
    //EMBED SEScripts.MultiAgentNetwork.MAN.Models.AgentMessage

}
