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

using SEScripts.Lib.LoggerNS;
using SEScripts.Lib;

namespace SEScripts.XUI.XML
{
    public class UIController : XMLParentNode
    {
        XMLTree ui;
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

        public enum FONT
        {
            Debug, Red, Green, Blue, White, DarkBlue, UrlNormal, UrlHighlight, ErrorMessageBoxCaption, ErrorMessageBoxText,
            InfoMessageBoxCaption, InfoMessageBoxText, ScreenCaption, GameCredits, LoadingScreen, BuildInfo, BuildInfoHighlight,
            Monospace, MONO = Monospace, DEFAULT = Debug
        }

        Dictionary<FONT, long> Fonts = new Dictionary<FONT, long> {
            {FONT.Debug, 151057691},
            {FONT.Red, -795103743 },
            {FONT.Green, -161094011 },
            {FONT.Blue, 1920284339 },
            {FONT.White, 48665683 },
            {FONT.DarkBlue, 1919824171 },
            {FONT.UrlNormal, 992097699 },
            {FONT.UrlHighlight, -807552222 },
            {FONT.ErrorMessageBoxCaption, 1458347610 },
            {FONT.ErrorMessageBoxText, 895781166 },
            {FONT.InfoMessageBoxCaption, 837834442 },
            {FONT.InfoMessageBoxText, 1833612699 },
            {FONT.ScreenCaption, 1216738022 },
            {FONT.GameCredits, -1859174863 },
            {FONT.LoadingScreen, 741958017 },
            {FONT.BuildInfo, 1184185815 },
            {FONT.BuildInfoHighlight, -270950170 },
            {FONT.Monospace, 1147350002 }
        };

        public UIController(XMLTree rootNode)
        {
            Logger.debug("UIController constructor()");
            Logger.IncLvl();
            Type = "CTRL";

            UIStack = new Stack<XMLTree>();
            UserInputBindings = new List<XMLTree>();
            UserInputActive = false;
            InputDataCache = "";
            ui = rootNode;
            ui.SetParent(this);
            if (GetSelectedNode() == null && ui.IsSelectable())
            {
                ui.SelectFirst();
            }

            CollectUserInputBindings();

            Logger.DecLvl();
        }

        public static UIController FromXML(string xml)
        {
            Logger.debug("UIController FromXMLString()");
            Logger.IncLvl();
            XMLTree rootNode = XMLWRAPPER.ParseXML(xml);
            Logger.DecLvl();
            return new UIController(rootNode);
        }

        public void ApplyScreenProperties(IMyTextPanel screen)
        {
            Logger.debug("UIController.ApplyScreenProperties()");
            Logger.IncLvl();
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

            if(ui.GetAttribute("fontfamily") != null)
            {
                string font = ui.GetAttribute("font");
                FONT fontName;
                long fontValue;
                if(Enum.TryParse<FONT>(font, out fontName))
                {
                    screen.SetValue<long>("Font", Fonts[fontName]);
                }
                else if(long.TryParse(font, out fontValue))
                {
                    screen.SetValue<long>("Font", fontValue);
                }
                
            }

            Logger.DecLvl();
        }

        public void Call(List<string> parameters)
        {
            Logger.debug("UIController.Main()");
            Logger.IncLvl();

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
                        FollowRoute(new Route(refresh));
                    }
                    break;
                case "revert":
                    RevertUI();
                    break;
                default:
                    break;
            }

