﻿using System;
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
    public class HiddenData : DataStore
    {
        public HiddenData() : base()
        {
            Type = "hiddendata";
        }
        /*protected override string PostRender(List<string> segments, int width, int availableWidth)
        {
            return null;
        }*/
        public override NodeBox RenderCache
        {
            get
            {
                NodeBox cache = new NodeBoxTree();
                cache.ForcedWidth = 0;
                cache.Height = 0;
                return cache;
            }
        }
    }


    //EMBED SEScripts.XUI.XML.DataStore
}
