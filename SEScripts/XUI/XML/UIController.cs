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

namespace SEScripts.XUI.XML
{
    public class UIController : XMLParentNode
    {
        XMLTree ui;
        public Stack<XMLTree> UIStack;
        public string Type;

        public UIController(XMLTree rootNode)
        {
            Logger.debug("UIController constructor()");
            Logger.IncLvl();
            Type = "CTRL";
            UIStack = new Stack<XMLTree>();
            ui = rootNode;
            ui.SetParent(this);
            if (GetSelectedNode() == null && ui.IsSelectable())
            {
                ui.SelectFirst();
            }

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
    }

    //EMBED SEScripts.XUI.XML.XMLParentNode
    //EMBED SEScripts.XUI.XML.XMLTree
}
