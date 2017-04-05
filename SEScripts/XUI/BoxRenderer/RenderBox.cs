using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SEScripts.Lib;
using SEScripts.Lib.LoggerNS;
using SEScripts.Lib.Profilers;

namespace SEScripts.XUI.BoxRenderer
{
    public abstract class RenderBox
    {
        protected bool minHeightIsCached;
        protected bool minWidthIsCached;
        protected int minHeightCache;
        protected int minWidthCache;
        public bool DEBUG = false;
        public char PadChar;
        public enum TextAlign { LEFT, RIGHT, CENTER }
        public enum FlowDirection { HORIZONTAL, VERTICAL }
        //public abstract int Height { get; set; }

        public abstract void Add(string box);
        public abstract void Add(StringBuilder box);
        public abstract void AddAt(int position, string box);
        public abstract void AddAt(int position, StringBuilder box);
        public abstract StringBuilder GetLine(int index);
        public abstract StringBuilder GetLine(int index, int maxWidth, int maxHeight);
        public abstract void Clear();
        private RenderBox.FlowDirection _Flow;
        private RenderBox.TextAlign _Align;
        protected int _MinWidth;
        protected int _MaxWidth;
        protected int _DesiredWidth;
        protected int _MinHeight;
        protected int _MaxHeight;
        protected int _DesiredHeight;
        public RenderBox Parent;
        public string type;

        public int GetActualWidth(int maxWidth)
        {
            using (Logger logger = new Logger("RenderBox.GetActualWidth(int)", Logger.Mode.LOG))
            {
                //Logger.debug("NodeBox.GetActualWidth(int)");
                //Logger.IncLvl();
                if (MaxWidth != -1)
                    maxWidth = (maxWidth == -1 ? MaxWidth : Math.Min(MaxWidth, maxWidth));
                if (maxWidth == -1)
                {
                    //Logger.debug("actual width equals min width");
                    //Logger.DecLvl();
                    return Math.Max(MinWidth, DesiredWidth);
                }
                else
                {
                    int desired;
                    if (DesiredWidth == -1)
                    {
                        //Logger.debug("actual width equals max width");
                        desired = maxWidth;
                    }
                    else
                    {
                        //Logger.debug("actual width equals desired width, but if desired<min -> width=min and if desired>max -> width = max");
                        desired = Math.Max(MinWidth, DesiredWidth);
                    }
                    //Logger.DecLvl();
                    logger.log("actual width: " + Math.Min(desired, maxWidth), Logger.Mode.LOG);
                    return Math.Min(desired, maxWidth);
                }
            }

        }
        public int GetActualHeight(int maxHeight)
        {
            using (Logger logger = new Logger("RenderBox.GetActualHeight(int)", Logger.Mode.LOG))
            {
                logger.log("implicit max height: " + maxHeight, Logger.Mode.LOG);
                logger.log("explicit max height: " + MaxHeight, Logger.Mode.LOG);
                //Logger.debug("NodeBox.GetActualHeight(int)");
                //Logger.IncLvl();
                if (MaxHeight != -1)
                    maxHeight = (maxHeight == -1 ? MaxHeight : Math.Min(MaxHeight, maxHeight));

                if (maxHeight == -1)
                {
                    logger.log("actual height equals min height", Logger.Mode.LOG);
                    //Logger.DecLvl();
                    return DesiredHeight == -1 ? MinHeight : Math.Min(MinHeight, DesiredHeight);
                }
                else
                {
                    int desired = DesiredHeight == -1 ? MinHeight : Math.Max(MinHeight, DesiredHeight);
                    //Logger.DecLvl();
                    logger.log("actual height: " + Math.Min(desired, maxHeight), Logger.Mode.LOG);
                    return Math.Min(desired, maxHeight);
                }
            }

        }

        public RenderBox.TextAlign Align
        {
            get { return _Align; }
            set
            {
                _Align = value;
            }
        }

        public virtual RenderBox.FlowDirection Flow
        {
            get { return _Flow; }
            set {
                _Flow = value;
                ClearCache();
            }
        }

