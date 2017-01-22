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
            Type = "textinput";
            Selectable = true;
            CursorPosition = -1;
            PreventDefault("LEFT/ABORT");
            PreventDefault("RIGHT/SUBMIT");
            SetAttribute("maxlength", "10");
            SetAttribute("value", "");
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
                    IncreaseLetter();
                    break;
                case "DOWN":
                    DecreaseLetter();
                    break;
                default:
                    base.OnKeyPressed(keyCode);
                    break;
            }
        }

        private void IncreaseLetter()
        {
            if (CursorPosition == -1)
            {
                return;
            }
            char[] value = GetAttribute("value").ToCharArray();
            char letter = value[CursorPosition];
            switch (letter)
            {
                case ' ':
                    value[CursorPosition] = 'a';
                    break;
                case 'z':
                    value[CursorPosition] = 'A';
                    break;
                case 'Z':
                    value[CursorPosition] = '0';
                    break;
                case '9':
                    value[CursorPosition] = ' ';
                    break;
                default:
                    value[CursorPosition] = (char)(((int)value[CursorPosition]) + 1);
                    break;
            }
            SetAttribute("value", new string(value));
        }

        private void DecreaseLetter()
        {
            if (CursorPosition == -1)
            {
                return;
            }
            char[] value = GetAttribute("value").ToCharArray();
            char letter = value[CursorPosition];
            switch (letter)
            {
                case ' ':
                    value[CursorPosition] = '9';
                    break;
                case '0':
                    value[CursorPosition] = 'Z';
                    break;
                case 'a':
                    value[CursorPosition] = ' ';
                    break;
                case 'A':
                    value[CursorPosition] = 'z';
                    break;
                default:
                    value[CursorPosition] = (char)(((int)value[CursorPosition]) - 1);
                    break;
            }
            SetAttribute("value", new string(value));
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
                SetAttribute("value", GetAttribute("value") + " ");
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
