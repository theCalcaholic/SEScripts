﻿using System;
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
    public abstract class XMLTree : XMLParentNode
    {
        public string Type;
        private XMLParentNode Parent;
        private List<string> PreventDefaults;
        protected List<XMLTree> Children;
        protected bool Selectable;
        protected bool ChildrenAreSelectable;
        private bool Selected;
        protected int SelectedChild;
        protected bool Activated;
        protected Dictionary<string, string> Attributes;

        public XMLTree()
        {
            PreventDefaults = new List<string>();
            Parent = null;
            Children = new List<XMLTree>();
            Selectable = false;
            ChildrenAreSelectable = false;
            Selected = false;
            SelectedChild = -1;
            Activated = false;
            Attributes = new Dictionary<string, string>();
            Type = "NULL";

            // set attribute defaults
            SetAttribute("alignself", "left");
            SetAttribute("aligntext", "left");
            SetAttribute("selected", "false");
            SetAttribute("selectable", "false");
            SetAttribute("flowdirection", "vertical");
        }

        public bool IsSelectable()
        {
            Logger.debug(Type + ": IsSelectable():");
            Logger.IncLvl();
            Logger.DecLvl();
            return Selectable || ChildrenAreSelectable;
        }

        public bool IsSelected()
        {
            Logger.debug(Type + ": IsSelected():");
            Logger.IncLvl();
            Logger.DecLvl();
            return Selected;
        }

        public XMLTree GetSelectedSibling()
        {
            Logger.debug(Type + ": GetSelectedSibling():");
            Logger.IncLvl();
            if (!Selected)
            {
                Logger.DecLvl();
                return null;
                //throw new Exception( 
                //    "Node is not selected. You can only get the selected Node from one of it's parent nodes!"); 
            }

            if (SelectedChild == -1)
            {
                Logger.DecLvl();
                return this;
            }
            else
            {
                Logger.DecLvl();
                return Children[SelectedChild].GetSelectedSibling();
            }
        }

        public virtual void AddChild(XMLTree child)
        {
            Logger.debug(Type + ": AddChild():");
            Logger.IncLvl();
            Children.Add(child);
            child.SetParent(this as XMLParentNode);
            UpdateSelectability(child);
            Logger.DecLvl();
        }

        public void SetParent(XMLParentNode parent)
        {
            Logger.debug(Type + ": SetParent():");
            Logger.IncLvl();
            Parent = parent;
            Logger.DecLvl();
        }

        public XMLParentNode GetParent()
        {
            Logger.debug(Type + ": GetParent():");
            Logger.IncLvl();
            Logger.DecLvl();
            return Parent;
        }

        public XMLTree GetChild(int i)
        {
            Logger.debug(Type + ": GetChild():");
            Logger.IncLvl();
            Logger.DecLvl();
            return i < Children.Count ? Children[i] : null;
        }

        public XMLTree GetNode(Func<XMLTree, bool> filter)
        {
            if (filter(this))
            {
                return this;
            }
            else
            {
                XMLTree child = GetChild(0);
                XMLTree childResult;
                for (int i = 1; child != null; i++)
                {
                    childResult = child.GetNode(filter);
                    if (childResult != null)
                    {
                        return childResult;
                    }

                    child = GetChild(i);
                }
            }
            return null;
        }

        public List<XMLTree> GetAllNodes(Func<XMLTree, bool> filter)
        {
            List<XMLTree> nodeList = new List<XMLTree>();
            GetAllNodes(filter, ref nodeList);
            return nodeList;
        }

        private void GetAllNodes(Func<XMLTree, bool> filter, ref List<XMLTree> nodeList)
        {
            if (filter(this))
            {
                nodeList.Add(this);
            }
            XMLTree child = GetChild(0);
            for (int i = 1; child != null; i++)
            {
                child.GetAllNodes(filter, ref nodeList);
                child = GetChild(i);
            }
        }

        public virtual void UpdateSelectability(XMLTree child)
        {
            Logger.debug(Type + ": UpdateSelectability():");
            Logger.IncLvl();
            bool ChildrenWereSelectable = ChildrenAreSelectable;
            ChildrenAreSelectable = ChildrenAreSelectable || child.IsSelectable();
            if (Parent != null && (Selectable || ChildrenAreSelectable) != (Selectable || ChildrenWereSelectable))
            {
                Parent.UpdateSelectability(this);
            }

            Logger.DecLvl();
        }

        public bool SelectFirst()
        {
            Logger.debug(Type + ": SelectFirst():");
            Logger.IncLvl();
            if (SelectedChild != -1)
            {
                Children[SelectedChild].Unselect();
            }

            SelectedChild = -1;
            bool success = (Selectable || ChildrenAreSelectable) ? SelectNext() : false;
            Logger.DecLvl();
            return success;
        }

        public bool SelectLast()
        {
            Logger.debug(Type + ": SelectLast():");
            Logger.IncLvl();
            if (SelectedChild != -1)
            {
                Children[SelectedChild].Unselect();
            }

            SelectedChild = -1;
            Logger.DecLvl();
            return (Selectable || ChildrenAreSelectable) ? SelectPrevious() : false;
        }

        public void Unselect()
        {
            Logger.debug(Type + ": Unselect():");
            Logger.IncLvl();
            if (SelectedChild != -1)
            {
                Children[SelectedChild].Unselect();
            }
            Selected = false;
            Activated = false;
            Logger.DecLvl();
        }

        public virtual bool SelectNext()
        {
            Logger.debug(Type + ": SelectNext():");
            Logger.IncLvl();
            bool WasSelected = IsSelected();
            if (SelectedChild == -1 || !Children[SelectedChild].SelectNext())
            {
                Logger.debug(Type + ": find next child to select...");
                SelectedChild++;
                while ((SelectedChild < Children.Count && (!Children[SelectedChild].SelectFirst())))
                {
                    SelectedChild++;
                }

                if (SelectedChild == Children.Count)
                {
                    SelectedChild = -1;
                    Selected = Selectable && !Selected;
                }
                else
                {
                    Selected = true;
                }
            }

            if (!Selected)
            {
                Unselect();
            }
            if (!WasSelected && IsSelected())
            {
                OnSelect();
            }

            Logger.DecLvl();
            return Selected;
        }

        public virtual bool SelectPrevious()
        {
            Logger.debug(Type + ": SelectPrevious():");
            Logger.IncLvl();
            bool WasSelected = IsSelected();
            if (SelectedChild == -1) { SelectedChild = Children.Count; }
            if (SelectedChild == Children.Count || !Children[SelectedChild].SelectPrevious())
            {
                SelectedChild--;
                while (SelectedChild > -1 && !Children[SelectedChild].SelectLast())
                {
                    SelectedChild--;
                }

                if (SelectedChild == -1)
                {
                    Selected = Selectable && !Selected;
                }
                else
                {
                    Selected = true;
                }
            }

            if (!Selected)
            {
                Unselect();
            }
            if (!WasSelected && IsSelected())
            {
                OnSelect();
            }

            Logger.DecLvl();
            return Selected;
        }

        public virtual void OnSelect() { }

        public virtual string GetAttribute(string key)
        {
            Logger.debug(Type + ": GetAttribute(" + key + "):");
            Logger.IncLvl();
            if (Attributes.ContainsKey(key))
            {
                Logger.DecLvl();
                return Attributes[key];
            }

            Logger.DecLvl();
            return null;
        }

        public virtual void SetAttribute(string key, string value)
        {
            Logger.debug(Type + ": SetAttribute():");
            Logger.IncLvl();
            if (key == "selectable")
            {
                bool shouldBeSelectable = value == "true";
                if (Selectable != shouldBeSelectable)
                {
                    Selectable = shouldBeSelectable;
                    if (Parent != null)
                    {
                        Parent.UpdateSelectability(this);
                    }
                }
            }

            if (key == "activated")
            {
                bool shouldBeActivated = value == "true";
                Activated = shouldBeActivated;
            }

            Attributes[key] = value;
            Logger.DecLvl();
        }

        public void KeyPress(string keyCode)
        {
            Logger.debug(Type + ": _KeyPress():");
            Logger.IncLvl();
            Logger.debug("button: " + keyCode);
            OnKeyPressed(keyCode);
            if (Parent != null && !PreventDefaults.Contains(keyCode))
            {
                Parent.KeyPress(keyCode);
            }

            Logger.DecLvl();
        }

        public virtual void OnKeyPressed(string keyCode)
        {
            Logger.debug(Type + ": OnKeyPressed()");
            Logger.IncLvl();
            switch (keyCode)
            {
                case "ACTIVATE":
                    ToggleActivation();
                    break;
                default:
                    break;
            }

            Logger.DecLvl();
        }

        public virtual void ToggleActivation()
        {
            Logger.debug(Type + ": ToggleActivation()");
            Logger.IncLvl();
            Activated = !Activated;
            Logger.DecLvl();
        }

        public void PreventDefault(string keyCode)
        {
            Logger.debug(Type + ": PreventDefault()");
            Logger.IncLvl();
            if (!PreventDefaults.Contains(keyCode))
            {
                PreventDefaults.Add(keyCode);
            }

            Logger.DecLvl();
        }

        public void AllowDefault(string keyCode)
        {
            Logger.debug(Type + ": AllowDefault()");
            Logger.IncLvl();
            if (PreventDefaults.Contains(keyCode))
            {
                PreventDefaults.Remove(keyCode);
            }

            Logger.DecLvl();
        }

        public void FollowRoute(Route route)
        {
            Logger.debug(Type + ": FollowRoute");
            Logger.IncLvl();
            if (Parent != null)
            {
                Parent.FollowRoute(route);
            }

            Logger.DecLvl();
        }

        public virtual Dictionary<string, string> GetValues(Func<XMLTree, bool> filter)
        {
            Logger.debug(Type + ": GetValues()");
            Logger.IncLvl();
            Dictionary<string, string> dict = new Dictionary<string, string>();
            string name = GetAttribute("name");
            string value = GetAttribute("value");
            if (name != null && value != null)
            {
                dict[name] = value;
            }

            Dictionary<string, string> childDict;
            foreach (XMLTree child in Children)
            {
                childDict = child.GetValues(filter);
                foreach (string key in childDict.Keys)
                {
                    if (!dict.ContainsKey(key))
                    {
                        dict[key] = childDict[key];
                    }
                }
            }

            Logger.DecLvl();
            return dict;
        }

        public int GetWidth(int maxWidth)
        {
            Logger.debug(Type + ".GetWidth()");
            Logger.IncLvl();
            string attributeWidthValue = GetAttribute("width");
            if (attributeWidthValue == null)
            {
                Logger.DecLvl();
                return 0;
                //return maxWidth;
            }
            else
            {
                if (attributeWidthValue[attributeWidthValue.Length - 1] == '%')
                {
                    Logger.debug("is procent value (" + Single.Parse(attributeWidthValue.Substring(0, attributeWidthValue.Length - 1)).ToString() + ")");
                    Logger.DecLvl();
                    return (int)(Single.Parse(attributeWidthValue.Substring(0, attributeWidthValue.Length - 1)) / 100f * maxWidth);
                }
                else if (maxWidth == 0)
                {
                    Logger.DecLvl();
                    return Int32.Parse(attributeWidthValue);
                }
                else
                {
                    Logger.DecLvl();
                    return Math.Min(maxWidth, Int32.Parse(attributeWidthValue));
                }
            }
        }

        public string Render(int availableWidth)
        {
            Logger.debug(Type + ".Render()");
            Logger.IncLvl();
            List<string> segments = new List<string>();
            int width = GetWidth(availableWidth);
            PreRender(ref segments, width, availableWidth);
            RenderText(ref segments, width, availableWidth);
            string renderString = PostRender(segments, width, availableWidth);

            Logger.DecLvl();
            return renderString;
        }

        protected virtual void PreRender(ref List<string> segments, int width, int availableWidth)
        {
            Logger.debug(Type + ".PreRender()");
            Logger.IncLvl();
            Logger.DecLvl();
        }

        protected virtual void RenderText(ref List<string> segments, int width, int availableWidth)
        {
            Logger.debug(Type + ".RenderText()");
            Logger.IncLvl();
            for (int i = 0; i < Children.Count; i++)
            {
                if (GetAttribute("flowdirection") == "vertical")
                {
                    string childString = RenderChild(Children[i], width);
                    if (childString != null)
                    {
                        if (i > 0 && Children[i - 1].Type == "textnode" && (Children[i].Type == "textnode" || Children[i].Type == "br"))
                        {
                            segments[segments.Count - 1] += childString;
                        }
                        else
                        {
                            segments.Add(childString);
                        }
                    }
                    else
                    {
                    }
                }
                else
                {
                    string childString = RenderChild(Children[i], width);
                    if (childString != null)
                    {
                        availableWidth -= TextUtils.GetTextWidth(childString);
                        segments.Add(childString);
                    }
                }
            }

            Logger.DecLvl();
        }

        protected virtual string PostRender(List<string> segments, int width, int availableWidth)
        {
            Logger.debug(Type + ".PostRender()");
            Logger.IncLvl();
            string renderString = "";
            string flowdir = GetAttribute("flowdirection");
            string alignChildren = GetAttribute("alignchildren");
            string alignSelf = GetAttribute("alignself");
            int totalWidth = 0;

            foreach (string segment in segments)
            {
                int lineWidth = TextUtils.GetTextWidth(segment);
                if (lineWidth > totalWidth)
                {
                    totalWidth = lineWidth;
                }
            }
            totalWidth = Math.Min(availableWidth, Math.Max(width, totalWidth));

            if (flowdir == "vertical")
            {
                for (int i = 0; i < segments.Count; i++)
                {
                    switch (alignChildren)
                    {
                        case "right":
                            segments[i] = TextUtils.PadText(segments[i], totalWidth, TextUtils.PadMode.LEFT);
                            break;
                        case "center":
                            segments[i] = TextUtils.CenterText(segments[i], totalWidth);
                            break;
                        default:
                            segments[i] = TextUtils.PadText(segments[i], totalWidth, TextUtils.PadMode.RIGHT);
                            break;
                    }
                }
                renderString = String.Join("\n", segments.ToArray());
            }
            else
            {
                renderString = String.Join("", segments.ToArray());
            }

            if (availableWidth - totalWidth > 0)
            {
                if (alignSelf == "center")
                {
                    Logger.log("Center element...");
                    renderString = TextUtils.CenterText(renderString, availableWidth);
                }
                else if (alignSelf == "right")
                {
                    Logger.log("Aligning element right...");
                    renderString = TextUtils.PadText(renderString, availableWidth, TextUtils.PadMode.RIGHT);
                }
            }

            Logger.DecLvl();
            return renderString;
        }

        protected virtual string RenderChild(XMLTree child, int availableWidth)
        {
            Logger.log(Type + ".RenderChild()");
            Logger.IncLvl();
            Logger.DecLvl();
            return child.Render(availableWidth);
        }
    }

    //EMBED SEScripts.XUI.XML.XMLParentNode
    //EMBED SEScripts.Lib.TextUtils
}
