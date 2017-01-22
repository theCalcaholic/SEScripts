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
    public class Hidden : XMLTree
    {
        public Hidden() : base()
        {
            Type = "hidden";
        }
        protected override string PostRender(List<string> segments, int width, int availableWidth)
        {
            return null;
        }
    }


    //EMBED SEScripts.XUI.XML.DataStore
}
