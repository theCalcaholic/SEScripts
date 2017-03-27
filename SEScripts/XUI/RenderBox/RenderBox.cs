using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SEScripts.Lib;
using SEScripts.Lib.LoggerNS;

namespace SEScripts.XUI
{
    public class RenderBoxTree : RenderBox
    {
        List<RenderBox> Boxes;
        private int _Height;

        public RenderBox this[int i]
        {
            get
            {
                return Boxes[i];
            }
            set
            {
                Boxes[i] = value;
            }
        }

        public int Count
        {
            get
            {
                Logger.debug("NodeBoxTree.Count.get");
                return Boxes.Count;
            }
        }

        public override int Height
        {
            get
            {
                Logger.debug("NodeBoxTree.Height.get");
                Logger.IncLvl();
                int height = 0;
                foreach(RenderBox box in Boxes)
                {
                    if(Flow == RenderBox.FlowDirection.HORIZONTAL)
                    {
                        height = Math.Max(height, box.Height);
                    }
                    else
                    {
                        height += box.Height;
                    }
                }
                Logger.debug("height = " + height.ToString());
                Logger.DecLvl();
                return height;
            }
            set
            {
                Logger.debug("NodeBoxTree.Height.set");
                Logger.IncLvl();
                Logger.debug("height = " + value.ToString());
                _Height = value;
                Logger.DecLvl();
            }
        }

        public override int MinWidth
        {
            get {
                Logger.debug("NodeBoxTree.MinWidth.get");
                Logger.IncLvl();
                int minWidth = (Flow == RenderBox.FlowDirection.HORIZONTAL ? 0 : _MinWidth);
                int boxMinWidth;
                foreach(RenderBox box in Boxes)
                {
                    if(Flow == RenderBox.FlowDirection.HORIZONTAL)
                    {
                        boxMinWidth = box.MinWidth;
                        if (boxMinWidth > 0)
                        {
                            minWidth++;
                            minWidth += boxMinWidth;
                            Logger.debug("min width + " + boxMinWidth);
                        }
                    }
                    else
                    {
                        minWidth = Math.Max(box.MinWidth, minWidth);
                    }
                }
                if (Flow == RenderBox.FlowDirection.HORIZONTAL)
                    minWidth = Math.Max(_MinWidth, minWidth - 1);
                Logger.debug("minwidth = " + minWidth);
                Logger.DecLvl();
                return Math.Max(minWidth, 0);
            }
        }

        public RenderBoxTree() : base()
        {
            Logger.debug("NodeBoxTree constructor()");
            Boxes = new List<RenderBox>();
            _Height = 0;
        }

        public override void Add(string box)
        {
            AddAt(Boxes.Count, box);
        }

        public override void AddAt(int position, string box)
        {
            Logger.debug("NodeBoxTree.AddAt(int, string)");
            Logger.IncLvl();
            AddAt(position, new RenderBoxLeaf(box));
            Logger.DecLvl();
        }

        public override void Add(StringBuilder box)
        {
            AddAt(Boxes.Count, box);
        }

        public override void AddAt(int position, StringBuilder box)
        {
            Logger.debug("NodeBoxTree.AddAt(int, StringBuilder)");
            Logger.IncLvl();
            AddAt(position, new RenderBoxLeaf(box));
            Logger.DecLvl();
        }

        public void AddAt(int position, RenderBox box)
        {
            Logger.debug("NodeBoxTree.AddAt(int, NodeBox)");
            Logger.IncLvl();
            Boxes.AddOrInsert<RenderBox>(box, position);
            Logger.DecLvl();
        }

        public void Add(RenderBox box)
        {
            Logger.debug("NodeBoxTree.Add(NodeBox)");
            Logger.IncLvl();
            AddAt(Boxes.Count, box);
            Logger.DecLvl();
        }


        public override StringBuilder GetLine(int index)
        {
            return GetLine(index, -1);
        }

