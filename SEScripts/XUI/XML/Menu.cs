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
    public class Menu : XMLTree
    {

        public Menu() : base()
        {
            Type = "menu";
        }

        public override void AddChild(XMLTree child)
        {
            Logger.debug(Type + ": Add child():");
            Logger.IncLvl();
            if (child.Type != "menuitem" && child.IsSelectable())
            {
                Logger.DecLvl();
                throw new Exception(
                    "ERROR: Only children of type <menupoint> or children that are not selectable are allowed!"
                    + " (type was: <" + child.Type + ">)");
            }
            base.AddChild(child);
            Logger.DecLvl();
        }

        protected override string RenderChild(XMLTree child, int width)
        {
            P.Me.CustomData = Logger.History;
            string renderString = "";
            string prefix = "     ";
            if (child.Type == "menuitem")
            {
                renderString += (child.IsSelected() ? ">> " : prefix);
            }
            renderString += base.RenderChild(child, width);
            return renderString;
        }
    }

    //EMBED SEScripts.XUI.XML.XMLTree
}
