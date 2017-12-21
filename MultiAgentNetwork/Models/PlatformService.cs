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
		public class MANPlatformService
		{
			public string Id;
			public string Description;
			public MANAgentId Provider;
			private bool IsUIProvider;
			public bool ProvidesUI
			{
				get { return IsUIProvider; }
			}

			private MANAgentId PermissionMask;

			public MANPlatformService(string id, string description, MANAgentId provider) : this(id, description, provider, new MANAgentId("ANY@ANY"), false) { }
			public MANPlatformService(string id, string description, MANAgentId provider, MANAgentId permissionMask) : this(id, description, provider, permissionMask, false) { }
			public MANPlatformService(string id, string description, MANAgentId provider, MANAgentId permissionMask, bool providesUI)
			{
				Id = id;
				Description = description;
				Provider = provider;
				IsUIProvider = providesUI;
				PermissionMask = permissionMask;
			}

			public bool HasPermissions(MANAgentId agent)
			{
				return agent.Matches(PermissionMask);
			}

			public string ToXML()
			{
				return "<service" + (ProvidesUI ? " providesUI" : "") + " id='" + Id.ToString() + "' description='" + Description + "' provider='" + Provider + "' permissions='" + PermissionMask + "'/>";
			}

			public string ToString()
			{
				return ToXML();
			}

			public static MANPlatformService FromXMLString(string xml)
			{
				XMLTree serviceNode = XML.ParseXML(xml).GetNode((node) => node.Type == "service");
				return MANPlatformService.FromXMLNode(serviceNode);
			}

			public static MANPlatformService FromXMLNode(XMLTree node)
			{
				if (node.Type != "service")
				{
					return null;
				}
				if (node.GetAttribute("id") == null || node.GetAttribute("provider") == null)
				{
					return null;
				}
				return new MANPlatformService(
					node.GetAttribute("id"),
					node.GetAttribute("description") ?? "",
					new MANAgentId(node.GetAttribute("provider")),
					new MANAgentId(node.GetAttribute("permissions") ?? "ANY@ANY"),
					node.GetAttribute("providesui") != null && node.GetAttribute("providesui") != "false"
					);
			}

			public bool Equals(MANPlatformService other)
			{
				return Id == other.Id && Provider == other.Provider;
			}

			public override bool Equals(Object obj)
			{
				if (obj == null)
					return false;

				MANPlatformService serviceObj = obj as MANPlatformService;
				if (serviceObj == null)
					return false;
				else
					return Equals(serviceObj);
			}

			public static bool operator ==(MANPlatformService service1, MANPlatformService service2)
			{
				if (((object)service1) == null || ((object)service2) == null)
					return Object.Equals(service1, service2);

				return service1.Equals(service2);
			}

			public static bool operator !=(MANPlatformService service1, MANPlatformService service2)
			{
				if (((object)service1) == null || ((object)service2) == null)
					return !Object.Equals(service1, service2);

				return !(service1.Equals(service2));
			}

		}
	}
}