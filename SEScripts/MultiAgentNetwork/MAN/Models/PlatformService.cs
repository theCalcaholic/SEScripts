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
using SEScripts.ParseLib.XUI;

namespace SEScripts.MultiAgentNetwork.MAN.Models
{
    public class PlatformService
    {
        public string Id;
        public string Description;
        public AgentId Provider;
        private bool IsUIProvider;
        public bool ProvidesUI
        {
            get { return IsUIProvider; }
        }

        private AgentId PermissionMask;

        public PlatformService(string id, string description, AgentId provider) : this(id, description, provider, new AgentId("ANY@ANY"), false) { }
        public PlatformService(string id, string description, AgentId provider, AgentId permissionMask) : this(id, description, provider, permissionMask, false) { }
        public PlatformService(string id, string description, AgentId provider, AgentId permissionMask, bool providesUI)
        {
            Id = id;
            Description = description;
            Provider = provider;
            IsUIProvider = providesUI;
            PermissionMask = permissionMask;
        }

        public bool HasPermissions(AgentId agent)
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

        public static PlatformService FromXMLString(string xml)
        {
            XML.XMLTree serviceNode = XML.ParseXML(xml).GetNode((node) => node.Type == "service");
            return PlatformService.FromXMLNode(serviceNode);
        }

        public static PlatformService FromXMLNode(XML.XMLTree node)
        {
            if(node.Type != "service")
            {
                return null;
            }
            if (node.GetAttribute("id") == null || node.GetAttribute("provider") == null)
            {
                return null;
            }
            return new PlatformService(
                node.GetAttribute("id"),
                node.GetAttribute("description") ?? "",
                new AgentId(node.GetAttribute("provider")),
                new AgentId(node.GetAttribute("permissions") ?? "ANY@ANY"),
                node.GetAttribute("providesui") != null && node.GetAttribute("providesui") != "false"
                );
        }

        public bool Equals(PlatformService other)
        {
            return Id == other.Id && Provider == other.Provider;
        }

        public override bool Equals(Object obj)
        {
            if (obj == null)
                return false;

            PlatformService serviceObj = obj as PlatformService;
            if (serviceObj == null)
                return false;
            else
                return Equals(serviceObj);
        }

        public static bool operator ==(PlatformService service1, PlatformService service2)
        {
            if (((object)service1) == null || ((object)service2) == null)
                return Object.Equals(service1, service2);

            return service1.Equals(service2);
        }

        public static bool operator !=(PlatformService service1, PlatformService service2)
        {
            if (((object)service1) == null || ((object)service2) == null)
                return !Object.Equals(service1, service2);

            return !(service1.Equals(service2));
        }
        
    }
    //EMBED SEScripts.MultiAgentNetwork.MAN.Models.AgentId
}
