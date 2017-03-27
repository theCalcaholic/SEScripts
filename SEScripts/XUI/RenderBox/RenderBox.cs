using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SEScripts.Lib;
using SEScripts.Lib.LoggerNS;

namespace SEScripts.XUI.BoxRenderer
{
    public abstract class RenderBox
    {
        public String PadString;
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

        public int GetActualWidth(int maxWidth)
        {
            Logger.debug("NodeBox.GetActualWidth(int)");
            Logger.IncLvl();
            if(MaxWidth != -1)
                maxWidth = (maxWidth == -1 ? MaxWidth : Math.Min(MaxWidth, maxWidth));
            if (maxWidth == -1)
            {
                Logger.debug("actual width equals min width");
                Logger.DecLvl();
                return MinWidth;
            }
            else
            {
                int desired;
                if (DesiredWidth == -1)
                {
                    Logger.debug("actual width equals max width");
                    desired = maxWidth;
                }
                else
                {
                    Logger.debug("actual width equals desired width, but if desired<min -> width=min and if desired>max -> width = max");
                    desired = Math.Max(MinWidth, DesiredWidth);
                }
                Logger.DecLvl();
                return (maxWidth == -1 ? desired : Math.Min(desired, maxWidth));
            }

        }
        public int GetActualHeight(int maxHeight)
        {
            Logger.debug("NodeBox.GetActualHeight(int)");
            Logger.IncLvl();
            if (MaxHeight != -1)
                maxHeight = (maxHeight == -1 ? MaxHeight : Math.Min(MaxHeight, maxHeight));
            if (maxHeight == -1)
            {
                Logger.debug("actual width equals min height");
                Logger.DecLvl();
                return MinHeight;
            }
            else
            {
                int desired;
                if (DesiredHeight == -1)
                {
                    Logger.debug("actual width equals max height");
                    desired = maxHeight;
                }
                else
                {
                    Logger.debug("actual height equals desired height, but if desired<min -> height=min and if desired>max -> height = max");
                    desired = Math.Max(MinHeight, DesiredHeight);
                }
                Logger.DecLvl();
                return (maxHeight == -1 ? desired : Math.Min(desired, maxHeight));
            }

        }

        public RenderBox.TextAlign Align
        {
            get { return _Align; }
            set { _Align = value; }
        }

        public virtual RenderBox.FlowDirection Flow
        {
            get { return _Flow; }
            set { _Flow = value; }
        }

        public virtual int MinWidth
        {
            get
            {
                Logger.debug("NodeBox.MinWidth.get()");
                Logger.IncLvl();
                Logger.debug("minwidth = " + _MinWidth);
                Logger.DecLvl();
                return _MinWidth;
            }
            set
            {
                Logger.debug("NodeBox.MinWidth.set()");
                Logger.IncLvl();
                Logger.debug("minwidth = " + value);
                _MinWidth = Math.Max(0, value);
                Logger.DecLvl();
            }
        }

        public int DesiredWidth
        {
            get
            {
                Logger.debug("NodeBox.DesiredWidth.get()");
                Logger.IncLvl();
                Logger.debug("desiredwidth = " + _DesiredWidth);
                Logger.DecLvl();
                return _DesiredWidth;
            }
            set
            {
                Logger.debug("NodeBox.DesiredWidth.set()");
                Logger.IncLvl();
                Logger.debug("desiredwidth = " + value);
                _DesiredWidth = value;
                Logger.DecLvl();
            }
        }

        public int MaxWidth
        {
            get
            {
                Logger.debug("NodeBox.MaxWidth.get()");
                Logger.IncLvl();
                Logger.debug("maxwidth = " + _MaxWidth);
                Logger.DecLvl();
                return _MaxWidth;
            }
            set
            {
                Logger.debug("NodeBox.MaxWidth.set()");
                Logger.IncLvl();
                Logger.debug("maxwidth = " + value);
                _MaxWidth = value;
                Logger.DecLvl();
            }
        }

        public virtual int MinHeight
        {
            get
            {
                Logger.debug("NodeBox.MinHeight.get()");
                Logger.IncLvl();
                Logger.debug("minheight = " + _MinHeight);
                Logger.DecLvl();
                return _MinHeight;
            }
            set
            {
                Logger.debug("NodeBox.MinHeight.set()");
                Logger.IncLvl();
                Logger.debug("minheight = " + value);
                _MinHeight = Math.Max(0, value);
                Logger.DecLvl();
            }
        }

