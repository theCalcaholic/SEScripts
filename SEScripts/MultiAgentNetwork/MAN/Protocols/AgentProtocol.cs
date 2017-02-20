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
using SEScripts.MultiAgentNetwork.MAN.Agents;
using SEScripts.MultiAgentNetwork.MAN.Models;

namespace SEScripts.MultiAgentNetwork.MAN.Protocols
{
    public abstract class AgentProtocol
    {
        static int ChatCountValue;
        private int ChatIdValue;
        private int PartnerIdValue;
        protected Agent Holder;
        public int TTL;
        

        public static int ChatCount
        {
            get
            {
                return ChatCountValue;
            }
        }

        public int ChatId
        {
            get
            {
                return ChatIdValue;
            }
        }

        public int PartnerId
        {
            get { return PartnerIdValue; }
        }

        public abstract string GetProtocolId();

        public virtual void NotifyEvent(string eventId)
        {

        }

        public AgentProtocol(Agent agent)
        {
            Logger.debug("AgentProtocol constructor()");
            Logger.IncLvl();
            ChatIdValue = ChatCount;
            ChatCountValue++;
            Holder = agent;
            Logger.DecLvl();
        }

        public bool TrySetId(int id)
        {
            Logger.debug("AgentProtocol.TrySetId()");
            Logger.IncLvl();
            if (id == ChatId)
            {
                Logger.DecLvl();
                return true;
            }
            else if (id >= ChatCount)
            {
                ChatCountValue = id + 1;
                ChatIdValue = id;
                Logger.DecLvl();
                return true;
            }
            else
            {
                Logger.DecLvl();
                return false;
            }
        }

        public abstract void Restart();

        public virtual void Start() { }

        public virtual void Stop()
        {
            Logger.debug("AgentProtocol.Stop()");
            if (ChatId == ChatCount - 1)
            {
                ChatCountValue = ChatId;
            }
            Holder.StopChat(ChatId);
            Logger.DecLvl();
        }

        public virtual void ReceiveMessage(AgentMessage msg)
        {
            Logger.debug("AgentProtocol.ReceiveMessage()");
            Logger.IncLvl();
            if (msg.Status == AgentMessage.StatusCodes.CHATIDNOTACCEPTED)
            {
                string[] contentSplit = msg.Content.Split(':');
                if (contentSplit.Length == 2 && contentSplit[0] == "validId")
                {
                    int oldId = ChatId;
                    int desiredId = -1;
                    if (!Int32.TryParse(contentSplit[1], out desiredId))
                    {
                        Logger.log("WARNING: Invalid chat id requested!");
                        Stop();
                    } else
                    {
                        desiredId = Math.Max(ChatCount, desiredId);
                        if (Holder.UpdateChatId(oldId, desiredId) && TrySetId(desiredId))
                        {
                            Restart();
                        }
                        else
                        {
                            Logger.log("ERROR: Could not change chat id");
                            Stop();
                        }
                    }
                }
            }
            Logger.DecLvl();
        }

        public static string MakeRoute(AgentId aid, string protocolId, string argument)
        {
            return "man:" + aid + "::" + protocolId + "(" + Parser.Sanitize(argument) + ")";
        }

        public virtual void Setup() { }

    }
    

    //EMBED SEScripts.MultiAgentNetwork.MAN.Agents.Agent
    //EMBED SEScripts.MultiAgentNetwork.MAN.Models.AgentMessage
}
