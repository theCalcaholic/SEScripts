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
    public class Space : XMLTree
    {
        public Space() : base()
        {
            //Logger.debug("Space constructor()");
            //Logger.IncLvl();
            Type = "space";
            SetAttribute("width", "0");
            //Logger.DecLvl();
        }

        /*protected override void RenderText(ref List<string> segments, int width, int availableWidth)
        {
            //Logger.debug(Type + ".RenderText()");
            //Logger.IncLvl();
            segments.Add(TextUtils.CreateStringOfLength(" ", width));
            //Logger.DecLvl();
        }*/

        public override RenderBox GetRenderBox(int maxWidth, int maxHeight)
        {
            //Logger.debug("GetRenderCache(int)");
            //Logger.IncLvl();
            RenderBox cache = new RenderBoxLeaf();
            cache.type = Type;
            cache.MinHeight = 1;
            int width = ResolveSize(GetAttribute("width"), maxWidth);
            cache.MinWidth = width;
            cache.MaxWidth = width;
            //Logger.DecLvl();
            return cache;
        }
    }


    //EMBED SEScripts.XUI.XML.XMLTree
}
