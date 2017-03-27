using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SEScripts.Lib;
using SEScripts.Lib.LoggerNS;

namespace SEScripts.XUI.BoxRenderer
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
                foreach (RenderBox box in Boxes)
                {
                    if (Flow == RenderBox.FlowDirection.HORIZONTAL)
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
            get
            {
                Logger.debug("NodeBoxTree.MinWidth.get");
                Logger.IncLvl();
                int minWidth = (Flow == RenderBox.FlowDirection.HORIZONTAL ? 0 : _MinWidth);
                int boxMinWidth;
                foreach (RenderBox box in Boxes)
                {
                    if (Flow == RenderBox.FlowDirection.HORIZONTAL)
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
                if (floatingMaxWidth != -1)
                    floatingMaxWidth = Math.Max(floatingMaxWidth - MinWidth, 0) - 1;
                StringBuilder nextLine;
                int boxMinWidth;
                foreach (RenderBox box in Boxes)
                {
                    boxMinWidth = box.MinWidth;
                    nextLine = box.GetLine(index, 1 + floatingMaxWidth + boxMinWidth);
                    if (floatingMaxWidth != -1)
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
}
