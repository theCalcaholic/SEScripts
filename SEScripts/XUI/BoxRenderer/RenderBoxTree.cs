using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox.Game.Gui;
using SEScripts.Lib;
//using SEScripts.Lib.LoggerNS;
using SEScripts.Lib.Profilers;

namespace SEScripts.XUI.BoxRenderer
{
    public class RenderBoxTree : IRenderBox
    {
        List<IRenderBox> Boxes;

        public IRenderBox this[int i]
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
                return Boxes.Count;
            }
        }

        public override int MinHeight
        {
            get
            {
                //using (Logger logger = new Logger("RenderBoxTree.MinHeight.get", Logger.Mode.LOG))
                //{
                    if (minHeightIsCached && false)
                        return minHeightCache;
                    //int minHeight = (Flow != FlowDirection.HORIZONTAL ? 0 : _MinHeight);
                    int minHeight = 0;
                    int boxMinHeight;
                    foreach (IRenderBox box in Boxes)
                    {
                        if (Flow == IRenderBox.FlowDirection.HORIZONTAL)
                        {
                            minHeight = Math.Max(minHeight, box.MinHeight);
                        }
                        else
                        {
                            boxMinHeight = box.MinHeight;
                            if (boxMinHeight > 0)
                            {
                                minHeight += boxMinHeight;
                                //logger.log("min height + " + boxMinHeight, Logger.Mode.LOG);
                            }
                        }
                    }
                    minHeightCache = Math.Max(0, Math.Max(_MinHeight, minHeight));
                    //logger.log("minheight = " + minHeightCache.ToString(), Logger.Mode.LOG);
                    minHeightIsCached = true;
                    //Logger.DecLvl();
                    return minHeightCache;
                //}
            }
        }

        public override int MinWidth
        {
            get
            {
                //using (var logger = new Logger("RenderBoxTree.MinWidth.Get", Logger.Mode.LOG))
                //{
                    //logger.log("type: " + type);
                    if (minWidthIsCached && false)
                        return minWidthCache;
                    int minWidth = (Flow == IRenderBox.FlowDirection.HORIZONTAL ? 0 : _MinWidth);
                    int boxMinWidth;
                    foreach (IRenderBox box in Boxes)
                    {
                        if (Flow == IRenderBox.FlowDirection.HORIZONTAL)
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
                    if (Flow == IRenderBox.FlowDirection.HORIZONTAL)
                        minWidth = Math.Max(_MinWidth, minWidth - 1);
                    //Logger.debug("minwidth = " + minWidth);
                    minWidthCache = minWidth;
                    minWidthIsCached = true;
                    //Logger.DecLvl();
                    return minWidthCache;
                //}
            }
        }

        public override int DesiredWidth
        {
            get
            {
                if (desiredWidthIsCached && false)
                    return desiredWidthCache;
                int desiredWidth = (Flow == IRenderBox.FlowDirection.HORIZONTAL ? 0 : _DesiredWidth);
                int boxDesWidth;
                foreach (IRenderBox box in Boxes)
                {
                    if (Flow == IRenderBox.FlowDirection.HORIZONTAL)
                    {
                        boxDesWidth = box.DesiredWidth;
                        if (boxDesWidth > 0)
                        {
                            desiredWidth++;
                            desiredWidth += boxDesWidth;
                        }
                    }
                    else
                    {
                        desiredWidth = Math.Max(box.DesiredWidth, desiredWidth);
                    }
                }
                if (Flow == IRenderBox.FlowDirection.HORIZONTAL)
                    desiredWidth = Math.Max(_DesiredWidth, desiredWidth - 1);
                desiredWidthCache = desiredWidth;
                desiredWidthIsCached = true;
                return desiredWidthCache;
            }
        }

        public override int DesiredHeight
        {
            get
            {
                //using (Logger logger = new Logger("RenderBoxTree.DesiredHeight.get", Logger.Mode.LOG))
                //{
                    //logger.log("Type: " + type);
                    if (desiredHeightIsCached && false)
                        return desiredHeightCache;
                    //int minHeight = (Flow != FlowDirection.HORIZONTAL ? 0 : _MinHeight);
                    int desiredHeight = 0;
                    int boxDesHeight;
                    foreach (IRenderBox box in Boxes)
                    {
                        if (Flow == IRenderBox.FlowDirection.HORIZONTAL)
                        {
                            desiredHeight = Math.Max(desiredHeight, box.DesiredHeight);
                        }
                        else
                        {
                            boxDesHeight = Math.Max(box.DesiredHeight, box.MinHeight);
                            desiredHeight += boxDesHeight;
                            //logger.log("desired height + " + boxDesHeight, Logger.Mode.LOG);
                        }
                    }
                    desiredHeightCache = Math.Max(_DesiredHeight, desiredHeight);
                    //logger.log("desired height = " + desiredHeightCache.ToString(), Logger.Mode.LOG);
                    desiredHeightIsCached = true;
                    //Logger.DecLvl();
                    return desiredHeightCache;
                //}
            }
        }

        public RenderBoxTree() : base()
        {
            Boxes = new List<IRenderBox>();
        }

        public override void Add(string box)
        {
            //using (new SimpleProfiler("RenderBoxTree.Add(string)"))
            //{
                AddAt(Boxes.Count, box);
            //}
        }

        public override void AddAt(int position, string box)
        {
            AddAt(position, new RenderBoxLeaf(box));
        }

        public override void Add(StringBuilder box)
        {
            AddAt(Boxes.Count, box);
        }

        public override void AddAt(int position, StringBuilder box)
        {
            AddAt(position, new RenderBoxLeaf(box));
        }

        public void AddAt(int position, IRenderBox box)
        {
            Boxes.AddOrInsert<IRenderBox>(box, position);
            box.Parent = this;
            ClearCache();
        }

        public void Add(IRenderBox box)
        {
            AddAt(Boxes.Count, box);
        }


        public override StringBuilder GetLine(int index)
        {
                return GetLine(index, int.MaxValue, int.MaxValue);
        }

        public override StringBuilder GetLine(int index, int maxWidth, int maxHeight)
        {
            //using (Logger logger = new Logger("RenderBoxTree.GetLine(int, int, int)", Logger.Mode.LOG))
            //{
                //logger.log("Type: " + type, Logger.Mode.LOG);
                //logger.log("implicit max width: " + maxWidth, Logger.Mode.LOG);
                //logger.log("explicit max width: " + MaxWidth);
                //logger.log("implicit max height: " + maxHeight, Logger.Mode.LOG);
                //logger.log("explicit max height: " + MaxHeight);
                //logger.log("min width: " + MinWidth);
                //Logger.debug("NodeBoxTree.GetLine(int, int)");
                //Logger.IncLvl();
                StringBuilder line = new StringBuilder();
                
                maxWidth = Math.Min(maxWidth, MaxWidth);
                
                maxHeight = Math.Min(maxHeight, MaxHeight);

                int boxMinHeight;
                int boxHeight;
                int boxMaxHeight;
                //bool foundLine = false;
                if (Flow == IRenderBox.FlowDirection.VERTICAL)
                {
                    int floatingMaxHeight = maxHeight;
                    floatingMaxHeight = floatingMaxHeight - MinHeight - 1;
                    //logger.log("floating max height: " + floatingMaxHeight, Logger.Mode.LOG);
                    foreach (IRenderBox box in Boxes)
                    {
                        boxMinHeight = box.MinHeight;
                        boxMaxHeight = floatingMaxHeight + boxMinHeight + 1;
                        //logger.log("boxMaxHeight: " + boxMaxHeight, Logger.Mode.LOG);
                        boxHeight = box.GetActualHeight(boxMaxHeight);
                        if (index < boxHeight)
                        {
                            line = box.GetLine(index, maxWidth, boxMaxHeight);
                            AlignLine(ref line, maxWidth, box.Align, box.PadChar);
                            //Logger.debug("child box width is " + TextUtils.GetTextWidth(line));
                            //foundLine = true;
                            break;
                        }
                        else
                        {
                            //logger.log("Decreasing index by " + boxHeight, Logger.Mode.LOG);
                            index -= boxHeight;
                            floatingMaxHeight = floatingMaxHeight - boxHeight + boxMinHeight;
                        }
                    }
                }
                else
                {
                    int floatingMaxWidth = maxWidth;
                    floatingMaxWidth = floatingMaxWidth - MinWidth - 1;
                    //logger.log("floatingMaxWidth: " + floatingMaxWidth);
                    StringBuilder nextLine;
                    int boxMinWidth;
                    foreach (IRenderBox box in Boxes)
                    {
                        boxMinWidth = box.MinWidth;
                        nextLine = box.GetLine(index, 1 + floatingMaxWidth + boxMinWidth, maxHeight);
                        //logger.log("child width: " + TextUtils.GetTextWidth(nextLine.ToString()));
                        floatingMaxWidth = floatingMaxWidth - TextUtils.GetTextWidth(nextLine.ToString()) + boxMinWidth;
                        //logger.log("new floatingmaxWidth: " + floatingMaxWidth);
                        line.Append(nextLine);
                        //Logger.debug("child box width is: " + TextUtils.GetTextWidth(nextLine));
                    }
                    //foundLine = index < Height;
                }
                AlignLine(ref line, maxWidth);
                //Logger.debug("line is: {" + line + "}");
                //Logger.DecLvl();
                return line;
            //}
        }

        public override void Clear()
        {
            //Logger.debug("NodeBoxTree.Clear()");
            //Logger.IncLvl();
            Boxes.Clear();
            ClearCache();
            //Logger.DecLvl();
        }

        public override void CalculateDimensions(int maxWidth, int maxHeight)
        {
            //using (Logger logger = new Logger("RenderBoxTree.CalculateDynamicHeight(int)", Logger.Mode.LOG))
            //{
                //logger.log("Type: " + type, Logger.Mode.LOG);
                //logger.log("implicit max width: " + maxWidth, Logger.Mode.LOG);
                //logger.log("explicit max width: " + MaxWidth, Logger.Mode.LOG);
                //logger.log("implicit max height: " + maxHeight, Logger.Mode.LOG);
                //logger.log("explicit max height: " + MaxHeight, Logger.Mode.LOG);
                //logger.log("min width: " + MinWidth);

                maxWidth = Math.Min(maxWidth, MaxWidth);
                maxHeight = Math.Min(maxHeight, MaxHeight);

                int boxMinHeight;
                int boxHeight;
                int boxMaxHeight;
                //bool foundLine = false;
                if (Flow == IRenderBox.FlowDirection.HORIZONTAL)
                {
                    int floatingMaxWidth = maxWidth - MinWidth - 1;
                    int boxMinWidth;
                    int boxMaxWidth;
                    int boxWidth;
                    foreach (IRenderBox box in Boxes)
                    {
                        boxMinWidth = box.MinWidth;
                        boxMaxWidth = Math.Max(boxMinWidth, 1 + floatingMaxWidth + boxMinWidth);
                        //logger.log("floating max width: " + floatingMaxWidth);
                        //logger.log("boxminwidth: " + boxMinWidth);
                        box.CalculateDimensions(boxMaxWidth, maxHeight);
                        boxWidth = box.GetActualWidth(boxMaxWidth);
                        floatingMaxWidth = floatingMaxWidth - boxWidth + boxMinWidth;
                    }
                }
                else
                {
                    int floatingMaxHeight = Math.Max(maxHeight - MinHeight, 0) - 1;
                    //logger.log("floating max height: " + floatingMaxHeight, Logger.Mode.LOG);
                    foreach (IRenderBox box in Boxes)
                    {
                        boxMinHeight = box.MinHeight;
                        boxMaxHeight = floatingMaxHeight + boxMinHeight + 1;
                        //logger.log("boxMaxHeight: " + boxMaxHeight, Logger.Mode.LOG);
                        boxHeight = box.GetActualHeight(boxMaxHeight);
                        //logger.log("maxWidth: " + maxWidth.ToString());
                        box.CalculateDimensions(maxWidth, boxMaxHeight);
                        floatingMaxHeight = Math.Max(0, floatingMaxHeight - boxHeight + boxMinHeight);
                    }
                }
                //minWidthIsCached = false;
                //minHeightIsCached = false;
            //}
        }

        public override void Initialize(int maxWidth, int maxHeight)
        {
            // RecursivelyParseWidthDefinitions(maxWidth, maxHeight);
            CalculateDimensions(maxWidth, maxHeight);
        }
        
    }
}
