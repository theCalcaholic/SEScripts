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
using System.Linq;

using SEScripts.MultiAgentNetwork.MAN.Protocols;
using SEScripts.MultiAgentNetwork.MAN.Models;
using SEScripts.ParseLib.XUI;
using SEScripts.Lib.DataStorage;

namespace SEScripts.MultiAgentNetwork.MAN.Agents
{
    //WRAPPER: SEScripts.MultiAgentFramework.MAFWRAPPER
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
                Id = GenerateId(program.Me);
            }
            program.Me.CustomName = Id.Name;

            new ServiceRegistrationProtocol.Platform(this).Setup();
            //MessageRoutingProtocol.RegisterServices(this);
            new ProvideServiceInformationProtocol(this).Setup();
        }

        public void RegisterBuffers(List<string> bufferNames)
        {
            foreach (string name in bufferNames)
            {
                IMyTextPanel buffer = GTS.GetBlockWithName(name) as IMyTextPanel;
                if (buffer != null)
                {
                    buffer.CustomName = "RBUFFER-" + Id.Name;
                    buffer.WritePublicText("");
                    buffer.CustomData = "";
                    ReceptionBuffers.Add(buffer);
                }
            }
        }

        public void CollectPlatformMessages()
        {
            foreach (IMyTextPanel buffer in ReceptionBuffers)
            {
                string text = buffer.CustomData;
                buffer.CustomData = "";
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

            Logger.log("Looking for agent '" + message.Receiver + "'...");
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
            

            if (status == AgentMessage.StatusCodes.RECEIVERNOTFOUND || status == AgentMessage.StatusCodes.PLATFORMNOTFOUND)
            {
                Logger.log("no receiver found; looking for service '" + message.Service + "'...");
                if (message.Service != null && PlatformServices.ContainsKey(message.Service))
                {
                    Logger.log("service is known.");
                    if (message.Receiver.MatchesPlatform(Id))
                    {
                        if(PlatformServices.ContainsKey(message.Service))
                        {
                            List<PlatformService> pServices = PlatformServices[message.Service].FindAll(
                                service => message.Receiver.Matches(service.Provider) && message.Sender != service.Provider);
                            if(message.Receiver.Name == "ALL")
                            {
                                foreach(PlatformService pService in pServices)
                                {
                                    AgentMessage sendMsg = message.Duplicate();
                                    sendMsg.Receiver = pService.Provider;
                                    forwarded |= SendMessage( ref sendMsg);
                                }

                            }
                            else
                            {
                                if(pServices.ElementAtOrDefault(0) != null)
                                {
                                    message.Receiver = pServices[0].Provider;
                                    forwarded = SendMessage(ref message);

                                }
                            }

                        }
                    }

                }
                if (!forwarded)
                {
                    base.ReceiveMessage(message, status);
                }
            }
            else
            {
                base.ReceiveMessage(message, status);
            }
        }

        public override bool SendMessage(ref AgentMessage message)
        {
            if (base.SendMessage(ref message))
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
            Logger.IncLvl();
            Logger.debug("PlatformAgent.SendToPlatform()");
            if (platform == "ALL" || platform == "ANY")
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
                        if( SendToPlatform(message, platformName) )
                        {
                            success = true;
                            if(platform == "ANY")
                            {
                                return true;
                            }
                        }
                    }
                }
                Logger.DecLvl();
                return success;
            }
            else
            {
                Logger.log("Trying to send to platform '" + platform + "'...");
                IMyTextPanel buffer = GTS.GetBlockWithName("RBUFFER-" + platform) as IMyTextPanel;
                if (buffer == null)
                {
                    Logger.log("No corresponding buffer found with name 'RBUFFER-" + platform + "'.");
                    Logger.DecLvl();
                    return false;
                }
                else
                {
                    Logger.log("Writing message to reception buffer of platform '" + platform + "'...");
                    buffer.CustomData += message.ToString();
                    Logger.DecLvl();
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

        public virtual AgentId GenerateId(IMyProgrammableBlock me)
        {
            System.Text.RegularExpressions.Regex braceRegex = new System.Text.RegularExpressions.Regex(@"-{.*}$");
            string name;
            if (braceRegex.IsMatch(me.CustomName))
            {
                name = braceRegex.Replace(me.CustomName, "-{" + PlatformAgent.GenerateSuffix() + "}");
            }
            else
            {
                name = me.CustomName + "-{" + PlatformAgent.GenerateSuffix() + "}";
            }
            return new AgentId(name + "@" + name);
        }
    }

    //EMBED SEScripts.MultiAgentNetwork.MAN.Agents.Agent
    //EMBED SEScripts.MultiAgentNetwork.MAN.Protocols.ServiceRegistrationProtocol
    //!EMBED SEScripts.MultiAgentNetwork.MAN.Protocols.MessageRoutingProtocol
    //EMBED SEScripts.MultiAgentNetwork.MAN.Protocols.ProvideServiceInformationProtocol
}
