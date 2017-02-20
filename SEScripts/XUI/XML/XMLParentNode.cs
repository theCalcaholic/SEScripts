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
    public interface XMLParentNode
    {

        XMLParentNode GetParent();

        void UpdateSelectability(XMLTree child);

        void KeyPress(string keyCode);

        void FollowRoute(Route route);

        bool SelectNext();

        void DetachChild(XMLTree child);
    }

    //EMBED SEScripts.XUI.XML.XMLTree
}
