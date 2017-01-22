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
using SEScripts.MultiAgentFramework.MAF.Protocols;
using SEScripts.MultiAgentFramework.MAF.Models;

namespace SEScripts.MultiAgentFramework.MAF.Agents
{
    public class PlatformAgent : Agent
    {
        public int RefreshInterval;
        public ServiceRegister PlatformServices;
        public List<IMyTextPanel> ReceptionBuffers;

        public PlatformAgent(MyGridProgram program) : base(program)
        {
            PlatformServices = new ServiceRegister();
            ReceptionBuffers = new List<IMyTextPanel>();
            RefreshInterval = 500;

            DataStorage db = DataStorage.Load(program.Storage ?? "");
            string name;
            if (db.Exists<string>("id"))
            {
                Id = new AgentId(db.Get<string>("id"));
                List<IMyTerminalBlock> buffers = new List<IMyTerminalBlock>();
                GTS.SearchBlocksOfName("RBUFFER-" + Id.Name, buffers);
                foreach (IMyTerminalBlock buffer in buffers)
                {
                    IMyTextPanel tBuffer = buffer as IMyTextPanel;
                    if (tBuffer != null)
                    {
                        ReceptionBuffers.Add(tBuffer);
                    }
                }
            }
            else
            {
                System.Text.RegularExpressions.Regex braceRegex = new System.Text.RegularExpressions.Regex(@"-{.*}$");
                if (braceRegex.IsMatch(program.Me.CustomName))
                {
                    name = braceRegex.Replace(program.Me.CustomName, "-{" + PlatformAgent.GenerateSuffix() + "}");
                }
                else
                {
                    name = program.Me.CustomName + "-{" + PlatformAgent.GenerateSuffix() + "}";
                }
                Id = new AgentId(name + "@" + name);
            }
            program.Me.SetCustomName(Id.Name);

            ServiceRegistrationProtocol.Platform.RegisterServices(this);
            MessageRoutingProtocol.RegisterServices(this);
            ProvideServiceInformationProtocol.RegisterServices(this);
        }

        public void RegisterBuffers(List<string> bufferNames)
        {
            foreach (string name in bufferNames)
            {
                IMyTextPanel buffer = GTS.GetBlockWithName(name) as IMyTextPanel;
                if (buffer != null)
                {
                    buffer.SetCustomName("RBUFFER-" + Id.Name);
                    buffer.WritePublicText("");
                    buffer.WritePrivateText("");
                    ReceptionBuffers.Add(buffer);
                }
            }
        }

        public void CollectPlatformMessages()
        {
            foreach (IMyTextPanel buffer in ReceptionBuffers)
            {
                string text = buffer.GetPrivateText();
                buffer.WritePrivateText("");
                List<XML.XMLTree> messages = XML.ParseXML(Parser.UnescapeQuotes(text)).GetAllNodes(node => node.Type == "message");
                foreach (XML.XMLTree message in messages)
                {
                    ReceiveMessage((AgentMessage)message);
                }
            }
        }

        public override void ReceiveMessage(AgentMessage message)
        {
            if (message.Receiver.Platform == Id.Name)
            {
                message.Receiver.Platform = "local";
            }

            Logger.log("Looking for agent '" + message.Receiver.Id + "'...");
            AgentMessage.StatusCodes status = AssignMessage(message);
            Logger.log("Status: " + status.ToString());
            ReceiveMessage(message, status);
        }

