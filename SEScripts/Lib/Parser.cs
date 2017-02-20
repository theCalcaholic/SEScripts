using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;

using VRageMath;
using VRage.Game;
using VRage.Collections;
using Sandbox.ModAPI.Ingame;
using VRage.Game.Components;
using VRage.Game.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using Sandbox.Game.EntityComponents;
using SpaceEngineers.Game.ModAPI.Ingame;
using VRage.Game.ObjectBuilders.Definitions;

namespace SEScripts.Lib
{
    public static class Parser
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
            Logger.debug("Parser.Sanitize()");
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
            //Logger.debug("GetNextUnescaped():");
            //Logger.IncLvl();
            int end = start + count - 1;
            int needlePos = haystack.IndexOfAny(needles, start, end - start + 1);
            while (needlePos > 0 && haystack[needlePos - 1] == '\\')
            {
                needlePos = haystack.IndexOfAny(needles, needlePos + 1, end - needlePos);
            }
            //Logger.DecLvl();
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

        public static int GetNextOutsideQuotes(
            char[] needles,
            string haystack,
            bool ignoreEscapedQuotes
        )
        {
            //Logger.debug("GetNextOutsideQuotes():");
            //Logger.IncLvl();
            char[] quoteChars = new char[] { '\'', '"' };
            int needlePos = -1;
            int quoteEnd = -1;
            int quoteStart;
            //Logger.debug("needle: |" + new string(needles) + "|");
            //Logger.debug("haystack: |" + haystack + "|");
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
                //Logger.debug("quoteStart position: " + quoteStart.ToString()
                //    + ", quoteEnd position: " + quoteEnd.ToString());
                if (quoteStart == -1)
                {
                    //Logger.debug("searching for needle in:: " + haystack.Substring(quoteEnd + 1));
                    needlePos = GetNextUnescaped(needles, haystack, quoteEnd + 1);
                }
                else
                {
                    //Logger.debug("found start quote: " + haystack.Substring(quoteStart));
                    //Logger.debug("searching for needle in: "
                    //    + haystack.Substring(quoteEnd + 1, quoteStart - quoteEnd));
                    needlePos = GetNextUnescaped(
                        needles,
                        haystack,
                        quoteEnd + 1,
                        quoteStart - quoteEnd - 1
                    );
                    if (needlePos != -1)
                    {
                        Logger.debug("found needle: " + haystack.Substring(needlePos));
                    }
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
                    //Logger.debug("found end quote: " + haystack.Substring(quoteEnd));
                }
            }
            //Logger.debug("yay!");
            //Logger.DecLvl();
            return needlePos;
        }

        public static List<String> ParamString2List(string arg)
        {
            //Logger.debug("Parser.ParamString2List()");
            //Logger.IncLvl();

            arg = arg.Trim() + " ";
            List<string> argList = new List<string>();
            char[] quoteChars = new char[] { '\'', '"' };
            int spacePos = -1;
            while (spacePos != arg.Length - 1)
            {
                arg = arg.Substring(spacePos + 1);
                spacePos = Parser.GetNextOutsideQuotes(new char[] { ' ', '\n' }, arg);
                argList.Add(arg.Substring(0, spacePos).Trim(quoteChars));
            }
            //Logger.DecLvl();
            return argList;
        }

        public static Dictionary<string, string> GetXMLAttributes(string attributeString)
        {
            //Logger.debug("GetXMLAttributes():");
            //Logger.IncLvl();
            //Logger.debug("attribute string is: <" + attributeString + ">");
            Dictionary<string, string> attributes = new Dictionary<string, string>();
            char[] quoteChars = new char[] { '\'', '"' };
            List<string> attributeList = ParamString2List(attributeString);
            int equalSign;
            foreach (string attribute in attributeList)
            {
                equalSign = attribute.IndexOf('=');
                if (equalSign == -1)
                {
                    attributes[attribute.Substring(0).ToLower()] = "true";
                }
                else
                {
                    attributes[attribute.Substring(0, equalSign).ToLower()] =
                        attribute.Substring(equalSign + 1).Trim(quoteChars);
                }
            }
            //Logger.debug("attribute dict: {");
            //Logger.IncLvl();
            //foreach (string key in attributes.Keys)
            //{
                //Logger.debug(key + ": " + attributes[key]);
            //}
            //Logger.debug("}");
            //Logger.DecLvl();
            //Logger.DecLvl();
            return attributes;
        }

    }

    //EMBED SEScripts.Lib.Logger
}
