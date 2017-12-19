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
            //using (new Logger("Menu.AddChile(XMLTree)"))
            //{
                if (child.Type != "menuitem" && child.IsSelectable())
                {
                    throw new Exception(
                        "ERROR: Only children of type <menupoint> or children that are not selectable are allowed!"
                        + " (type was: <" + child.Type + ">)");
                }
                base.AddChild(child);
            //}
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

        public override Dictionary<string, string> GetValues(Func<XMLTree, bool> filter)
        {
            Dictionary<string, string> values = base.GetValues(filter);
            string name = GetAttribute("name");
            string value = GetChild(SelectedChild).GetAttribute("value");
            if (filter(this) && IsSelected() && name != null && value != null)
            {
                values[name] = value;
            }
            return values;
        }

        public override IRenderBox GetRenderBox(int containerWidth, int containerHeight)
        {
            using (new Logger("Menu.GetRenderBox(int, int)"))
            {
                RenderBoxTree prefix = new RenderBoxTree();
                prefix.MinHeight = 1;
                prefix.MaxHeight = 1;
                prefix.type = Type + "_prefix";
                RenderBoxLeaf prefixSelected = new RenderBoxLeaf(">>");
                prefixSelected.type = Type + "_prefixSelected";
                int prefixWidth = TextUtils.GetTextWidth(">> ");
                prefix.MaxWidth = prefixWidth;
                prefix.MinWidth = prefixWidth;
                prefixSelected.MaxWidth = prefixWidth;
                prefixSelected.MinWidth = prefixWidth - 1;
                prefixSelected.MaxHeight = 1;
                RenderBoxTree cache = new RenderBoxTree();
                UpdateRenderCacheProperties(cache, containerWidth, containerHeight);
                cache.type = Type;
                RenderBoxTree menuPoint;
                IRenderBox.TextAlign align;
                foreach (XMLTree child in Children)
                {
                    menuPoint = new RenderBoxTree();
                    menuPoint.type = Type + "_menupoint";
                    menuPoint.Flow = IRenderBox.FlowDirection.HORIZONTAL;
                    if (child.IsSelected())
                    {
                        menuPoint.Add(prefixSelected);
                    }
                    else
                    {
                        menuPoint.Add(prefix);
                    }
                    IRenderBox childBox = child.GetRenderBox(Math.Max(cache._MinWidth, cache._DesiredWidth), Math.Max(cache._MaxHeight, cache._DesiredHeight));
                    menuPoint.Add(childBox);
                    if (child.GetAttribute("alignself") == null
                            && Enum.TryParse<IRenderBox.TextAlign>(GetAttribute("alignchildren")?.ToUpper() ?? "LEFT", out align))
                        menuPoint.Align = align;
                    cache.Add(menuPoint);
                }
                return cache;
            }
        }
    }

    //EMBED SEScripts.XUI.XML.XMLTree
}
