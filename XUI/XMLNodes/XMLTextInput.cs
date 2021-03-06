﻿using Sandbox.Game.EntityComponents;
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
	partial class Program
	{
		public class XMLTextInput : XMLTree
		{
			int CursorPosition;

			public XMLTextInput()
			{
				//Logger.log("TextInput constructor()");
				//Logger.IncLvl();
				Type = "textinput";
				Selectable = true;
				CursorPosition = -1;
				PreventDefault("LEFT/ABORT");
				PreventDefault("RIGHT/SUBMIT");
				SetAttribute("maxlength", "10");
				SetAttribute("value", "");
				SetAttribute("allowedchars", " a-z0-9");
				//Logger.DecLvl();
			}

			public override void OnKeyPressed(string keyCode)
			{
				switch (keyCode)
				{
					case "LEFT/ABORT":
						DecreaseCursorPosition();
						break;
					case "RIGHT/SUBMIT":
						IncreaseCursorPosition();
						break;
					case "UP":
						DecreaseLetter();
						break;
					case "DOWN":
						IncreaseLetter();
						break;
					default:
						base.OnKeyPressed(keyCode);
						break;
				}
			}

			public override void SetAttribute(string key, string value)
			{
				if (key == "allowedchars")
				{
					if (!System.Text.RegularExpressions.Regex.IsMatch(value,
						@"([^-\\]-[^-\\]|[^-\\]|\\-|\\\\)*"))
					{
						throw new Exception("Invalid format of allowed characters!");
					}

				}
				//if (key == "value")
				//using (new Logger("set value: " + value)) { }
				base.SetAttribute(key, value);
			}

			private void IncreaseLetter()
			{
				//Logger.log("TextInput.IncreaseLetter()");
				//Logger.IncLvl();
				if (CursorPosition == -1)
				{
					return;
				}
				char[] value = GetAttribute("value").ToCharArray();
				char letter = value[CursorPosition];
				string[] charSets = GetAllowedCharSets();
				for (int i = 0; i < charSets.Length; i++)
				{
					if ((charSets[i].Length == 1 && charSets[i][0] == value[CursorPosition])
						|| (charSets[i].Length == 3 && charSets[i][2] == value[CursorPosition]))
					{
						//Logger.log("letter outside class, setting to: " + charSets[i == 0 ? charSets.Length - 1 : i - 1][0] + ". (chars[" + ((i + 1) % charSets.Length) + "])");
						value[CursorPosition] = charSets[(i + 1) % charSets.Length][0];
						SetAttribute("value", new string(value));
						//Logger.DecLvl();
						return;
					}
				}

				//Logger.log("letter inside class, setting to: " + (char)(((int)value[CursorPosition]) + 1));
				value[CursorPosition] = (char)(((int)value[CursorPosition]) + 1);
				SetAttribute("value", new string(value));
				//Logger.DecLvl();
			}

			private void DecreaseLetter()
			{
				//Logger.log("TextInput.DecreaseLetter()");
				//Logger.IncLvl();
				if (CursorPosition == -1)
				{
					return;
				}
				char[] value = GetAttribute("value").ToCharArray();
				char[] chars = GetAttribute("allowedchars").ToCharArray();
				string[] charSets = GetAllowedCharSets();
				for (int i = 0; i < charSets.Length; i++)
				{
					if (charSets[i][0] == value[CursorPosition])
					{
						int index = (i == 0 ? charSets.Length - 1 : i - 1);
						//Logger.log("letter outside class, setting to: " + charSets[index][charSets[index].Length - 1] + ". (chars[" + (index) + "])");
						value[CursorPosition] = charSets[index][charSets[index].Length - 1];
						SetAttribute("value", new string(value));
						return;
					}
				}
				//Logger.log("letter inside class, setting to: " + (char)(((int)value[CursorPosition]) - 1));
				value[CursorPosition] = (char)(((int)value[CursorPosition]) - 1);
				SetAttribute("value", new string(value));
				//Logger.DecLvl();
			}

			private string[] GetAllowedCharSets()
			{

				string charString = GetAttribute("allowedchars");
				System.Text.RegularExpressions.MatchCollection matches =
					System.Text.RegularExpressions.Regex.Matches(charString, @"[^-\\]-[^-\\]|[^-\\]|\\-|\\\\");
				string[] charSets = new string[matches.Count];
				int i = 0;
				foreach (System.Text.RegularExpressions.Match match in matches)
				{
					string matchString = match.ToString();
					if (matchString == "\\-")
					{
						charSets[i] = "-";
					}
					else if (matchString == "\\\\")
					{
						charSets[i] = "\\";
					}
					else
					{
						charSets[i] = matchString;
					}
					i++;
				}
				//P.Echo("Char sets found: " + string.Join(",", charSets));
				return charSets;
			}

			private void IncreaseCursorPosition()
			{
				if (CursorPosition < Single.Parse(GetAttribute("maxlength")) - 1)
				{
					CursorPosition++;
				}
				else
				{
					CursorPosition = 0;
					DecreaseCursorPosition();
					KeyPress("DOWN");
				}
				if (CursorPosition != -1)
				{
					PreventDefault("UP");
					PreventDefault("DOWN");
				}
				if (CursorPosition >= GetAttribute("value").Length)
				{
					string[] charSets = GetAllowedCharSets();
					SetAttribute("value", GetAttribute("value") + charSets[0][0]);
				}
			}

			private void DecreaseCursorPosition()
			{
				if (CursorPosition > -1)
				{
					CursorPosition--;
				}
				if (CursorPosition == -1)
				{
					AllowDefault("UP");
					AllowDefault("DOWN");
				}
			}

			/*protected override void RenderText(ref List<string> segments, int width, int availableWidth)
			{
				string value = GetAttribute("value");
				if (CursorPosition != -1)
				{
					value = value.Substring(0, CursorPosition)
						+ "|" + value.Substring(CursorPosition, 1) + "|"
						+ value.Substring(CursorPosition + 1);
				}
				else if (value.Length == 0)
				{
					value = "_" + value;
				}
				segments.Add((IsSelected() ? new string(new char[] { (char)187 }) : "  ") + " " + value);

			}*/

			public override IRenderBox GetRenderBox(int containerWidth, int containerHeight)
			{
				//using (Logger logger = new Logger("TextInput.GetRenderCache(int)", Logger.Mode.LOG))
				//{
				RenderBoxTree cache = new RenderBoxTree();
				UpdateRenderCacheProperties(cache, containerWidth, containerHeight);
				RenderBoxLeaf content = new RenderBoxLeaf();
				cache.type = Type;
				cache.Flow = IRenderBox.FlowDirection.HORIZONTAL;
				cache.Add(content);
				content.Add((IsSelected() ? new string(new char[] { (char)187 }) : " ") + " ");
				content.MinWidth = TextUtils.GetTextWidth(new string(new char[] { (char)187, ' ' }));

				string value = GetAttribute("value");
				//logger.log("value: " + value, Logger.Mode.LOG);
				if (CursorPosition != -1)
				{
					content.Add(value.Substring(0, CursorPosition));
					content.Add("|");
					content.Add(value.Substring(CursorPosition, 1));
					content.Add("|");
					content.Add(value.Substring(CursorPosition + 1));
				}
				else
				{
					if (value.Length == 0)
						content.Add("_");
					content.Add(value);
				}
				/*
                for(int i = 0; i < cache.Count; i++)
                {
                    cache[i].MaxWidth = cache[i].MinWidth;
                }*/
				return cache;
			}
			//}
		}


		//EMBED SEScripts.XUI.XML.DataStore

	}
}