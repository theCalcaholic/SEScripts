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
		public abstract class MANAgentProtocol
		{
			static int ChatCountValue;
			private int ChatIdValue;
			private int PartnerIdValue;
			protected MANAgent Holder;
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

			public MANAgentProtocol(MANAgent agent)
			{
				ChatIdValue = ChatCount;
				ChatCountValue++;
				Holder = agent;
			}

			public bool TrySetId(int id)
			{
				if (id == ChatId)
					return true;
				else if (id >= ChatCount)
				{
					ChatCountValue = id + 1;
					ChatIdValue = id;
					return true;
				}
				else
					return false;
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

			public virtual void ReceiveMessage(MANAgentMessage msg)
			{
				if (msg.Status == MANAgentMessage.StatusCodes.CHATIDNOTACCEPTED)
				{
					string[] contentSplit = msg.Content.Split(':');
					if (contentSplit.Length == 2 && contentSplit[0] == "validId")
					{
						int oldId = ChatId;
						int desiredId = -1;
						if (!Int32.TryParse(contentSplit[1], out desiredId))
							Stop();
						else
						{
							desiredId = Math.Max(ChatCount, desiredId);
							if (Holder.UpdateChatId(oldId, desiredId) && TrySetId(desiredId))
								Restart();
							else
								Stop();
						}
					}
				}
			}

			public static string MakeRoute(MANAgentId aid, string protocolId, string argument)
			{
				return "man:" + aid + "::" + protocolId + "(" + XMLParser.Sanitize(argument) + ")";
			}

			public virtual void Setup() { }

		}
	}
}