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

namespace SEScripts.MultiAgentNetwork.MAN.Models
{
    public class ServiceRegister : Dictionary<string, List<PlatformService>>
    {
        public ServiceRegister Merge(ServiceRegister other)
        {
            ServiceRegister merged = new ServiceRegister();
            foreach(KeyValuePair<string, List<PlatformService>> item in this)
            {
                merged.Add(item.Key, item.Value);
            }
            foreach(KeyValuePair<string, List<PlatformService>> item in other)
            {
                if(merged.ContainsKey(item.Key))
                {
                    merged[item.Key].AddList<PlatformService>(item.Value);
                    merged[item.Key] = Util.Uniques<PlatformService>(merged[item.Key]);
                } else
                {
                    merged[item.Key] = item.Value;
                }
            }
            return merged;
        }

        public string ToXML()
        {
            string xml = "<services>";
            foreach(KeyValuePair<string, List<PlatformService>> item in this) {
                foreach(PlatformService service in item.Value)
                {
                    xml += service.ToXML();
                }
            }
            xml += "</services>";
            return xml;
        }
    }

    //EMBED SEScripts.MultiAgentNetwork.MAN.Models.PlatformService
}