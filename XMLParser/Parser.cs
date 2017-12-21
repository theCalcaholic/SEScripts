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
		public static class XMLParser
		{

			public static string PackData(Dictionary<string, string> data)
			{
				StringBuilder dataString = new StringBuilder();
				foreach (string key in data.Keys)
				{
					dataString.Append(key + "=\"" + data[key] + "\" ");
				}
				return dataString.ToString();
			}

			public static string Sanitize(string xmlDefinition)
			{
				return xmlDefinition.Replace("\"", "\\\"").Replace("'", "\\'");
			}

			public static string UnescapeQuotes(string xmlDefinition)
			{
				return xmlDefinition.Replace("\\\"", "\"").Replace("\\'", "'");
			}

			public static int GetNextUnescaped(char[] needles, string haystack)
			{
				return GetNextUnescaped(needles, haystack, 0);
			}

			public static int GetNextUnescaped(char[] needles, string haystack, int start)
			{
				return GetNextUnescaped(needles, haystack, start, haystack.Length - start);
			}

			public static int GetNextUnescaped(char[] needles, string haystack, int start, int count)
			{
				int end = start + count - 1;
				int needlePos = haystack.IndexOfAny(needles, start, end - start + 1);
				while (needlePos > 0 && haystack[needlePos - 1] == '\\')
				{
					needlePos = haystack.IndexOfAny(needles, needlePos + 1, end - needlePos);
				}
				return needlePos;
			}

			public static int GetNextOutsideQuotes(char needle, string haystack)
			{
				return GetNextOutsideQuotes(new char[] { needle }, haystack);
			}

			public static int GetNextOutsideQuotes(char needle, string haystack, bool ignoreEscapedQuotes)
			{
				return GetNextOutsideQuotes(new char[] { needle }, haystack, ignoreEscapedQuotes);
			}

			public static int GetNextOutsideQuotes(char[] needles, string haystack)
			{
				return GetNextOutsideQuotes(needles, haystack, true);
			}

			public static int GetNextOutsideQuotes(char[] needles, string haystack, bool ignoreEscapedQuotes)
			{
				char[] quoteChars = new char[] { '\'', '"' };
				int needlePos = -1;
				int quoteEnd = -1;
				int quoteStart;
				while (needlePos == -1)
				{
					if (ignoreEscapedQuotes)
					{
						quoteStart = GetNextUnescaped(quoteChars, haystack, quoteEnd + 1);
					}
					else
					{
						quoteStart = haystack.IndexOfAny(quoteChars, quoteEnd + 1);
					}
					if (quoteStart == -1)
					{
						needlePos = GetNextUnescaped(needles, haystack, quoteEnd + 1);
					}
					else
					{
						needlePos = GetNextUnescaped(
							needles,
							haystack,
							quoteEnd + 1,
							quoteStart - quoteEnd - 1
						);
						if (ignoreEscapedQuotes)
						{
							quoteEnd = GetNextUnescaped(
								new char[] { haystack[quoteStart] },
								haystack,
								quoteStart + 1
							);
						}
						else
						{
							quoteEnd = haystack.IndexOf(haystack[quoteStart], quoteStart + 1);
						}
					}
				}
				return needlePos;
			}

			public static List<String> ParamString2List(string arg)
			{
				arg = arg.Trim() + " ";
				List<string> argList = new List<string>();
				char[] quoteChars = new char[] { '\'', '"' };
				int spacePos = -1;
				while (spacePos != arg.Length - 1)
				{
					arg = arg.Substring(spacePos + 1);
					spacePos = XMLParser.GetNextOutsideQuotes(new char[] { ' ', '\n' }, arg);
					argList.Add(arg.Substring(0, spacePos).Trim(quoteChars));
				}
				return argList;
			}

			public static Dictionary<string, string> GetXMLAttributes(string attributeString)
			{
				Dictionary<string, string> attributes = new Dictionary<string, string>();
				char[] quoteChars = new char[] { '\'', '"' };
				List<string> attributeList = ParamString2List(attributeString);
				int equalChar;
				foreach (string attribute in attributeList)
				{
					equalChar = attribute.IndexOf('=');
					if (equalChar == -1)
					{
						attributes[attribute.Substring(0).ToLower()] = "true";
					}
					else
					{
						attributes[attribute.Substring(0, equalChar).ToLower()] =
							attribute.Substring(equalChar + 1).Trim(quoteChars);
					}
				}
				return attributes;
			}

		}
		
	}
}