            Logger.DecLvl();
            return;
        }

        public void LoadXML(string xmlString)
        {
            LoadUI(XMLWRAPPER.ParseXML(xmlString));
        }

        public void LoadUI(XMLTree rootNode)
        {
            Logger.debug("UIController: LoadUI():");
            Logger.IncLvl();
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

            Logger.DecLvl();
        }

        public void ClearUIStack()
        {
            UIStack = new Stack<XMLTree>();
        }

        public void RevertUI()
        {
            Logger.log("UIController: RevertUI():");
            Logger.IncLvl();
            if (UIStack.Count == 0)
            {
                Logger.log("Error: Can't revert: UI stack is empty.");
                Logger.DecLvl();
                return;
            }

            ui = UIStack.Pop();
            ui.SetParent(this);
            Logger.DecLvl();
        }

        public string Render()
        {
            Logger.debug("UIController: Render():");
            Logger.IncLvl();
            Logger.DecLvl();
            return ui.Render(0);
        }

        public void RenderTo(IMyTextPanel panel)
        {
            Logger.debug("UIController.RenderTo()");
            Logger.IncLvl();
            int panelWidth = 0;
            string panelType = panel.BlockDefinition.SubtypeId;
            Logger.debug("Type: " + panelType);

            if (panelType == "LargeTextPanel" || panelType == "SmallTextPanel")
            {
                panelWidth = 658;
            }
            else if(panelType == "LargeLCDPanel" || panelType == "SmallLCDPanel")
            {
                panelWidth = 658;
            }
            else if(panelType == "SmallLCDPanelWide" || panelType == "LargeLCDPanelWide")
            {
                panelWidth = 1316;
            }
            else if(panelType == "LargeBlockCorner_LCD_1" || panelType == "LargeBlockCorner_LCD_2"
                || panelType == "SmallBlockCorner_LCD_1" || panelType == "SmallBlockCorner_LCD_2")
            { }
            else if(panelType == "LargeBlockCorner_LCD_Flat_1" || panelType == "LargeBlockCorner_LCD_Flat_2"
                || panelType == "SmallBlockCorner_LCD_Flat_1" || panelType == "SmallBlockCorner_LCD_Flat_2")
            { }

            int width = (int)(((float)panelWidth) / panel.GetValue<Single>("FontSize"));
            if (panel.GetValue<long>("Font") == Fonts[FONT.MONO])
            {
                TextUtils.SelectFont(TextUtils.FONT.MONOSPACE);
            }
            else
            {
                TextUtils.SelectFont(TextUtils.FONT.DEFAULT);
            }
            Logger.log("Font configured...");

            Logger.debug("font size: " + panel.GetValue<Single>("FontSize").ToString());
            Logger.debug("resulting width: " + width.ToString());
            string text = ui.Render(width);
            Logger.debug("rendering <" + text);
            panel.WritePublicText(text);
            Logger.DecLvl();
        }

        public void KeyPress(string keyCode)
        {
            Logger.debug("UIController: KeyPress():");
            Logger.IncLvl();
            switch (keyCode)
            {
                case "LEFT/ABORT":
                    RevertUI();
                    break;
            }

            Logger.DecLvl();
        }

        public XMLTree GetSelectedNode()
        {
            Logger.debug("UIController: GetSelectedNode():");
            Logger.IncLvl();
            XMLTree sibling = ui.GetSelectedSibling();
            Logger.DecLvl();
            return sibling;
        }

        public XMLTree GetNode(Func<XMLTree, bool> filter)
        {
            Logger.debug("UIController: GetNode()");
            Logger.IncLvl();
            Logger.DecLvl();
            return ui.GetNode(filter);
        }

        public List<XMLTree> GetAllNodes(Func<XMLTree, bool> filter)
        {
            Logger.debug("UIController: GetAllNodes()");
            Logger.IncLvl();
            Logger.DecLvl();
            return ui.GetAllNodes(filter);
        }

        public void UpdateSelectability(XMLTree child) { }

        public void FollowRoute(Route route)
        {
            Logger.debug("UIController: FollowRoute():");
            Logger.IncLvl();
            route.Follow(this);
            Logger.DecLvl();
        }

        public XMLParentNode GetParent()
        {
            return null;
        }

        public Dictionary<string, string> GetValues()
        {
            Logger.debug("UIController.GetValues()");
            Logger.IncLvl();
            return GetValues((node) => true);
        }

        public Dictionary<string, string> GetValues(Func<XMLTree, bool> filter)
        {
            Logger.debug("UIController.GetValues()");
            Logger.IncLvl();
            if (ui == null)
            {
                Logger.DecLvl();
                return null;
            }

            Logger.DecLvl();
            return ui.GetValues(filter);
        }

        public string GetPackedValues(Func<XMLTree, bool> filter)
        {
            return Parser.PackData(GetValues(filter)).ToString();
        }

        public void DetachChild(XMLTree xml)
        {
            if(xml == ui)
            {
                ui = null;
            }
        }

        public string GetPackedValues()
        {
            Logger.debug("UIController.GetPackedValues()");
            Logger.IncLvl();
            Logger.DecLvl();
            return GetPackedValues(node => true);
        }

        public bool SelectNext()
        {
            return ui.SelectNext();
        }

        public void SetUserInputSource(IMyTerminalBlock sourceBlock, TextInputMode mode)
        {
            if(mode == TextInputMode.PUBLIC_TEXT && (sourceBlock as IMyTextPanel) == null)
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
            Logger.debug("UIController.RefreshUserInput()");
            Logger.IncLvl();
            if(!UserInputActive || UserInputSource == null)
            {
                return false;
            }

            // get input data
            string inputData = null;
            switch(UserInputMode)
            {
                case TextInputMode.CUSTOM_DATA:
                    inputData = UserInputSource?.CustomData;
                    break;
                case TextInputMode.PUBLIC_TEXT:
                    inputData = (UserInputSource as IMyTextPanel)?.GetPublicText();
                    break;
            }
            bool inputHasChanged = true;
            if( inputData == null || inputData == InputDataCache)
            {
                inputHasChanged = false;
            }

            Logger.debug("input has " + (inputHasChanged ? "" : "not ") + "changed");
            Logger.debug("Iterating input bindings (" + UserInputBindings.Count + " bindings registered).");

            // update ui input bindings
            string binding;
            string fieldValue;
            foreach (XMLTree node in UserInputBindings)
            {
                binding = node.GetAttribute("inputbinding");
                if(binding != null)
                {
                    Logger.debug("binding found at " + node.Type + " node for field: " + binding);
                    fieldValue = node.GetAttribute(binding.ToLower());
                    Logger.debug("field is " + (fieldValue ?? "EMPTY") + ".");
                    if(!inputHasChanged && fieldValue != null && fieldValue != InputDataCache)
                    {
                        Logger.debug("applying field value: " + fieldValue);
                        inputData = fieldValue;
                        inputHasChanged = true;
                    }
                    else if(inputHasChanged)
                    {
                        Logger.debug("Updating field value to input: " + inputData);
                        node.SetAttribute(binding.ToLower(), inputData);
                    }
                }
            }
            if(inputHasChanged)
            {
                InputDataCache = inputData;
            }

            // update user input device
            switch (UserInputMode)
            {
                case TextInputMode.CUSTOM_DATA:
                    if(UserInputSource != null)
                    {
                        UserInputSource.CustomData = InputDataCache;
                    }
                    break;
                case TextInputMode.PUBLIC_TEXT:
                    (UserInputSource as IMyTextPanel)?.WritePublicText(InputDataCache);
                    break;
            }

            return inputHasChanged;
        }

        private void CollectUserInputBindings()
        {
            Logger.debug("UIController.CollectUserInputBindings()");
            XMLTree node;
            Queue<XMLParentNode> nodes = new Queue<XMLParentNode>();
            nodes.Enqueue(ui);
            while(nodes.Count != 0)
            {
                node = nodes.Dequeue() as XMLTree;
                if(!node.HasUserInputBindings)
                {
                    Logger.debug("node has no userinputbindings");
                }
                if (node != null && node.HasUserInputBindings)
                {
                    Logger.debug("Checking " + node.Type + " node...");
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

    //EMBED SEScripts.XUI.XML.XMLParentNode
    //EMBED SEScripts.XUI.XML.XMLTree
}
