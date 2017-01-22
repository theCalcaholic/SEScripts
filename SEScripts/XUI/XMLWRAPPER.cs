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

using SEScripts.Lib;
using SEScripts.XUI.XML;

namespace SEScripts.XUI
{
    //EMBED SEScripts.Lib.Logger
    //EMBED SEScripts.Lib.Parser
    //EMBED SEScripts.Lib.TextUtils

    public static class XMLWRAPPER
    {
        public static Dictionary<string, Func<XML.XMLTree>> NodeRegister = new Dictionary<string, Func<XML.XMLTree>> {
        {"root", () => { return new XML.RootNode(); } },
        {"menu", () => { return new XML.Menu(); } },
        {"menuitem", () => { return new XML.MenuItem(); } },
        {"progressbar", () => { return new XML.ProgressBar(); } },
        {"container", () => { return new XML.Container(); } },
        {"hl", () => { return new XML.HorizontalLine(); } },
        {"uicontrols", () => { return new UIControls(); } },
        {"textinput", () => { return new TextInput(); } },
        {"submitbutton", () => { return new SubmitButton(); } },
        {"br", () => { return new Break(); } },
        {"space", () => { return new Space(); } },
        {"hidden", () => { return new Hidden(); } },
        {"hiddendata", () => { return new Hidden(); } },
        {"meta", () => { return new MetaNode(); } }
    };

        public static XMLTree CreateNode(string type)
        {
            Logger.debug("XML.CreateNode()");
            Logger.IncLvl();
            type = type.ToLower();
            if (NodeRegister.ContainsKey(type))
            {
                Logger.DecLvl();
                return NodeRegister[type]();
            }
            else
            {
                Logger.DecLvl();
                return new Generic(type);
            }
        }

        public static XMLTree ParseXML(string xmlString)
        {
            char[] spaceChars = { ' ', '\n' };
            Logger.debug("ParseXML");
            Logger.IncLvl();

            RootNode root = new RootNode();
            XMLTree currentNode = root;
            string type;
            Logger.debug("Enter while loop");
            while (xmlString.Length > 0)
            {
                if (xmlString[0] == '<')
                {
                    Logger.debug("found tag");
                    if (xmlString[1] == '/')
                    {
                        Logger.debug("tag is end tag");
                        int spacePos = xmlString.IndexOfAny(spaceChars);
                        int bracketPos = xmlString.IndexOf('>');
                        int typeLength = (spacePos == -1 ? bracketPos : Math.Min(spacePos, bracketPos)) - 2;
                        type = xmlString.Substring(2, typeLength).ToLower();
                        if (type != currentNode.Type)
                        {
                            throw new Exception("Invalid end tag ('" + type + "(!= " + currentNode.Type + "') found (node has been ended but not started)!");
                        }

                        currentNode = currentNode.GetParent() as XMLTree;
                        xmlString = xmlString.Substring(bracketPos + 1);
                    }
                    else
                    {
                        Logger.debug("tag is start tag");
                        int spacePos = xmlString.IndexOfAny(spaceChars);
                        int bracketPos = Parser.GetNextOutsideQuotes('>', xmlString);
                        int typeLength = (spacePos == -1 ? bracketPos : Math.Min(spacePos, bracketPos)) - 1;
                        type = xmlString.Substring(1, typeLength).ToLower().TrimEnd(new char[] { '/' });
                        XMLTree newNode = XMLWRAPPER.CreateNode(type);

                        if (newNode == null)
                        {
                            int closingBracketPos = xmlString.IndexOf("<");
                            int textLength = closingBracketPos == -1 ? xmlString.Length : closingBracketPos;
                            newNode = new XML.TextNode(xmlString.Substring(0, textLength).Trim());
                        }

                        Logger.debug("add new node of type '" + newNode.Type + "=" + type + "' to current node");
                        currentNode.AddChild(newNode);
                        Logger.debug("added new node to current node");
                        if (spacePos != -1 && spacePos < bracketPos)
                        {
                            string attrString = xmlString.Substring(typeLength + 2, bracketPos - typeLength - 2);
                            attrString = attrString.TrimEnd(new char[] { '/' });
                            Logger.debug("get xml attributes. attribute string: '" + attrString + "/" + xmlString + "'");
                            Dictionary<string, string> attributes =
                                Parser.GetXMLAttributes(attrString);
                            Logger.debug("got xml attributes");
                            foreach (string key in attributes.Keys)
                            {
                                newNode.SetAttribute(key, attributes[key]);
                            }
                        }

                        if (newNode.Type == "textnode" || bracketPos == -1 || xmlString[bracketPos - 1] != '/')
                        {
                            currentNode = newNode;
                        }

                        xmlString = xmlString.Substring(bracketPos + 1);
                    }
                }
                else
                {
                    int bracketPos = xmlString.IndexOf("<");
                    int textLength = bracketPos == -1 ? xmlString.Length : bracketPos;
                    XMLTree newNode = new XML.TextNode(xmlString.Substring(0, textLength).Trim());
                    if (newNode.Render(0) != null)
                    {
                        currentNode.AddChild(newNode);
                    }
                    xmlString = bracketPos == -1 ? "" : xmlString.Substring(bracketPos);
                }
            }

            Logger.debug("parsing finished");
            Logger.DecLvl();
            return root;
        }

        //EMBED SEScripts.XUI.XML.RootNode
        //EMBED SEScripts.XUI.XML.TextNode
        //EMBED SEScripts.XUI.XML.Route
        //EMBED SEScripts.XUI.XML.Generic

        //EMBED SEScripts.XUI.XML.UIController
        //EMBED SEScripts.XUI.XML.Menu
        //EMBED SEScripts.XUI.XML.MenuItem
        //EMBED SEScripts.XUI.XML.ProgressBar
        //EMBED SEScripts.XUI.XML.Container
        //EMBED SEScripts.XUI.XML.HorizontalLine
        //EMBED SEScripts.XUI.XML.UIControls
        //EMBED SEScripts.XUI.XML.TextInput
        //EMBED SEScripts.XUI.XML.SubmitButton
        //EMBED SEScripts.XUI.XML.Break
        //EMBED SEScripts.XUI.XML.Space
        //EMBED SEScripts.XUI.XML.Hidden
        //EMBED SEScripts.XUI.XML.HiddenData
        //EMBED SEScripts.XUI.XML.MetaNode

        //EMBED SEScripts.XUI.XML.DataStore
    }
}
