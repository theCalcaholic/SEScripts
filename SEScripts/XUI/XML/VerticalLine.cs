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
    public class VerticalLine : XMLTree
    {
        public VerticalLine() : base()
        {
            Type = "vl";
            SetAttribute("height", "100%");
            //SetAttribute("minwidth", "4");
        }

        /*protected override void RenderText(ref List<string> segments, int width, int availableWidth)
        {
            segments.Add(TextUtils.CreateStringOfLength("_", width, TextUtils.RoundMode.CEIL));
        }*/

        public override IRenderBox GetRenderBox(int maxWidth, int maxHeight)
        {
            //using (new Logger("VerticalLine.GetRenderBox()"))
            //{
                IRenderBox cache = new RenderBoxLeaf();
                cache.PadChar = '|';
                cache.type = Type;
                //cache.Add("_");
                UpdateRenderCacheProperties(cache, maxWidth, maxHeight);
                cache.MinWidth = TextUtils.GetCharWidth('|');
                return cache;
            //}
        }
    }

    //EMBED SEScripts.XUI.XML.XMLTree
}
