using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SEScripts.Lib;

namespace SEScripts.XUI
{
    public class NodeBuilder : NodeRenderCache
    {
        private bool Required;
        public NodeRenderCache ParentSegment;
        string Cache;
        //bool HasChanged;
        List<NodeRenderCache> Segments;

        public bool IsNewline
        {
            get
            {
                return false;
            }
        }

        public bool IsEmpty
        {
            get
            {
                return Segments.Count == 0;
            }
        }
        public bool IsRequired
        {
            get { return Required; }
            set { Required = value; }
        }
        public int Width
        {
            get
            {
                int maxWidth = 0;
                int currWidth = 0;
                for (int i = 0; i < Segments.Count; i++)
                {
                    if(Segments[i].IsNewline)
                    {
                        maxWidth = Math.Max(currWidth, maxWidth);
                        currWidth = 0;
                    }
                    else
                    {
                        currWidth += Segments[i].Width;
                    }
                }
                maxWidth = Math.Max(currWidth, maxWidth);
                return maxWidth;
            }
        }
        
        public NodeRenderCache GetLine(int index)
        {
            NodeBuilder line = new NodeBuilder();
            int currentIndex = 0;
            foreach (NodeRenderCache segment in Segments)
            {

                if (segment.IsNewline)
                {
                    currentIndex++;
                    if (currentIndex > index)
                    {
                        return line;
                    }
                }
                else
                {
                    line.LinkSegment(segment.GetLine(index - currentIndex));
                }
            }
            return line;
        }

        public IEnumerable<NodeRenderCache> GetLines()
        {
            NodeRenderCache line = GetLine(0);
            yield return line;
            for (int i = 1; !line.IsEmpty; i++)
            {
                line = GetLine(i);
                yield return line;
            }
        }

        public int Length
        {
            get
            {
                int length = 0;
                foreach(NodeRenderCache segment in Segments)
                {
                    length += segment.Length;
                }
                return length;
            }
        }
        public NodeRenderCache Parent
        {
            get
            {
                return ParentSegment;
            }
            set
            {
                ParentSegment = value;
            }
        }

        public NodeBuilder()
        {
            Cache = "";
            //HasChanged = false;
            Segments = new List<NodeRenderCache>();
        }

        public void Append(NodeRenderCache segment)
        {
            Segments.Add(segment);
        }

        public void Append(String segment)
        {
            Segments.Add(new TextNodeRenderCache(segment));
        }

        /*public override string ToString()
        {
            if (HasChanged)
            {
                StringBuilder result = new StringBuilder();
                foreach (StringSegment segment in Segments)
                {
                    result.Append(segment.ToString());
                }
                Cache = result.ToString();
            }
            return Cache;
        }*/
        


        public List<string> RenderedLeaves
        {
            get
            {
                List<string> leaves = new List<string>();
                foreach(NodeRenderCache segment in Segments)
                {
                    leaves.AddList<string>(segment.RenderedLeaves);
                }
                return leaves;
            }
        }

        public NodeRenderCache this[int i]
        {
            get
            {
                return Segments[i];
            }
            set
            {
                Segments[i] = value;
                //AnnounceChange();
            }
        }

        public void Add(NodeRenderCache segment)
        {
            segment.Parent = this;
            Segments.Add(segment);
            //AnnounceChange();
        }

        private void LinkSegment(NodeRenderCache segment)
        {
            Segments.Add(segment);
        }

        public void Add(string segment)
        {
            Add(new TextNodeRenderCache(segment));
        }

        /*public void AnnounceChange()
        {

            if (!HasChanged)
            {
                HasChanged = true;
                Parent?.AnnounceChange();
            }
        }*/

        public string Render()
        {
            return String.Join("", RenderedLeaves);
        }

        public void Clear()
        {
            Segments.Clear();
        }

        public void AddNewline()
        {
            Add(TextNodeRenderCache.CreateNewline());
        }
    }

    public class TextNodeRenderCache : NodeRenderCache
    {
        bool _isNewline;
        public bool IsNewline
        {
            get
            {
                return _isNewline;
            }
            set
            {
                _isNewline = value;
            }
        }
        public bool IsEmpty
        {
            get
            {
                return Content.Length == 0;
            }
        }
        string Content;
        NodeRenderCache ParentSegment;
        bool Required;
        public bool IsRequired
        {
            get { return Required; }
            set { Required = value; }
        }
        public int Width
        {
            get { return TextUtils.GetTextWidth(new StringBuilder(Content)); }
        }

        public int Length
        {
            get { return Content.Length; }
        }
        public NodeRenderCache Parent
        {
            get
            {
                return ParentSegment;
            }
            set
            {
                ParentSegment = value;
            }
        }

        public NodeRenderCache GetLine(int index)
        {
            if (index == 0)
            {
                return this;
            }
            return new TextNodeRenderCache("");
        }
        public List<string> RenderedLeaves {
            get
            {
                return new List<string> { this.Content };
            }
        }
        private TextNodeRenderCache(bool isNewline)
        {
            _isNewline = isNewline;
            if(IsNewline)
            {
                Content = "\n";
            }
        }
        public TextNodeRenderCache(string content) : this(false)
        {
            Content = content.Replace("\n", "");
        }

        public void Clear()
        {
            Content = "";
        }

        public static TextNodeRenderCache CreateNewline()
        {
            return new TextNodeRenderCache(true);
        }

        public string Render()
        {
            return Content;
        }
        //public void AnnounceChange() { }
    }

    public interface NodeRenderCache
    {
        bool IsNewline
        {
            get;
        }
        bool IsRequired
        {
            get;
            set;
        }

        bool IsEmpty
        {
            get;
        }

        int Length
        {
            get;
        }

        int Width
        {
            get;
        }


        NodeRenderCache Parent
        {
            get;
            set;
        }
        List<string> RenderedLeaves { get; }
        
        //void AnnounceChange();
        void Clear();

        NodeRenderCache GetLine(int index);

        string Render();
    }
}
