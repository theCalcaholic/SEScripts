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
    public class Hidden : XMLTree
    {
        public Hidden() : base()
        {
            Type = "hidden";
        }
        /*protected override string PostRender(List<string> segments, int width, int availableWidth)
        {
            return null;
        }*/

        public override IRenderBox GetRenderBox(int maxWidth, int maxHeight)
        {
            //using (new Logger("Hidden.GetRenderCache(int)", Logger.Mode.LOG))
            //{
                IRenderBox cache = new RenderBoxTree();
                cache.type = Type;
                cache.MaxWidth = 0;
                cache.MaxHeight = 0;
                return cache;
            //}
        }
    }


    //EMBED SEScripts.XUI.XML.DataStore
}
