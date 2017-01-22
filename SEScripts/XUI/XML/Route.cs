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

using SEScripts.Lib;

namespace SEScripts.XUI.XML
{
    public class Route
    {
        string Definition;
        static Dictionary<string, Action<UIController>> UIFactories =
            new Dictionary<string, Action<UIController>>();

        public Route(string definition)
        {
            Logger.debug("Route constructor():");
            Logger.IncLvl();
            Definition = definition;
            Logger.debug("xml string is: " + Definition.Substring(4));
            Logger.DecLvl();
        }

        public void Follow(UIController controller)
        {
            Logger.debug("Route: GetUI():");
            Logger.IncLvl();
            XMLTree ui = null;
            if (Definition == "revert")
            {
                controller.RevertUI();
            }
            else if (Definition.Substring(0, 4) == "xml:")
            {
                ui = ParseXML(Parser.UnescapeQuotes(Definition.Substring(4)));
                controller.LoadUI(ui);
            }
            else if (Definition.Substring(0, 3) == "fn:" && UIFactories.ContainsKey(Definition.Substring(3)))
            {
                UIFactories[Definition.Substring(3)](controller);
            }

            Logger.DecLvl();
        }

        static public void RegisterRouteFunction(string id, Action<UIController> fn)
        {
            UIFactories[id] = fn;
        }
    }

    //EMBED SEScripts.XUI.XML.XMLTree
    //EMBED SEScripts.XUI.XML.UIController
    //EMBED SEScripts.XUI.XML.UIFactory
}
