﻿using Sandbox.Game.EntityComponents;
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
	/*partial class Program : MyGridProgram
	{
		
    public class MANReceiveMessageProtocol : AgentProtocol
    {
        public override string GetProtocolId()
        { return "receive"; }

        public MANReceiveMessageProtocol(Agent agent) : base(agent)
        {
            //Logger.debug("ReceiveMessageProtocol constructor()");
        }
        public override void Restart(){}

        public virtual void Start() { }
        
        public virtual void ReceiveMessage(AgentMessage msg)
        {
            //Logger.debug("ReceiveMessageProtocol.ReceiveMessage()");
            //Logger.IncLvl();

            AgentMessage.StatusCodes responseStatus = AgentMessage.StatusCodes.UNDEFINED;
            bool isValidReceiver = msg.Receiver.Matches(Holder.Id);
            bool chatAssigned = false;
            if(!isValidReceiver)
            {
                responseStatus = AgentMessage.StatusCodes.RECEIVERNOTFOUND;
            }
            else
            {
                chatAssigned = AssignChat(msg, ref responseStatus);
            }

            bool shouldForward = !isValidReceiver
                || (msg.Receiver.Name == "ALL" || msg.Receiver.Platform == "ALL")
                || ((msg.Receiver.Name == "ANY" || msg.Receiver.Platform == "ANY")
                    && (responseStatus == AgentMessage.StatusCodes.RECEIVERNOTFOUND
                        || responseStatus == AgentMessage.StatusCodes.PLATFORMNOTFOUND
                        || responseStatus == AgentMessage.StatusCodes.SERVICENOTFOUND));

            if(shouldForward)
            {
                Holder.SendMessage(ref msg);
            }
            else if(responseStatus == AgentMessage.StatusCodes.CHATIDNOTACCEPTED
                || responseStatus == AgentMessage.StatusCodes.CHATNOTFOUND
                || responseStatus == AgentMessage.StatusCodes.SERVICENOTFOUND
                || responseStatus == AgentMessage.StatusCodes.RECEIVERNOTFOUND)
            {
                AgentMessage sendMsg = msg.MakeResponse(
                    Holder.Id,
                    responseStatus,
                    ""
                    );
                Holder.SendMessage(ref sendMsg);
            }


            Stop();
            //Logger.DecLvl();
        }

        private bool AssignChat(AgentMessage msg, ref AgentMessage.StatusCodes status)
        {
            if( msg.Service == "response" )
            {
                if(msg.SenderChatId == -1)
                {
                    status = AgentMessage.StatusCodes.UNKNOWNERROR;
                    return false;
                }
                else if(!Holder.SenderChats.ContainsKey(msg.SenderChatId)) 
                {
                    status = AgentMessage.StatusCodes.CHATNOTFOUND;
                    return false;
                }
                else
                {
                    status = AgentMessage.StatusCodes.OK;
                    Holder.Chats[msg.ChatId].ReceiveMessage(msg);
                    return true;
                }
            }
            else if(!Holder.Services.ContainsKey(msg.Service))
            {
                status = AgentMessage.StatusCodes.SERVICENOTFOUND;
                return false;
            }
            else
            {
                AgentProtocol chat = Holder.Services[msg.Service].Create(Holder);
                if (msg.SenderChatId != -1)
                {
                    if (!chat.TrySetId(msg.SenderChatId))
                    {
                        chat.Stop();
                        status = AgentMessage.StatusCodes.CHATIDNOTACCEPTED;
                        return false;
                    }
                }
                Holder.AddChat(chat);
                chat.ReceiveMessage(msg);
                status = AgentMessage.StatusCodes.OK;
                return true;
            }
        }
                
        public static void RegisterServices(Agent agent)
        {
            agent.RegisterService("receive", (holder) => new ReceiveMessageProtocol(holder));
        }

    }

		//EMBED SEScripts.MultiAgentNetwork.MAN.Agents.Agent
		//EMBED SEScripts.MultiAgentNetwork.MAN.Models.AgentMessage
	}*/
}