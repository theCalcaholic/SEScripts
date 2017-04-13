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

//using SEScripts.Lib;
using SEScripts.MultiAgentNetwork.MAN.Agents;
using SEScripts.MultiAgentNetwork.MAN.Models;
using SEScripts.ParseLib.XUI;

namespace SEScripts.MultiAgentNetwork.MAN.Protocols
{
    class RequestRouteProtocol : AgentProtocol
    {
        String RouteDefinition;
        const string LoadingPage = "<meta refresh historyDisabled backgroundColor='000000' fontColor='CC0000'/><br/><br/><hl/>Loading...<hl/>";
        const string DefaultColors = "<meta historyDisabled fontColor='FFFFFF' backgroundColor='000000'/>";
        const string Header = "<uicontrols>Browser</uicontrols><hl/>";
        public override string GetProtocolId()
        { return "request-route"; }

        private XML.UIController UI;

        public RequestRouteProtocol(Agent agent) : base(agent)
        {
            //Logger.debug("RequestRouteProtocol constructor()");
        }

        public override void Restart()
        {
            //Logger.debug("RequestRouteProtocol.Restart()");
            //Logger.IncLvl();
            if (RouteDefinition != null) {
                RequestRoute(RouteDefinition);
            }
            //Logger.DecLvl();
        }

        public override void ReceiveMessage(AgentMessage msg)
        {
            //Logger.debug("RequestRouteProtocol.ReceiveMessage()");
            //Logger.IncLvl();
            if (msg.Status != AgentMessage.StatusCodes.OK)
            {
                base.ReceiveMessage(msg);
                return;
            }
            if(msg.GetAttribute("uiupdates") != null || msg.GetAttribute("uiupdates") != "false")
            {
                ListenForUIUpdate(true);
            }

            UITerminalAgent UIAgent = Holder as UITerminalAgent;
            if (UIAgent != null)
            {
                try
                {
                    XML.XMLTree xml = XML.ParseXML(msg.Content);
                    UIAgent.LoadUI(xml);
                    UIAgent.UpdateScreen();

                } catch
                {
                    //Logger.log("WARNING: Invalid UI received from " + msg.Sender);
                }
                //UIAgent.Call(new List<string> { "" });
                Stop();
            }
            else 
            {
                //Logger.log("WARNING: Agent is no UITerminalAgent. Can not display UI.");
                base.ReceiveMessage(msg);
                Stop();
                //Logger.DecLvl();
                return;
            }
            //Logger.DecLvl();
        }

        public bool RequestRoute(string routeString)
        {
            //Logger.log("RequestRouteProtocol.RequestRoute()");
            //Logger.IncLvl();
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

                AgentId id = new AgentId(mafRouteMatch.Groups["provider"].Value);
                string service = mafRouteMatch.Groups["service"].Value;
                string content = Parser.UnescapeQuotes(mafRouteMatch.Groups["argument"].Value);

                UITerminalAgent UIAgent = Holder as UITerminalAgent;
                content += " " + (UIAgent?.UI.GetPackedValues() ?? "");
                UIAgent?.LoadXML(LoadingPage);
                UIAgent?.UpdateScreen();

                RouteDefinition = routeString;
                AgentMessage request = new AgentMessage(
                    Holder.Id,
                    id,
                    AgentMessage.StatusCodes.OK,
                    content,
                    service,
                    ChatId
                    );
                request.TargetInterface = AgentMessage.Interfaces.UI;
                request.SenderChatId = ChatId;
                Holder.ScheduleMessage(request);
                //Logger.DecLvl();
                logger.log("requesting route: " + request.ToXML(), Logger.Mode.LOG);
                return true;
            }
        }

        public void ListenForUIUpdate(bool on)
        {
            if(on)
            {
                Holder.RegisterService("update-ui", (agent) => new RequestRouteProtocol(agent));
            }
            else
            {
                Holder.Services.Remove("update-ui");
            }
        }

        public override void Setup()
        {
            //Logger.debug("RequestRouteProtocol.RegisterServices()");
            //Logger.IncLvl();
            /*Holder.RegisterService(
                GetProtocolId(),
                (agent) => {
                    return new RequestRouteProtocol(agent);
                });*/
            XML.Route.RouteHandlers.Add("man", (def, controller) => {
                using (Logger logger = new Logger("Handle man route", Logger.Mode.LOG))
                {
                    RequestRouteProtocol request = new RequestRouteProtocol(Holder);
                    Holder.AddChat(request);
                    request.UI = controller;
                    request.RequestRoute(def);
                }
            });
            //Logger.DecLvl();
        }

    }
    //EMBED SEScripts.MultiAgentNetwork.MAN.Protocols.AgentProtocol
    //EMBED SEScripts.MultiAgentNetwork.MAN.Agents.Agent
    //EMBED SEScripts.MultiAgentNetwork.MAN.Agents.UITerminalAgent
    //EMBED SEScripts.MultiAgentNetwork.MAN.Models.AgentMessage
    //EMBED SEScripts.MultiAgentNetwork.MAN.Models.AgentId

}
