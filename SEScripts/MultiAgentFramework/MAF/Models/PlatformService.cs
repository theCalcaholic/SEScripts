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

namespace SEScripts.MultiAgentFramework.MAF.Models
{
    public class PlatformService
    {
        public string Id;
        public string Description;
        public AgentId Provider;
        private bool isUIProvider;
        public bool ProvidesUI
        {
            get { return isUIProvider; }
        }

        public PlatformService(string id, string description, AgentId provider) : this(id, description, provider, false) { }
        public PlatformService(string id, string description, AgentId provider, bool providesUI)
        {
            Id = id;
            Description = description;
            Provider = provider;
            isUIProvider = providesUI;
        }

        public string ToXML()
        {
            return "<service id='" + Id.ToString() + "' description='" + Description + "' provider='" + Provider.Id + "'/>";
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
}
