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
                using (new SimpleProfiler("RenderBoxTree.Count.get"))
                {
                    //Logger.debug("NodeBoxTree.Count.get");
                    return Boxes.Count;
                }
            }
        }

        public override int MinHeight
        {
            get
            {
                using (new SimpleProfiler("RenderBoxTree.MinHeight.get"))
                {
                    //Logger.debug("NodeBoxTree.MinHeight.get");
                    //Logger.IncLvl();
                    if (minHeightIsCached)
                        return minHeightCache;
                    //int minHeight = (Flow != FlowDirection.HORIZONTAL ? 0 : _MinHeight);
                    int minHeight = 0;
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
                                minHeight += boxMinHeight;
                                //Logger.debug("min height + " + boxMinHeight);
                            }
                        }
                    }
                    //Logger.debug("minheight = " + minHeight.ToString());
                    minHeightCache = Math.Max(0, Math.Max(_MinHeight, minHeight));
                    minHeightIsCached = true;
                    //Logger.DecLvl();
                    return minHeightCache;
                }
            }
        }

        public override int MinWidth
        {
            get
            {
                using (new SimpleProfiler("RenderBoxTree.MinWidth.get"))
                {
                    //Logger.debug("NodeBoxTree.MinWidth.get");
                    //Logger.IncLvl();
                    if (minWidthIsCached)
                        return minWidthCache;
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
                                //Logger.debug("min width + " + boxMinWidth);
                            }
                        }
                        else
                        {
                            minWidth = Math.Max(box.MinWidth, minWidth);
                        }
                    }
                    if (Flow == RenderBox.FlowDirection.HORIZONTAL)
                        minWidth = Math.Max(_MinWidth, minWidth - 1);
                    //Logger.debug("minwidth = " + minWidth);
                    minWidthCache = Math.Max(minWidth, 0);
                    minWidthIsCached = true;
                    //Logger.DecLvl();
                    return minWidthCache;
                }
            }
        }

        public RenderBoxTree() : base()
        {
            using (new Logger("RenderBoxTree.__construct()", Logger.Mode.LOG))
            {
                //Logger.debug("NodeBoxTree constructor()");
                Boxes = new List<RenderBox>();
            }
        }

        public override void Add(string box)
        {
            using (new SimpleProfiler("RenderBoxTree.Add(string)"))
            {
                AddAt(Boxes.Count, box);
            }
        }

        public override void AddAt(int position, string box)
        {
            using (new SimpleProfiler("RenderBoxTree.AddAt(int, string)"))
            {
                //Logger.debug("NodeBoxTree.AddAt(int, string)");
                //Logger.IncLvl();
                AddAt(position, new RenderBoxLeaf(box));
                //Logger.DecLvl();
            }
        }

        public override void Add(StringBuilder box)
        {
            using (new SimpleProfiler("RenderBoxTree.Add(StringBuilder)"))
            {
                AddAt(Boxes.Count, box);
            }
        }

        public override void AddAt(int position, StringBuilder box)
        {
            using (new SimpleProfiler("RenderBoxTree.AddAt(int, StringBuilder)"))
            {
                //Logger.debug("NodeBoxTree.AddAt(int, StringBuilder)");
                //Logger.IncLvl();
                AddAt(position, new RenderBoxLeaf(box));
                //Logger.DecLvl();
            }
        }

        public void AddAt(int position, RenderBox box)
        {
            using (new SimpleProfiler("RenderBoxTree.AddAt(int, RenderBox)"))
            {
                //Logger.debug("NodeBoxTree.AddAt(int, NodeBox)");
                //Logger.IncLvl();
                Boxes.AddOrInsert<RenderBox>(box, position);
                box.Parent = this;
                ClearCache();
                //Logger.DecLvl();
            }
        }

        public void Add(RenderBox box)
        {
            using (new SimpleProfiler("RenderBoxTree.Add(RenderBox)"))
            {
                //Logger.debug("NodeBoxTree.Add(NodeBox)");
                //Logger.IncLvl();
                AddAt(Boxes.Count, box);
                //Logger.DecLvl();
            }
        }


        public override StringBuilder GetLine(int index)
        {
                return GetLine(index, -1, -1);
        }

        public override StringBuilder GetLine(int index, int maxWidth, int maxHeight)
        {
            using (Logger logger = new Logger("RenderBoxTree.GetLine(int, int, int)", Logger.Mode.LOG))
            {
                //Logger.debug("NodeBoxTree.GetLine(int, int)");
                //Logger.IncLvl();
                StringBuilder line = new StringBuilder();
                int floatingMaxHeight = maxHeight;
                if (floatingMaxHeight != -1)
                    floatingMaxHeight = Math.Max(floatingMaxHeight - MinHeight, 0) - 1;
                int boxMinHeight;
                int boxHeight;
                int boxMaxHeight;
                //bool foundLine = false;
                if (Flow == RenderBox.FlowDirection.VERTICAL)
                {
                    foreach (RenderBox box in Boxes)
                    {
                        boxMinHeight = box.MinHeight;
                        boxMaxHeight = floatingMaxHeight + boxMinHeight + 1;
                        boxHeight = box.GetActualHeight(boxMaxHeight);
                        if (index < boxHeight)
                        {
                            line = box.GetLine(index, maxWidth, boxMaxHeight);
                            //Logger.debug("child box width is " + TextUtils.GetTextWidth(line));
                            //foundLine = true;
                            break;
                        }
                        else
                        {
                            logger.log("Decreasing index by " + boxHeight, Logger.Mode.LOG);
                            index -= boxHeight;
                            if(floatingMaxHeight != -1)
                                floatingMaxHeight -= Math.Max(0, (boxHeight - boxMinHeight));
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
                            floatingMaxWidth = Math.Max(0, floatingMaxWidth - TextUtils.GetTextWidth(nextLine.ToString()) + boxMinWidth);
                        line.Append(nextLine);
                        //Logger.debug("child box width is: " + TextUtils.GetTextWidth(nextLine));
                    }
                    //foundLine = index < Height;
                }
                AlignLine(ref line, maxWidth);
                //Logger.debug("line is: {" + line + "}");
                //Logger.DecLvl();
                return line;
            }
        }

        public override void Clear()
        {
            //Logger.debug("NodeBoxTree.Clear()");
            //Logger.IncLvl();
            Boxes.Clear();
            ClearCache();
            //Logger.DecLvl();
        }

    }
}
