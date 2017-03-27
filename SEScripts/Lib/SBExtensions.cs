using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEScripts.Lib
{
    public static class SBExtensions
    {
        public static void AppendSubstr(StringBuilder me, StringBuilder append, int start, int count)
        {
            me.Capacity = me.Capacity + append.Length;
            int loopEnd = Math.Min(append.Length, start + count);
            for (int i = start; i < loopEnd; i++)
            {
                me.Append(append[i]);
            }
        }
    }
}
