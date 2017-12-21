using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System;
using VRage.Collections;
using VRage.Game.Components;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ObjectBuilders.Definitions;
using VRage.Game;
using VRageMath;

namespace IngameScript
{
	partial class Program : MyGridProgram
	{
		public class RenderBoxLeaf : IRenderBox
		{
			public string Content;
			int DynamicHeight;
			int TextWidth;
			int offsetCache;
			int lastIndex;
			Dictionary<int, StringBuilder> LineCache;
			private List<string> _renderedLines;

			public override IRenderBox.FlowDirection Flow
			{
				get { return IRenderBox.FlowDirection.VERTICAL; }
				set { }
			}

			public override int MinHeight
			{
				get
				{
					//using (new Logger("RenderBoxLeaf.MinHeight.get", Logger.Mode.LOG))
					//{
					//if (!InitState.Initialized)
					//    Initialize(int.MaxValue, int.MaxValue);


					if (minHeightIsCached && false)
						return minHeightCache;

					if (Content.Length > 0)
					{
						minHeightCache = Math.Max(_MinHeight, LineCache?.Count ?? (Content.Length == 0 ? 0 : 1));
					}
					else
					{
						minHeightCache = _MinHeight;
					}
					return minHeightCache;
					//}
				}
				set
				{
					_MinHeight = value;
					ClearCache();
				}
			}

			public override int MaxWidth
			{
				get
				{
					if (TextWidth != 0)
						return TextWidth;
					return _MaxWidth;
				}
			}

