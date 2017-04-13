using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SEScripts.XUI.BoxRenderer;
using SEScripts.Lib.Profilers;

namespace SEScripts.Lib
{
    public static class SBExtensions
    {
        public static void AppendSubstr(StringBuilder me, StringBuilder append, int start, int count)
        {
            //using (new SimpleProfiler("SBExtensions.AppendSubstr(StringBuilder, StringBuilder, int, int)"))
            //{
                me.Capacity = me.Capacity + append.Length;
                int loopEnd = Math.Min(append.Length, start + count);
                for (int i = start; i < loopEnd; i++)
                {
                    me.Append(append[i]);
                }
            //}
        }
    }
}
