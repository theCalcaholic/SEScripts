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
		public class MANAgent
		{
			protected IMyTimerBlock Timer;
			protected TimeSpan ElapsedTimeValue;
			public TimeSpan ElapsedTime
			{ get { return ElapsedTimeValue; } }
			protected bool RefreshScheduled;
			public MANAgentId Id;
			public MyGridProgram Prog;
			public IMyGridTerminalSystem GTS
			{
				get
				{
					return Prog.GridTerminalSystem;
				}
			}
			public Dictionary<string, MANService> Services;
			protected Dictionary<int, MANAgentProtocol> Chats;
			protected List<MANAgentMessage> ScheduledMessages;
			protected Dictionary<string, List<MANAgentProtocol>> EventListeners;
			protected Dictionary<string, object> Knowledge;

			public MANAgent(MyGridProgram program)
			{
				//Logger.debug("Agent constructor");
				//Logger.IncLvl();
				DataStorage db = DataStorage.Load(program.Storage ?? "");
				Prog = program;
				ElapsedTimeValue = new TimeSpan(0);
				Timer = null;
				if (db.Exists<string>("id"))
				{
					Id = new MANAgentId(db.Get<string>("id"));
				}
				else
				{
					Id = new MANAgentId(program.Me.CustomName + "@local");
				}
				Knowledge = new Dictionary<string, object>();
				Services = new Dictionary<string, MANService>();
				Chats = new Dictionary<int, MANAgentProtocol>();
				ScheduledMessages = new List<MANAgentMessage>();
				EventListeners = new Dictionary<string, List<MANAgentProtocol>>();

				new MANPrintProtocol(this).Setup();
				//Logger.DecLvl();
			}

			public void Save(out string data)
			{
				DataStorage db = DataStorage.GetInstance();
				db.Set<string>("id", Id.ToString());
				db.Save(out data);
			}

			public void SetKnowledgeEntry(string key, object value, MANAgentProtocol chat)
			{
				SetKnowledgeEntry(key, value, chat, false);
			}

			public void SetKnowledgeEntry(string key, object value, MANAgentProtocol chat, bool global)
			{
				//Logger.debug("Agent.SetKnowledgeEntry()");
				//Logger.IncLvl();
				string actualKey = global ? key : chat.GetProtocolId() + "_" + key;
				Knowledge[actualKey] = value;
				//Logger.DecLvl();
			}

			public object GetKnowledgeEntry(string key, MANAgentProtocol chat)
			{
				return GetKnowledgeEntry(key, chat, false);
			}

			public object GetKnowledgeEntry(string key, MANAgentProtocol chat, bool global)
			{
				//Logger.debug("Agent.GetKnowledgeEntry()");
				//Logger.IncLvl();
				string actualKey = global ? key : chat.GetProtocolId() + "_" + key;
				//Logger.DecLvl();
				return Knowledge.GetValueOrDefault(actualKey, null);
			}

			public void OnEvent(string eventId, MANAgentProtocol chat)
			{
				//Logger.debug("Agent.OnEvent( )");
				//Logger.IncLvl();
				if (!EventListeners.ContainsKey(eventId))
				{
					EventListeners[eventId] = new List<MANAgentProtocol>();
				}
				EventListeners[eventId].Add(chat);
				//Logger.DecLvl();
			}

			public void Event(string eventId)
			{
				//Logger.debug("Agent.Event( " + eventId + ")");
				//Logger.IncLvl();
				if (!EventListeners.ContainsKey(eventId))
				{
					//Logger.DecLvl();
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
				//Logger.DecLvl();
			}

			public virtual void ReceiveMessage(MANAgentMessage msg)
			{
				MANAgentMessage.StatusCodes status = AssignMessage(msg);
				ReceiveMessage(msg, status);
			}

			public virtual void ReceiveMessage(MANAgentMessage msg, MANAgentMessage.StatusCodes status)
			{
				if (Id.MatchesPlatform(msg.Receiver) && msg.Receiver.Name == "ALL")
				{
					MANAgentMessage sendMsg = msg.Duplicate();
					SendMessage(ref sendMsg);
				}

				if (status == MANAgentMessage.StatusCodes.UNKNOWNERROR)
				{
					return;
				}
				else if (status == MANAgentMessage.StatusCodes.RECEIVERNOTFOUND
					|| status == MANAgentMessage.StatusCodes.PLATFORMNOTFOUND
					|| status == MANAgentMessage.StatusCodes.SERVICENOTFOUND
					|| status == MANAgentMessage.StatusCodes.CHATNOTFOUND)
				{
					if (Id.MatchesPlatform(msg.Receiver) && msg.Receiver.Name == "ANY")
					{
						SendMessage(ref msg);
					}
					else
					{
						MANAgentMessage sendMsg = msg.Duplicate();
						if (!(
							(status == MANAgentMessage.StatusCodes.RECEIVERNOTFOUND || status == MANAgentMessage.StatusCodes.PLATFORMNOTFOUND)
							&& SendMessage(ref sendMsg)))
						{
							sendMsg = msg.MakeResponse(
								this.Id,
								status,
								""
								);
							SendMessage(ref sendMsg);
						}
					}
				}
				else if (status == MANAgentMessage.StatusCodes.CHATIDNOTACCEPTED)
				{
					MANAgentMessage response = msg.MakeResponse(
						Id,
						status,
						"validId:" + MANAgentProtocol.ChatCount.ToString()
					);
					SendMessage(ref response);
				}
			}

			protected virtual MANAgentMessage.StatusCodes AssignMessage(MANAgentMessage message)
			{
				if (!Id.MatchesPlatform(message.Receiver))
					return MANAgentMessage.StatusCodes.PLATFORMNOTFOUND;
				else if (!Id.MatchesName(message.Receiver))
					return MANAgentMessage.StatusCodes.RECEIVERNOTFOUND;
				else if (message.Service == "response")
				{
					if (!Chats.ContainsKey(message.ReceiverChatId))
					{
						if (
							message.Status != MANAgentMessage.StatusCodes.UNKNOWNERROR
							&& message.Status != MANAgentMessage.StatusCodes.CHATNOTFOUND
							&& message.Status != MANAgentMessage.StatusCodes.SERVICENOTFOUND
						)
						{
							return MANAgentMessage.StatusCodes.ABORT;
						}
						return MANAgentMessage.StatusCodes.CHATNOTFOUND;
					}
					else
					{
						Chats[message.ReceiverChatId].ReceiveMessage(message);
						return MANAgentMessage.StatusCodes.OK;
					}
				}
				else if (!Services.ContainsKey(message.Service))
				{
					if (message.Status == MANAgentMessage.StatusCodes.CHATNOTFOUND)
						return MANAgentMessage.StatusCodes.ABORT;
					else if (message.Status == MANAgentMessage.StatusCodes.SERVICENOTFOUND)
						return MANAgentMessage.StatusCodes.ABORT;
					else if (message.Status == MANAgentMessage.StatusCodes.SERVICENOTFOUND)
						return MANAgentMessage.StatusCodes.ABORT;
					else
						return MANAgentMessage.StatusCodes.SERVICENOTFOUND;
				}
				else
				{
					MANAgentProtocol chat = Services[message.Service].Create(this);
					if (message.ReceiverChatId != -1)
					{
						if (!chat.TrySetId(message.ReceiverChatId))
						{
							chat.Stop();
							return MANAgentMessage.StatusCodes.CHATIDNOTACCEPTED;
						}
					}
					AddChat(chat);
					chat.ReceiveMessage(message);
					return MANAgentMessage.StatusCodes.OK;
				}
			}

			public virtual bool SendMessage(ref MANAgentMessage msg)
			{
				using (Logger logger = new Logger("Agent.SendMessage(ref AgentMessage)", Logger.Mode.LOG))
				{
					IMyProgrammableBlock targetBlock = null;
					if (msg.Receiver == Id)
					{
						ReceiveMessage(msg);
					}
					else if (msg.Receiver.Name != Id.Name && (msg.Receiver.Platform == "local" || msg.Receiver.Platform == Id.Platform))
					{
						targetBlock = GTS.GetBlockWithName(msg.Receiver.Name) as IMyProgrammableBlock;
						if (targetBlock == null)
						{
							//Logger.log("WARNING: Receiver with id '" + msg.Receiver.ToString() + "' not found locally!");
						}
					}
					else
					{
						//Logger.log("Receiver not local. Trying to find corresponding platform agent.");
						targetBlock = GTS.GetBlockWithName(msg.Receiver.Platform) as IMyProgrammableBlock;
						if (targetBlock == null)
						{
							if (Id.Platform != Id.Name)
							{
								targetBlock = GTS.GetBlockWithName(Id.Platform) as IMyProgrammableBlock;
							}
							if (targetBlock == null)
							{
								//Logger.log("WARNING: Not registered at any platform! Only local communication possible!");
							}
						}
					}
					if (targetBlock == null)
					{
						return false;
					}
					if (msg.Receiver.Platform == "local" && msg.Sender.Platform == Id.Platform)
					{
						msg.Sender.Platform = "local";
					}

					logger.log("Message: " + msg.ToXML(), Logger.Mode.LOG);

					if (!targetBlock.TryRun("message \"" + msg.ToString() + "\""))
					{
						ScheduleMessage(msg);
					}
					return true;
				}
			}

			public void ScheduleMessage(MANAgentMessage msg)
			{
				ScheduledMessages.Add(msg);
				ScheduleRefresh();
			}

			public void SendScheduledMessages()
			{
				for (int i = ScheduledMessages.Count - 1; i >= 0; i--)
				{
					MANAgentMessage msg = ScheduledMessages[i];
					ScheduledMessages.RemoveAt(i);
					SendMessage(ref msg);
				}
			}

			public void RegisterService(string id, Func<MANAgent, MANAgentProtocol> createChat)
			{
				RegisterService(id, createChat, new Dictionary<string, string>());
			}

			public void RegisterService(string id, Func<MANAgent, MANAgentProtocol> createChat, Dictionary<string, string> options)
			{
				Services.Add(id, new MANService(
					id,
					(options.GetValueOrDefault("description") as String) ?? "",
					this.Id,
					new MANAgentId(options.GetValueOrDefault("permissions", "ANY@ANY")),
					options.ContainsKey("providesui") && options["providesui"] != "false",
					createChat)
				);
			}

			public bool AddChat(MANAgentProtocol chat)
			{
				if (!Chats.ContainsKey(chat.ChatId))
				{
					Chats[chat.ChatId] = chat;
					return true;
				}
				return false;

			}

			public bool UpdateChatId(int oldId, int newId)
			{
				if (Chats.ContainsKey(oldId) && !Chats.ContainsKey(newId))
				{
					Chats[newId] = Chats[oldId];
					Chats.Remove(oldId);
					return true;
				}
				return false;

			}

			public void StopChat(int chatId)
			{
				if (Chats.ContainsKey(chatId))
				{
					StopChat(Chats[chatId]);
				}
			}
			public void StopChat(MANAgentProtocol chat)
			{
				foreach (KeyValuePair<string, List<MANAgentProtocol>> listeners in EventListeners)
				{
					for (int i = listeners.Value.Count - 1; i >= 0; i--)
					{
						if (listeners.Value[i] == chat)
							listeners.Value.RemoveAt(i);
					}
				}
				Chats.Remove(chat.ChatId);
			}

			public void SetTimer(IMyTimerBlock timer)
			{
				Timer = timer;
			}

			public void ScheduleRefresh()
			{
				RefreshScheduled = true;
				if (Timer != null && !Timer.IsCountingDown)
					Timer.GetActionWithName("Start").Apply(Timer);

			}

			public virtual void Refresh(TimeSpan elapsedTime)
			{
				ElapsedTimeValue += elapsedTime;
				if (!RefreshScheduled)
					return;
				RefreshScheduled = false;
				SendScheduledMessages();
				Event("refresh");
				ElapsedTimeValue = new TimeSpan(0);
			}

		}
	}
}