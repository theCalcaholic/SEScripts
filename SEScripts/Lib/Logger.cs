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

namespace SEScripts.Lib
{
    public static class Logger
    {
        static IMyTextPanel DebugPanel;
        static public bool DEBUG = false;
        static int offset = 0;

        public static void log(string msg)
        {
            if (DebugPanel == null)
            {
                return;
            }
            string prefix = "";
            for (int i = 0; i < offset; i++)
            {
                prefix += "  ";
            }
            DebugPanel.WritePublicText(prefix + msg + "\n", true);
            //P.Echo(prefix + msg);
        }

        public static void debug(string msg)
        {
            if (!DEBUG)
            {
                return;
            }
            log(msg);
        }

        public static void IncLvl()
        {
            offset += 2;
        }

        public static void DecLvl()
        {
            offset = offset - 2;
        }
    }

}
