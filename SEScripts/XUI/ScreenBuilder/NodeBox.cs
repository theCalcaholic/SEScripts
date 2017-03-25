using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SEScripts.Lib;
using SEScripts.Lib.LoggerNS;

namespace SEScripts.XUI
{
    public class NodeBoxTree : NodeBox
    {
        List<NodeBox> Boxes;
        private int _Height;

        public NodeBox this[int i]
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
                foreach(NodeBox box in Boxes)
                {
                    if(Flow == NodeBox.FlowDirection.HORIZONTAL)
                    {
                        height = Math.Max(height, box.Height);
                    }
                    else
                    {
                        height += box.Height;
                    }
                }
                Logger.DecLvl();
                return height;
            }
            set
            {
                Logger.debug("NodeBoxTree.Height.get");
                _Height = value;
            }
        }

        public override int MinWidth
        {
            get {
                Logger.debug("NodeBoxTree.MinWidth.get");
                Logger.IncLvl();
                int minWidth = (Flow == NodeBox.FlowDirection.HORIZONTAL ? 0 : _MinWidth);
                int boxMinWidth;
                foreach(NodeBox box in Boxes)
                {
                    if(Flow == NodeBox.FlowDirection.HORIZONTAL)
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
                if (Flow == NodeBox.FlowDirection.HORIZONTAL)
                    minWidth = Math.Max(_MinWidth, minWidth - 1);
                Logger.debug("Got MinWidth (" + minWidth + ")...");
                Logger.DecLvl();
                return Math.Max(minWidth, 0);
            }
        }

        public NodeBoxTree() : base()
        {
            Logger.debug("NodeBoxTree constructor()");
            Boxes = new List<NodeBox>();
            _Height = 0;
        }

        public override void Add(string box)
        {
            AddAt(Boxes.Count, box);
        }

        public override void AddAt(int position, string box)
        {
            Logger.debug("NodeBoxTree.Add(string)");
            Logger.IncLvl();
            AddAt(position, new NodeBoxLeaf(box));
            Logger.DecLvl();
        }

        public override void Add(StringBuilder box)
        {
            AddAt(Boxes.Count, box);
        }

        public override void AddAt(int position, StringBuilder box)
        {
            Logger.debug("NodeBoxTree.Add(StringBuilder)");
            Logger.IncLvl();
            AddAt(position, new NodeBoxLeaf(box));
            Logger.DecLvl();
        }

        public void AddAt(int position, NodeBox box)
        {
            Logger.debug("NodeBoxTree.Add(NodeBox)");
            Logger.IncLvl();
            Boxes.AddOrInsert<NodeBox>(box, position);
            Logger.DecLvl();
        }

        public void Add(NodeBox box)
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
            if (Flow == NodeBox.FlowDirection.VERTICAL)
            {
                foreach (NodeBox box in Boxes)
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
                StringBuilder nextLine;
                foreach (NodeBox box in Boxes)
                {
                    nextLine = box.GetLine(index, maxWidth);
                    if(maxWidth != -1)
                        maxWidth = Math.Max(0, maxWidth - TextUtils.GetTextWidth(nextLine));
                    line.AppendStringBuilder(nextLine);
                    Logger.debug("child box width is: " + TextUtils.GetTextWidth(nextLine));
                }
                //foundLine = index < Height;
            }
            AlignLine(ref line, maxWidth);
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


    public class NodeBoxLeaf : NodeBox
    {
        StringBuilder Content;
        private int _Height;

        public override NodeBox.FlowDirection Flow
        {
            get { return NodeBox.FlowDirection.VERTICAL; }
            set {}
        }

        public override int Height
        {
            get
            {
                Logger.debug("NodeBoxLeaf.Height.get");
                if ( Content.Length > 0)
                {
                    return 1;
                }
                else
                {
                    return 0;
                }
            }
            set
            {
                Logger.debug("NodeBoxLeaf.Height.set");
                _Height = value;
            }
        }

        public override int MinWidth
        {
            get
            {
                Logger.debug("NodeBoxLeaf.MinWidth.get");
                int contentWidth = Math.Max(TextUtils.GetTextWidth(Content), _MinWidth);
                /*if (_MinWidth >= 0 && _MinWidth < contentWidth)
                {
                    return _MinWidth;
                }
                else
                {
                    return contentWidth;
                }*/
                return contentWidth;
            }
            set
            {
                Logger.debug("NodeBoxLeaf.MinWidth.set()");
                _MinWidth = value;
            }
        }

        public NodeBoxLeaf()
        {
            Logger.debug("NodeBoxLeaf constructor()");
            _Height = 0;
            Content = new StringBuilder();
        }

        public NodeBoxLeaf(StringBuilder content) : this()
        {
            Logger.debug("NodeBoxLeaf constructor(StringBuilder)");
            Logger.IncLvl();
            Add(content);
            Logger.DecLvl();
        }

        public NodeBoxLeaf(string content) : this()
        {
            Logger.debug("NodeBoxLeaf constructor(string)");
            Logger.IncLvl();
            Add(content);
            if(Content.Length > 0)
            {
                _Height = 1;
            }
            Logger.DecLvl();
        }

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
                Content.AppendStringBuilder(box);
            }
            Logger.DecLvl();
        }

        public override void Add(StringBuilder box)
        {
            Logger.debug("NodeBoxLeaf.Add(StringBuilder)");
            Logger.IncLvl();
            Content.AppendStringBuilder(box.Replace("\n", ""));
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
            if (index == 0)
            {
                StringBuilder line = new StringBuilder(Content.ToString());
                AlignLine(ref line, maxWidth);
                Logger.DecLvl();
                return line;
            }
            Logger.DecLvl();
            return new StringBuilder();
        }

        public override void Clear()
        {
            Content = new StringBuilder();
        }


    }

    public abstract class NodeBox
    {
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
        private NodeBox.FlowDirection _Flow;
        private NodeBox.TextAlign _Align;
        protected int _MinWidth;
        protected int _MaxWidth;
        protected int _DesiredWidth;
        protected int _ForcedWidth;
        public int Width
        {
            get
            {
                Logger.debug("NodeBox.Width.get()");
                return _ForcedWidth;
                /*if(MaxWidth > -1)
                {
                    return Math.Max(0, Math.Min(MinWidth, MaxWidth));
                }
                else
                {
                    return Math.Max(0, MinWidth);
                }*/
            }
            set
            {
                Logger.debug("NodeBox.Width.get()");
                _ForcedWidth = value;
            }
        }

        public NodeBox.TextAlign Align
        {
            get { return _Align; }
            set { _Align = value; }
        }

        public virtual NodeBox.FlowDirection Flow
        {
            get { return _Flow; }
            set { _Flow = value; }
        }

        public virtual int MinWidth
        {
            get
            {
                Logger.debug("NodeBox.MinWidth.get()");
                return _MinWidth;
            }
            set
            {
                Logger.debug("NodeBox.MinWidth.set()");
                _MinWidth = Math.Max(0, value);
            }
        }

        public int DesiredWidth
        {
            get
            {
                Logger.debug("NodeBox.DesiredWidth.get()");
                return _DesiredWidth;
            }
            set
            {
                Logger.debug("NodeBox.DesiredWidth.get()");
                _DesiredWidth = value;
            }
        }

        public int MaxWidth
        {
            get
            {
                Logger.debug("NodeBox.MaxWidth.get()");
                return _MaxWidth;
            }
            set
            {
                Logger.debug("NodeBox.MaxWidth.get()");
                _MaxWidth = value;
            }
        }

        public NodeBox()
        {
            Logger.debug("NodeBox constructor()");
            _Flow = NodeBox.FlowDirection.VERTICAL;
            _Align = NodeBox.TextAlign.LEFT;
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
            int actualWidth;
            if (Width == -1)
            {
                if(maxWidth == -1)
                {
                    actualWidth = MinWidth;
                    Logger.debug("actual width equals min width");
                }
                else
                {
                    actualWidth = Math.Max(MinWidth, maxWidth);
                    Logger.debug("actual width equals maximum of min and max widths");
                }
            }
            else
            {
                actualWidth = Width;
                Logger.debug("actual width equals forced width");
            }
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
                        line = TextUtils.PadText(line, actualWidth, TextUtils.PadMode.BOTH);
                        break;
                    case TextAlign.RIGHT:
                        line = TextUtils.PadText(line, actualWidth, TextUtils.PadMode.LEFT);
                        break;
                    default:
                        line = TextUtils.PadText(line, actualWidth, TextUtils.PadMode.RIGHT);
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
                    line.TrimEnd(1);
                }
            }
            else
            {
                Logger.debug("neither padding nor clipping...");
            }
            Logger.DecLvl();
        }


        public virtual StringBuilder RenderLine(int index, int maxWidth)
        {
            Logger.debug("NodeBox.RenderLine()");
            Logger.IncLvl();
            StringBuilder result = new StringBuilder();
            int width = (maxWidth >= 0) ? Math.Min(maxWidth, MinWidth) : MinWidth;
            StringBuilder line = GetLine(index, maxWidth);
            int diff = width - TextUtils.GetTextWidth(line);
            Logger.debug(diff.ToString());
            if (diff <= 0)
            {
                Logger.debug("line to wide...");
                Logger.debug("line was: " + line);
                Logger.debug("Width was: " + width);
                Logger.DecLvl();
                return line;
            }
            else if (Align == NodeBox.TextAlign.CENTER)
            {
                Logger.debug("Aligning center...");
                StringBuilder padding = TextUtils.CreateStringOfLength(" ", diff / 2);
                result.AppendStringBuilder(padding);
                result.AppendStringBuilder(line);
                result.AppendStringBuilder(padding);
                Logger.DecLvl();
                return result;
            }
            else
            {
                StringBuilder padding = TextUtils.CreateStringOfLength(" ", diff);
                if (Align == NodeBox.TextAlign.LEFT)
                {
                    Logger.debug("Aligning left...");
                    result.AppendStringBuilder(line);
                    result.AppendStringBuilder(padding);
                    Logger.DecLvl();
                    return result;
                }
                else if (Align == NodeBox.TextAlign.RIGHT)
                {
                    Logger.debug("Aligning right...");
                    result.AppendStringBuilder(padding);
                    result.AppendStringBuilder(line);
                    Logger.DecLvl();
                    return result;
                }
            }
            Logger.DecLvl();
            return null;

        }

        public string Render(int maxWidth)
        {
            Logger.debug("NodeBox.Render()");
            Logger.IncLvl();
            StringBuilder result = new StringBuilder();
            foreach(StringBuilder line in GetLines(maxWidth))
            {
                result.AppendStringBuilder(line);
                result.Append("\n");
            }
            result.Remove(result.Length - 1, 1);
            Logger.DecLvl();
            return result.ToString();
        }
    }


}
