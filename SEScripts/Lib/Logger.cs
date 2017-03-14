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

namespace SEScripts.Lib.LoggerNS
{
    public static class Logger
    {
        private static StringBuilder Log = new StringBuilder();
        public static string Output
        {
            get { return Log.ToString(); }
        }
        static IMyTextPanel DebugPanel;
        static public bool DEBUG = false;
        public static int offset = 0;
        private static StringBuilder Prefix = new StringBuilder();
        


        public static void log(string msg)
        {
            Log.Append(Prefix);
            Log.Append(msg + "\n");
            Console.WriteLine(Prefix + msg);
            //!UNCOMMENT P.Echo(Prefix + msg);
        }

        public static void debug(string msg)
        {
            if (DEBUG)
            {
                log(msg);
            }
        }

        public static void IncLvl()
        {
            Prefix.Append("  ");
        }

        public static void DecLvl()
        {
            if( Prefix.Length >= 2)
                Prefix.Remove(Prefix.Length - 2, 2);
        }
    }

}
