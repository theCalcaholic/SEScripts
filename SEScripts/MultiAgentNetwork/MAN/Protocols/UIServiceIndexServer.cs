using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SEScripts.MultiAgentNetwork.MAN.Agents;
using SEScripts.MultiAgentNetwork.MAN.Models;
using SEScripts.ParseLib.XUI;
//using SEScripts.Lib;

namespace SEScripts.MultiAgentNetwork.MAN.Protocols
{
    public class UIServiceIndexServer : AgentProtocol
    {
        static string XMLHeader = "<meta fontColor='000000' backgroundColor='999999' fontSize='1.5' $ATTRIBUTES$/><uicontrols>$TITLE$</uicontrols><hl/>";
        public override string GetProtocolId()
        {
            return "get-ui-services-index";
        }
        private int RefreshTime;
        private int ResetTime;
        
        public Dictionary<string, List<PlatformService>> Services
        {
            get
            {
                if(Holder.GetKnowledgeEntry("UISERVICES", this) == null)
                {
                    Holder.SetKnowledgeEntry("UISERVICES", new Dictionary<string, List<PlatformService>>(), this);
                }
                return Holder.GetKnowledgeEntry("UISERVICES", this) as Dictionary<string, List<PlatformService>>;
            }
        }

        public Dictionary<string, string> Platforms
        {
            get
            {
                if(Holder.GetKnowledgeEntry("UIPLATFORMS", this) == null)
                {
                    Holder.SetKnowledgeEntry("UIPLATFORMS", new Dictionary<string, string>(), this);
                }
                return Holder.GetKnowledgeEntry("UIPLATFORMS", this) as Dictionary<string, string>;
            }
        }

        AgentId UIReceiver;

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

        public UIServiceIndexServer(Agent holder) : base(holder)
        {
            //Logger.log("UIServiceIndexServer constructor");
            RefreshTime = 1250;
            ResetTime = 10000;
        }

        public override void ReceiveMessage(AgentMessage msg)
        {
            //Logger.IncLvl();
            //Logger.debug("UIServicesIndexServer.ReceiveMessage()");
            if(msg.Status == AgentMessage.StatusCodes.OK)
            {
                ReceiveServices(msg);
            }
            else
            {
                base.ReceiveMessage(msg);
            }
            //Logger.DecLvl();
        }

        private void ReceiveServices(AgentMessage msg)
        {
            //Logger.debug("UIServicesIndexServer.ReceiveServices()");
            //Logger.IncLvl();

            XML.XMLTree messageXML;
            try
            {
                //Logger.log("test");
                messageXML = XML.ParseXML(msg.Content);
            }
            catch
            {
                //Logger.log("WARNING: Invalid message received!");
                //Logger.DecLvl();
                return;
            }
            string platformId = msg.Sender.Platform;
            string platformName = messageXML.GetNode((node) => node.Type == "platforminfo")?.GetAttribute("platformname");
            List<PlatformService> services = new List<PlatformService>();
            Services[platformId] = new List<PlatformService>();
            Platforms[platformId] = Parser.UnescapeQuotes(platformName ?? platformId);
            PlatformService service;
            foreach (XML.XMLTree serviceNode in messageXML.GetAllNodes((node) => node.Type == "service"))
            {
                service = PlatformService.FromXMLNode(serviceNode);
                if (service != null)
                {
                    Services[platformId].Add(service);
                }
            }
            /*if(UIReceiver != null && Holder.Services.ContainsKey(GetProtocolId()))
            {
                AgentMessage fakeRequest = new AgentMessage(UIReceiver, Holder.Id, AgentMessage.StatusCodes.OK, "", GetProtocolId());
                Holder.ReceiveMessage(fakeRequest);
            }*/

            if (HomePageActive) LoadHomeScreen();
            //Logger.DecLvl();
        }
        