			public override int MinWidth
			{
				get
				{
					//using (Logger logger = new Logger("RenderBoxLeaf.MinWidth.get", Logger.Mode.LOG))
					//{
					if (minWidthIsCached && false)
						return minWidthCache;
					minWidthCache = Math.Max(TextWidth,
						Content.Length == 0 ?
							_MinWidth :
							Math.Max(25, _MinWidth));
					minWidthIsCached = true;
					//logger.log("content: " + Content);
					//logger.log("minwidth: " + minWidthCache);
					return minWidthCache;
					//}
				}
				set
				{
					_MinWidth = value;
					ClearCache();
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
				return GetLine(index, int.MaxValue, int.MaxValue);
			}


			public override StringBuilder GetLine(int index, int maxWidth, int maxHeight)
			{
				return GetLine(index, maxWidth, maxHeight, true);

			}

			public StringBuilder GetLine(int index, int maxWidth, int maxHeight, bool doAlign)
			{
				//using (Logger logger = new Logger("RenderBoxLeaf.GetLine(int, int, int)", Logger.Mode.LOG))
				//{
				//logger.log("type: " + type, Logger.Mode.LOG);
				//logger.log("index: " + index, Logger.Mode.LOG);
				//logger.log("maxwidth: " + maxWidth, Logger.Mode.LOG);
				if (LineCache.ContainsKey(index))
				{
					return LineCache[index];
				}
				StringBuilder line = new StringBuilder();
				if ((DynamicHeight > 0 && index < DynamicHeight) || index < _MaxHeight)
				{
					int height = GetActualHeight(maxHeight);
					int width = Math.Min(maxWidth, MaxWidth);
					int offset = 0;
					int spacePos = -1;
					int i = 0;
					/*if (index == lastIndex + 1)
                    {
                        offset = offsetCache;
                        i = lastIndex;
                    }*/
					//using (Logger logger1 = new Logger("loop"))
					for (; i < index; i++)
					{
						if (offset < Content.Length)
						{
							//logger1.log("old offset: " + offset);
							string lineString = TextUtils.SubstringOfWidth(Content, width, offset);
							int length = lineString.Length;
							spacePos = Content.LastIndexOf(' ', Math.Min(Content.Length - 1, offset + length), length);
							if (spacePos == -1 || offset + length == Content.Length)
								offset += length;
							else
								offset = spacePos + 1;
							//logger1.log("line string: " + lineString);
							//logger.log("offset: " + offset);
							//logger1.log("length: " + length);
							//logger1.log("space pos: " + spacePos);
							//offset += TextUtils.SubstringOfWidth(Content, width, offset).Length;

						}
					}
					offsetCache = offset;
					lastIndex = index;
					if (offset < Content.Length)
					{
						string lineString = TextUtils.SubstringOfWidth(Content, width, offset).Trim();
						//logger.log("line: " + lineString, Logger.Mode.LOG);
						if (lineString.Length + offset < Content.Length)
						{
							spacePos = lineString.LastIndexOf(' ', lineString.Length - 1, lineString.Length);
							if (spacePos != -1 && Content[offset + lineString.Length] != ' ')
							{
								lineString = lineString.Substring(0, spacePos);
							}
						}
						//logger.log("result: " + lineString);
						line = new StringBuilder(lineString);
						if (doAlign) AlignLine(ref line, maxWidth);
						if (line.Length > 0)
							LineCache[index] = line;
					}
					else if (doAlign)
					{
						AlignLine(ref line, maxWidth);
					}
				}
				else if (doAlign)
				{
					AlignLine(ref line, MinWidth);
				}



				//logger.log("Content is: " + Content, Logger.Mode.LOG);
				//logger.log("Line (" + index + ") is: " + line, Logger.Mode.LOG);
				//logger.log("line is {" + line + "}", Logger.Mode.LOG);
				//Logger.log("instructions: " + (P.Runtime.CurrentInstructionCount - instructions) + " -> " + P.Runtime.CurrentInstructionCount + "/" + P.Runtime.MaxInstructionCount)
				//Logger.DecLvl();
				return line;
				//}
			}

			public override void Clear()
			{
				Content = "";
				DynamicHeight = -1;
				TextWidth = 0;
				LineCache = new Dictionary<int, StringBuilder>();
				ClearCache();
			}


			public override void CalculateDimensions(int maxWidth, int maxHeight)
			{
				//using (Logger logger = new Logger("RenderBoxLeaf.CalculateDynamicHeight(int, int)", Logger.Mode.LOG))
				//{

				InitState.Initialized = true;
				//logger.log("Type: " + type, Logger.Mode.LOG);
				//logger.log("implicit max width: " + maxWidth, Logger.Mode.LOG);
				//logger.log("explicit max width: " + MaxWidth, Logger.Mode.LOG);
				//logger.log("implicit max height: " + maxHeight, Logger.Mode.LOG);
				//logger.log("explicit max height: " + MaxHeight, Logger.Mode.LOG);
				//logger.log("min width: " + MinWidth);
				//if(InitState.Initialized) logger.log("min height: " + MinHeight);


				BuildLineCache(maxWidth, maxHeight);
				//Console.WriteLine("linecache size: " + LineCache.Count);
				TextWidth = 0;
				int lineWidth;
				foreach (StringBuilder currLine in LineCache.Values)
				{
					lineWidth = TextUtils.GetTextWidth(currLine.ToString());
					TextWidth = Math.Max(lineWidth, TextWidth);

				}
				DynamicHeight = LineCache.Count();
				InitState.MaxWidth = maxWidth;
				InitState.MaxHeight = maxHeight;

				for (int i = 0; i < LineCache.Count(); i++)
				{
					StringBuilder line = LineCache[i];
					AlignLine(ref line, maxWidth);
					LineCache[i] = line;
				}

				//logger.log("dynamic height:" + DynamicHeight);
				//logger.log("text width: " + TextWidth);
				//minWidthIsCached = false;
				//minHeightIsCached = false;

				//}

			}

			public override void Initialize(int maxWidth, int maxHeight)
			{
				//ParseWidthDefinitions(maxWidth, maxHeight);
				CalculateDimensions(maxWidth, maxHeight);
			}

			private void BuildLineCache(int maxWidth, int maxHeight)
			{
				LineCache = new Dictionary<int, StringBuilder>();
				StringBuilder line;
				int index = -1;
				if (maxWidth <= 0)
				{
					int height = GetActualHeight(maxHeight);
					for (int i = 0; i < height; i++)
						LineCache[i] = new StringBuilder();
					return;
				}
				else
				{
					//Console.WriteLine("start loop");

					do
					{
						line = GetLine(++index, maxWidth, maxHeight, false);
						//Console.WriteLine("content length: " + Content.Length);
						//Console.WriteLine("offsetcache: " + offsetCache);
					}
					while (LineCache.ContainsKey(index));
				}
			}
		}
	}
}