        public override void ReceiveMessage(AgentMessage message, AgentMessage.StatusCodes status)
        {
            if (message.Receiver.Platform == "local")
            {
                message.Receiver.Platform = Id.Platform;
            }
            bool forwarded = false;
            if (status == AgentMessage.StatusCodes.RECEIVERNOTFOUND)
            {
                Logger.log("no receiver found; looking for service '" + message.Service + "'...");
                if (message.Service != null && PlatformServices.ContainsKey(message.Service))
                {
                    Logger.log("service is known.");
                    AgentId receiver = null;
                    if (message.Receiver == new AgentId("ANY@local") || message.Receiver == new AgentId("ANY@ANY"))
                    {
                        if (PlatformServices[message.Service].Count > 0)
                        {
                            receiver = PlatformServices[message.Service][0].Provider;
                        }
                        else if (message.Receiver.Platform != "ANY")
                        {
                            receiver = null;
                        }
                    }
                    else
                    {
                        receiver = PlatformServices[message.Service].Find(service => service.Provider == message.Receiver).Provider;
                    }
                    if (receiver != null)
                    {
                        Logger.log("Capable agent found: '" + receiver.Name + "'");
                        message.Receiver = receiver;
                        status = AgentMessage.StatusCodes.OK;
                    }
                    else
                    {
                        Logger.log("No capable agent found.");
                    }

                }
                //TODO: Implement forwarding to other platforms;
                Logger.log("Trying to forward message... to '" + message.Receiver.Id + "'.");
                if (SendMessage(message))
                {
                    Logger.log("message forwarded.");
                    forwarded = true;
                }
                else
                {
                    base.ReceiveMessage(message, status);
                }
            }
            else
            {
                base.ReceiveMessage(message, status);
            }
        }

        public override bool SendMessage(AgentMessage message)
        {
            if (base.SendMessage(message))
            {
                Logger.log("base.SendMessage(message) did succeed.");
                return true;
            }
            else if (message.Sender.Platform == "local")
            {
                message.Sender.Platform = Id.Name;
            }

            if (message.Receiver.Platform != Id.Name)
            {
                Logger.log("Calling SendToPlatform(message, platform)...");
                return SendToPlatform(message, message.Receiver.Platform);
            }
            else
            {
                return false;
            }

        }

        public bool SendToPlatform(AgentMessage message, string platform)
        {
            if (platform == "ALL")
            {
                List<IMyTerminalBlock> buffers = new List<IMyTerminalBlock>();
                GTS.SearchBlocksOfName("RBUFFER-", buffers,
                    (block) => (
                        block.CustomName != "RBUFFER-" + Id.Name
                        && (block as IMyTextPanel) != null)
                );
                bool success = false;
                foreach (IMyTerminalBlock buffer in buffers)
                {
                    string platformName = buffer.CustomName.Replace("RBUFFER-", "");
                    if (platformName != Id.Name)
                    {
                        success = success || SendToPlatform(message, platformName);
                    }
                }
                return success;
            }
            else
            {
                Logger.log("Trying to send to platform '" + platform + "'...");
                IMyTextPanel buffer = GTS.GetBlockWithName("RBUFFER-" + platform) as IMyTextPanel;
                if (buffer == null)
                {
                    Logger.log("No corresponding buffer found with name 'RBUFFER-" + platform + "'.");
                    return false;
                }
                else
                {
                    Logger.log("Writing message to reception buffer of platform '" + platform + "'...");
                    buffer.WritePrivateText(message.ToString(), true);
                    return true;
                }
            }

        }

        public static string GenerateSuffix()
        {
            Logger.debug("PlatformAgent.GenerateSuffix()");
            Logger.IncLvl();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            Random rnd = new Random();
            char[] title = new char[8];
            for (int i = 0; i < 8; i++)
            {
                title[i] = chars[rnd.Next(0, chars.Length)];
            }
            Logger.DecLvl();
            return new string(title);
        }

        public override void Refresh(TimeSpan elapsedTime)
        {
            bool refreshScheduled = RefreshScheduled
                || (ElapsedTime.Milliseconds + elapsedTime.Milliseconds >= RefreshInterval);
            RefreshScheduled = refreshScheduled;
            if (Timer != null)
            {
                if (RefreshInterval < 1000)
                {
                    Timer.GetActionWithName("TriggerNow").Apply(Timer);
                }
                else
                {
                    Timer.GetActionWithName("Start").Apply(Timer);
                }
            }
            base.Refresh(elapsedTime);
            if (!refreshScheduled)
            {
                return;
            }
            CollectPlatformMessages();
        }
    }

    //EMBED SEScripts.MultiAgentFramework.MAF.Agents.Agent
    //EMBED SEScripts.MultiAgentFramework.MAF.Protocols.ServiceRegistrationProtocol
    //EMBED SEScripts.MultiAgentFramework.MAF.Protocols.MessageRoutingProtocol
    //EMBED SEScripts.MultiAgentFramework.MAF.Protocols.ProvideServiceInformationProtocol
}
