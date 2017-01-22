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
using SEScripts.XUI.XML.DataStore;

namespace SEScripts.XUI.XMLExtensions
{
    
    class XMLMessage : DataStore
    {
        XMLMessage() : base()
        {
            XML.Type = "message";
        }

        static void Register()
        {
            XML.NodeRegister.Add("message", () => { return new XMLMessage(); });
        }
    }
}
