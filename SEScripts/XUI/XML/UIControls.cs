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
    public class UIControls : XMLTree
    {
        UIController Controller;

        public UIControls() : base()
        {
            Type = "uicontrols";
            Controller = null;
            SetAttribute("selectable", "false");
        }

        private void UpdateController()
        {
            Controller = RetrieveRoot() as UIController;
            SetAttribute("selectable", (Controller != null && Controller.UIStack.Count > 0) ? "true" : "false");
            if (IsSelectable())
            {
                PreventDefault("LEFT/ABORT");
                PreventDefault("RIGHT/SUBMIT");
            }
            else
            {
                AllowDefault("LEFT/ABORT");
                AllowDefault("RIGHT/SUBMIT");
            }
            GetParent().UpdateSelectability(this);
            if (IsSelected() && !IsSelectable())
            {
                GetParent().SelectNext();
            }
        }

        public override void OnKeyPressed(string keyCode)
        {
            if (Controller == null)
            {
                UpdateController();
            }
            switch (keyCode)
            {
                case "LEFT/ABORT":
                case "RIGHT/SUBMIT":
                    if (Controller != null && Controller.UIStack.Count > 0)
                    {
                        Controller.RevertUI();
                    }
                    break;
            }
        }

        /*protected override string PostRender(List<string> segments, int width, int availableWidth)
        {
            if (Controller == null)
            {
                UpdateController();
            }
            string prefix;
            if (!IsSelectable())
            {
                prefix = "";
            }
            else
            {
                prefix = IsSelected() ? "<<" : TextUtils.CreateStringOfLength(" ", TextUtils.GetTextWidth("<<"));
            }
            string renderString = base.PostRender(segments, width, availableWidth);
            int prefixSpacesCount = TextUtils.CreateStringOfLength(" ", TextUtils.GetTextWidth(prefix)).Length;
            string tmpPrefix = "";
            for (int i = 0; i < prefixSpacesCount; i++)
            {
                if ((renderString.Length - 1) < i || renderString[i] != ' ')
                {
                    tmpPrefix += " ";
                }
            }
            renderString = prefix + (tmpPrefix + renderString).Substring(prefixSpacesCount);
            return renderString;
            //renderString = prefix + renderString;
        }*/

        public override RenderBox GetRenderBox(int maxWidth, int maxHeight)
        {
            //using (new Logger("XMLTree<" + Type + ">.GetRenderBox(int, int)"))
            //{
                //Logger.debug("UIControls.GetRenderCache(int)");
                //Logger.IncLvl();
                RenderBoxTree cache = new RenderBoxTree();
                cache.type = Type;
                if (Controller == null)
                {
                    UpdateController();
                }

                if (IsSelectable())
                {
                    RenderBox childCache = new RenderBoxLeaf(IsSelected() ?
                        new StringBuilder("<<") :
                        TextUtils.CreateStringOfLength(' ', TextUtils.GetTextWidth("<<")));
                    childCache.MaxWidth = childCache.MinWidth;
                    cache.Add(childCache);
                }
                RenderBoxTree contentCache = new RenderBoxTree();
                contentCache.Flow = GetAttribute("flow") == "horizontal" ? RenderBox.FlowDirection.HORIZONTAL : RenderBox.FlowDirection.VERTICAL;

                foreach (XMLTree child in Children)
                {
                    contentCache.Add(child.GetRenderBox(maxWidth, maxHeight));
                }
                cache.Add(contentCache);

                UpdateRenderCacheProperties(cache, maxWidth, maxHeight);
                cache.Flow = RenderBox.FlowDirection.HORIZONTAL;
                
                return cache;
            }
        //}
    }


    //EMBED SEScripts.XUI.XML.XMLTree
}
