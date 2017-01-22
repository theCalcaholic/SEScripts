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
    public class Agent
    {
        protected IMyTimerBlock Timer;
        protected TimeSpan ElapsedTime;
        protected bool RefreshScheduled;
        public AgentId Id;
        protected IMyGridTerminalSystem GTS;
        public Dictionary<string,Service> Services;
        protected Dictionary<int, AgentProtocol> Chats;
        protected List<AgentMessage> ScheduledMessages;
        protected Dictionary<string, List<AgentProtocol>> EventListeners;

        public Agent(MyGridProgram program)
        {
            Logger.debug("Agent constructor");
            Logger.IncLvl();
            DataStorage db = DataStorage.Load(program.Storage ?? "");
            GTS = program.GridTerminalSystem;
            ElapsedTime = new TimeSpan(0);
            Timer = null;
            if (db.Exists<string>("id"))
            {
                Id = new AgentId(db.Get<string>("id"));
            }
            else
            {
                Id = new AgentId(program.Me.CustomName + "@local");
            }
            Services = new Dictionary<string, Service>();
            Chats = new Dictionary<int, AgentProtocol>();
            ScheduledMessages = new List<AgentMessage>();
            EventListeners = new Dictionary<string, List<AgentProtocol>>();

            PrintProtocol.RegisterServices(this);
            Logger.DecLvl();
        }

        public void Save(out string data)
        {
            DataStorage db = DataStorage.GetInstance();
            db.Set<string>("id", Id.Id);
            db.Save(out data);
        }

        public void OnEvent(string eventId, AgentProtocol chat)
        {
            if (!EventListeners.ContainsKey(eventId))
            {
                EventListeners[eventId] = new List<AgentProtocol>();
            }
            EventListeners[eventId].Add(chat);
        }

        public void Event(string eventId)
        {
            if (!EventListeners.ContainsKey(eventId))
            {
                return;
            }
            foreach (AgentProtocol chat in EventListeners[eventId])
            {
                if (chat != null)
                {
                    chat.NotifyEvent(eventId);
                }
            }
        }

        public virtual void ReceiveMessage(AgentMessage msg)
        {
            Logger.debug("Agent.ReceiveMessage(AgentMessage)");
            Logger.IncLvl();
            AgentMessage.StatusCodes status = AssignMessage(msg);
            ReceiveMessage(msg, status);
            Logger.DecLvl();
        }

        public virtual void ReceiveMessage(AgentMessage msg, AgentMessage.StatusCodes status)
        {
            Logger.debug("Agent.ReceiveMessage(AgentMessage, AgentMessage.StatusCode)");
            Logger.IncLvl();
            if (
                status == AgentMessage.StatusCodes.UNKNOWNERROR
                || status == AgentMessage.StatusCodes.CHATNOTFOUND
                || status == AgentMessage.StatusCodes.SERVICENOTFOUND
                || status == AgentMessage.StatusCodes.RECEIVERNOTFOUND
            )
            {
                SendMessage(
                    msg.MakeResponse(
                        this.Id,
                        status,
                        ""
                    )
                );
                if (status == AgentMessage.StatusCodes.RECEIVERNOTFOUND)
                {
                    Logger.log("WARNING: Message Receiver Id does not conform with this agent's Id!");
                }
                else if (status == AgentMessage.StatusCodes.CHATNOTFOUND)
                {
                    Logger.log("WARNING: ChatId not found!");
                }
                else if (status == AgentMessage.StatusCodes.SERVICENOTFOUND)
                {
                    Logger.log("WARNING: No service with id '" + msg.Service + "' found!");
                }
            }
            else if (status == AgentMessage.StatusCodes.CHATIDNOTACCEPTED)
            {
                SendMessage(msg.MakeResponse(
                    this.Id,
                    status,
                    "validId:" + AgentProtocol.ChatCount.ToString()
                ));
            }
            Logger.DecLvl();

        }

        protected virtual AgentMessage.StatusCodes AssignMessage(AgentMessage message)
        {
            Logger.debug("Agent.AssignMessage()");
            Logger.IncLvl();
            if (message.Receiver != Id)
            {
                Logger.log("receiver does not match this agent.");
                return AgentMessage.StatusCodes.RECEIVERNOTFOUND;
            }
            else if (message.Service == "response")
            {
                if (!Chats.ContainsKey(message.ChatId))
                {
                    if (
                        message.Status != AgentMessage.StatusCodes.UNKNOWNERROR
                        && message.Status != AgentMessage.StatusCodes.CHATNOTFOUND
                        && message.Status != AgentMessage.StatusCodes.SERVICENOTFOUND
                    )
                    {
                        return AgentMessage.StatusCodes.ABORT;
                    }
                    Logger.DecLvl();
                    return AgentMessage.StatusCodes.CHATNOTFOUND;
                }
                else
                {
                    Logger.log("Transmit message to chat " + message.ChatId.ToString());
                    Chats[message.ChatId].ReceiveMessage(message);
                    return AgentMessage.StatusCodes.OK;
                }
            }
            else if (!Services.ContainsKey(message.Service))
            {
                Logger.log("WARNING: Service not found");
                if (message.Status == AgentMessage.StatusCodes.CHATNOTFOUND)
                {
                    Logger.log("WARNING: Requested chat id did not exist on '" + (message.Sender.Id ?? "") + "'.");
                    Logger.DecLvl();
                    return AgentMessage.StatusCodes.ABORT;
                }
                else if (message.Status == AgentMessage.StatusCodes.SERVICENOTFOUND)
                {
                    Logger.log("WARNING: Requested service '" + (message.Service ?? "") + "' did not exist on '" + (message.Sender.Id ?? "") + "'.");
                    Logger.DecLvl();
                    return AgentMessage.StatusCodes.ABORT;
                }
                else if (message.Status == AgentMessage.StatusCodes.SERVICENOTFOUND)
                {
                    Logger.log("WARNING: An unknown error occured at '" + (message.Sender.Id ?? "") + "'.");
                    Logger.DecLvl();
                    return AgentMessage.StatusCodes.ABORT;
                }
                else
                {
                    Logger.DecLvl();
                    return AgentMessage.StatusCodes.SERVICENOTFOUND;
                }
            }
            else
            {
                Logger.log("create protocol '" + message.Service + "'.");
                AgentProtocol chat = Services[message.Service].Create(this);
                if (message.ChatId != -1)
                {
                    if (!chat.TrySetId(message.ChatId))
                    {
                        chat.Stop();
                        return AgentMessage.StatusCodes.CHATIDNOTACCEPTED;
                    }
                }
                Chats[chat.ChatId] = chat;
                Logger.log("Transfer message to chat.");
                chat.ReceiveMessage(message);
                Logger.DecLvl();
                return AgentMessage.StatusCodes.OK;
            }
        }

        public virtual bool SendMessage(AgentMessage msg)
        {
            Logger.debug("Agent.SendMessage");
            Logger.IncLvl();
            Logger.log("Sending message of '" + msg.Sender.Id + "' to '" + msg.Receiver.Id + "'...");
            IMyProgrammableBlock targetBlock;
            Logger.log("comparing receiver platform '" + msg.Receiver.Platform + "' and own platform '" + Id.Platform + "'...");
            if (msg.Receiver.Platform == "local" || msg.Receiver.Platform == Id.Platform)
            {
                targetBlock = GTS.GetBlockWithName(msg.Receiver.Name) as IMyProgrammableBlock;
                if (targetBlock == null)
                {
                    Logger.log("WARNING: Receiver with id '" + msg.Receiver.Id + "' not found locally!");
                }
            }
            else
            {
                Logger.log("Receiver not local. Trying to find corresponding platform agent.");
                targetBlock = GTS.GetBlockWithName(msg.Receiver.Platform) as IMyProgrammableBlock;
                if (targetBlock == null)
                {
                    if (Id.Platform != Id.Name)
                    {
                        targetBlock = GTS.GetBlockWithName(Id.Platform) as IMyProgrammableBlock;
                    }
                    if (targetBlock == null)
                    {
                        Logger.log("WARNING: Not registered at any platform! Only local communication possible!");
                    }
                }
            }
            if (targetBlock == null)
            {
                Logger.DecLvl();
                return false;
            }
            if (msg.Receiver.Platform == "local" && msg.Sender.Platform == Id.Platform)
            {
                msg.Sender.Platform = "local";
            }

            if (!targetBlock.TryRun("message \"" + msg.ToString() + "\""))
            {
                ScheduleMessage(msg);
            }
            if (ScheduledMessages.Count > 0)
            {
                ScheduleRefresh();
            }
            Logger.DecLvl();
            return true;
        }

        public void ScheduleMessage(AgentMessage msg)
        {
            Logger.debug("Agent.ScheduleMessage()");
            Logger.IncLvl();
            ScheduledMessages.Add(msg);
            Logger.DecLvl();
        }

        public void SendScheduledMessages()
        {
            Logger.debug("Agent.SendScheduledMessages()");
            Logger.IncLvl();
            for (int i = ScheduledMessages.Count - 1; i >= 0; i--)
            {
                AgentMessage msg = ScheduledMessages[i];
                ScheduledMessages.RemoveAt(i);
                SendMessage(msg);
            }
            Logger.DecLvl();
        }

        public void RegisterService(string id, Func<Agent, AgentProtocol> createChat)
        {
            RegisterService(id, "", createChat);
        }

        public void RegisterService(string id, string description, Func<Agent, AgentProtocol> createChat)
        {
            Logger.debug("Agent.RegisterService()");
            Logger.IncLvl();
            Services.Add(id, new Service(id, description, this.Id, createChat));
            Logger.DecLvl();
        }

        public void StopChat(int chatId)
        {
            Chats.Remove(chatId);
        }

        public void SetTimer(IMyTimerBlock timer)
        {
            Timer = timer;
        }

        public void ScheduleRefresh()
        {
            RefreshScheduled = true;
            if (Timer != null && !Timer.IsCountingDown)
            {
                Timer.GetActionWithName("Start").Apply(Timer);
            }

        }

        public virtual void Refresh(TimeSpan elapsedTime)
        {
            ElapsedTime += elapsedTime;
            if (!RefreshScheduled)
            {
                return;
            }
            RefreshScheduled = false;
            SendScheduledMessages();
            ElapsedTime = new TimeSpan(0);
        }

    }

    //EMBED SEScripts.MultiAgentFramework.MAF.Protocols.AgentProtocol
    //EMBED SEScripts.MultiAgentFramework.MAF.Models.AgentMessage
    //EMBED SEScripts.MultiAgentFramework.MAF.Models.AgentId
    //EMBED SEScripts.MultiAgentFramework.MAF.Models.Service
    //EMBED SEScripts.MultiAgentFramework.MAF.Protocols.PrintProtocol
}
