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

namespace SEScripts.MultiAgentNetwork.MAN
{
    public class AgentId : IEquatable<AgentId>
    {
        private string Id;
        public string Name
        {
            get
            {
                string[] idSplit = Id.Split('@');
                return idSplit[0];
            }
            set
            {
                string[] idSplit = Id.Split('@');
                string platform = (idSplit.Length > 1 ? idSplit[1] : "");
                Id = value + "@" + platform;
            }
        }
        public string Platform
        {
            get
            {
                string[] idSplit = Id.Split('@');
                if (idSplit.Length > 1)
                {
                    return idSplit[1];
                }
                else
                {
                    return "local";
                }
            }
            set
            {
                string[] idSplit = Id.Split('@');
                Id = idSplit[0] + "@" + value;
            }
        }

        public override string ToString()
        {
            return Name + "@" + Platform;
        }

        public AgentId(string id)
        {
            Id = id;
        }

        public bool MatchesName(AgentId other)
        {
            return other.Name == "ANY" 
                || other.Name == "ALL" 
                || other.Name == Name
                || Name == "ANY"
                || Name == "ALL";
        }

        public bool MatchesPlatform(AgentId other)
        {
            return other.Platform == "local"
                    || other.Platform == "ALL"
                    || other.Platform == "ANY"
                    || other.Platform == ""
                    || other.Platform == Platform
                    || Platform == "ALL"
                    || Platform == "ANY";
        }

        public bool Matches(AgentId other)
        {
            return MatchesName(other) && MatchesPlatform(other);
        }

        public bool Equals(AgentId other)
        {

            return Name == other.Name && (
                Platform == other.Platform
                || Platform == "local"
                || other.Platform == "local"
                || Platform == ""
                || other.Platform == "");
        }

        public override bool Equals(Object obj)
        {
            if (obj == null)
                return false;

            AgentId aidObj = obj as AgentId;
            if (aidObj == null)
                return false;
            else
                return Equals(aidObj);
        }

        public static bool operator ==(AgentId id1, AgentId id2)
        {
            if (((object)id1) == null || ((object)id2) == null)
                return Object.Equals(id1, id2);

            return id1.Equals(id2);
        }

        public static bool operator !=(AgentId id1, AgentId id2)
        {
            if (((object)id1) == null || ((object)id2) == null)
                return !Object.Equals(id1, id2);

            return !(id1.Equals(id2));
        }
    }
}
