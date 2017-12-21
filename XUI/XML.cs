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
		public class XML
		{
			public static Dictionary<string, Func<XMLTree>> NodeRegister = new Dictionary<string, Func<XMLTree>> {
				{"root", () => { return new XMLRootNode(); } },
				{"menu", () => { return new XMLMenu(); } },
				{"menuitem", () => { return new XMLMenuItem(); } },
				{"progressbar", () => { return new XMLProgressBar(); } },
				{"hl", () => { return new XMLHorizontalLine(); } },
				{"vl", () => { return new XMLVerticalLine(); } },
				{"uicontrols", () => { return new XMLUIControls(); } },
				{"textinput", () => { return new XMLTextInput(); } },
				{"submitbutton", () => { return new XMLSubmitButton(); } },
				{"br", () => { return new XMLLineBreak(); } },
				{"space", () => { return new XMLSpace(); } },
				{"hidden", () => { return new XMLHidden(); } },
				{"hiddendata", () => { return new XMLHidden(); } },
				{"meta", () => { return new XMLMetaNode(); } }
			};

			public static XMLTree CreateNode(string type)
			{
				//Logger.debug("XML.CreateNode()"); 
				//Logger.IncLvl();
				type = type.ToLower();
				if (NodeRegister.ContainsKey(type))
				{
					//Logger.DecLvl();
					return NodeRegister[type]();
				}
				else
				{
					//Logger.DecLvl();
					return new XMLGenericNode(type);
				}
			}

			public static XMLTree ParseXML(string xmlString)
			{
				char[] spaceChars = { ' ', '\n' };
				//Logger.debug("ParseXML");
				//Logger.IncLvl();

				XMLRootNode root = new XMLRootNode();
				XMLTree currentNode = root;
				string type;
				////Logger.debug("Enter while loop");
				while (xmlString.Length > 0)
				{
					if (xmlString[0] == '<')
					{
						////Logger.debug("found tag");
						if (xmlString[1] == '/')
						{
							////Logger.debug("tag is end tag");
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
							int spacePos = xmlString.IndexOfAny(spaceChars);
							int bracketPos = XMLParser.GetNextOutsideQuotes('>', xmlString);
							int typeLength = (spacePos == -1 ? bracketPos : Math.Min(spacePos, bracketPos)) - 1;
							type = xmlString.Substring(1, typeLength).ToLower().TrimEnd(new char[] { '/' });
							XMLTree newNode = XML.CreateNode(type);

							if (newNode == null)
							{
								int closingBracketPos = xmlString.IndexOf("<");
								int textLength = closingBracketPos == -1 ? xmlString.Length : closingBracketPos;
								newNode = new XMLTextNode(xmlString.Substring(0, textLength).Trim());
							}
							
							currentNode.AddChild(newNode);
							if (spacePos != -1 && spacePos < bracketPos)
							{
								string attrString = xmlString.Substring(typeLength + 2, bracketPos - typeLength - 2);
								attrString = attrString.TrimEnd(new char[] { '/' });
								Dictionary<string, string> attributes =
									XMLParser.GetXMLAttributes(attrString);
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
						XMLTree newNode = new XMLTextNode(xmlString.Substring(0, textLength).Trim());
						if (true || newNode.GetRenderBox(-1, -1) != null)
						{
							currentNode.AddChild(newNode);
						}
						xmlString = bracketPos == -1 ? "" : xmlString.Substring(bracketPos);
					}
				}

				return root;

			}
		}
	}
	
}
