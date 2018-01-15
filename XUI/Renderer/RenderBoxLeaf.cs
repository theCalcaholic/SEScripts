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
using System.Text.RegularExpressions;

namespace IngameScript
{
	partial class Program : MyGridProgram
	{
		public class RenderBoxLeaf : IRenderBox
		{
			public string Content;
			int DynamicHeight;
			int TextWidth;
			int lastIndex;
			StringBuilder[] LineCache;
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

					if (minHeightIsCached && CacheEnabled)
						return minHeightCache;

					if (Content.Length > 0)
						minHeightCache = Math.Max(_MinHeight, LineCache?.Length ?? 1);
					else
						minHeightCache = _MinHeight;
					return minHeightCache;
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
					if (minWidthIsCached && CacheEnabled)
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
					if( _DesiredHeight != -1 )
						return _DesiredHeight;
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
				if (index < LineCache.Length && LineCache[index] != null)
					return LineCache[index];
				else
				{
					var line = new StringBuilder();
					AlignLine(ref line, GetActualWidth(MinWidth));
					return line;
				}
			}

			public override void Clear()
			{
				Content = "";
				DynamicHeight = -1;
				TextWidth = 0;
				LineCache = null;
				ClearCache();
			}


			public override void CalculateDimensions(int maxWidth, int maxHeight)
			{

				BuildLineCache(maxWidth, maxHeight);
				TextWidth = 0;
				int lineWidth;
				foreach (StringBuilder currLine in LineCache)
				{
					if (currLine == null)
						break;
					lineWidth = TextUtils.GetTextWidth(currLine.ToString());
					TextWidth = Math.Max(lineWidth, TextWidth);

				}
				DynamicHeight = LineCache.Count();

				for (int i = 0; i < LineCache.Count() && LineCache[i] != null; i++)
				{
					StringBuilder line = LineCache[i];
					AlignLine(ref line, GetActualWidth(maxWidth));
					LineCache[i] = line;
				}
			}

			public override void Initialize(int maxWidth, int maxHeight)
			{
				CalculateDimensions(maxWidth, maxHeight);
				Initialized = true;
			}

			private void BuildLineCache(int maxWidth, int maxHeight)
			{
				StringBuilder line;
				int index = -1;
				if (maxWidth <= 0)
				{
					int height = GetActualHeight(maxHeight);
					LineCache = new StringBuilder[height];
					for (int i = 0; i < height; i++)
						LineCache[i] = new StringBuilder();
					return;
				}
				else
				{
					LineCache = new StringBuilder[4];
					
					var lines = BuildLines(maxWidth, maxHeight);

					LineCache = lines.ToArray();
				}
			}

			private List<StringBuilder> BuildLines(int maxWidth, int maxHeight)
			{
				List<StringBuilder> lines = new List<StringBuilder>();
				int height = GetActualHeight(maxHeight);
				int width = Math.Min(maxWidth, MaxWidth);
				int offset = 0;
				int spacePos = -1;

				while( offset < Content.Length
					&& ((DynamicHeight > 0 && lines.Count < DynamicHeight)
						|| lines.Count < _MaxHeight) )
				{	
					string lineString = TextUtils.SubstringOfWidth(Content, width, offset);
					string trimmedLine = lineString.Trim();
					if (trimmedLine.Length + offset < Content.Length)
					{
						spacePos = trimmedLine.LastIndexOf(' ', trimmedLine.Length - 1, trimmedLine.Length);
						if (spacePos != -1 && Content[offset + trimmedLine.Length] != ' ')
							trimmedLine = trimmedLine.Substring(0, spacePos);
					}
					lines.Add(new StringBuilder(trimmedLine));
					
					
					int length = lineString.Length;
					spacePos = Content.LastIndexOf(' ', Math.Min(Content.Length - 1, offset + length), length);
					if (spacePos == -1 || offset + length == Content.Length)
						offset += length;
					else
						offset = spacePos + 1;
				}
				
				return lines;
			}
			
		}
	}
}