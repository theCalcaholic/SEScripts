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
    public class SubmitButton : MenuItem
    {
        public SubmitButton()
        {
            Type = "submitbutton";
            SetAttribute("flowdirection", "horizontal");
        }

        /*protected override void PreRender(ref List<string> segments, int width, int availableWidth)
        {
            segments.Add(IsSelected() ? "[[  " : "[   ");
            base.PreRender(ref segments, width, availableWidth);
        }*/

        /*protected override string PostRender(List<string> segments, int width, int availableWidth)
        {
            segments.Add(IsSelected() ? "  ]]" : "   ]");
            return base.PostRender(segments, width, availableWidth);
        }*/
    }

    //EMBED SEScripts.XUI.XML.MenuItem
}
