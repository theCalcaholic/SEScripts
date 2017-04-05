using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEScripts.Lib.Profilers
{

    public class SimpleProfiler : IDisposable
    {
        private int InstructionCountBefore;
        static private Dictionary<string, MethodInfo> MethodInfos = new Dictionary<string, MethodInfo>();
        private string currentMethod;
        private static Program _Prog;
        public static Object Prog
        {
            set { _Prog = (Program)value; }
        }

        public SimpleProfiler(string methodName = "")
        {
            currentMethod = methodName;
            if(_Prog != null) InstructionCountBefore = _Prog.Runtime.CurrentInstructionCount;
        }

        public void Dispose()
        {
            if (_Prog == null) return;
            int instructionCountAfter = _Prog.Runtime.CurrentInstructionCount;
            if (!MethodInfos.ContainsKey(currentMethod))
            {
                MethodInfos[currentMethod] = new MethodInfo();
            }
            MethodInfos[currentMethod].TotalInstructionCount += instructionCountAfter - InstructionCountBefore;
            MethodInfos[currentMethod].TimesRun++;
            MethodInfos[currentMethod].Name = currentMethod;
        }

        public static string Evaluate()
        {
            SortedList<int, MethodInfo> Methods = new SortedList<int, MethodInfo>(); ;
            foreach (var entry in MethodInfos)
            {
                int instructionCount = entry.Value.TotalInstructionCount;
                while (Methods.ContainsKey(-instructionCount))
                    instructionCount--;
                Methods.Add(-instructionCount, entry.Value);
            }
            StringBuilder result = new StringBuilder("Profiler Results:\n");
            string indent = "    ";
            foreach (var method in Methods.Values)
            {
                result.Append(method.Name).Append("\n");
                result.Append(indent).Append("times run: ").Append(method.TimesRun).Append("\n");
                result.Append(indent).Append("average instructions per run: ")
                    .Append(method.TotalInstructionCount / method.TimesRun).Append("\n");
                result.Append(indent).Append("total instructions: ")
                    .Append(method.TotalInstructionCount).Append("\n");
            }
            MethodInfos.Clear();
            return result.ToString();
        }

        class MethodInfo
        {
            public int TotalInstructionCount;
            public int TimesRun;
            public string Name;

            public MethodInfo()
            {
                TotalInstructionCount = 0;
                TimesRun = 0;
            }
        }
    }
}
