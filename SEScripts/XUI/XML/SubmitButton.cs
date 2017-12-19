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

        public override IRenderBox GetRenderBox(int containerWidth, int containerHeight)
        {
            //Logger.debug("SubmitButton.GetRenderCache(int)");
            //Logger.IncLvl();
            RenderBoxTree cache = new RenderBoxTree();
            cache.type = Type;
            RenderBoxLeaf childCache = new RenderBoxLeaf(IsSelected() ? "[[  " : "[   ");
            childCache.MaxWidth = childCache.MinWidth;
            cache.Add(childCache);
            RenderBoxTree contentCache = new RenderBoxTree();
            contentCache.Flow = GetAttribute("flow") == "horizontal" ? IRenderBox.FlowDirection.HORIZONTAL : IRenderBox.FlowDirection.VERTICAL;
            UpdateRenderCacheProperties(cache, containerWidth, containerHeight);
            foreach (XMLTree child in Children)
            {
                contentCache.Add(child.GetRenderBox(Math.Max(cache._DesiredWidth, cache._DesiredWidth), Math.Max(cache._DesiredHeight, cache._MinHeight)));
            }
            cache.Add(contentCache);
            childCache = new RenderBoxLeaf(IsSelected() ? "  ]]" : "   ]");
            childCache.MaxWidth = childCache.MinWidth;
            cache.Add(childCache);


            cache.Flow = IRenderBox.FlowDirection.HORIZONTAL;

            //Logger.DecLvl();
            return cache;
        }
    }

    //EMBED SEScripts.XUI.XML.MenuItem
}
