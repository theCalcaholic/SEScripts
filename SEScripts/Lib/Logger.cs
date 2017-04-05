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
    public class Logger : IDisposable
    {
        public enum Mode { DEBUG, LOG, ERROR, WARNING, CONSOLE};
        private static StringBuilder Log = new StringBuilder();
        static public bool DEBUG = false;
        protected static StringBuilder Prefix = new StringBuilder();
        protected Program Prog;
        protected Mode logMode;
        private bool disposed;
        public static string Output
        {
            get { return Log.ToString(); }
        }

        public Logger(string message, 
            [System.Runtime.CompilerServices.CallerMemberName] string methodName = "") : this(message, Mode.DEBUG, methodName) {}

        public Logger(string message, Mode mode,
            [System.Runtime.CompilerServices.CallerMemberName] string methodName = "") : this(message, mode, null, methodName) {}

        public Logger(string message, Mode mode, Program program,
            [System.Runtime.CompilerServices.CallerMemberName] string methodName = "")
        {
            disposed = false;
            if (!DEBUG && mode == Mode.DEBUG)
                return;
            Prog = program;
            logMode = mode;
            log(message, logMode);
            IncLvl();
        }

        public void log(string message, Mode mode)
        {
            log(new StringBuilder(message), mode);
        }
        public void log(StringBuilder message, Mode mode)
        {

            StringBuilder msg = new StringBuilder().Append(Prefix);
            if (logMode != Mode.LOG && logMode != Mode.CONSOLE)
                msg.Append(logMode.ToString()).Append(": ");
            msg.Append(message);
            Log.Append(msg).Append("\n");

            if (logMode == Mode.CONSOLE)
            {
                Console.WriteLine(msg); //REMOVE
                Prog?.Echo(msg.ToString());
            }
            //MERGESCRIPT TEST
        }

        private void IncLvl()
        {
            Prefix.Append("  ");
        }

        private void DecLvl()
        {
            if( Prefix.Length >= 2)
                Prefix.Remove(Prefix.Length - 2, 2);
        }
        
        public virtual void Dispose()
        {
            if (!disposed)
            {
                DecLvl();
            }
            disposed = true;
        }
    }

}
