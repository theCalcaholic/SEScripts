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
using SEScripts.MultiAgentNetwork.MAN.Protocols;
using SEScripts.MultiAgentNetwork.MAN.Models;

namespace SEScripts.MultiAgentNetwork.MAN.Agents
{
    public class Agent
    {
        protected IMyTimerBlock Timer;
        protected TimeSpan ElapsedTimeValue;
        public TimeSpan ElapsedTime
        { get { return ElapsedTimeValue; } }
        protected bool RefreshScheduled;
        public AgentId Id;
        public MyGridProgram Prog;
        public IMyGridTerminalSystem GTS {
            get
            {
                return Prog.GridTerminalSystem;
            }
        }
        public Dictionary<string,Service> Services;
        protected Dictionary<int, AgentProtocol> Chats;
        protected List<AgentMessage> ScheduledMessages;
        protected Dictionary<string, List<AgentProtocol>> EventListeners;
        protected Dictionary<string, object> Knowledge;

        public Agent(MyGridProgram program)
        {
            Logger.debug("Agent constructor");
            Logger.IncLvl();
            DataStorage db = DataStorage.Load(program.Storage ?? "");
            Prog = program;
            ElapsedTimeValue = new TimeSpan(0);
            Timer = null;
            if (db.Exists<string>("id"))
            {
                Id = new AgentId(db.Get<string>("id"));
            }
            else
            {
                Id = new AgentId(program.Me.CustomName + "@local");
            }
            Knowledge = new Dictionary<string, object>();
            Services = new Dictionary<string, Service>();
            Chats = new Dictionary<int, AgentProtocol>();
            ScheduledMessages = new List<AgentMessage>();
            EventListeners = new Dictionary<string, List<AgentProtocol>>();

            new PrintProtocol(this).Setup();
            Logger.DecLvl();
        }

        public void Save(out string data)
        {
            DataStorage db = DataStorage.GetInstance();
            db.Set<string>("id", Id.ToString());
            db.Save(out data);
        }

        public void SetKnowledgeEntry(string key, object value, AgentProtocol chat)
        {
            SetKnowledgeEntry(key, value, chat, false);
        }

        public void SetKnowledgeEntry(string key, object value, AgentProtocol chat, bool global)
        {
            Logger.debug("Agent.SetKnowledgeEntry()");
            Logger.IncLvl();
            string actualKey = global ? key : chat.GetProtocolId() + "_" + key;
            Knowledge[actualKey] = value;
            Logger.DecLvl();
        }

        public object GetKnowledgeEntry(string key, AgentProtocol chat)
        {
            return GetKnowledgeEntry(key, chat, false);
        }

        public object GetKnowledgeEntry(string key, AgentProtocol chat, bool global)
        {
            Logger.debug("Agent.GetKnowledgeEntry()");
            Logger.IncLvl();
            string actualKey = global ? key : chat.GetProtocolId() + "_" + key;
            Logger.DecLvl();
            return Knowledge.GetValueOrDefault(actualKey, null);
        }

        public void OnEvent(string eventId, AgentProtocol chat)
        {
            Logger.debug("Agent.OnEvent( )");
            Logger.IncLvl();
            if (!EventListeners.ContainsKey(eventId))
            {
                EventListeners[eventId] = new List<AgentProtocol>();
            }
            EventListeners[eventId].Add(chat);
            Logger.DecLvl();
        }

