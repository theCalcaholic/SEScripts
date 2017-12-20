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
//using SEScripts.Lib.LoggerNS;
using SEScripts.XUI.BoxRenderer;

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

        public override IRenderBox GetRenderBox(int maxWidth, int maxHeight)
        {
            //using (new Logger("Break.GetRenderBox(int, int)"))
            //{
                IRenderBox cache = new RenderBoxLeaf("\n");
                cache.type = Type;
                cache.MaxHeight = (GetParent() as XMLTree)?.GetAttribute("flow") == "horizontal" ? 1 : 0;
                cache.MaxWidth = 0;
                return cache;
            //}
        }
    }


    //EMBED SEScripts.XUI.XML.TextNode
}
