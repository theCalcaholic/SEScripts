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
using SEScripts.XUI.BoxRenderer;

namespace SEScripts.XUI.XML
{
    public class SubmitButton : MenuItem
    {
        public SubmitButton()
        {
            Type = "submitbutton";
            SetAttribute("flowdirection", "horizontal");
        }

        /*protected override void PreRender(ref List<string> segments, int width, int availableWidth)
        {
            segments.Add(IsSelected() ? "[[  " : "[   ");
            base.PreRender(ref segments, width, availableWidth);
        }*/

        /*protected override string PostRender(List<string> segments, int width, int availableWidth)
        {
            segments.Add(IsSelected() ? "  ]]" : "   ]");
            return base.PostRender(segments, width, availableWidth);
        }*/

        public override RenderBox GetRenderBox(int maxWidth, int maxHeight)
        {
            //Logger.debug("SubmitButton.GetRenderCache(int)");
            //Logger.IncLvl();
            RenderBoxTree cache = new RenderBoxTree();
            cache.type = Type;
            RenderBoxLeaf childCache = new RenderBoxLeaf(IsSelected() ? "[[  " : "[   ");
            childCache.MaxWidth = childCache.MinWidth;
            cache.Add(childCache);
            RenderBoxTree contentCache = new RenderBoxTree();
            contentCache.Flow = GetAttribute("flow") == "horizontal" ? RenderBox.FlowDirection.HORIZONTAL : RenderBox.FlowDirection.VERTICAL;
            foreach (XMLTree child in Children)
            {
                contentCache.Add(child.GetRenderBox(maxWidth, maxHeight));
            }
            cache.Add(contentCache);
            childCache = new RenderBoxLeaf(IsSelected() ? "  ]]" : "   ]");
            childCache.MaxWidth = childCache.MinWidth;
            cache.Add(childCache);

            UpdateRenderCacheProperties(cache, maxWidth, maxHeight);

            cache.Flow = RenderBox.FlowDirection.HORIZONTAL;

            //Logger.DecLvl();
            return cache;
        }
    }

    //EMBED SEScripts.XUI.XML.MenuItem
}
