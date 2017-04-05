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
    public class RenderBoxLeaf : RenderBox
    {
        string Content;

        public override RenderBox.FlowDirection Flow
        {
            get { return RenderBox.FlowDirection.VERTICAL; }
            set { }
        }

        public override int MinHeight
        {
            get
            {
                using (new SimpleProfiler("RenderBoxLeaf.MinHeight.get"))
                {
                    //Logger.debug("NodeBoxLeaf.MinHeight.get");
                    //Logger.IncLvl();
                    if (minHeightIsCached && false)
                        return minHeightCache;
                    if (Content.Length > 0)
                    {
                        minHeightCache = Math.Max(_MinHeight, 1);
                    }
                    else
                    {
                        minHeightCache = _MinHeight;
                    }
                    minHeightIsCached = true;
                    //Logger.debug("minheight = " + minHeightCache);
                    //Logger.DecLvl();
                    return minHeightCache;
                }
            }
            set
            {
                using (new SimpleProfiler("RenderBoxLeaf.MinHeight.set"))
                {
                    //Logger.debug("NodeBoxLeaf.MinHeight.set");
                    //Logger.IncLvl();
                    //Logger.debug("minheight = " + value);
                    _MinHeight = value;
                    ClearCache();
                    //Logger.DecLvl();
                }
            }
        }

        public override int MinWidth
        {
            get
            {
                using (new SimpleProfiler("RenderBoxLeaf.MinWidth.get"))
                {
                    //Logger.debug("NodeBoxLeaf.MinWidth.get");
                    //Logger.IncLvl();
                    if (minWidthIsCached && false)
                        return minWidthCache;
                    minWidthCache = MinHeight == 0 ? 0 : Math.Max(TextUtils.GetTextWidth(Content), _MinWidth);
                    //Logger.debug("minwidth = " + minWidth);
                    minWidthIsCached = true;
                    //Logger.DecLvl();
                    return minWidthCache;
                }
            }
            set
            {
                using (new SimpleProfiler("RenderBoxLeaf.MinWidth.set"))
                {
                    //Logger.debug("NodeBoxLeaf.MinWidth.set()");
                    //Logger.IncLvl();
                    //Logger.debug("minwidth = " + value);
                    _MinWidth = value;
                    ClearCache();
                    //Logger.DecLvl();
                }
            }
        }

        public RenderBoxLeaf()
        {
            using (new Logger("RenderBoxLeaf.__construct()", Logger.Mode.LOG))
            {
                //Logger.debug("NodeBoxLeaf constructor()");
                Content = "";
                ClearCache();
            }
        }

        public RenderBoxLeaf(StringBuilder content) : this()
        {
            using (new Logger("RenderBoxLeaf.__construct(StringBuilder)", Logger.Mode.LOG))
            {
                //Logger.debug("NodeBoxLeaf constructor(StringBuilder)");
                //Logger.IncLvl();
                Add(content);
                //Logger.DecLvl();
            }
        }

        public RenderBoxLeaf(string content) : this(new StringBuilder(content))
        { }

        public override void AddAt(int position, StringBuilder box)
        {
            using (new SimpleProfiler("RenderBoxLeaf.AddAt(int, StringBuilder)"))
            {
                //Logger.debug("NodeBoxLeaf.AddAt(int, StringBuilder)");
                //Logger.IncLvl();
                /*box.Replace("\n", "");
                box.Replace("\r", "");*/
                if (position == 0)
                {
                    Content = box.ToString() + Content;
                }
                else
                {
                    Content += box;
                }
                ClearCache();
                //Logger.DecLvl();
            }
        }

        public override void Add(StringBuilder box)
        {
            using (new SimpleProfiler("RenderBoxLeaf.Add(StringBuilder)"))
            {
                //Logger.debug("NodeBoxLeaf.Add(StringBuilder)");
                //Logger.IncLvl();
                AddAt(1, box);
                //Logger.DecLvl();
            }
        }
        public override void AddAt(int position, string box)
        {
            using (new SimpleProfiler("RenderBoxLeaf.AddAt(int, string)"))
            {
                //Logger.debug("NodeBoxLeaf.AddAt(int, string)");
                //Logger.IncLvl();
                AddAt(position, new StringBuilder(box));
                //Logger.DecLvl();
            }
        }

        public override void Add(string box)
        {
            using (new SimpleProfiler("RenderBoxLeaf.Add(string)"))
            {
                //Logger.debug("NodeBoxLeaf.Add(string)");
                //Logger.IncLvl();
                Add(new StringBuilder(box));
                //Logger.DecLvl();
            }
        }

        public override StringBuilder GetLine(int index)
        {
            return GetLine(index, -1, -1);
        }

        public override StringBuilder GetLine(int index, int maxWidth, int maxHeight)
        {
            using (Logger logger = new Logger("RenderBoxLeaf.GetLine(int, int, int)", Logger.Mode.LOG))
            {
                logger.log("type: " + type, Logger.Mode.LOG);
                logger.log("index: " + index, Logger.Mode.LOG);
                logger.log("maxwidth: " + maxWidth, Logger.Mode.LOG);
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
                //Logger.debug("line is {" + line + "}");
                //Logger.log("instructions: " + (P.Runtime.CurrentInstructionCount - instructions) + " -> " + P.Runtime.CurrentInstructionCount + "/" + P.Runtime.MaxInstructionCount)
                //Logger.DecLvl();
                return line;
            }
        }

        public override void Clear()
        {
            using (new SimpleProfiler("RenderBoxLeaf.Clear()"))
            {
                Content = "";
                ClearCache();
            }
        }


    }
}