        public virtual int MinWidth
        {
            get
            {
                using (new SimpleProfiler("RenderBox.MinWidth.get"))
                {
                    //Logger.debug("NodeBox.MinWidth.get()");
                    //Logger.IncLvl();
                    //Logger.debug("minwidth = " + _MinWidth);
                    //Logger.DecLvl();
                    return _MinWidth;
                }
            }
            set
            {
                using (new SimpleProfiler("RenderBox.MinWidth.get"))
                {
                    //Logger.debug("NodeBox.MinWidth.set()");
                    //Logger.IncLvl();
                    //Logger.debug("minwidth = " + value);
                    _MinWidth = Math.Max(0, value);
                    ClearCache();
                    //Logger.DecLvl();
                }
            }
        }

        public int DesiredWidth
        {
            get
            {
                //Logger.debug("NodeBox.DesiredWidth.get()");
                //Logger.IncLvl();
                //Logger.debug("desiredwidth = " + _DesiredWidth);
                //Logger.DecLvl();
                return _DesiredWidth;
            }
            set
            {
                //Logger.debug("NodeBox.DesiredWidth.set()");
                //Logger.IncLvl();
                //Logger.debug("desiredwidth = " + value);
                _DesiredWidth = value;
                //Logger.DecLvl();
            }
        }

        public int MaxWidth
        {
            get
            {
                //Logger.debug("NodeBox.MaxWidth.get()");
                //Logger.IncLvl();
                //Logger.debug("maxwidth = " + _MaxWidth);
                //Logger.DecLvl();
                return _MaxWidth;
            }
            set
            {
                //Logger.debug("NodeBox.MaxWidth.set()");
                //Logger.IncLvl();
                //Logger.debug("maxwidth = " + value);
                _MaxWidth = value;
                //Logger.DecLvl();
            }
        }

        public virtual int MinHeight
        {
            get
            {
                using (new SimpleProfiler("RenderBox.MinHeight.get"))
                {
                    //Logger.debug("NodeBox.MinHeight.get()");
                    //Logger.IncLvl();
                    //Logger.debug("minheight = " + _MinHeight);
                    //Logger.DecLvl();
                    return _MinHeight;
                }
            }
            set
            {
                using (new SimpleProfiler("RenderBox.MinHeight.set"))
                {
                    //Logger.debug("NodeBox.MinHeight.set()");
                    //Logger.IncLvl();
                    //Logger.debug("minheight = " + value);
                    _MinHeight = Math.Max(0, value);
                    ClearCache();
                    //Logger.DecLvl();
                }
            }
        }

        public int DesiredHeight
        {
            get
            {
                //Logger.debug("NodeBox.DesiredHeight.get()");
                //Logger.IncLvl();
                //Logger.debug("desiredheight = " + _DesiredHeight);
                //Logger.DecLvl();
                return _DesiredHeight;
            }
            set
            {
                //Logger.debug("NodeBox.DesiredHeight.set()");
                //Logger.IncLvl();
                //Logger.debug("desiredheight = " + value);
                _DesiredHeight = value;
                //Logger.DecLvl();
            }
        }

        public int MaxHeight
        {
            get
            {
                //Logger.debug("NodeBox.MaxHeight.get()");
                //Logger.IncLvl();
                //Logger.debug("maxheight = " + _MaxHeight);
                //Logger.DecLvl();
                return _MaxHeight;
            }
            set
            {
                //Logger.debug("NodeBox.MaxHeight.set()");
                //Logger.IncLvl();
                //Logger.debug("maxheight = " + value);
                using (Logger logger = new Logger("RenderBox.MaxHeight.set", Logger.Mode.LOG))
                {
                    logger.log("value: " + value, Logger.Mode.LOG);
                    _MaxHeight = value;
                    ClearCache();
                }
                //Logger.DecLvl();
            }
        }

        public RenderBox()
        {
            using (new Logger("RenderBox.__construct()", Logger.Mode.LOG))
            {
                //Logger.debug("NodeBox constructor()");
                PadChar = ' ';
                _Flow = RenderBox.FlowDirection.VERTICAL;
                _Align = RenderBox.TextAlign.LEFT;
                _MinWidth = 0;
                _MaxWidth = -1;
                _DesiredWidth = -1;
                _MinHeight = 0;
                _MaxHeight = -1;
                _DesiredHeight = -1;
                minHeightIsCached = false;
                minWidthIsCached = false;
            }
        }