        public void Event(string eventId)
        {
            Logger.debug("Agent.Event( " + eventId + ")");
            Logger.IncLvl();
            if (!EventListeners.ContainsKey(eventId))
            {
                Logger.DecLvl();
                return;
            }
            for (int i = EventListeners[eventId].Count - 1; i >= 0; i--)
            {
                if (EventListeners[eventId][i] == null)
                {
                    EventListeners[eventId].RemoveAt(i);
                }
                else
                {
                    EventListeners[eventId][i].NotifyEvent(eventId);
                }
            }
            Logger.DecLvl();
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

            if (Id.MatchesPlatform(msg.Receiver) && msg.Receiver.Name == "ALL")
            {
                AgentMessage sendMsg = msg.Duplicate();
                SendMessage(ref sendMsg);
            }

            if (status == AgentMessage.StatusCodes.UNKNOWNERROR)
            {
                return;
            }
            else if( status == AgentMessage.StatusCodes.RECEIVERNOTFOUND 
                || status == AgentMessage.StatusCodes.PLATFORMNOTFOUND
                || status == AgentMessage.StatusCodes.SERVICENOTFOUND 
                || status == AgentMessage.StatusCodes.CHATNOTFOUND )
            {
                if (Id.MatchesPlatform(msg.Receiver) && msg.Receiver.Name == "ANY")
                {
                    SendMessage(ref msg);
                }
                else
                {
                    AgentMessage sendMsg = msg.Duplicate();
                    if(!(
                        (status == AgentMessage.StatusCodes.RECEIVERNOTFOUND || status == AgentMessage.StatusCodes.PLATFORMNOTFOUND)
                        && SendMessage(ref sendMsg)))
                    {
                        sendMsg = msg.MakeResponse(
                            this.Id,
                            status,
                            ""
                            );
                        SendMessage(ref sendMsg);
                    }
                    if (status == AgentMessage.StatusCodes.RECEIVERNOTFOUND || status == AgentMessage.StatusCodes.PLATFORMNOTFOUND)
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
            }
            else if (status == AgentMessage.StatusCodes.CHATIDNOTACCEPTED)
            {
                AgentMessage response = msg.MakeResponse(
                    Id,
                    status,
                    "validId:" + AgentProtocol.ChatCount.ToString()
                );
                SendMessage(ref response);
            }



            Logger.DecLvl();

        }

        protected virtual AgentMessage.StatusCodes AssignMessage(AgentMessage message)
        {
            Logger.debug("Agent.AssignMessage()");
            Logger.IncLvl();

            
            if (!Id.MatchesPlatform(message.Receiver))
            {
                Logger.log("receiver platform does not match local platform.");
                return AgentMessage.StatusCodes.PLATFORMNOTFOUND;
            }
            else if(!Id.MatchesName(message.Receiver))
            {
                Logger.log("receiver does not match this agent.");
                return AgentMessage.StatusCodes.RECEIVERNOTFOUND;
            }
            else if (message.Service == "response")
            {
                if (!Chats.ContainsKey(message.ReceiverChatId))
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
                    Logger.log("Transmit message to chat " + message.ReceiverChatId.ToString());
                    Chats[message.ReceiverChatId].ReceiveMessage(message);
                    Logger.DecLvl();
                    return AgentMessage.StatusCodes.OK;
                }
            }
            else if (!Services.ContainsKey(message.Service))
            {
                Logger.log("WARNING: Service not found");
                if (message.Status == AgentMessage.StatusCodes.CHATNOTFOUND)
                {
                    Logger.log("WARNING: Requested chat id did not exist on '" + (message.Sender.ToString() ?? "") + "'.");
                    Logger.DecLvl();
                    return AgentMessage.StatusCodes.ABORT;
                }
                else if (message.Status == AgentMessage.StatusCodes.SERVICENOTFOUND)
                {
                    Logger.log("WARNING: Requested service '" + (message.Service ?? "") + "' did not exist on '" + (message.Sender.ToString() ?? "") + "'.");
                    Logger.DecLvl();
                    return AgentMessage.StatusCodes.ABORT;
                }
                else if (message.Status == AgentMessage.StatusCodes.SERVICENOTFOUND)
                {
                    Logger.log("WARNING: An unknown error occured at '" + (message.Sender.ToString() ?? "") + "'.");
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
                if (message.ReceiverChatId != -1)
                {
                    if (!chat.TrySetId(message.ReceiverChatId))
                    {
                        chat.Stop();
                        return AgentMessage.StatusCodes.CHATIDNOTACCEPTED;
                    }
                }
                AddChat(chat);
                Logger.log("Transfer message to chat.");
                chat.ReceiveMessage(message);
                Logger.DecLvl();
                return AgentMessage.StatusCodes.OK;
            }
        }

        public virtual bool SendMessage(ref AgentMessage msg)
        {
            Logger.debug("Agent.SendMessage");
            Logger.IncLvl();
            Logger.log("Sending message of '" + msg.Sender.ToString() + "' to '" + msg.Receiver.ToString() + "'...");
            Logger.log("Requested service: " + msg.Service);
            Logger.log("Message content: " + msg.Content);
            Logger.log("Message status: " + msg.Status.ToString());
            IMyProgrammableBlock targetBlock = null;
            Logger.log("comparing receiver platform '" + msg.Receiver.Platform + "' and own platform '" + Id.Platform + "'...");
            if (msg.Receiver == Id)
            {
                ReceiveMessage(msg);
            }
            else if (msg.Receiver.Name != Id.Name && (msg.Receiver.Platform == "local" || msg.Receiver.Platform == Id.Platform))
            {
                targetBlock = GTS.GetBlockWithName(msg.Receiver.Name) as IMyProgrammableBlock;
                if (targetBlock == null)
                {
                    Logger.log("WARNING: Receiver with id '" + msg.Receiver.ToString() + "' not found locally!");
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
            Logger.DecLvl();
            return true;
        }

        public void ScheduleMessage(AgentMessage msg)
        {
            Logger.debug("Agent.ScheduleMessage()");
            Logger.IncLvl();
            ScheduledMessages.Add(msg);
            ScheduleRefresh();
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
                SendMessage(ref msg);
            }
            Logger.DecLvl();
        }

        public void RegisterService(string id, Func<Agent, AgentProtocol> createChat)
        {
            RegisterService(id, createChat, new Dictionary<string, string> ());
        }

        public void RegisterService(string id, Func<Agent, AgentProtocol> createChat, Dictionary<string, string> options)
        {
            Logger.debug("Agent.RegisterService()");
            Logger.IncLvl();

            Services.Add(id, new Service(
                id, 
                (options.GetValueOrDefault("description") as String) ?? "",
                this.Id, 
                new AgentId(options.GetValueOrDefault("permissions", "ANY@ANY")),
                options.ContainsKey("providesui") && options["providesui"] != "false",
                createChat)
            );
            Logger.DecLvl();
        }

        public bool AddChat(AgentProtocol chat)
        {
            Logger.debug("Agent.AddChat();");
            Logger.IncLvl();
            if(Chats.ContainsKey(chat.ChatId))
            {
                Logger.DecLvl();
                return false;
            }
            else
            {
                Chats[chat.ChatId] = chat;
                Logger.DecLvl();
                return true;
            }
        }

        public bool UpdateChatId(int oldId, int newId)
        {
            if(!Chats.ContainsKey(oldId) || Chats.ContainsKey(newId))
            {
                return false;
            }
            else
            {
                Chats[newId] = Chats[oldId];
                Chats.Remove(oldId);
                return true;
            }

        }

        public void StopChat(int chatId)
        {
            if (Chats.ContainsKey(chatId))
            {
                StopChat(Chats[chatId]);
            }
        }
        public void StopChat(AgentProtocol chat)
        {
            Logger.log("Agent.StopChat() {" + chat.GetProtocolId() + "}");
            Logger.IncLvl();

            foreach(KeyValuePair<string, List<AgentProtocol>> listeners in EventListeners)
            {
                for (int i = listeners.Value.Count - 1; i >= 0; i--)
                {
                    if (listeners.Value[i] == chat)
                    {
                        listeners.Value.RemoveAt(i);
                    }
                }
            }
            Chats.Remove(chat.ChatId);
            Logger.DecLvl();
        }

        public void SetTimer(IMyTimerBlock timer)
        {
            Timer = timer;
        }

        public void ScheduleRefresh()
        {
            Logger.debug("Agent.ScheduleRefresh()");
            Logger.IncLvl();
            RefreshScheduled = true;
            if (Timer != null && !Timer.IsCountingDown)
            {
                Timer.GetActionWithName("Start").Apply(Timer);
            }
            Logger.DecLvl();

        }

        public virtual void Refresh(TimeSpan elapsedTime)
        {
            Logger.log("Agent.Refresh()");
            Logger.IncLvl();
            ElapsedTimeValue += elapsedTime;
            if (!RefreshScheduled)
            {
                return;
            }
            RefreshScheduled = false;
            SendScheduledMessages();
            Event("refresh");
            ElapsedTimeValue = new TimeSpan(0);
            Logger.DecLvl();
        }

    }

    //EMBED SEScripts.MultiAgentNetwork.MAN.Protocols.AgentProtocol
    //EMBED SEScripts.MultiAgentNetwork.MAN.Models.AgentMessage
    //EMBED SEScripts.MultiAgentNetwork.MAN.Models.AgentId
    //EMBED SEScripts.MultiAgentNetwork.MAN.Models.Service
    //EMBED SEScripts.MultiAgentNetwork.MAN.Protocols.PrintProtocol
}
