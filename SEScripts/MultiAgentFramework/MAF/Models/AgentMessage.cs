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

namespace SEScripts.MultiAgentFramework.MAF.Models
{
    public class AgentMessage : XML.DataStore
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
            CHATIDNOTACCEPTED
        }
        public string Content
        {
            get
            {
                return Parser.UnescapeQuotes(GetAttribute("content"));
            }
            set
            {
                SetAttribute("content", Parser.Sanitize(value));
            }
        }
        public AgentId Sender
        {
            get
            {
                return new AgentId(GetAttribute("sender"));
            }
            set
            {
                SetAttribute("sender", value.Id);
            }
        }
        public AgentId Receiver
        {
            get
            {
                return new AgentId(GetAttribute("receiver"));
            }
            set
            {
                SetAttribute("receiver", value.Id);
            }
        }
        public int ChatId
        {
            get
            {
                int chatid;
                if (Int32.TryParse(GetAttribute("chatid"), out chatid))
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
                SetAttribute("chatid", value.ToString());
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

        public AgentMessage() : base()
        {
            Logger.debug("AgentMessage constructor()");
            Logger.IncLvl();
            Type = "message";
            Attributes = new Dictionary<string, string>();
            Logger.DecLvl();
        }

        public AgentMessage(AgentId sender, AgentId receiver, string msg) : this()
        {
            Logger.debug("AgentMessage constructor(AgentId, AgentId, string)");
            Logger.IncLvl();
            Content = msg;
            Sender = sender;
            Receiver = receiver;
            Status = StatusCodes.OK;
            Service = "response";
            Logger.DecLvl();
        }

        public AgentMessage(
            AgentId sender,
            AgentId receiver,
            StatusCodes status,
            string content,
            string requestedService,
            int chatId
        ) : this(sender, receiver, content)
        {
            Logger.debug("AgentMessage constructor(AgentId, AgentId, StatusCodes, string, string, int)");
            Logger.IncLvl();
            Status = status;
            Service = requestedService;
            ChatId = chatId;
            Logger.DecLvl();
        }

        public AgentMessage(
            AgentId sender,
            AgentId receiver,
            StatusCodes status,
            string content,
            string requestedService
        ) : this(sender, receiver, status, content, requestedService, -1)
        {
        }

        public AgentMessage MakeResponse(AgentId newSender, StatusCodes status, string msg)
        {
            Logger.debug("AgentMessage.MakeResponse()");
            Logger.IncLvl();
            AgentMessage aMsg = new AgentMessage(
                newSender,
                Sender,
                status,
                msg,
                "response"
            );
            if (ChatId >= 0)
            {
                aMsg.ChatId = ChatId;
            }
            Logger.DecLvl();
            return aMsg;
        }

        public AgentMessage Duplicate()
        {
            return new AgentMessage(
                Sender,
                Receiver,
                Status,
                Content,
                Service,
                ChatId
            );
        }

        public override string ToString()
        {
            Logger.debug("AgentMessage.ToString()");
            Logger.IncLvl();
            Logger.DecLvl();
            return ToXML();
        }

        public string ToXML()
        {
            Logger.debug("AgentMessage.ToXML()");
            Logger.IncLvl();
            string xml = "<message ";
            xml += Parser.PackData(GetValues((node) => true));
            xml += "/>";
            Logger.DecLvl();
            Logger.log("sanitize message");
            return Parser.Sanitize(xml);
        }

        public static AgentMessage FromXML(string xml)
        {
            Logger.debug("AgentMessage.FromXML()");
            Logger.IncLvl();
            SetUp();
            xml = Parser.UnescapeQuotes(xml);
            XML.XMLTree xmlNode = XML.ParseXML(xml);
            AgentMessage msg = xmlNode.GetNode((node) => node.Type == "message") as AgentMessage;
            Logger.DecLvl();
            Logger.log("unescape message");
            return msg;
        }

        public static void SetUp()
        {
            Logger.debug("AgentMessage.SetUp()");
            Logger.IncLvl();
            if (!XML.NodeRegister.ContainsKey("message"))
            {
                XML.NodeRegister.Add("message", () => {
                    return new AgentMessage();
                }
                );
            }
            Logger.DecLvl();
        }

    }

    //EMBED SEScripts.MultiAgentFramework.MAF.Models.AgentId
}
