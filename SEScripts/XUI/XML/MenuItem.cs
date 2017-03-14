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

using SEScripts.Lib.LoggerNS;

namespace SEScripts.XUI.XML
{
    public class MenuItem : XMLTree
    {
        Route TargetRoute;

        public MenuItem() : this(null) { }

        public MenuItem(Route targetRoute) : base()
        {
            Type = "menuitem";
            Selectable = true;
            SetRoute(targetRoute);
            PreventDefault("RIGHT/SUBMIT");
        }

        public override void SetAttribute(string key, string value)
        {
            Logger.debug(Type + ": SetAttribute():");
            Logger.IncLvl();
            switch (key)
            {
                case "route":
                    Logger.debug("prepare to set route...");
                    SetRoute(new Route(value));
                    if (TargetRoute == null)
                    {
                        Logger.debug("Failure!");
                    }
                    else
                    {
                        Logger.debug("Success!");
                    }

                    break;
                default:
                    base.SetAttribute(key, value);
                    break;
            }

            Logger.DecLvl();
        }

        public override void OnKeyPressed(string keyCode)
        {
            Logger.debug(Type + ": OnKeyPressed():");
            switch (keyCode)
            {
                case "RIGHT/SUBMIT":
                    if (TargetRoute != null)
                    {
                        Logger.debug("Follow Target Route!");
                        FollowRoute(TargetRoute);
                    }
                    else
                    {
                        Logger.debug("No route set!");
                    }

                    break;
            }

            base.OnKeyPressed(keyCode);
            Logger.DecLvl();
        }

        public void SetRoute(Route route)
        {
            TargetRoute = route;
        }
        
    }

    //EMBED SEScripts.XUI.XML.XMLTree
    //EMBED SEScripts.XUI.XML.Route
}
