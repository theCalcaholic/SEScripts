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
using SEScripts.XUI.BoxRenderer;

namespace SEScripts.XUI.XML
{
    public class Menu : XMLTree
    {
        RenderBox prefix;
        RenderBox prefixSelected;

        public Menu() : base()
        {
            Type = "menu";
            prefix = new RenderBoxLeaf("     ");
            prefixSelected = new RenderBoxLeaf(">> ");
            int prefixWidth = Math.Max(prefix.MinWidth, prefixSelected.MinWidth);
            prefix.MaxWidth = prefixWidth;
            prefixSelected.MaxWidth = prefixWidth;
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

        /*protected override string RenderChild(XMLTree child, int width)
        {
            string renderString = "";
            string prefix = "     ";
            if (child.Type == "menuitem")
            {
                renderString += (child.IsSelected() ? ">> " : prefix);
            }
            renderString += base.RenderChild(child, width);
            return renderString;
        }*/

        public override RenderBox GetRenderBox(int maxWidth)
        {
            Logger.debug("Menu.GetRenderCache(int)");
            Logger.IncLvl();
            RenderBoxTree cache = new RenderBoxTree();
            RenderBoxTree menuPoint;
            foreach (XMLTree child in Children)
            {
                menuPoint = new RenderBoxTree();
                menuPoint.Flow = RenderBox.FlowDirection.HORIZONTAL;
                if(child.IsSelected())
                {
                    menuPoint.Add(prefixSelected);
                }
                else
                {
                    menuPoint.Add(prefix);
                }
                menuPoint.Add(child.GetRenderBox(maxWidth));
                cache.Add(menuPoint);
            }
            UpdateRenderCacheProperties(cache, maxWidth);
            Logger.DecLvl();
            return cache;
        }
    }

    //EMBED SEScripts.XUI.XML.XMLTree
}