        public int DesiredHeight
        {
            get
            {
                Logger.debug("NodeBox.DesiredHeight.get()");
                Logger.IncLvl();
                Logger.debug("desiredheight = " + _DesiredHeight);
                Logger.DecLvl();
                return _DesiredHeight;
            }
            set
            {
                Logger.debug("NodeBox.DesiredHeight.set()");
                Logger.IncLvl();
                Logger.debug("desiredheight = " + value);
                _DesiredHeight = value;
                Logger.DecLvl();
            }
        }

        public int MaxHeight
        {
            get
            {
                Logger.debug("NodeBox.MaxHeight.get()");
                Logger.IncLvl();
                Logger.debug("maxheight = " + _MaxHeight);
                Logger.DecLvl();
                return _MaxHeight;
            }
            set
            {
                Logger.debug("NodeBox.MaxHeight.set()");
                Logger.IncLvl();
                Logger.debug("maxheight = " + value);
                _MaxHeight = value;
                Logger.DecLvl();
            }
        }

        public RenderBox()
        {
            Logger.debug("NodeBox constructor()");
            PadString = " ";
            _Flow = RenderBox.FlowDirection.VERTICAL;
            _Align = RenderBox.TextAlign.LEFT;
            _MinWidth = 0;
            _MaxWidth = -1;
            _DesiredWidth = -1;
            _MinHeight = 0;
            _MaxHeight = -1;
            _DesiredHeight = -1;
        }

        public IEnumerable<StringBuilder> GetLines(int maxWidth, int maxHeight)
        {
            Logger.debug("NodeBox.GetRenderedLines()");
            Logger.IncLvl();
            int height = GetActualHeight(maxHeight);
            for (int i = 0; i < height; i++)
            {
                yield return GetLine(i, maxWidth, maxHeight);
            }
            Logger.DecLvl();

        }
        public IEnumerable<StringBuilder> GetLines()
        {            
            Logger.debug("NodeBox.GetLines()");
            Logger.IncLvl();
            int height = GetActualHeight(0);
            for (int i = 0; i < height; i++)
            {
                yield return GetLine(i);
            }
            Logger.DecLvl();

        }
        
        protected void AlignLine(ref StringBuilder line)
        {
            AlignLine(ref line, -1);
        }

        protected void AlignLine(ref StringBuilder line, int maxWidth)
        {
            Logger.debug("NodeBox.AlignLine()");
            Logger.IncLvl();
            Logger.debug("max width is " + maxWidth);
            int actualWidth = GetActualWidth(maxWidth);
            Logger.debug("actual width is " + actualWidth);
            Logger.debug("line width is " + TextUtils.GetTextWidth(line));
            Logger.debug("line is: |" + line + "|");
            int remainingWidth = actualWidth - TextUtils.GetTextWidth(line);
            Logger.debug("remaining width is " + remainingWidth);

            if (remainingWidth > 0) // line is not wide enough; padding necessary
            {
                //Logger.debug("line is so far: |" + line.ToString() + "|");
                Logger.debug("padding...");
                switch (Align)
                {
                    case TextAlign.CENTER:
                        line = TextUtils.PadText(line, actualWidth, TextUtils.PadMode.BOTH, PadString);
                        break;
                    case TextAlign.RIGHT:
                        line = TextUtils.PadText(line, actualWidth, TextUtils.PadMode.LEFT, PadString);
                        break;
                    default:
                        line = TextUtils.PadText(line, actualWidth, TextUtils.PadMode.RIGHT, PadString);
                        break;
                }
                //Logger.debug("line is so far: |" + line.ToString() + "|");
            }
            else if (remainingWidth < 0)
            {
                Logger.debug("clipping");
                line = new StringBuilder(line.ToString());

                while (remainingWidth < 0)
                {
                    remainingWidth += TextUtils.GetLetterWidth(line[line.Length - 1]) + 1;
                    line.Remove(line.Length - 1, 1);
                }
            }
            else
            {
                Logger.debug("neither padding nor clipping...");
            }
            Logger.debug("aligned line is: {" + line + "}");
            Logger.DecLvl();
        }
        
        public string Render(int maxWidth, int maxHeight)
        {
            Logger.debug("NodeBox.Render()");
            Logger.IncLvl();
            StringBuilder result = new StringBuilder();
            foreach(StringBuilder line in GetLines(maxWidth, maxHeight))
            {
                result.Append(line);
                result.Append("\n");
            }
            if(result.Length > 0)
                result.Remove(result.Length - 1, 1);
            Logger.DecLvl();
            return result.ToString();
        }
    }


}