        public override StringBuilder GetLine(int index, int maxWidth)
        {
            Logger.debug("NodeBoxTree.GetLine(int, int)");
            Logger.IncLvl();
            StringBuilder line = new StringBuilder();
            //bool foundLine = false;
            if (Flow == RenderBox.FlowDirection.VERTICAL)
            {
                foreach (RenderBox box in Boxes)
                {
                    if (index < box.Height)
                    {
                        line = box.GetLine(index, maxWidth);
                        Logger.debug("child box width is " + TextUtils.GetTextWidth(line));
                        //foundLine = true;
                        break;
                    }
                    else
                    {
                        index -= box.Height;
                    }
                }
            }
            else
            {
                int floatingMaxWidth = maxWidth;
                if(floatingMaxWidth != -1)
                    floatingMaxWidth = Math.Max(floatingMaxWidth - MinWidth, 0) - 1;
                StringBuilder nextLine;
                int boxMinWidth;
                foreach (RenderBox box in Boxes)
                {
                    boxMinWidth = box.MinWidth;
                    nextLine = box.GetLine(index, 1 + floatingMaxWidth + boxMinWidth);
                    if(floatingMaxWidth != -1)
                        floatingMaxWidth = Math.Max(0, floatingMaxWidth - TextUtils.GetTextWidth(nextLine) + boxMinWidth);
                    line.Append(nextLine);
                    Logger.debug("child box width is: " + TextUtils.GetTextWidth(nextLine));
                }
                //foundLine = index < Height;
            }
            AlignLine(ref line, maxWidth);
            Logger.debug("line is: {" + line + "}");
            Logger.DecLvl();
            return line;
        }

        public override void Clear()
        {
            Logger.debug("NodeBoxTree.Clear()");
            Logger.IncLvl();
            Boxes.Clear();
            Logger.DecLvl();
        }

    }


    public class RenderBoxLeaf : RenderBox
    {
        StringBuilder Content;
        private int _Height;

        public override RenderBox.FlowDirection Flow
        {
            get { return RenderBox.FlowDirection.VERTICAL; }
            set {}
        }

        public override int Height
        {
            get
            {
                Logger.debug("NodeBoxLeaf.Height.get");
                Logger.IncLvl();
                Logger.debug("height = " + _Height);
                Logger.DecLvl();
                return _Height;
            }
            set
            {
                Logger.debug("NodeBoxLeaf.Height.set");
                Logger.IncLvl();
                Logger.debug("height = " + value);
                _Height = value;
                Logger.DecLvl();
            }
        }

        public override int MinWidth
        {
            get
            {
                Logger.debug("NodeBoxLeaf.MinWidth.get");
                Logger.IncLvl();
                int contentWidth = Math.Max(TextUtils.GetTextWidth(Content), _MinWidth);
                /*if (_MinWidth >= 0 && _MinWidth < contentWidth)
                {
                    return _MinWidth;
                }
                else
                {
                    return contentWidth;
                }*/
                Logger.debug("minwidth = " + contentWidth);
                Logger.DecLvl();
                return contentWidth;
            }
            set
            {
                Logger.debug("NodeBoxLeaf.MinWidth.set()");
                Logger.IncLvl();
                Logger.debug("minwidth = " + value);
                _MinWidth = value;
                Logger.DecLvl();
            }
        }

        public RenderBoxLeaf()
        {
            Logger.debug("NodeBoxLeaf constructor()");
            _Height = 0;
            Content = new StringBuilder();
        }

        public RenderBoxLeaf(StringBuilder content) : this()
        {
            Logger.debug("NodeBoxLeaf constructor(StringBuilder)");
            Logger.IncLvl();
            Add(content);
            if (Content.Length > 0)
            {
                _Height = 1;
            }
            Logger.DecLvl();
        }

        public RenderBoxLeaf(string content) : this(new StringBuilder(content))
        {}

        public override void AddAt(int position, StringBuilder box)
        {
            Logger.debug("NodeBoxLeaf.AddAt(int, StringBuilder)");
            Logger.IncLvl();
            if (position == 0)
            {
                Content = new StringBuilder(box.ToString() + Content.ToString());
            }
            else
            {
                Content.Append(box);
            }
            Logger.DecLvl();
        }

