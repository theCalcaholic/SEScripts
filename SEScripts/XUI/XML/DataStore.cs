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
    public abstract class DataStore : XMLTree
    {
        public DataStore() : base() { }

        public override Dictionary<string, string> GetValues(Func<XMLTree, bool> filter)
        {
            Dictionary<string, string> dict = base.GetValues(filter);
            if (!filter(this))
            {
                return dict;
            }
            foreach (KeyValuePair<string, string> data in Attributes)
            {
                if (!dict.ContainsKey(data.Key))
                {
                    dict[data.Key] = data.Value;
                }
            }
            return dict;
        }
    }


    //EMBED SEScripts.XUI.XML.XMLTree
}
