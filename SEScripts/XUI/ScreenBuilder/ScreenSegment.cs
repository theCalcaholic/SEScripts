using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SEScripts.Lib;

namespace SEScripts.XUI
{
    class ScreenSegment
    {
        public int ContentWidth
        {
            get { return TextUtils.GetTextWidth(Content); }
        }
        public int MinWidth;
        public int ActualWidth
        {
            get{ return Math.Max(MinWidth, ContentWidth); }
        }
        public StringBuilder Content;

        public ScreenSegment() : this(0, "") { }

        public ScreenSegment(int minWidth) : this(minWidth, "") { }

        public ScreenSegment(int minWidth, string content) : this(minWidth, new StringBuilder(content)) { }

        public ScreenSegment(int minWidth, StringBuilder content)
        {
            MinWidth = minWidth;
            Content = content;
        }

    }
}
