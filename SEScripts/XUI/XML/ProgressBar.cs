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
    public class ProgressBar : XMLTree
    {
        float StepSize
        {
            get
            {
                float stepSize;
                if (!Single.TryParse(GetAttribute("stepsize"), out stepSize))
                {
                    return 0.1f;
                }
                return stepSize;
            }
            set
            {
                string valueString = Math.Max(0.001f, Math.Min(0.009f, value)).ToString();
                if (valueString.Length > 5)
                {
                    valueString += valueString.Substring(0, 5);
                }
                SetAttribute("stepsize", valueString);
            }
        }
        public float FillLevel
        {
            get
            {
                float fillLevel;
                if (!Single.TryParse(GetAttribute("value"), out fillLevel))
                {
                    return 0.0f;
                }
                return fillLevel;
            }
            set
            {
                string valueString = Math.Max(0f, Math.Min(1f, value)).ToString();
                if (valueString.Length > 5)
                {
                    valueString = valueString.Substring(0, 5);
                }
                SetAttribute("value", valueString);
            }
        }

        public ProgressBar() : this(0f)
        {
        }

        public ProgressBar(float fillLevel) : this(fillLevel, false)
        {
        }

        public ProgressBar(float fillLevel, bool selectable) : base()
        {
            Type = "progressbar";
            PreventDefault("LEFT/ABORT");
            PreventDefault("RIGHT/SUBMIT");
            SetAttribute("width", "500");
            SetAttribute("filledstring", "|");
            SetAttribute("emptystring", "'");
            SetAttribute("value", fillLevel.ToString());
            SetAttribute("stepsize", "0.05");
            SetAttribute("selectable", selectable ? "true" : "false");
        }

        public void IncreaseFillLevel()
        {
            Logger.debug(Type + ".IncreaseFillLevel()");
            Logger.IncLvl();
            FillLevel += StepSize;
            Logger.DecLvl();
        }

        public void DecreaseFillLevel()
        {
            Logger.debug(Type + ".DecreaseFillLevel()");
            Logger.IncLvl();
            FillLevel -= StepSize;
            Logger.DecLvl();
        }

        public override void OnKeyPressed(string keyCode)
        {
            Logger.debug(Type + ": OnKeyPressed():");
            Logger.IncLvl();
            switch (keyCode)
            {
                case "LEFT/ABORT":
                    DecreaseFillLevel();
                    break;
                case "RIGHT/SUBMIT":
                    IncreaseFillLevel();
                    break;
            }

            base.OnKeyPressed(keyCode);
            Logger.DecLvl();
        }

        /*protected override void RenderText(ref List<string> segments, int width, int availableWidth)
        {
            Logger.debug(Type + ".RenderText()");
            Logger.IncLvl();
            string suffix = IsSelected() ? ">" : "  ";
            string prefix = IsSelected() ? "<" : "  ";
            string renderString = prefix + "[";
            float fillLevel = FillLevel;
            string fillString = GetAttribute("filledstring");
            string emptyString = GetAttribute("emptystring");
            int innerWidth = (width - 2 * TextUtils.GetTextWidth("[]"));
            renderString += TextUtils.CreateStringOfLength(fillString, (int)(innerWidth * fillLevel));
            renderString += TextUtils.CreateStringOfLength(emptyString, (int)(innerWidth * (1 - fillLevel)));
            renderString += "]" + suffix;
            segments.Add(renderString);
            Logger.DecLvl();
        }*/

        public override NodeBox RenderCache
        {
            get
            {
                NodeBoxTree cache = new NodeBoxTree();
                NodeBox prefix = new NodeBoxLeaf(
                    (IsSelected() ? "<" : " ") + "[");
                NodeBox suffix = new NodeBoxLeaf(
                    "]" + (IsSelected() ? ">" : " "));
                cache.Add(prefix);
                cache.Add(suffix);

                return cache;
            }
        }
    }

    //EMBED SEScripts.XUI.XML.XMLTree
}
