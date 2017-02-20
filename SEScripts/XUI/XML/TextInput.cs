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
    public class TextInput : XMLTree
    {
        int CursorPosition;

        public TextInput()
        {
            Logger.log("TextInput constructor()");
            Logger.IncLvl();
            Type = "textinput";
            Selectable = true;
            CursorPosition = -1;
            PreventDefault("LEFT/ABORT");
            PreventDefault("RIGHT/SUBMIT");
            SetAttribute("maxlength", "10");
            SetAttribute("value", "");
            SetAttribute("allowedchars", " a-z0-9");
            Logger.DecLvl();
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

        private void IncreaseLetter()
        {
            Logger.log("TextInput.IncreaseLetter()");
            Logger.IncLvl();
            if (CursorPosition == -1)
            {
                return;
            }
            char[] value = GetAttribute("value").ToCharArray();
            char letter = value[CursorPosition];
            char[] chars = GetAttribute("allowedchars").ToCharArray();
            for (int i = 0; i < chars.Length; i++)
            {
                if (
                    chars[i] != '-' 
                    && chars[i] == value[CursorPosition] 
                    && !(i < chars.Length - 1 && chars[i + 1] == '-'))
                {
                    Logger.log("letter outside class, setting to: " + chars[i == 0 ? chars.Length - 1 : i - 1] + ". (chars[" + ((i + 1) % chars.Length) + "])");
                    value[CursorPosition] = chars[(i + 1) % chars.Length];
                    SetAttribute("value", new string(value));
                    Logger.DecLvl();
                    return;
                }
            }

            Logger.log("letter inside class, setting to: " + (char)(((int)value[CursorPosition]) + 1));
            value[CursorPosition] = (char)(((int)value[CursorPosition]) + 1);
            SetAttribute("value", new string(value));
            Logger.DecLvl();
        }

        private void DecreaseLetter()
        {
            Logger.log("TextInput.DecreaseLetter()");
            Logger.IncLvl();
            if (CursorPosition == -1)
            {
                return;
            }
            char[] value = GetAttribute("value").ToCharArray();
            char[] chars = GetAttribute("allowedchars").ToCharArray();
            for(int i = 0; i < chars.Length; i++)
            {
                if(
                    chars[i] != '_' 
                    && chars[i] == value[CursorPosition] 
                    && !(i > 0 && chars[i-1] == '-'))
                {
                    Logger.log("letter outside class, setting to: " + chars[i == 0 ? chars.Length - 1 : i - 1] + ". (chars[" + (i == 0 ? chars.Length - 1 : i - 1) + "])");
                    value[CursorPosition] = chars[ i == 0 ? chars.Length - 1 : i - 1];
                    SetAttribute("value", new string(value));
                    return;
                }
            }
            Logger.log("letter inside class, setting to: " + (char)(((int)value[CursorPosition]) - 1));
            value[CursorPosition] = (char)(((int)value[CursorPosition]) - 1);
            SetAttribute("value", new string(value));
            Logger.DecLvl();
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
                string chars = GetAttribute("allowedchars");
                SetAttribute("value", GetAttribute("value") + chars[0]);
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

        protected override void RenderText(ref List<string> segments, int width, int availableWidth)
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
                /*char[] valueChr = (" " + value).ToCharArray(); 
                valueChr[0] = (char) 187; 
                value = new string(valueChr);*/
                value = "_" + value;
            }
            segments.Add((IsSelected() ? new string(new char[] { (char)187 }) : "  ") + " " + value);

        }
    }


    //EMBED SEScripts.XUI.XML.DataStore

}
