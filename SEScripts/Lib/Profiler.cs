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

using SEScripts.Lib.LoggerNS;

namespace SEScripts.Lib.ProfilerNS
{
    class Profiler : Logger
    {
        private int InstructionCountBefore;
        static private Dictionary<string, MethodInfo> MethodInfos = new Dictionary<string, MethodInfo>();
        private string currentMethod;
        
        public Profiler(Program program, Mode mode,
            [System.Runtime.CompilerServices.CallerMemberName] string methodName = "")
            : base(methodName, mode, program)
        {
            currentMethod = methodName;
            InstructionCountBefore = Prog?.Runtime.CurrentInstructionCount ?? 0;
        }

        public override void Dispose()
        {
            if(!MethodInfos.ContainsKey(currentMethod))
            {
                MethodInfos[currentMethod] = new MethodInfo();
            }
            MethodInfos[currentMethod].TotalInstructionCount += Prog.Runtime.CurrentInstructionCount - InstructionCountBefore;
            MethodInfos[currentMethod].TimesRun++;
            MethodInfos[currentMethod].Name = currentMethod;
            base.Dispose();
        }

        public static string Evaluate()
        {
            SortedList<int, MethodInfo> Methods = new SortedList<int, MethodInfo>(); ;
            foreach(var entry in MethodInfos)
            {
                int instructionCount = entry.Value.TotalInstructionCount;
                while (Methods.ContainsKey(instructionCount))
                    instructionCount++;
                Methods.Add(instructionCount, entry.Value);
            }
            StringBuilder result = new StringBuilder();
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
            return result.ToString();
        }
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
