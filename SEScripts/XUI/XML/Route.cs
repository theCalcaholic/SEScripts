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
using SEScripts.Lib.LoggerNS;

namespace SEScripts.XUI.XML
{
    public class Route
    {
        static public Dictionary<string, Action<string, UIController>> RouteHandlers = new Dictionary<string, Action<string, UIController>>
        {
            {
                "revert", (def, controller) => { controller.RevertUI(); }
            },
            {
                "xml", (def, controller) =>
                {
                    XMLTree ui = XMLWRAPPER.ParseXML(Parser.UnescapeQuotes(def));
                    controller.LoadUI(ui);
                }
            },
            {
                "fn", (def, controller) =>
                {
                    if(UIFactories.ContainsKey(def))
                    {
                        UIFactories[def](controller);
                    }
                }
            }
        };
        static Dictionary<string, Action<UIController>> UIFactories =
            new Dictionary<string, Action<UIController>>();

        string Definition;

        public Route(string definition)
        {
            //Logger.debug("Route constructor():");
            //Logger.IncLvl();
            Definition = definition;
            //Logger.DecLvl();
        }

        public void Follow(UIController controller)
        {
            using (Logger logger = new Logger("Route.Follow(UIController)", Logger.Mode.LOG))
            {
                logger.log("route def: " + Definition, Logger.Mode.LOG);
                //Logger.debug("Route.Follow()");
                //Logger.IncLvl();
                string[] DefTypeAndValue = Definition.Split(new char[] { ':' }, 2);
                if (Route.RouteHandlers.ContainsKey(DefTypeAndValue[0].ToLower()))
                {
                    Route.RouteHandlers[DefTypeAndValue[0].ToLower()](
                        DefTypeAndValue.Length >= 2 ? DefTypeAndValue[1] : null, controller
                    );
                }
                else
                {
                    logger.log("route not understood.", Logger.Mode.WARNING);
                }
            }

            //Logger.DecLvl();
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
