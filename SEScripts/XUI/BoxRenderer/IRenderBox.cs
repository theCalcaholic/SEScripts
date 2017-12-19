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
    public abstract class IRenderBox
    {
        protected bool minHeightIsCached;
        protected bool minWidthIsCached;
        protected bool desiredHeightIsCached;
        protected bool desiredWidthIsCached;
        protected int minHeightCache;
        protected int minWidthCache;
        protected int desiredHeightCache;
        protected int desiredWidthCache;
        public bool DEBUG = false;
        public char PadChar;
        public InitializationState InitState;
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
        public abstract void Initialize(int maxWidth, int maxHeight);
        public abstract void CalculateDimensions(int maxWidth, int maxHeight);
        private IRenderBox.FlowDirection _Flow;
        private IRenderBox.TextAlign _Align;
        public int _MinWidth;
        public int _MaxWidth;
        public int _DesiredWidth;
        public int _MinHeight;
        public int _MaxHeight;
        public int _DesiredHeight;
        public IRenderBox Parent;
        public string type;
        private bool RenderingInProcess;
        public string MinWidthDef;
        public string MaxWidthDef;
        public string MinHeightDef;
        public string MaxHeightDef;
        public string DesiredWidthDef;
        public string DesiredHeightDef;

        public virtual int GetActualWidth(int maxWidth)
        {
            using (Logger logger = new Logger("RenderBox.GetActualWidth(int)", Logger.Mode.LOG))
            {
                logger.log("Type: " + type, Logger.Mode.LOG);
                if (this as RenderBoxLeaf != null)
                    logger.log("content: |" + (this as RenderBoxLeaf).Content + "|", Logger.Mode.LOG);
                logger.log("implicit max width: " + maxWidth, Logger.Mode.LOG);
                logger.log("explicit max width: " + MaxWidth, Logger.Mode.LOG);
                logger.log("min width: " + MinWidth, Logger.Mode.LOG);
                logger.log("desired width: " + DesiredWidth, Logger.Mode.LOG);
                maxWidth = Math.Min(MaxWidth, maxWidth);
                
                int desired;
                desired = Math.Max(DesiredWidth, MinWidth);
                desired = Math.Min(desired, maxWidth);
                return desired;
            }

        }

        public int GetIndependentWidth(int maxWidth)
        {
            using (Logger logger = new Logger("RenderBoxTree.GetIndependentWidth(int)", Logger.Mode.LOG))
            {
                logger.log("Type: " + type, Logger.Mode.LOG);
                logger.log("implicit max width: " + maxWidth, Logger.Mode.LOG);
                logger.log("explicit max width: " + MaxWidth, Logger.Mode.LOG);
                logger.log("min width: " + _MinWidth, Logger.Mode.LOG);
                logger.log("desired width: " + DesiredWidth, Logger.Mode.LOG);
                maxWidth = Math.Min(MaxWidth, maxWidth);

                int desired;
                desired = Math.Max(DesiredWidth, _MinWidth);
                desired = Math.Min(desired, maxWidth);
                return desired;
            }

        }
        public int GetActualHeight(int maxHeight)
        {
            using (Logger logger = new Logger("RenderBox.GetActualHeight(int)", Logger.Mode.LOG))
            {
                logger.log("Type: " + type, Logger.Mode.LOG);
                logger.log("implicit max height: " + maxHeight, Logger.Mode.LOG);
                logger.log("explicit max height: " + MaxHeight, Logger.Mode.LOG);
                maxHeight = Math.Min(MaxHeight, maxHeight);
                
                int desired = DesiredHeight == -1 ? MinHeight : Math.Max(MinHeight, DesiredHeight);
                //Logger.DecLvl();
                logger.log("maxheight: " + maxHeight, Logger.Mode.LOG);
                logger.log("minheight: " + MinHeight, Logger.Mode.LOG);
                logger.log("Desired Height: " + DesiredHeight, Logger.Mode.LOG);
                logger.log("actual height: " + Math.Min(desired, maxHeight) + " (min( " + desired + ", " + maxHeight + ")", Logger.Mode.LOG);
                return Math.Max(0, Math.Min(desired, maxHeight));
            }

        }

        public IRenderBox.TextAlign Align
        {
            get { return _Align; }
            set
            {
                _Align = value;
            }
        }

        public virtual IRenderBox.FlowDirection Flow
        {
            get { return _Flow; }
            set
            {
                _Flow = value;
                ClearCache();
            }
        }

        public virtual int MinWidth
        {
            get
            {
                /*using (new SimpleProfiler("RenderBox.MinWidth.get"))
                //{
                    //Logger.debug("NodeBox.MinWidth.get()");
                    //Logger.IncLvl();
                    //Logger.debug("minwidth = " + _MinWidth);
                    //Logger.DecLvl();*/
                return _MinWidth;
                //}
            }
            set
            {
                //using (new SimpleProfiler("RenderBox.MinWidth.get"))
                //{
                //Logger.debug("NodeBox.MinWidth.set()");
                //Logger.IncLvl();
                //Logger.debug("minwidth = " + value);
                _MinWidth = Math.Max(0, value);
                ClearCache();
                //Logger.DecLvl();
                //}
            }
        }

        public virtual int DesiredWidth
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

        public virtual int MaxWidth
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
                if (value < 0)
                    _MaxWidth = int.MaxValue;
                else
                    _MaxWidth = value;
                //Logger.DecLvl();
            }
        }

        public virtual int MinHeight
        {
            get
            {
                //using (new SimpleProfiler("RenderBox.MinHeight.get"))
                //{
                //Logger.debug("NodeBox.MinHeight.get()");
                //Logger.IncLvl();
                //Logger.debug("minheight = " + _MinHeight);
                //Logger.DecLvl();
                return _MinHeight;
                //}
            }
            set
            {
                //using (new SimpleProfiler("RenderBox.MinHeight.set"))
                //{
                //Logger.debug("NodeBox.MinHeight.set()");
                //Logger.IncLvl();
                //Logger.debug("minheight = " + value);
                _MinHeight = Math.Max(0, value);
                ClearCache();
                //Logger.DecLvl();
                //}
            }
        }

        public virtual int DesiredHeight
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
                return _MaxHeight;
            }
            set
            {
                if (value < 0)
                    _MaxHeight = int.MaxValue;
                else
                    _MaxHeight = value;
                ClearCache();
            }
        }

        public IRenderBox()
        {
            PadChar = ' ';
            _Flow = IRenderBox.FlowDirection.VERTICAL;
            _Align = IRenderBox.TextAlign.LEFT;
            _MinWidth = 0;
            _MaxWidth =  int.MaxValue;
            _DesiredWidth = -1;
            _MinHeight = 0;
            _MaxHeight = int.MaxValue;
            _DesiredHeight = -1;
            minHeightIsCached = false;
            minWidthIsCached = false;
            desiredHeightIsCached = false;
            desiredWidthIsCached = false;
            InitState = new InitializationState();
        }

        public bool IsRenderingInProgress()
        {
            return RenderingInProcess || (Parent?.IsRenderingInProgress() ?? false);
        }

        public virtual IEnumerable<StringBuilder> GetLines(int maxWidth, int maxHeight)
        {
            int height = GetActualHeight(maxHeight);
            for (int i = 0; i < height; i++)
            {
                yield return GetLine(i, maxWidth, maxHeight);
            }
        }
        public IEnumerable<StringBuilder> GetLines()
        {
            int height = GetActualHeight(int.MaxValue);
            for (int i = 0; i < height; i++)
            {
                yield return GetLine(i, int.MaxValue, int.MaxValue);
            }
        }

        protected void AlignLine(ref StringBuilder line)
        {
            AlignLine(ref line, int.MaxValue);
        }

        protected void AlignLine(ref StringBuilder line, int maxWidth)
        {
            AlignLine(ref line, maxWidth, Align, PadChar);
        }

        protected void AlignLine(ref StringBuilder line, IRenderBox.TextAlign Alignment)
        {
            AlignLine(ref line, int.MaxValue, Alignment, PadChar);
        }

        protected void AlignLine(ref StringBuilder line, int maxWidth, IRenderBox.TextAlign Alignment, char padChar)
        {
            using (Logger logger = new Logger("RenderBox.AlignLine(ref StringBuilder, int)", Logger.Mode.LOG))
            {
            logger.log("Type: " + type);
            logger.log("pad char is: " + padChar);
                logger.log("this.PadChar is: " + PadChar);
            logger.log("max width is " + maxWidth, Logger.Mode.LOG);
            int actualWidth = GetActualWidth(maxWidth);
            logger.log("actualWidth: " + actualWidth, Logger.Mode.LOG);
            logger.log("line is: |" + line + "|", Logger.Mode.LOG);
                logger.log("line width: " + TextUtils.GetTextWidth(line.ToString()));
            int remainingWidth = actualWidth - TextUtils.GetTextWidth(line.ToString());
            logger.log("remaining width is " + remainingWidth, Logger.Mode.LOG);
            logger.log("Aligning " + _Align.ToString() + "...");

            if (remainingWidth > 0) // line is not wide enough; padding necessary
            {
                logger.log("padding...", Logger.Mode.LOG);
                switch (Alignment)
                {
                    case TextAlign.CENTER:
                        line = TextUtils.PadText(line.ToString(), actualWidth, TextUtils.PadMode.BOTH, padChar);
                        break;
                    case TextAlign.RIGHT:
                        line = TextUtils.PadText(line.ToString(), actualWidth, TextUtils.PadMode.LEFT, padChar);
                        break;
                    default:
                        line = TextUtils.PadText(line.ToString(), actualWidth, TextUtils.PadMode.RIGHT, padChar);
                        break;
                }
            }
            else if (remainingWidth < 0)
            {
                logger.log("clipping", Logger.Mode.LOG);
                line = new StringBuilder(line.ToString());

                while (remainingWidth < 0)
                {
                    remainingWidth += TextUtils.GetTextWidth(new string(new char[] { line[line.Length - 1] })) + 1;
                    line.Remove(line.Length - 1, 1);
                }
            }
            else
            {
                logger.log("neither padding nor clipping...", Logger.Mode.LOG);
            }
            logger.log("aligned line is: {" + line + "}", Logger.Mode.LOG);
            }
        }

        public string Render(int maxWidth, int maxHeight)
        {
            Initialize(maxWidth, maxHeight);
            StringBuilder result = new StringBuilder();
            foreach (StringBuilder line in GetLines(maxWidth, maxHeight))
            {
                result.Append(line);
                result.Append("\n");
            }
            if (result.Length > 0)
                result.Remove(result.Length - 1, 1);
            return result.ToString();
        }

        public void ClearCache()
        {
            minHeightIsCached = false;
            minWidthIsCached = false;
            if (Parent != null)
                Parent?.ClearCache();
        }

        public static int? ResolveSize(string widthString, int max)
        {
            if (widthString == null)
                return null;

            widthString = widthString?.Trim();
            if (widthString[widthString.Length - 1] == '%' && Single.TryParse(widthString.Substring(0, widthString.Length - 1), out float fWidth))
            {
                if (max < 0 || max == int.MaxValue)
                    return null;
                return (int)(fWidth / 100f * Math.Max(0, max));
            }
            else
            {
                int iWidth;
                if (int.TryParse(widthString, out iWidth))
                    return iWidth;
                return -1;
            }
        }
    }
    

}
