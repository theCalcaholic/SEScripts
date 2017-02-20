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

namespace SEScripts.Lib
{
    public static class Util
    {

        public static List<T> Uniques<T>(List<T> list) {

            List<T> listOut = new List<T>();
            bool duplicate;
            foreach(T itemIn in list)
            {
                duplicate = false;
                foreach (T itemOut in listOut)
                {
                    if( itemOut.Equals(itemIn) )
                    {
                        duplicate = true;
                    }
                }
                if(!duplicate)
                {
                    listOut.Add(itemIn);
                }
            }
            return listOut;
        }
    }
}
