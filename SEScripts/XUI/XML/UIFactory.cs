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

using SEScripts.Lib.LoggerNS;

namespace SEScripts.XUI.XML
{
    public abstract class UIFactory
    {
        private int Count;
        private int Max;
        private List<UIController> UIs;

        public UIFactory() : this(null) { }

        public UIFactory(List<UIController> uiList)
        {
            //Logger.debug("UIFactory constructor");
            //Logger.IncLvl();
            if (uiList == null)
            {
                UIs = new List<UIController>();
            }
            UIs = uiList;
            //Logger.DecLvl();
        }

        public abstract XMLTree Render(UIController controller);

        protected void UpdateUIs(XMLTree renderedUI)
        {
            foreach (UIController ui in UIs)
            {
                ui.LoadUI(renderedUI);
            }
        }
    }


    //EMBED SEScripts.XUI.XML.UIController
    //EMBED SEScripts.XUI.XML.XMLTree
}
