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
        public string Content;
        int DynamicHeight;
        int MinWidthOverride;
        int offsetCache;
        int lastIndex;
        Dictionary<int, StringBuilder> LineCache;

        public override RenderBox.FlowDirection Flow
        {
            get { return RenderBox.FlowDirection.VERTICAL; }
            set { }
        }

        public override int MinHeight
        {
            get
            {
                if (!InitState.Initialized)
                    Initialize(Int32.MaxValue, Int32.MaxValue);

                if (Content.Length > 0)
                {
                    minHeightCache = Math.Max(_MinHeight, LineCache.Count);
                }
                else
                {
                    minHeightCache = _MinHeight;
                }
                /*if(MaxHeight != -1)
                {
                    minHeightCache = Math.Min(minHeightCache, MaxHeight);
                }*/
                return minHeightCache;
            }
            set
            {
                _MinHeight = value;
                ClearCache();
            }
        }

        public override int MinWidth
        {
            get
            {
                //using (new SimpleProfiler("RenderBoxLeaf.MinWidth.get"))
                //{
                    //Logger.debug("NodeBoxLeaf.MinWidth.get");
                    //Logger.IncLvl();
                    if (minWidthIsCached && false)
                        return minWidthCache;
                    minWidthCache = Math.Max(MinWidthOverride, 
                        Content.Length == 0 ? 
                        _MinWidth : 
                        Math.Max(25, _MinWidth));
                    //Logger.debug("minwidth = " + minWidth);
                    minWidthIsCached = true;
                    //Logger.DecLvl();
                    return minWidthCache;
                //}
            }
            set
            {
                //using (new SimpleProfiler("RenderBoxLeaf.MinWidth.set"))
                //{
                    //Logger.debug("NodeBoxLeaf.MinWidth.set()");
                    //Logger.IncLvl();
                    //Logger.debug("minwidth = " + value);
                    _MinWidth = value;
                    ClearCache();
                    //Logger.DecLvl();
                //}
            }
        }

        public override int DesiredHeight
        {
            get
            {
                if (DynamicHeight == -1 || _DesiredHeight != -1)
                    return base.DesiredHeight;
                else
                    return DynamicHeight;
            }
        }

        public RenderBoxLeaf()
        {
            //using (new Logger("RenderBoxLeaf.__construct()", Logger.Mode.LOG))
            //{
                //Logger.debug("NodeBoxLeaf constructor()");
                Clear();
            //}
        }

        public RenderBoxLeaf(StringBuilder content) : this()
        {
            //using (new Logger("RenderBoxLeaf.__construct(StringBuilder)", Logger.Mode.LOG))
            //{
                //Logger.debug("NodeBoxLeaf constructor(StringBuilder)");
                //Logger.IncLvl();
                Add(content);
                //Logger.DecLvl();
            //}
        }

        public RenderBoxLeaf(string content) : this(new StringBuilder(content))
        { }

        public override void AddAt(int position, StringBuilder box)
        {
            //using (new SimpleProfiler("RenderBoxLeaf.AddAt(int, StringBuilder)"))
            //{
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
            //}
        }

        public override void Add(StringBuilder box)
        {
            //using (new SimpleProfiler("RenderBoxLeaf.Add(StringBuilder)"))
            //{
                //Logger.debug("NodeBoxLeaf.Add(StringBuilder)");
                //Logger.IncLvl();
                AddAt(1, box);
                //Logger.DecLvl();
            //}
        }
        public override void AddAt(int position, string box)
        {
            //using (new SimpleProfiler("RenderBoxLeaf.AddAt(int, string)"))
            //{
                //Logger.debug("NodeBoxLeaf.AddAt(int, string)");
                //Logger.IncLvl();
                AddAt(position, new StringBuilder(box));
                //Logger.DecLvl();
            //}
        }

        public override void Add(string box)
        {
            //using (new SimpleProfiler("RenderBoxLeaf.Add(string)"))
            //{
                //Logger.debug("NodeBoxLeaf.Add(string)");
                //Logger.IncLvl();
                Add(new StringBuilder(box));
                //Logger.DecLvl();
            //}
        }

        public override StringBuilder GetLine(int index)
        {
            return GetLine(index, -1, -1);
        }

        public override StringBuilder GetLine(int index, int maxWidth, int maxHeight)
        {
            using (Logger logger = new Logger("RenderBoxLeaf.GetLine(int, int, int)", Logger.Mode.LOG))
            {
                //logger.log("type: " + type, Logger.Mode.LOG);
                //logger.log("index: " + index, Logger.Mode.LOG);
                //logger.log("maxwidth: " + maxWidth, Logger.Mode.LOG);
                if(LineCache.ContainsKey(index))
                {
                    return LineCache[index];
                }
                StringBuilder line = new StringBuilder();
                if (index < _MaxHeight)
                {
                    int height = GetActualHeight(maxHeight);
                    int width = GetActualWidth(maxWidth);
                    int offset = 0;
                    int spacePos = -1;
                    int i = 0;
                    /*if (index == lastIndex + 1)
                    {
                        offset = offsetCache;
                        i = lastIndex;
                    }*/
                    using (Logger logger1 = new Logger("loop"))
                        for (; i < index; i++)
                        {
                            if (offset < Content.Length)
                            {
                                logger1.log("old offset: " + offset);
                                string lineString = TextUtils.SubstringOfWidth(Content, width, offset);
                                int length = lineString.Length;
                                spacePos = Content.LastIndexOf(' ', Math.Min(Content.Length - 1, offset + length), length);
                                if (spacePos == -1 || offset + length == Content.Length)
                                    offset += length;
                                else
                                    offset = spacePos + 1;
                                logger1.log("line string: " + lineString);
                                logger.log("offset: " + offset);
                                logger1.log("length: " + length);
                                logger1.log("space pos: " + spacePos);
                                //offset += TextUtils.SubstringOfWidth(Content, width, offset).Length;

                            }
                        }
                    offsetCache = offset;
                    lastIndex = index;
                    if (offset < Content.Length)
                    {
                        string lineString = TextUtils.SubstringOfWidth(Content, width, offset).Trim();
                        logger.log("line: " + lineString, Logger.Mode.LOG);
                        if (lineString.Length + offset < Content.Length)
                        {
                            spacePos = lineString.LastIndexOf(' ', lineString.Length - 1, lineString.Length);
                            if (spacePos != -1 && Content[offset + lineString.Length] != ' ')
                            {
                                lineString = lineString.Substring(0, spacePos);
                            }
                        }
                        logger.log("result: " + lineString);
                        line = new StringBuilder(lineString);
                        AlignLine(ref line, maxWidth);
                        if (line.Length > 0)
                            LineCache[index] = line;
                    }
                    else
                    {
                        AlignLine(ref line, maxWidth);
                    }
                }
                else
                {
                    AlignLine(ref line, maxWidth);
                }



                logger.log("Content is: " + Content, Logger.Mode.LOG);
                logger.log("Line (" + index + ") is: " + line, Logger.Mode.LOG);
                logger.log("line is {" + line + "}", Logger.Mode.LOG);
                //Logger.log("instructions: " + (P.Runtime.CurrentInstructionCount - instructions) + " -> " + P.Runtime.CurrentInstructionCount + "/" + P.Runtime.MaxInstructionCount)
                //Logger.DecLvl();
                return line;
            }
        }

        public override void Clear()
        {
            //using (new SimpleProfiler("RenderBoxLeaf.Clear()"))
            //{
                Content = "";
                DynamicHeight = -1;
                MinWidthOverride = 0;
                LineCache = new Dictionary<int, StringBuilder>();
                ClearCache();
            //}
        }


        public override void CalculateDynamicHeight(int maxWidth, int maxHeight)
        {
            using (Logger logger = new Logger("RenderBoxLeaf.CalculateDynamicHeight(int, int)", Logger.Mode.LOG))
            {

                InitState.Initialized = true;
                logger.log("Type: " + type, Logger.Mode.LOG);
                logger.log("implicit max width: " + maxWidth, Logger.Mode.LOG);
                logger.log("explicit max width: " + MaxWidth, Logger.Mode.LOG);
                logger.log("implicit max height: " + maxHeight, Logger.Mode.LOG);
                logger.log("explicit max height: " + MaxHeight, Logger.Mode.LOG);
                logger.log("min width: " + MinWidth);
                if(InitState.Initialized) logger.log("min height: " + MinHeight);
                /*logger.log("Type: " + type, Logger.Mode.LOG);
                logger.log("max width: " + maxWidth, Logger.Mode.LOG);
                logger.log("max height: " + MaxHeight, Logger.Mode.LOG);
                int contentWidth = TextUtils.GetTextWidth(Content);
                logger.log("content width: " + contentWidth, Logger.Mode.LOG);
                if (maxWidth <= 0 || contentWidth == 0)
                    DynamicHeight = -1;
                else
                {
                    maxWidth = (_DesiredWidth == -1 ? maxWidth : Math.Min(_DesiredWidth, maxWidth));
                    DynamicHeight = 0;
                    MinWidthOverride = 0;
                    int wordWidth;
                    int widthSum = 0;
                    foreach (string word in words)
                    {
                        wordWidth = TextUtils.GetTextWidth(word);
                        widthSum += wordWidth;
                        if (widthSum > maxWidth)
                        {
                            DynamicHeight++;
                            widthSum = TextUtils.GetTextWidth(word.Substring(
                                word.LastIndexOf))
                        }
                        else
                            MinWidthOverride = Math.Max(MinWidthOverride, wordWidth);
                    }
                }
                logger.log("Dynamic Height is: " + DynamicHeight, Logger.Mode.LOG);*/

                LineCache = new Dictionary<int, StringBuilder>();
                StringBuilder line;
                int index = -1;
                if(maxWidth <= 0)
                {
                    for(int i = 0; i < GetActualHeight(maxHeight); i++)
                        LineCache[i] = new StringBuilder();
                    MinWidthOverride = 0;
                    return;
                }
                //Console.WriteLine("start loop");

                do
                {
                    line = GetLine(++index, maxWidth, maxHeight);
                    //Console.WriteLine("content length: " + Content.Length);
                    //Console.WriteLine("offsetcache: " + offsetCache);
                }
                while (LineCache.ContainsKey(index));
                //Console.WriteLine("linecache size: " + LineCache.Count);
                MinWidthOverride = 0;
                foreach(StringBuilder currLine in LineCache.Values)
                {
                    MinWidthOverride = Math.Max(TextUtils.GetTextWidth(currLine.ToString()), MinWidthOverride);

                }

                InitState.MaxWidth = maxWidth;
                InitState.MaxHeight = maxHeight;
                
            }

        }

        public override void Initialize(int maxWidth, int maxHeight)
        {
            CalculateDynamicHeight(maxWidth, maxHeight);
        }

    }
}
