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
using SEScripts.Lib.LoggerNS;
using SEScripts.XUI.BoxRenderer;

namespace SEScripts.XUI.XML
{
    public class ProgressBar : XMLTree
    {
        RenderBox emptyBar;
        RenderBox filledBar;

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
                if (fillLevel < 0 || fillLevel > 1)
                    return 0.0f;
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

        public override RenderBox GetRenderBox(int maxWidth, int maxHeight)
        {
            Logger.debug("ProgressBar.GetRenderCache(int)");
            Logger.IncLvl();
            RenderBoxTree cache = new RenderBoxTree();
            int outerWidth = TextUtils.GetTextWidth(IsSelected() ? new StringBuilder("<[]>") : new StringBuilder(" [] ")) + 2;
            RenderBox prefix = new RenderBoxLeaf(
                (IsSelected() ? "<" : " ") + "[");
            prefix.MaxWidth = prefix.MinWidth;
            RenderBox suffix = new RenderBoxLeaf(
                "]" + (IsSelected() ? ">" : " "));
            suffix.MaxWidth = suffix.MinWidth;
            cache.Add(prefix);

            filledBar = new RenderBoxLeaf();
            filledBar.PadString = GetAttribute("filledstring");
            filledBar.MinHeight = 1;
            cache.Add(filledBar);

            emptyBar = new RenderBoxLeaf();
            emptyBar.MinHeight = 1;
            emptyBar.PadString = GetAttribute("emptystring");
            cache.Add(emptyBar);

            cache.Add(suffix);

            int width = ResolveSize(GetAttribute("minwidth"), maxWidth);
            if (width >= outerWidth)
            {
                filledBar.MinWidth = (int)((width - outerWidth) * FillLevel);
                emptyBar.MinWidth = (int)((width - outerWidth) * (1 - FillLevel));
            }
            width = ResolveSize(GetAttribute("maxwidth"), maxWidth);
            if (width >= outerWidth)
            {
                filledBar.MaxWidth = (int) ((width - outerWidth) * FillLevel);
                emptyBar.MaxWidth = (int)((width - outerWidth) * (1 - FillLevel));
            }
            width = ResolveSize(GetAttribute("width"), maxWidth);
            if (width >= outerWidth)
            {
                filledBar.DesiredWidth = (int)((width - outerWidth) * FillLevel);
                emptyBar.DesiredWidth = (int)((width - outerWidth) * (1 - FillLevel));
            }
            width = ResolveSize(GetAttribute("forcewidth"), maxWidth);
            if (width >= outerWidth)
            {
                int forcedWidth = (int)((width - outerWidth) * FillLevel);
                filledBar.MaxWidth = forcedWidth;
                filledBar.MinWidth = forcedWidth;
                forcedWidth = (int)((width - outerWidth) * (1 - FillLevel));
                emptyBar.MinWidth = forcedWidth;
                emptyBar.MaxWidth = forcedWidth;
            }
            UpdateRenderCacheProperties(cache, maxWidth, maxHeight);

            Logger.log("filledBar: ");
            Logger.DEBUG = false;
            Logger.log("  fillLevel: " + FillLevel);
            Logger.log("  min width: " + filledBar.MinWidth);
            Logger.log("  max width: " + filledBar.MaxWidth);
            Logger.log("  desired width: " + filledBar.DesiredWidth);
            Logger.log("  minheight: " + filledBar.MinHeight);
            Logger.DEBUG = true;
            Logger.log("  actual width: " + filledBar.GetActualWidth(maxWidth));

            cache.Flow = RenderBox.FlowDirection.HORIZONTAL;
            //GetAttribute("flow") == "horizontal" ? NodeBox.FlowDirection.HORIZONTAL : NodeBox.FlowDirection.VERTICAL;

            switch (GetAttribute("alignself"))
            {
                case "right":
                    cache.Align = RenderBox.TextAlign.RIGHT;
                    break;
                case "center":
                    cache.Align = RenderBox.TextAlign.CENTER;
                    break;
                default:
                    cache.Align = RenderBox.TextAlign.LEFT;
                    break;
            }
            Logger.DecLvl();
            return cache;
        }
    }

    //EMBED SEScripts.XUI.XML.XMLTree
}
