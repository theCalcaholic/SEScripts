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
    public class TextNode : XMLTree
    {
        public string Content;

        public TextNode(string content) : base()
        {
            Logger.debug("TextNode constructor()");
            Logger.IncLvl();
            Type = "textnode";
            Content = content.Replace("\n", "");
            Content = Content.Trim(new char[] { '\n', ' ', '\r' });
            RenderBox = new NodeBoxLeaf(Content);
            RerenderRequired = false;
            Logger.DecLvl();
        }

        //protected override void RenderText(ref List<string> segments, int width, int availableWidth) { }

        /*protected override string PostRender(List<string> segments, int width, int availableWidth)
        {
            return Content;
        }*/

        protected override void BuildRenderCache()
        {
            Logger.debug(Type + ".BuildRenderCache()");
            Logger.IncLvl();
            RenderBox.Clear();
            RenderBox.Add(Content);
            RerenderRequired = false;
            Logger.DecLvl();
        }
    }

    //EMBED SEScripts.XUI.XML.XMLTree
}
