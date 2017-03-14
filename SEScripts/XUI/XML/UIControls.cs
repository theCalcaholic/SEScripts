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
    }


    //EMBED SEScripts.XUI.XML.XMLTree
}