        public IEnumerable<StringBuilder> GetLines(int maxWidth, int maxHeight)
        {
            using (new SimpleProfiler("RenderBox.GetLines(int, int)"))
            {
                //Logger.debug("NodeBox.GetRenderedLines()");
                //Logger.IncLvl();
                int height = GetActualHeight(maxHeight);
                for (int i = 0; i < height; i++)
                {
                    yield return GetLine(i, maxWidth, maxHeight);
                }
                //Logger.DecLvl();
            }

        }
        public IEnumerable<StringBuilder> GetLines()
        {
            using (new SimpleProfiler("RenderBox.GetLines()"))
            {
                //Logger.debug("NodeBox.GetRenderedLines()");
                //Logger.IncLvl();
                int height = GetActualHeight(-1);
                for (int i = 0; i < height; i++)
                {
                    yield return GetLine(i, -1, -1);
                }
                //Logger.DecLvl();
            }
        }

        protected void AlignLine(ref StringBuilder line)
        {
            using (new SimpleProfiler("RenderBox.AlignLine(ref StringBuilder)"))
            {
                AlignLine(ref line, -1);
            }
        }

        protected void AlignLine(ref StringBuilder line, int maxWidth)
        {
            using (Logger logger = new Logger("RenderBox.AlignLine(ref StringBuilder, int)", Logger.Mode.LOG))
            {
                //Logger.debug("NodeBox.AlignLine()");
                //Logger.IncLvl();
                //Logger.debug("max width is " + maxWidth);
                int actualWidth = GetActualWidth(maxWidth);
                logger.log("actualWidth: " + actualWidth, Logger.Mode.LOG);
                //Logger.debug("actual width is " + actualWidth);
                //Logger.debug("line width is " + TextUtils.GetTextWidth(line));
                //Logger.debug("line is: |" + line + "|");
                int remainingWidth = actualWidth - TextUtils.GetTextWidth(line.ToString());
                //Logger.debug("remaining width is " + remainingWidth);

                if (remainingWidth > 0) // line is not wide enough; padding necessary
                {
                    ////Logger.debug("line is so far: |" + line.ToString() + "|");
                    //Logger.debug("padding...");
                    switch (Align)
                    {
                        case TextAlign.CENTER:
                            line = TextUtils.PadText(line.ToString(), actualWidth, TextUtils.PadMode.BOTH, PadChar);
                            break;
                        case TextAlign.RIGHT:
                            line = TextUtils.PadText(line.ToString(), actualWidth, TextUtils.PadMode.LEFT, PadChar);
                            break;
                        default:
                            line = TextUtils.PadText(line.ToString(), actualWidth, TextUtils.PadMode.RIGHT, PadChar);
                            break;
                    }
                    ////Logger.debug("line is so far: |" + line.ToString() + "|");
                }
                else if (remainingWidth < 0)
                {
                    //Logger.debug("clipping");
                    line = new StringBuilder(line.ToString());

                    while (remainingWidth < 0)
                    {
                        remainingWidth += TextUtils.GetTextWidth(new string(new char[] { line[line.Length - 1] })) + 1;
                        line.Remove(line.Length - 1, 1);
                    }
                }
                else
                {
                    //Logger.debug("neither padding nor clipping...");
                }
                //Logger.debug("aligned line is: {" + line + "}");
                //Logger.DecLvl();
            }
        }

        public string Render(int maxWidth, int maxHeight)
        {
            using (Logger logger = new Logger("RenderBox.Render(" + maxWidth + ", " + maxHeight + ")", Logger.Mode.LOG))
            {
                StringBuilder result = new StringBuilder();
                int i = 0;
                foreach (StringBuilder line in GetLines(maxWidth, maxHeight))
                {
                    logger.log("rendering line " + (i++), Logger.Mode.LOG);
                    result.Append(line);
                    result.Append("\n");
                }
                if (result.Length > 0)
                    result.Remove(result.Length - 1, 1);
                return result.ToString();
            }
        }

        public void ClearCache()
        {
            minHeightIsCached = false;
            minWidthIsCached = false;
            Parent?.ClearCache();
        }

    }

}
