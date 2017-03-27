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

        public override int MinHeight
        {
            get
            {
                Logger.debug("NodeBoxTree.MinHeight.get");
                Logger.IncLvl();
                int minHeight = (Flow != FlowDirection.HORIZONTAL ? 0 : _MinHeight);
                int boxMinHeight;
                foreach (RenderBox box in Boxes)
                {
                    if (Flow == RenderBox.FlowDirection.HORIZONTAL)
                    {
                        minHeight = Math.Max(minHeight, box.MinHeight);
                    }
                    else
                    {
                        boxMinHeight = box.MinHeight;
                        if (boxMinHeight > 0)
                        {
                            minHeight++;
                            minHeight += boxMinHeight;
                            Logger.debug("min height + " + boxMinHeight);
                        }
                    }
                }
                if (Flow != RenderBox.FlowDirection.HORIZONTAL)
                    minHeight = Math.Max(_MinHeight, minHeight - 1);
                Logger.debug("minheight = " + minHeight.ToString());
                Logger.DecLvl();
                return Math.Max(0, minHeight);
            }
            set
            {
                Logger.debug("NodeBoxTree.MinHeight.set");
                Logger.IncLvl();
                Logger.debug("minheight = " + value.ToString());
                _MinHeight = value;
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
            return GetLine(index, -1, -1);
        }

        public override StringBuilder GetLine(int index, int maxWidth, int maxHeight)
        {
            Logger.debug("NodeBoxTree.GetLine(int, int)");
            Logger.IncLvl();
            StringBuilder line = new StringBuilder();
            //bool foundLine = false;
            if (Flow == RenderBox.FlowDirection.VERTICAL)
            {
                int boxHeight;
                foreach (RenderBox box in Boxes)
                {
                    boxHeight = box.GetActualHeight(maxHeight);
                    if (index < boxHeight)
                    {
                        line = box.GetLine(index, maxWidth, maxHeight);
                        Logger.debug("child box width is " + TextUtils.GetTextWidth(line));
                        //foundLine = true;
                        break;
                    }
                    else
                    {
                        index -= boxHeight;
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
                    nextLine = box.GetLine(index, 1 + floatingMaxWidth + boxMinWidth, maxHeight);
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
