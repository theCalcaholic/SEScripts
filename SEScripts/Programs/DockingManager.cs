using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

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

namespace SEScripts.Programs.DockingManager
{
    class DockingManager
    {
        public enum ShipClass { CAPITAL, FREIGHTER, FIGHTER }
        Dictionary<string, DockingPort> Ports;

        public DockingManager()
        {
            Ports = new Dictionary<string, DockingPort>();
        }

        public void AddDockingPort(DockingPort port)
        {
            if(port != null)
            {
                if (Ports.ContainsKey(port.Name))
                    throw new Exception("ERROR: A port with the name '" + port.Name + "' was already added! Port names must be unique.");
                Ports.Add(port.Name, port);
            }
        }

        public string RequestDocking(ShipClass shipClass)
        {
            foreach(DockingPort port in Ports.Values)
            {
                if(!port.Enabled && port.CompatibleShipClass == shipClass)
                {
                    port.Enable();
                    return port.Name;
                }
            }
            return null;
        }

        public void DisablePort(string portName)
        {
            DockingPort port = Ports.GetValueOrDefault(portName, null);
            if(port != null)
            {
                port.Disable();
            }
        }

        public abstract class DockingPort
        {
            public enum PortClass { HANGAR, PAD, TUNNEL }
            protected IMyShipConnector _connector;
            protected ShipClass _compatibleShipClass;
            protected PortClass _class;
            protected string _name;
            protected bool _enabled;

            public IMyShipConnector Connector
            {
                get { return _connector; }
            }

            public ShipClass CompatibleShipClass
            {
                get { return _compatibleShipClass; }
            }

            public PortClass Class
            {
                get { return _class; }
            }

            public string Name
            {
                get { return _name; }
            }

            public bool Enabled
            {
                get { return _enabled; }
            }

            public DockingPort(string name, IMyShipConnector connector, ShipClass compatibleShips, PortClass portClass)
            {
                _connector = connector;
                _compatibleShipClass = compatibleShips;
                _class = portClass;
                _name = name;
                _enabled = false;
                Connector.Enabled = false;
            }

            public abstract void Enable();
            public abstract void Disable();
        }

        public class LandingPad : DockingPort
        {
            List<IMyLightingBlock> Lights;
            public LandingPad(string name, IMyShipConnector connector, ShipClass compatibleShips, 
                List<IMyLightingBlock> lights = null)
                : base(name, connector, compatibleShips, PortClass.PAD)
            {
                Lights = lights ?? new List<IMyLightingBlock>();
                Disable();
            }

            public override void Enable()
            {
                Connector.Enabled = true;
                foreach(IMyLightingBlock light in Lights)
                {
                    light.Color = Color.Green;
                }
            }

            public override void Disable()
            {
                _enabled = false;
                Connector.Enabled = false;
                foreach(IMyLightingBlock light in Lights)
                {
                    light.Color = Color.Red;
                }
            }
        }
    }
}
