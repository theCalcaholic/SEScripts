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
	partial class Program
	{
		public class XUIController : XMLParentNode
		{
			public XMLTree ui;
			public Stack<XMLTree> UIStack;
			public string Type;
			bool UserInputActive;
			IMyTerminalBlock UserInputSource;
			TextInputMode UserInputMode;
			List<XMLTree> UserInputBindings;
			string InputDataCache;
			public bool HasUserInputBindings
			{
				get { return UserInputActive && UserInputSource != null && UserInputBindings.Count > 0; }
				set { }
			}

			public enum TextInputMode { PUBLIC_TEXT, CUSTOM_DATA }
			
			public XUIController(XMLTree rootNode)
			{
				Type = "CTRL";

				UIStack = new Stack<XMLTree>();
				UserInputBindings = new List<XMLTree>();
				UserInputActive = false;
				InputDataCache = "";
				ui = rootNode;
				ui.SetParent(this);
				if (ui.GetAttribute("fontfamily") == "Monospace")
					TextUtils.SelectFont(TextUtils.FONT.MONOSPACE);
				else
					TextUtils.SelectFont(TextUtils.FONT.DEFAULT);
				if (GetSelectedNode() == null && ui.IsSelectable())
				{
					ui.SelectFirst();
				}

				CollectUserInputBindings();
			}

			public static XUIController FromXML(string xml)
			{
				//Logger.debug("UIController FromXMLString()");
				//Logger.IncLvl();
				XMLTree rootNode = XML.ParseXML(xml);
				//Logger.DecLvl();
				return new XUIController(rootNode);
			}

			public void ApplyScreenProperties(IMyTextPanel screen)
			{

				if (ui.GetAttribute("fontcolor") != null)
				{
					string colorString = ui.GetAttribute("fontcolor");
					colorString = "FF" +
					colorString.Substring(colorString.Length - 2, 2)
					+ colorString.Substring(colorString.Length - 4, 2)
					+ colorString.Substring(colorString.Length - 6, 2);
					Color fontColor = new Color(
						uint.Parse(colorString, System.Globalization.NumberStyles.AllowHexSpecifier)
					);
					screen.SetValue<Color>("FontColor", fontColor);
				}

				if (ui.GetAttribute("fontsize") != null)
				{
					screen.SetValue<Single>("FontSize", Single.Parse(ui.GetAttribute("fontsize")));
				}

				if (ui.GetAttribute("backgroundcolor") != null)
				{
					string colorString = ui.GetAttribute("backgroundcolor");
					colorString = "FF" +
					colorString.Substring(colorString.Length - 2, 2)
					+ colorString.Substring(colorString.Length - 4, 2)
					+ colorString.Substring(colorString.Length - 6, 2);
					Color fontColor = new Color(
					uint.Parse(colorString, System.Globalization.NumberStyles.AllowHexSpecifier));
					screen.SetValue<Color>("BackgroundColor", fontColor);
				}

				var fonts = new List<string>();
				screen.GetFonts(fonts);
				var selectedFont = ui.GetAttribute("fontfamily")?.ToLower();
				var newFont = fonts.Find(x => x.ToLower() == selectedFont);
				if (newFont != null)
				{
					screen.Font = newFont;
				}

			}

			public void Call(List<string> parameters)
			{
				//Logger.debug("UIController.Main()");
				//Logger.IncLvl();

				switch (parameters[0])
				{
					case "key":
						XMLTree selectedNode = GetSelectedNode();
						if (selectedNode != null)
						{
							selectedNode.KeyPress(parameters[1].ToUpper());
						}

						break;
					case "refresh":
						string refresh = ui.GetAttribute("refresh");
						if (refresh != null)
						{
							FollowRoute(new XUIRoute(refresh));
						}
						UpdateUserInput();
						break;
					case "revert":
						RevertUI();
						break;
					default:
						break;
				}

				//Logger.DecLvl();
				return;
			}

			public void LoadXML(string xmlString)
			{
				LoadUI(XML.ParseXML(xmlString));
			}

			public void LoadUI(XMLTree rootNode)
			{
				//Logger.debug("UIController: LoadUI():");
				//Logger.IncLvl();
				if (ui.GetAttribute("historydisabled") == null || ui.GetAttribute("historydisabled") != "true")
				{
					UIStack.Push(ui);
				}

				if (rootNode.GetAttribute("revert") != null && rootNode.GetAttribute("revert") == "true")
				{
					RevertUI();
				}
				else
				{
					ui = rootNode;
					ui.SetParent(this);
				}
				UserInputBindings = new List<XMLTree>();
				CollectUserInputBindings();

				//Logger.DecLvl();
			}

			public void ClearUIStack()
			{
				UIStack = new Stack<XMLTree>();
			}

			public void RevertUI()
			{
				//Logger.log("UIController: RevertUI():");
				//Logger.IncLvl();
				if (UIStack.Count == 0)
				{
					//Logger.log("Error: Can't revert: UI stack is empty.");
					//Logger.DecLvl();
					return;
				}

				ui = UIStack.Pop();
				ui.SetParent(this);
				//Logger.DecLvl();
			}

			public string Render(int screenWidth, int screenHeight)
			{
				if (ui.GetAttribute("fontfamily") == "Monospace")
					TextUtils.SelectFont(TextUtils.FONT.MONOSPACE);
				else
					TextUtils.SelectFont(TextUtils.FONT.DEFAULT);
				return ui.Render(screenWidth, screenHeight);
			}

			public string Render()
			{
				return Render(int.MaxValue, int.MaxValue);
			}

			public void RenderTo(IMyTextPanel panel)
			{
				//Logger.debug("UIController.RenderTo()");
				//Logger.IncLvl();
				int panelWidth = int.MaxValue;
				string panelType = panel.BlockDefinition.SubtypeId;
				//Logger.debug("Type: " + panelType);

				if (panelType == "LargeTextPanel" || panelType == "SmallTextPanel")
				{
					panelWidth = 658;
				}
				else if (panelType == "LargeLCDPanel" || panelType == "SmallLCDPanel")
				{
					panelWidth = 658;
				}
				else if (panelType == "SmallLCDPanelWide" || panelType == "LargeLCDPanelWide")
				{
					panelWidth = 1316;
				}
				else if (panelType == "LargeBlockCorner_LCD_1" || panelType == "LargeBlockCorner_LCD_2"
					|| panelType == "SmallBlockCorner_LCD_1" || panelType == "SmallBlockCorner_LCD_2")
				{ }
				else if (panelType == "LargeBlockCorner_LCD_Flat_1" || panelType == "LargeBlockCorner_LCD_Flat_2"
					|| panelType == "SmallBlockCorner_LCD_Flat_1" || panelType == "SmallBlockCorner_LCD_Flat_2")
				{ }

				int width = panelWidth == int.MaxValue ? int.MaxValue : (int)((panelWidth) / panel.GetValue<Single>("FontSize"));
				//TODO: Get height of screen
				int height = 20;
				ApplyScreenProperties(panel);
				
				string text = Render(width, height);
				panel.WritePublicText(text);
			}

			public void KeyPress(string keyCode)
			{
				//Logger.debug("UIController: KeyPress():");
				//Logger.IncLvl();
				switch (keyCode)
				{
					case "LEFT/ABORT":
						RevertUI();
						break;
				}

				//Logger.DecLvl();
			}

			public XMLTree GetSelectedNode()
			{
				//Logger.debug("UIController: GetSelectedNode():");
				//Logger.IncLvl();
				XMLTree sibling = ui.GetSelectedSibling();
				//Logger.DecLvl();
				return sibling;
			}

			public XMLTree GetNode(Func<XMLTree, bool> filter)
			{
				//Logger.debug("UIController: GetNode()");
				//Logger.IncLvl();
				//Logger.DecLvl();
				return ui.GetNode(filter);
			}

			public List<XMLTree> GetAllNodes(Func<XMLTree, bool> filter)
			{
				//Logger.debug("UIController: GetAllNodes()");
				//Logger.IncLvl();
				//Logger.DecLvl();
				return ui.GetAllNodes(filter);
			}

			public void UpdateSelectability(XMLTree child) { }

			public void FollowRoute(XUIRoute route)
			{
				//Logger.debug("UIController: FollowRoute():");
				//Logger.IncLvl();
				route.Follow(this);
				//Logger.DecLvl();
			}

			public XMLParentNode GetParent()
			{
				return null;
			}

			public Dictionary<string, string> GetValues()
			{
				//Logger.debug("UIController.GetValues()");
				//Logger.IncLvl();
				return GetValues((node) => true);
			}

			public Dictionary<string, string> GetValues(Func<XMLTree, bool> filter)
			{
				//Logger.debug("UIController.GetValues()");
				//Logger.IncLvl();
				if (ui == null)
				{
					//Logger.DecLvl();
					return null;
				}

				//Logger.DecLvl();
				return ui.GetValues(filter);
			}

			public string GetPackedValues(Func<XMLTree, bool> filter)
			{
				return XMLParser.PackData(GetValues(filter)).ToString();
			}

			public void DetachChild(XMLTree xml)
			{
				if (xml == ui)
				{
					ui = null;
				}
			}

			public string GetPackedValues()
			{
				//Logger.debug("UIController.GetPackedValues()");
				//Logger.IncLvl();
				//Logger.DecLvl();
				return GetPackedValues(node => true);
			}

			public bool SelectNext()
			{
				return ui.SelectNext();
			}

			public void SetUserInputSource(IMyTerminalBlock sourceBlock, TextInputMode mode)
			{
				if (mode == TextInputMode.PUBLIC_TEXT && (sourceBlock as IMyTextPanel) == null)
				{
					throw new Exception("Only Text Panels can be used as user input if PUBLIC_TEXT mode is selected!");
				}
				UserInputSource = sourceBlock;
				UserInputMode = mode;
			}

			public void EnableUserInput()
			{
				UserInputActive = true;
			}

			public void DisableUserInput()
			{
				UserInputActive = false;
			}

			public void RegisterInputBinding(XMLTree node)
			{
				UserInputBindings.Add(node);
			}

			public bool UpdateUserInput()
			{
				//using (new Logger("UIController.RefreshUserInput()", Logger.Mode.LOG))
				//{
				//Logger.IncLvl();
				if (!UserInputActive || UserInputSource == null)
				{
					return false;
				}

				// get input data
				string inputData = null;
				switch (UserInputMode)
				{
					case TextInputMode.CUSTOM_DATA:
						inputData = UserInputSource?.CustomData;
						break;
					case TextInputMode.PUBLIC_TEXT:
						inputData = (UserInputSource as IMyTextPanel)?.GetPublicText();
						break;
				}
				bool inputHasChanged = true;
				if (inputData == null || inputData == InputDataCache)
				{
					inputHasChanged = false;
				}

				//Logger.debug("input has " + (inputHasChanged ? "" : "not ") + "changed");
				//Logger.debug("Iterating input bindings (" + UserInputBindings.Count + " bindings registered).");

				// update ui input bindings
				string binding;
				string fieldValue;
				foreach (XMLTree node in UserInputBindings)
				{
					binding = node.GetAttribute("inputbinding");
					if (binding != null)
					{
						//Logger.debug("binding found at " + node.Type + " node for field: " + binding);
						fieldValue = node.GetAttribute(binding.ToLower());
						//Logger.debug("field is " + (fieldValue ?? "EMPTY") + ".");
						if (!inputHasChanged && fieldValue != null && fieldValue != InputDataCache)
						{
							//Logger.debug("applying field value: " + fieldValue);
							inputData = fieldValue;
							inputHasChanged = true;
						}
						else if (inputHasChanged)
						{
							//Logger.debug("Updating field value to input: " + inputData);
							node.SetAttribute(binding.ToLower(), inputData);
						}
					}
				}
				if (inputHasChanged)
				{
					InputDataCache = inputData;
				}

				// update user input device
				switch (UserInputMode)
				{
					case TextInputMode.CUSTOM_DATA:
						if (UserInputSource != null)
						{
							UserInputSource.CustomData = InputDataCache;
						}
						break;
					case TextInputMode.PUBLIC_TEXT:
						(UserInputSource as IMyTextPanel)?.WritePublicText(InputDataCache);
						break;
				}

				return inputHasChanged;
				//}
			}

			private void CollectUserInputBindings()
			{

				XMLTree node;
				Queue<XMLParentNode> nodes = new Queue<XMLParentNode>();
				nodes.Enqueue(ui);
				while (nodes.Count != 0)
				{
					node = nodes.Dequeue() as XMLTree;
					if (!node.HasUserInputBindings)
					{
						//logger.log("node has no userinputbindings", Logger.Mode.LOG);
					}
					if (node != null && node.HasUserInputBindings)
					{
						//logger.log("Checking " + node.Type + " node...", Logger.Mode.LOG);
						for (int i = 0; i < node.NumberOfChildren; i++)
						{
							nodes.Enqueue(node.GetChild(i));
						}
						if (node.GetAttribute("inputbinding") != null)
						{
							RegisterInputBinding(node);

						}
					}
				}
			}

		}

	}
}