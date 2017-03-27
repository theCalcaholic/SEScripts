using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SEScripts.Lib;
using SEScripts.Lib.LoggerNS;

namespace SEScripts.XUI.BoxRenderer
{
    public class RenderBoxLeaf : RenderBox
    {
        StringBuilder Content;
        private int _Height;

        public override RenderBox.FlowDirection Flow
        {
            get { return RenderBox.FlowDirection.VERTICAL; }
            set { }
        }

        public override int Height
        {
            get
            {
                Logger.debug("NodeBoxLeaf.Height.get");
                Logger.IncLvl();
                Logger.debug("height = " + _Height);
                Logger.DecLvl();
                return _Height;
            }
            set
            {
                Logger.debug("NodeBoxLeaf.Height.set");
                Logger.IncLvl();
                Logger.debug("height = " + value);
                _Height = value;
                Logger.DecLvl();
            }
        }

        public override int MinWidth
        {
            get
            {
                Logger.debug("NodeBoxLeaf.MinWidth.get");
                Logger.IncLvl();
                int contentWidth = Math.Max(TextUtils.GetTextWidth(Content), _MinWidth);
                /*if (_MinWidth >= 0 && _MinWidth < contentWidth)
                {
                    return _MinWidth;
                }
                else
                {
                    return contentWidth;
                }*/
                Logger.debug("minwidth = " + contentWidth);
                Logger.DecLvl();
                return contentWidth;
            }
            set
            {
                Logger.debug("NodeBoxLeaf.MinWidth.set()");
                Logger.IncLvl();
                Logger.debug("minwidth = " + value);
                _MinWidth = value;
                Logger.DecLvl();
            }
        }

        public RenderBoxLeaf()
        {
            Logger.debug("NodeBoxLeaf constructor()");
            _Height = 0;
            Content = new StringBuilder();
        }

        public RenderBoxLeaf(StringBuilder content) : this()
        {
            Logger.debug("NodeBoxLeaf constructor(StringBuilder)");
            Logger.IncLvl();
            Add(content);
            if (Content.Length > 0)
            {
                _Height = 1;
            }
            Logger.DecLvl();
        }

        public RenderBoxLeaf(string content) : this(new StringBuilder(content))
        { }

        public override void AddAt(int position, StringBuilder box)
        {
            Logger.debug("NodeBoxLeaf.AddAt(int, StringBuilder)");
            Logger.IncLvl();
            if (position == 0)
            {
                Content = new StringBuilder(box.ToString() + Content.ToString());
            }
            else
            {
                Content.Append(box);
            }
            Logger.DecLvl();
        }

        public override void Add(StringBuilder box)
        {
            Logger.debug("NodeBoxLeaf.Add(StringBuilder)");
            Logger.IncLvl();
            Content.Append(box.Replace("\n", ""));
            Logger.DecLvl();
        }
        public override void AddAt(int position, string box)
        {
            Logger.debug("NodeBoxLeaf.AddAt(int, string)");
            Logger.IncLvl();
            AddAt(position, new StringBuilder(box));
            Logger.DecLvl();
        }

        public override void Add(string box)
        {
            Logger.debug("NodeBoxLeaf.Add(string)");
            Logger.IncLvl();
            Add(new StringBuilder(box));
            Logger.DecLvl();
        }

        public override StringBuilder GetLine(int index)
        {
            return GetLine(index, -1);
        }

        public override StringBuilder GetLine(int index, int maxWidth)
        {
            Logger.debug("NodeBoxLeaf.GetLine()");
            Logger.IncLvl();
            StringBuilder line;
            if (index == 0)
            {
                line = new StringBuilder(Content.ToString());
            }
            else
            {
                line = new StringBuilder();
            }
            AlignLine(ref line, maxWidth);
            Logger.debug("line is {" + line + "}");
            Logger.DecLvl();
            return line;
        }

        public override void Clear()
        {
            Content = new StringBuilder();
        }


    }
}
