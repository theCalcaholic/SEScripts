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
using SEScripts.MultiAgentFramework.MAF.Agents;
using SEScripts.MultiAgentFramework.MAF.Models;

namespace SEScripts.MultiAgentFramework.MAF.Protocols
{
    public abstract class AgentProtocol
    {
        static int ChatCountValue;
        private int ChatIdValue;
        protected Agent Holder;
        public static string Id
        {
            get { return null; }
        }

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
            if (id == ChatId)
            {
                return true;
            }
            else if (id >= ChatCount)
            {
                ChatCountValue = id + 1;
                ChatIdValue = id;
                return true;
            }
            else
            {
                return false;
            }
        }

        public abstract void Restart();

        public virtual void Start() { }

        public virtual void Stop()
        {
            if (ChatId == ChatCount - 1)
            {
                ChatCountValue = ChatId;
            }
            Holder.StopChat(ChatId);
        }

        public virtual void ReceiveMessage(AgentMessage msg)
        {
            if (msg.Status == AgentMessage.StatusCodes.CHATIDNOTACCEPTED)
            {
                string[] contentSplit = msg.Content.Split(':');
                if (contentSplit.Length == 2 && contentSplit[0] == "validId")
                {
                    int chatId = -1;
                    if (Int32.TryParse(contentSplit[1], out chatId))
                    {
                        TrySetId(Math.Max(ChatCount, chatId));
                        Restart();
                    }
                    else
                    {
                        Logger.log("ERROR: Could not change chat id");
                    }
                }
            }
        }

        public static void RegisterServices(Agent agent) { }

    }

    //EMBED SEScripts.MultiAgentFramework.MAF.Agents.Agent
    //EMBED SEScripts.MultiAgentFramework.MAF.Models.AgentMessage
}
