﻿using System;
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
    public class Break : TextNode
    {
        public Break() : base("")
        {
            Type = "br";
        }

        //protected override void RenderText(ref List<string> segments, int width, int availableWidth) { }

        /*protected override string PostRender(List<string> segments, int width, int availableWidth)
        {
            return "";
        }*/

        public override RenderBox GetRenderBox(int maxWidth)
        {
            Logger.debug("Break.GetRenderCache(int)");
            Logger.IncLvl();
            RenderBox cache = new RenderBoxLeaf();
            cache.Height = 0;
            Logger.DecLvl();
            return cache;
        }
    }


    //EMBED SEScripts.XUI.XML.TextNode
}
