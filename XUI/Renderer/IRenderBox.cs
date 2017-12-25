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
		public abstract class IRenderBox
		{
			protected bool minHeightIsCached;
			protected bool minWidthIsCached;
			protected bool desiredHeightIsCached;
			protected bool desiredWidthIsCached;
			protected int minHeightCache;
			protected int minWidthCache;
			protected int desiredHeightCache;
			protected int desiredWidthCache;
			public bool DEBUG = false;
			public char PadChar;
			public InitializationState InitState;
			public static bool CacheEnabled = true;
			public enum TextAlign { LEFT, RIGHT, CENTER }
			public enum FlowDirection { HORIZONTAL, VERTICAL }

			//public abstract int Height { get; set; }

			public abstract void Add(string box);
			public abstract void Add(StringBuilder box);
			public abstract void AddAt(int position, string box);
			public abstract void AddAt(int position, StringBuilder box);
			public abstract StringBuilder GetLine(int index);
			public abstract StringBuilder GetLine(int index, int maxWidth, int maxHeight);
			public abstract void Clear();
			public abstract void Initialize(int maxWidth, int maxHeight);
			public abstract void CalculateDimensions(int maxWidth, int maxHeight);
			private IRenderBox.FlowDirection _Flow;
			private IRenderBox.TextAlign _Align;
			public int _MinWidth;
			public int _MaxWidth;
			public int _DesiredWidth;
			public int _MinHeight;
			public int _MaxHeight;
			public int _DesiredHeight;
			public IRenderBox Parent;
			public string type;
			private bool RenderingInProcess;

			public virtual int GetActualWidth(int maxWidth)
			{
				//using (Logger logger = new Logger("RenderBox.GetActualWidth(int)", Logger.Mode.LOG))
				//{
				//logger.log("Type: " + type, Logger.Mode.LOG);
				//if (this as RenderBoxLeaf != null)
				//    logger.log("content: |" + (this as RenderBoxLeaf).Content + "|", Logger.Mode.LOG);
				// logger.log("implicit max width: " + maxWidth, Logger.Mode.LOG);
				// logger.log("explicit max width: " + MaxWidth, Logger.Mode.LOG);
				//logger.log("min width: " + MinWidth, Logger.Mode.LOG);
				//logger.log("desired width: " + DesiredWidth, Logger.Mode.LOG);
				maxWidth = Math.Min(MaxWidth, maxWidth);

				int desired;
				desired = Math.Max(DesiredWidth, MinWidth);
				desired = Math.Min(desired, maxWidth);
				return desired;
				//}

			}

			public int GetIndependentWidth(int maxWidth)
			{
				//using (Logger logger = new Logger("RenderBoxTree.GetIndependentWidth(int)", Logger.Mode.LOG))
				//{
				//logger.log("Type: " + type, Logger.Mode.LOG);
				//logger.log("implicit max width: " + maxWidth, Logger.Mode.LOG);
				//logger.log("explicit max width: " + MaxWidth, Logger.Mode.LOG);
				//logger.log("min width: " + _MinWidth, Logger.Mode.LOG);
				//logger.log("desired width: " + DesiredWidth, Logger.Mode.LOG);
				maxWidth = Math.Min(MaxWidth, maxWidth);

				int desired;
				desired = Math.Max(DesiredWidth, _MinWidth);
				desired = Math.Min(desired, maxWidth);
				return desired;
				//}

			}
			public int GetActualHeight(int maxHeight)
			{
				//using (Logger logger = new Logger("RenderBox.GetActualHeight(int)", Logger.Mode.LOG))
				//{
				//logger.log("Type: " + type, Logger.Mode.LOG);
				//logger.log("implicit max height: " + maxHeight, Logger.Mode.LOG);
				//logger.log("explicit max height: " + MaxHeight, Logger.Mode.LOG);
				maxHeight = Math.Min(MaxHeight, maxHeight);

				int desired = DesiredHeight == -1 ? MinHeight : Math.Max(MinHeight, DesiredHeight);
				//Logger.DecLvl();
				//logger.log("maxheight: " + maxHeight, Logger.Mode.LOG);
				//logger.log("minheight: " + MinHeight, Logger.Mode.LOG);
				//logger.log("Desired Height: " + DesiredHeight, Logger.Mode.LOG);
				//logger.log("actual height: " + Math.Min(desired, maxHeight) + " (min( " + desired + ", " + maxHeight + ")", Logger.Mode.LOG);
				return Math.Max(0, Math.Min(desired, maxHeight));
				//}

			}

			public IRenderBox.TextAlign Align
			{
				get { return _Align; }
				set
				{
					_Align = value;
				}
			}

			public virtual IRenderBox.FlowDirection Flow
			{
				get { return _Flow; }
				set
				{
					_Flow = value;
					ClearCache();
				}
			}

			public virtual int MinWidth
			{
				get
				{
					return _MinWidth;
				}
				set
				{
					_MinWidth = Math.Max(0, value);
					ClearCache();
				}
			}

			public virtual int DesiredWidth
			{
				get
				{
					return _DesiredWidth;
				}
				set
				{
					_DesiredWidth = value;
				}
			}

			public virtual int MaxWidth
			{
				get
				{
					return _MaxWidth;
				}
				set
				{
					if (value < 0)
						_MaxWidth = int.MaxValue;
					else
						_MaxWidth = value;
				}
			}

			public virtual int MinHeight
			{
				get
				{
					return _MinHeight;
				}
				set
				{
					_MinHeight = Math.Max(0, value);
					ClearCache();
				}
			}

			public virtual int DesiredHeight
			{
				get
				{
					return _DesiredHeight;
				}
				set
				{
					_DesiredHeight = value;
				}
			}

			public int MaxHeight
			{
				get
				{
					return _MaxHeight;
				}
				set
				{
					if (value < 0)
						_MaxHeight = int.MaxValue;
					else
						_MaxHeight = value;
					ClearCache();
				}
			}

			public IRenderBox()
			{
				PadChar = ' ';
				_Flow = IRenderBox.FlowDirection.VERTICAL;
				_Align = IRenderBox.TextAlign.LEFT;
				_MinWidth = 0;
				_MaxWidth = int.MaxValue;
				_DesiredWidth = -1;
				_MinHeight = 0;
				_MaxHeight = int.MaxValue;
				_DesiredHeight = -1;
				minHeightIsCached = false;
				minWidthIsCached = false;
				desiredHeightIsCached = false;
				desiredWidthIsCached = false;
				InitState = new InitializationState();
			}

			public bool IsRenderingInProgress()
			{
				return RenderingInProcess || (Parent == null ? false : Parent.IsRenderingInProgress());
			}

			public virtual IEnumerable<StringBuilder> GetLines(int maxWidth, int maxHeight)
			{
				int height = GetActualHeight(maxHeight);
				for (int i = 0; i < height; i++)
				{
					yield return GetLine(i, maxWidth, maxHeight);
				}
			}
			public IEnumerable<StringBuilder> GetLines()
			{
				int height = GetActualHeight(int.MaxValue);
				for (int i = 0; i < height; i++)
				{
					yield return GetLine(i, int.MaxValue, int.MaxValue);
				}
			}

			protected void AlignLine(ref StringBuilder line)
			{
				AlignLine(ref line, int.MaxValue);
			}

			protected void AlignLine(ref StringBuilder line, int maxWidth)
			{
				AlignLine(ref line, maxWidth, Align, PadChar);
			}

			protected void AlignLine(ref StringBuilder line, IRenderBox.TextAlign Alignment)
			{
				AlignLine(ref line, int.MaxValue, Alignment, PadChar);
			}

			protected void AlignLine(ref StringBuilder line, int maxWidth, IRenderBox.TextAlign Alignment, char padChar)
			{
				//using (Logger logger = new Logger("RenderBox.AlignLine(ref StringBuilder, int)", Logger.Mode.LOG))
				//{
				//logger.log("Type: " + type);
				//logger.log("pad char is: " + padChar);
				//logger.log("this.PadChar is: " + PadChar);
				//logger.log("max width is " + maxWidth, Logger.Mode.LOG);
				int actualWidth = GetActualWidth(maxWidth);
				//logger.log("actualWidth: " + actualWidth, Logger.Mode.LOG);
				//logger.log("line is: |" + line + "|", Logger.Mode.LOG);
				//logger.log("line width: " + TextUtils.GetTextWidth(line.ToString()));
				int remainingWidth = actualWidth - TextUtils.GetTextWidth(line.ToString());
				//logger.log("remaining width is " + remainingWidth, Logger.Mode.LOG);
				//logger.log("Aligning " + _Align.ToString() + "...");

				if (remainingWidth > 0) // line is not wide enough; padding necessary
				{
					//logger.log("padding...", Logger.Mode.LOG);
					switch (Alignment)
					{
						case TextAlign.CENTER:
							line = TextUtils.PadText(line.ToString(), actualWidth, TextUtils.PadMode.BOTH, padChar);
							break;
						case TextAlign.RIGHT:
							line = TextUtils.PadText(line.ToString(), actualWidth, TextUtils.PadMode.LEFT, padChar);
							break;
						default:
							line = TextUtils.PadText(line.ToString(), actualWidth, TextUtils.PadMode.RIGHT, padChar);
							break;
					}
				}
				else if (remainingWidth < 0)
				{
					line = new StringBuilder(line.ToString());

					while (remainingWidth < 0)
					{
						remainingWidth += TextUtils.GetTextWidth(new string(new char[] { line[line.Length - 1] })) + 1;
						line.Remove(line.Length - 1, 1);
					}
				}
				//logger.log("aligned line is: {" + line + "}", Logger.Mode.LOG);
				//}
			}

			public string Render(int maxWidth, int maxHeight)
			{
				Initialize(maxWidth, maxHeight);
				StringBuilder result = new StringBuilder();
				foreach (StringBuilder line in GetLines(maxWidth, maxHeight))
				{
					new Logger("rendering line...");
					result.Append(line);
					result.Append("\n");
				}
				if (result.Length > 0)
					result.Remove(result.Length - 1, 1);
				return result.ToString();
			}

			public void ClearCache()
			{
				minHeightIsCached = false;
				minWidthIsCached = false;
				if (Parent != null)
					Parent?.ClearCache();
			}

			public static int? ResolveSize(string widthString, int max)
			{
				if (widthString == null)
					return null;

				widthString = widthString?.Trim();
				float fWidth;
				if (widthString[widthString.Length - 1] == '%' && Single.TryParse(widthString.Substring(0, widthString.Length - 1), out fWidth))
				{
					if (max < 0 || max == int.MaxValue)
						return null;
					return (int)(fWidth / 100f * Math.Max(0, max));
				}
				else
				{
					int iWidth;
					if (int.TryParse(widthString, out iWidth))
						return iWidth;
					return -1;
				}
			}
		}

		//EMBED SEScripts.XUI.BoxRenderer.InitializationState
	}
}