        public override void Add(StringBuilder box)
        {
            Logger.debug("NodeBoxLeaf.Add(StringBuilder)");
            Logger.IncLvl();
            Content.Append(box.Replace("\n", ""));
            Logger.DecLvl();
        }
        public override void AddAt(int position, string box)
        {
            Logger.debug("NodeBoxLeaf.AddAt(int, string)");
            Logger.IncLvl();
            AddAt(position, new StringBuilder(box));
            Logger.DecLvl();
        }

        public override void Add(string box)
        {
            Logger.debug("NodeBoxLeaf.Add(string)");
            Logger.IncLvl();
            Add(new StringBuilder(box));
            Logger.DecLvl();
        }

        public override StringBuilder GetLine(int index)
        {
            return GetLine(index, -1);
        }

        public override StringBuilder GetLine(int index, int maxWidth)
        {
            Logger.debug("NodeBoxLeaf.GetLine()");
            Logger.IncLvl();
            StringBuilder line;
            if (index == 0)
            {
                line = new StringBuilder(Content.ToString());
            }
            else
            {
                line = new StringBuilder();
            }
            AlignLine(ref line, maxWidth);
            Logger.debug("line is {" + line + "}");
            Logger.DecLvl();
            return line;
        }

        public override void Clear()
        {
            Content = new StringBuilder();
        }


    }

    public abstract class RenderBox
    {
        public String PadString;
        public enum TextAlign { LEFT, RIGHT, CENTER }
        public enum FlowDirection { HORIZONTAL, VERTICAL }
        public abstract int Height { get; set; }

        public abstract void Add(string box);
        public abstract void Add(StringBuilder box);
        public abstract void AddAt(int position, string box);
        public abstract void AddAt(int position, StringBuilder box);
        public abstract StringBuilder GetLine(int index);
        public abstract StringBuilder GetLine(int index, int maxWidth);
        public abstract void Clear();
        private RenderBox.FlowDirection _Flow;
        private RenderBox.TextAlign _Align;
        protected int _MinWidth;
        protected int _MaxWidth;
        protected int _DesiredWidth;
        protected int _ForcedWidth;
        public int ForcedWidth
        {
            get
            {
                Logger.debug("NodeBox.ForcedWidth.get()");
                Logger.IncLvl();
                Logger.debug("forcedwidth = " + _ForcedWidth);
                Logger.DecLvl();
                return _ForcedWidth;
            }
            set
            {
                Logger.debug("NodeBox.ForcedWidth.get()");
                Logger.IncLvl();
                Logger.debug("forcedwidth = " + value);
                _ForcedWidth = value;
                Logger.DecLvl();
            }
        }

        public int GetActualWidth(int maxWidth)
        {
            Logger.debug("NodeBox.GetActualWidth(int)");
            Logger.IncLvl();
            if(MaxWidth != -1)
                maxWidth = (maxWidth == -1 ? MaxWidth : Math.Min(MaxWidth, maxWidth));
            if (ForcedWidth == -1)
            {
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
                    Logger.DecLvl();
                    return maxWidth;// Math.Max(MinWidth, maxWidth);
                }
            }
            else
            {
                Logger.debug("actual width equals forced width");
                Logger.DecLvl();
                return ForcedWidth;
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

        public RenderBox()
        {
            Logger.debug("NodeBox constructor()");
            PadString = " ";
            _Flow = RenderBox.FlowDirection.VERTICAL;
            _Align = RenderBox.TextAlign.LEFT;
            _MinWidth = 0;
            _MaxWidth = -1;
            _DesiredWidth = -1;
            _ForcedWidth = -1;
        }

        public IEnumerable<StringBuilder> GetLines(int maxWidth)
        {
            Logger.debug("NodeBox.GetRenderedLines()");
            Logger.IncLvl();
            for (int i = 0; i < Height; i++)
            {
                yield return GetLine(i, maxWidth);
            }
            Logger.DecLvl();

        }
        public IEnumerable<StringBuilder> GetLines()
        {            
            Logger.debug("NodeBox.GetLines()");
            Logger.IncLvl();
            for (int i = 0; i < Height; i++)
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
        
        public string Render(int maxWidth)
        {
            Logger.debug("NodeBox.Render()");
            Logger.IncLvl();
            StringBuilder result = new StringBuilder();
            foreach(StringBuilder line in GetLines(maxWidth))
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
