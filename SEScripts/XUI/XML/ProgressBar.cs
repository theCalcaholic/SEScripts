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
//using SEScripts.Lib.LoggerNS;
using SEScripts.XUI.BoxRenderer;

namespace SEScripts.XUI.XML
{
    public class ProgressBar : XMLTree
    {
        IRenderBox emptyBar;
        IRenderBox filledBar;

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
            SetAttribute("minwidth", "500");
            SetAttribute("filledstring", "|");
            SetAttribute("emptystring", "'");
            SetAttribute("value", fillLevel.ToString());
            SetAttribute("stepsize", "0.05");
            SetAttribute("selectable", selectable ? "true" : "false");
        }

        public void IncreaseFillLevel()
        {
            //Logger.debug(Type + ".IncreaseFillLevel()");
            //Logger.IncLvl();
            FillLevel += StepSize;
            //Logger.DecLvl();
        }

        public void DecreaseFillLevel()
        {
            //Logger.debug(Type + ".DecreaseFillLevel()");
            //Logger.IncLvl();
            FillLevel -= StepSize;
            //Logger.DecLvl();
        }

        public override void OnKeyPressed(string keyCode)
        {
            //Logger.debug(Type + ": OnKeyPressed():");
            //Logger.IncLvl();
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
            //Logger.DecLvl();
        }

        /*protected override void RenderText(ref List<string> segments, int width, int availableWidth)
        {
            //Logger.debug(Type + ".RenderText()");
            //Logger.IncLvl();
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
            //Logger.DecLvl();
        }*/

        public override IRenderBox GetRenderBox(int maxWidth, int maxHeight)
        {
            //Logger.debug("ProgressBar.GetRenderCache(int)");
            //Logger.IncLvl();
            RenderBoxTree cache = new RenderBoxTree();
            cache.type = Type;
            int outerWidth = TextUtils.GetTextWidth(IsSelected() ? "<[]>" : " [] ") + 2;
            string prefixString = (IsSelected() ? "<" : " ") + "[";
            IRenderBox prefix = new RenderBoxLeaf(prefixString);
            prefix.type = "progressbarPrefix";
            prefix.MinWidth = TextUtils.GetTextWidth(prefixString);
            prefix.MaxWidth = prefix.MinWidth;
            prefix.MinHeight = 1;
            string suffixString = "]" + (IsSelected() ? ">" : " ");
            IRenderBox suffix = new RenderBoxLeaf(suffixString);
            suffix.type = "progressbarSuffix";
            suffix.MinWidth = TextUtils.GetTextWidth(suffixString);
            suffix.MaxWidth = suffix.MinWidth;
            suffix.MinHeight = 1;
            cache.Add(prefix);

            filledBar = new RenderBoxLeaf();
            filledBar.type = "filledBar";
            filledBar.PadChar = GetAttribute("filledstring")[0];
            filledBar.MinHeight = 1;
            cache.Add(filledBar);

            emptyBar = new RenderBoxLeaf();
            emptyBar.type = "emptybar";
            emptyBar.MinHeight = 1;
            emptyBar.PadChar = GetAttribute("emptystring")[0];
            cache.Add(emptyBar);

            cache.Add(suffix);

            int width = ResolveSize(GetAttribute("minwidth"), maxWidth) ?? 0;
            if (width >= outerWidth)
            {
                filledBar.MinWidth = (int)((width - outerWidth) * FillLevel);
                emptyBar.MinWidth = (int)((width - outerWidth) * (1 - FillLevel));
            }
            width = ResolveSize(GetAttribute("maxwidth"), maxWidth) ?? 0;
            if (width >= outerWidth)
            {
                filledBar.MaxWidth = (int) ((width - outerWidth) * FillLevel);
                emptyBar.MaxWidth = (int)((width - outerWidth) * (1 - FillLevel));
            }
            width = ResolveSize(GetAttribute("width"), maxWidth) ?? 0;
            if (width >= outerWidth)
            {
                filledBar.DesiredWidth = (int)((width - outerWidth) * FillLevel);
                emptyBar.DesiredWidth = (int)((width - outerWidth) * (1 - FillLevel));
            }
            width = ResolveSize(GetAttribute("forcewidth"), maxWidth) ?? 0;
            if (width >= outerWidth)
            {
                int forcedWidth = (int)((width - outerWidth) * FillLevel);
                filledBar.MaxWidth = forcedWidth;
                filledBar.MinWidth = forcedWidth;
                forcedWidth = (int)((width - outerWidth) * (1 - FillLevel));
                emptyBar.MinWidth = forcedWidth;
                emptyBar.MaxWidth = forcedWidth;
            }
            //UpdateRenderCacheProperties(cache, maxWidth, maxHeight);

            //Logger.log("filledBar: ");
            //Logger.debug = false;
            //Logger.log("  fillLevel: " + FillLevel);
            //Logger.log("  min width: " + filledBar.MinWidth);
            //Logger.log("  max width: " + filledBar.MaxWidth);
            //Logger.log("  desired width: " + filledBar.DesiredWidth);
            //Logger.log("  minheight: " + filledBar.MinHeight);
            //Logger.debug = true;
            //Logger.log("  actual width: " + filledBar.GetActualWidth(maxWidth));

            cache.Flow = IRenderBox.FlowDirection.HORIZONTAL;
            //GetAttribute("flow") == "horizontal" ? NodeBox.FlowDirection.HORIZONTAL : NodeBox.FlowDirection.VERTICAL;

            switch (GetAttribute("alignself"))
            {
                case "right":
                    cache.Align = IRenderBox.TextAlign.RIGHT;
                    break;
                case "center":
                    cache.Align = IRenderBox.TextAlign.CENTER;
                    break;
                default:
                    cache.Align = IRenderBox.TextAlign.LEFT;
                    break;
            }
            //Logger.DecLvl();
            return cache;
        }
    }

    //EMBED SEScripts.XUI.XML.XMLTree
}
