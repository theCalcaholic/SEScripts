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
    public class Menu : XMLTree
    {
        NodeBox prefix;
        NodeBox prefixSelected;

        public Menu() : base()
        {
            Type = "menu";
            prefix = new NodeBoxLeaf("     ");
            prefixSelected = new NodeBoxLeaf(">> ");
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

        public override NodeBox RenderCache
        {
            get
            {
                NodeBoxTree cache = new NodeBoxTree();
                foreach (XMLTree child in Children)
                {
                    if(child.IsSelected())
                    {
                        cache.Add(prefixSelected);
                    }
                    else
                    {
                        cache.Add(prefix);
                    }
                    cache.Add(child.RenderCache);
                }
                cache.Flow = GetAttribute("flow") == "horizontal" ? NodeBox.FlowDirection.HORIZONTAL : NodeBox.FlowDirection.VERTICAL;

                switch (GetAttribute("alignself"))
                {
                    case "right":
                        cache.Align = NodeBox.TextAlign.RIGHT;
                        break;
                    case "center":
                        cache.Align = NodeBox.TextAlign.CENTER;
                        break;
                    default:
                        cache.Align = NodeBox.TextAlign.LEFT;
                        break;
                }

                int result;
                if (Int32.TryParse(GetAttribute("minwidth"), out result))
                    cache.MinWidth = result;
                if (Int32.TryParse(GetAttribute("maxwidth"), out result))
                    cache.MaxWidth = result;
                if (Int32.TryParse(GetAttribute("width"), out result))
                    cache.DesiredWidth = result;
                if (Int32.TryParse(GetAttribute("forcewidth"), out result))
                    cache.ForcedWidth = result;
                if (Int32.TryParse(GetAttribute("height"), out result))
                    cache.Height = result;

                return cache;
            }
        }
    }

    //EMBED SEScripts.XUI.XML.XMLTree
}