        public override void Restart()
        { }
        public override void NotifyEvent(string eventId)
        {
            //Logger.log("UIServiceIndexServer.NotifyEvent()");
            //Logger.IncLvl();
            switch (eventId)
            {
                case "register":
                    RetrieveServices();
                    break;
                case "refresh":
                    int tuRefresh = (int)Holder.GetKnowledgeEntry("TimeUntilRefresh", this);
                    tuRefresh -= Holder.ElapsedTime.Milliseconds;
                    //Logger.log("Time Till Refresh: " + ttr);
                    if (tuRefresh < 0)
                    {
                        tuRefresh = RefreshTime;
                        RetrieveServices();
                    }
                    int tuReset = (int)Holder.GetKnowledgeEntry("TimeUntilReset", this);
                    tuReset -= Holder.ElapsedTime.Milliseconds;
                    if(tuReset < 0)
                    {
                        tuReset = ResetTime;
                        if (!HomePageActive)
                        {
                            (Holder as UITerminalAgent)?.UI.ClearUIStack();
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
            //Logger.DecLvl();
        }

        public void SetUIReceiver(AgentId receiver)
        {
            UIReceiver = receiver;
        }

        private void RetrieveServices()
        {
            //Logger.debug("UIServiceIndexServer.RetrieveServices()");
            //Logger.IncLvl();
            AgentMessage request = new AgentMessage(
                Holder.Id,
                new AgentId("ALL@ALL"),
                AgentMessage.StatusCodes.OK,
                "",
                "get-services",
                ChatId);
            request.TargetInterface = AgentMessage.Interfaces.UI;
            request.SenderChatId = ChatId;
            Holder.SendMessage(ref request);
            //Logger.DecLvl();
        }
        
        public string PageHome()
        {
            //Logger.debug("UIServiceIndexServer.PageHome()");
            //Logger.IncLvl();
            StringBuilder xml = new StringBuilder(XMLHeader).Replace("$TITLE$", "Platforms").Replace("$ATTRIBUTES$", "uiServiceIndexHome");
            xml.Append("<menu id='platformMenu'>");
            xml.Append(GetPlatformMenuitems());
            xml.Append("</menu>");
            //xml.Append("<menuItem route='" + MakeRoute(Holder.Id, GetProtocolId(), "page='refresh'") + "'>" + "Refresh</menuitem>");
            //Logger.DecLvl();
            return xml.ToString();
        }
        private StringBuilder GetPlatformMenuitems()
        {
            StringBuilder xml = new StringBuilder();
            foreach (string key in Platforms.Keys)
            {
                if(Services[key].Count > 0)
                    xml.Append("<menuItem id='" + key + "' route='fn:show-platform-services' platform='" + key + "'>" + Platforms[key] + "</menuitem>");
            }
            return xml;
        }

        public string PagePlatformServices(string platformId)
        {
            StringBuilder xml = new StringBuilder(XMLHeader).Replace("$TITLE$", Platforms[platformId] + " Services");
            xml.Append("<menu>");
            foreach(PlatformService service in Services[platformId])
            {
                xml.Append("<menuitem route='" + MakeRoute(service.Provider, service.Id, "") + "'>" + service.Description + "</menuitem>");
            }
            xml.Append("</menu>");
            return xml.ToString();
        }

        public void LoadHomeScreen()
        {
            //Logger.log("UIServiceIndexServer.LoadHomeScreen()");
            //Logger.IncLvl();
            UITerminalAgent UIAgent = Holder as UITerminalAgent;
            if(UIAgent != null)
            {
                XML.XMLTree meta = UIAgent.UI.GetNode((node) => node.Type == "meta");
                bool isLoaded = (meta?.GetAttribute("uiserviceindexhome") != null);
                if (HomePageActive && !isLoaded )
                {
                    //Logger.log("Setting HomPageActive to false");
                    HomePageActive = false;
                }
                else
                {
                    XML.XMLTree ui = XML.ParseXML(PageHome());
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
            //Logger.DecLvl();
        }

        public override void Setup()
        {
            XML.Route.RegisterRouteFunction("show-platform-services", (controller) =>
            {
                string platformId = controller.GetSelectedNode().GetAttribute("platform");
                if (platformId != null)
                {
                    controller.LoadUI(XML.ParseXML(PagePlatformServices(platformId)));
                }
            });

            //Logger.log("Create event listeners");
            Holder.SetKnowledgeEntry("TimeUntilRefresh", RefreshTime, this);
            Holder.SetKnowledgeEntry("TimeUntilReset", ResetTime, this);

            AgentProtocol chat = new UIServiceIndexServer(Holder);
            Holder.AddChat(chat);
            Holder.OnEvent("register", chat);
            Holder.OnEvent("refresh", chat);
            Holder.SetKnowledgeEntry("HomePageActive", false, this);
            //Logger.DecLvl();
        }
    }

    //EMBED SEScripts.MultiAgentNetwork.MAN.Protocols.UIServerProtocol
    //EMBED SEScripts.MultiAgentNetwork.MAN.Protocols.AgentProtocol
    //EMBED SEScripts.MultiAgentNetwork.MAN.Models.AgentMessage
    //EMBED SEScripts.MultiAgentNetwork.MAN.Models.PlatformService
    //EMBED SEScripts.MultiAgentNetwork.MAN.Models.AgentId
}
