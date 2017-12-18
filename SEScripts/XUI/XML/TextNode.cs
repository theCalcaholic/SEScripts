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
    public class TextNode : XMLTree
    {
        public string Content;

        public TextNode(string content) : base()
        {
            //Logger.debug("TextNode constructor()");
            //Logger.IncLvl();
            //Logger.debug("content: " + content);
            Type = "textnode";
            Content = content;
            Content.Replace("\n", "");
            Content = Content.Trim(new char[] { '\n', ' ', '\r' });
            SetAttribute("width", "100%");
            //Logger.debug("final content: " + Content);
            RerenderRequired = true;
            //Logger.DecLvl();
        }

        public override IRenderBox GetRenderBox(int maxWidth, int maxHeight)
        {
            //using (new Logger("XMLTree<" + Type + ">.GetRenderBox(int, int)"))
            //{
                //Logger.debug("TextNode.GetRenderCache(int)");
                //Logger.IncLvl();
                IRenderBox cache = new RenderBoxLeaf(Content);
                cache.type = Type;
                UpdateRenderCacheProperties(cache, maxWidth, maxHeight);
                //Logger.DecLvl();
                return cache;
            //}
        }

        //protected override void RenderText(ref List<string> segments, int width, int availableWidth) { }

        /*protected override string PostRender(List<string> segments, int width, int availableWidth)
        {
            return Content;
        }*/


        /*protected override void BuildRenderCache()
        {
            //Logger.debug(Type + ".BuildRenderCache()");
            //Logger.IncLvl();
            RerenderRequired = false;
            //Logger.DecLvl();
        }*/
    }

    //EMBED SEScripts.XUI.XML.XMLTree
}
