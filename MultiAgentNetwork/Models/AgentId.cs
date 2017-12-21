using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System;
using VRage.Collections;
using VRage.Game.Components;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ObjectBuilders.Definitions;
using VRage.Game;
using VRageMath;

namespace IngameScript
{
	partial class Program : MyGridProgram
	{
		public class MANAgentId : IEquatable<MANAgentId>
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

			public MANAgentId(string id)
			{
				Id = id;
			}

			public bool MatchesName(MANAgentId other)
			{
				return other.Name == "ANY"
					|| other.Name == "ALL"
					|| other.Name == Name
					|| Name == "ANY"
					|| Name == "ALL";
			}

			public bool MatchesPlatform(MANAgentId other)
			{
				return other.Platform == "local"
						|| other.Platform == "ALL"
						|| other.Platform == "ANY"
						|| other.Platform == ""
						|| other.Platform == Platform
						|| Platform == "ALL"
						|| Platform == "ANY";
			}

			public bool Matches(MANAgentId other)
			{
				return MatchesName(other) && MatchesPlatform(other);
			}

			public bool Equals(MANAgentId other)
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

				MANAgentId aidObj = obj as MANAgentId;
				if (aidObj == null)
					return false;
				else
					return Equals(aidObj);
			}

			public static bool operator ==(MANAgentId id1, MANAgentId id2)
			{
				if (((object)id1) == null || ((object)id2) == null)
					return Object.Equals(id1, id2);

				return id1.Equals(id2);
			}

			public static bool operator !=(MANAgentId id1, MANAgentId id2)
			{
				if (((object)id1) == null || ((object)id2) == null)
					return !Object.Equals(id1, id2);

				return !(id1.Equals(id2));
			}
		}
	}
}