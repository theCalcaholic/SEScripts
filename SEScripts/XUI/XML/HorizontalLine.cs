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
    public class HorizontalLine : XMLTree
    {
        public HorizontalLine() : base()
        {
            Type = "hl";
            SetAttribute("width", "100%");
        }

        /*protected override void RenderText(ref List<string> segments, int width, int availableWidth)
        {
            segments.Add(TextUtils.CreateStringOfLength("_", width, TextUtils.RoundMode.CEIL));
        }*/

        public override RenderBox GetRenderBox(int maxWidth)
        {
            Logger.debug("HorizontalLine.GetRenderCache(int)");
            Logger.IncLvl();
            RenderBox cache = new RenderBoxLeaf();
            //cache.Add("_");
            cache.Height = 1;
            cache.PadString = "_";
            UpdateRenderCacheProperties(cache, maxWidth);
            Logger.DecLvl();
            return cache;
        }
    }

    //EMBED SEScripts.XUI.XML.XMLTree
}
