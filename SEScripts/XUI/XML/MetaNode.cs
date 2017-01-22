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
    class MetaNode : Hidden
    {
        public MetaNode() : base()
        {
            Type = "meta";
        }

        public override Dictionary<string, string> GetValues(Func<XMLTree, bool> filter)
        {
            if (filter(this))
            {
                return Attributes;
            }
            else
            {
                return new Dictionary<string, string>();
            }
        }
    }
}
