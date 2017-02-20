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
    public class RootNode : XMLTree
    {

        public RootNode() : base()
        {
            Type = "root";
            PreventDefault("UP");
            PreventDefault("DOWN");
        }

        public override string GetAttribute(string key)
        {
            XMLTree meta = GetNode((node) => { return node.Type == "meta"; });
            string value;
            if (meta != null)
            {
                value = meta.GetAttribute(key);
            }
            else
            {
                value = base.GetAttribute(key);
            }
            switch (key)
            {
                case "width":
                    if (value == null)
                    {
                        value = "100%";
                    }
                    break;
            }
            return value;
        }

        public override void SetAttribute(string key, string value)
        {
            XMLTree meta = GetNode((node) => { return node.Type == "meta"; });
            if (meta != null)
            {
                meta.SetAttribute(key, value);
            }
            else
            {
                base.SetAttribute(key, value);
            }
        }

        public override void UpdateSelectability(XMLTree child)
        {
            base.UpdateSelectability(child);
            if (IsSelectable() && !IsSelected())
            {
                SelectFirst();
            }
        }

        public override bool SelectNext()
        {
            if (IsSelectable() && !base.SelectNext())
            {
                return SelectNext();
            }
            return true;
        }

        public override bool SelectPrevious()
        {
            if (!base.SelectPrevious())
            {
                return SelectPrevious();
            }
            return true;
        }

        public override void OnKeyPressed(string keyCode)
        {
            switch (keyCode)
            {
                case "UP":
                    SelectPrevious();
                    break;
                case "DOWN":
                    SelectNext();
                    break;
            }
        }
    }

    //EMBED SEScripts.XUI.XML.XMLTree
}
