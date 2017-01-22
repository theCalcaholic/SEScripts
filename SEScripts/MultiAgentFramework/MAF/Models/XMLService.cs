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

namespace SEScripts.MultiAgentFramework.MAF.Models
{
    class XMLService : XML.MenuItem
    {

        public static void SetUp()
        {
            Logger.debug("PlatformService.SetUp()");
            Logger.IncLvl();
            if (!XML.NodeRegister.ContainsKey("service"))
            {
                XML.NodeRegister.Add("service", () => {
                    return new XMLService();
                }
                );
            }
            Logger.DecLvl();
        }

        protected override void RenderText(ref List<string> segments, int width, int availableWidth)
        {
            string[] description = GetAttribute("description").Split('\n');
            segments.Add(
                GetAttribute("id") + ": " + description[0]
                );
            for(int i = 1; i < description.Length; i++)
            {
                segments.Add("  " + description[i]);
            }
        }
    }
}
