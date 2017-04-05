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
using SEScripts.MultiAgentNetwork.MAN.Protocols;
using SEScripts.MultiAgentNetwork.MAN.Models;
using SEScripts.ParseLib.XUI;

namespace SEScripts.MultiAgentNetwork.MAN.Agents
{
    public class UITerminalAgent : RegisteredAgent
    {
        public XML.UIController UI;
        public IMyTextPanel Screen;
        const string loadingPage = "<meta historyDisabled/><hl width='100%'/>Requesting Services...<hl width='100%'/>";

        public UITerminalAgent(MyGridProgram program, IMyTextPanel screen) : base(program)
        {
            //Logger.debug("UITerminalAgent constructor()");
            //Logger.IncLvl();
            Screen = screen;
            //Logger.log("Setting up UI");
            UI = XML.UIController.FromXML(loadingPage);

            //Logger.log("Setup RequestRouteProtocol");
            new RequestRouteProtocol(this).Setup();
            //Logger.log("Setup UIServiceIndex");
            UIServiceIndexServer indexServer = new UIServiceIndexServer(this);
            indexServer.Setup();

            indexServer.LoadHomeScreen();
            //UI.FollowRoute(new XML.Route(AgentProtocol.MakeRoute(Id, indexServer.GetProtocolId(),"page='refresh'")));
            //UpdateScreen();

            //Logger.DecLvl();
        }

        public void LoadXML(string uiString)
        {
            //Logger.debug("UITerminalAgent.LoadUI()");
            //Logger.IncLvl();
            UI.LoadXML(uiString);
            //Logger.DecLvl();
        }

        public void LoadUI(XML.XMLTree ui)
        {
            //Logger.debug("UITerminalAgent.LoadUI()");
            //Logger.IncLvl();
            UI.LoadUI(ui);
            if(UI.HasUserInputBindings)
            {
                ScheduleRefresh();
            }
            //Logger.DecLvl();
        }

        public void UpdateScreen()
        {
            //Logger.debug("UITerminalAgent.UpdateScreen()");
            //Logger.IncLvl();
            UI.RenderTo(Screen);
            UI.ApplyScreenProperties(Screen);
            //Logger.DecLvl();
        }

        public void Call(List<string> parameters)
        {
            //Logger.debug("UITerminalAgent.Call()");
            //Logger.IncLvl();
            UI.Call(parameters);
            UpdateScreen();
            if (UI.HasUserInputBindings)
            {
                ScheduleRefresh();
            }
            //Logger.DecLvl();
        }

        public override void Refresh(TimeSpan elapsedTime)
        {
            base.Refresh(elapsedTime);
            UI.Call(new List<string> { "refresh" });
            if (UI.UpdateUserInput())
            {
                UpdateScreen();
            }
        }
    }
    //EMBED SEScripts.MultiAgentNetwork.MAN.Agents.RegisteredAgent
    //EMBED SEScripts.MultiAgentNetwork.MAN.Protocols.RequestRouteProtocol
    //EMBED SEScripts.MultiAgentNetwork.MAN.Protocols.UIServiceIndexServer
}
