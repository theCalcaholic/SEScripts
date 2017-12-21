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
		public class MANAgentMessage : XMLDataStore
		{
			public enum StatusCodes
			{
				UNDEFINED,
				OK,
				CHATNOTFOUND,
				SERVICENOTFOUND,
				UNKNOWNERROR,
				ABORT,
				FORWARDED,
				RECEIVERNOTFOUND,
				PLATFORMNOTFOUND,
				CHATIDNOTACCEPTED
			}

			public enum Interfaces
			{
				UI,
				TEXT
			}
			public string Content
			{
				get
				{
					return XMLParser.UnescapeQuotes(GetAttribute("content"));
				}
				set
				{
					SetAttribute("content", XMLParser.Sanitize(value));
				}
			}
			public MANAgentId Sender
			{
				get
				{
					return new MANAgentId(GetAttribute("sender"));
				}
				set
				{
					SetAttribute("sender", value.ToString());
				}
			}
			public MANAgentId Receiver
			{
				get
				{
					return new MANAgentId(GetAttribute("receiver"));
				}
				set
				{
					SetAttribute("receiver", value.ToString());
				}
			}
			public int SenderChatId
			{
				get
				{
					int chatid;
					if (Int32.TryParse(GetAttribute("senderchatid"), out chatid))
					{
						return chatid;
					}
					else
					{
						return -1;
					}
				}
				set
				{
					SetAttribute("senderchatid", value.ToString());
				}
			}
			public int ReceiverChatId
			{
				get
				{
					int chatid;
					if (Int32.TryParse(GetAttribute("receiverchatid"), out chatid))
					{
						return chatid;
					}
					else
					{
						return -1;
					}
				}
				set
				{
					SetAttribute("receiverchatid", value.ToString());
				}
			}
			public string Service
			{
				get
				{
					return GetAttribute("service");
				}
				set
				{
					SetAttribute("service", value);
				}
			}
			public StatusCodes Status
			{
				get
				{
					StatusCodes status;
					if (Enum.TryParse<StatusCodes>(GetAttribute("status"), true, out status))
					{
						return status;
					}
					else
					{
						return StatusCodes.UNDEFINED;
					}
				}
				set
				{
					SetAttribute("status", value.ToString());
				}
			}

			public Interfaces TargetInterface
			{
				get
				{
					Interfaces target;
					if (Enum.TryParse<Interfaces>(GetAttribute("targetinterface"), true, out target))
					{
						return target;
					}
					else
					{
						return Interfaces.TEXT;
					}
				}
				set
				{
					SetAttribute("targetinterface", value.ToString());
				}
			}

			public MANAgentMessage() : base()
			{
				Type = "message";
				Attributes = new Dictionary<string, string>();
				Content = "";
				Status = StatusCodes.OK;
				Service = "response";
				TargetInterface = Interfaces.TEXT;
			}

			public MANAgentMessage(MANAgentId sender, MANAgentId receiver, string msg) : this()
			{
				Content = msg;
				Sender = sender;
				Receiver = receiver;
				Status = StatusCodes.OK;
				Service = "response";
				TargetInterface = Interfaces.TEXT;
			}

			public MANAgentMessage(
				MANAgentId sender,
				MANAgentId receiver,
				StatusCodes status,
				string content,
				string requestedService,
				int senderchatId
			) : this(sender, receiver, status, content, requestedService, senderchatId, -1) { }

			public MANAgentMessage(
				MANAgentId sender,
				MANAgentId receiver,
				StatusCodes status,
				string content,
				string requestedService,
				int senderChatId,
				int receiverChatId
			) : this(sender, receiver, content)
			{
				Status = status;
				Service = requestedService;
				SenderChatId = senderChatId;
				ReceiverChatId = receiverChatId;
				TargetInterface = Interfaces.TEXT;
			}

			public MANAgentMessage(
				MANAgentId sender,
				MANAgentId receiver,
				StatusCodes status,
				string content,
				string requestedService
			) : this(sender, receiver, status, content, requestedService, -1) {}

			public MANAgentMessage MakeResponse(MANAgentId newSender, StatusCodes status, string msg)
			{
				MANAgentMessage aMsg = new MANAgentMessage(
					newSender,
					Sender,
					status,
					msg,
					"response"
				);
				aMsg.SenderChatId = ReceiverChatId;
				aMsg.ReceiverChatId = SenderChatId;
				return aMsg;
			}

			public MANAgentMessage Duplicate()
			{
				MANAgentMessage msg = new MANAgentMessage(
					Sender,
					Receiver,
					Status,
					Content,
					Service,
					SenderChatId,
					ReceiverChatId
				);
				msg.TargetInterface = TargetInterface;
				return msg;
			}

			public override string ToString()
			{
				return ToXML();
			}

			public string ToXML()
			{
				string xml = "<message ";
				xml += XMLParser.PackData(GetValues((node) => true));
				xml += "/>";
				return XMLParser.Sanitize(xml);
			}

			public static MANAgentMessage FromXML(string xml)
			{
				SetUp();
				xml = XMLParser.UnescapeQuotes(xml);
				XMLTree xmlNode = XML.ParseXML(xml);
				MANAgentMessage msg = xmlNode.GetNode((node) => node.Type == "message") as MANAgentMessage;
				return msg;
			}

			public static void SetUp()
			{
				if (!XML.NodeRegister.ContainsKey("message"))
				{
					XML.NodeRegister.Add("message", () =>
					{
						return new MANAgentMessage();
					}
					);
				}
			}

		}

	}
}