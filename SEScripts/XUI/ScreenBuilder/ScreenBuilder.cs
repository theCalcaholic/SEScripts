using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEScripts.XUI
{
    class ScreenBuilder : System.Collections.Generic.IEnumerable<List<ScreenSegment>>
    {
        List<List<ScreenSegment>> Segments;
        int CurrentFrameStart;

        public void EndFrame()
        {
            CurrentFrameStart = Segments.Count;
        }

        private IEnumerable<List<ScreenSegment>> CurrentFrameRows()
        {
            for(int i = CurrentFrameStart; i < Segments.Count; i++)
            {
                yield return Segments[i];
            }
        }

        public IEnumerator<List<ScreenSegment>> GetEnumerator()
        {
            return CurrentFrameRows().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
