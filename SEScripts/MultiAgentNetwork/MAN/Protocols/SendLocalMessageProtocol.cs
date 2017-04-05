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

using SEScripts.Lib.LoggerNS;
using SEScripts.MultiAgentNetwork.MAN.Agents;
using SEScripts.MultiAgentNetwork.MAN.Models;

namespace SEScripts.MultiAgentNetwork.MAN.Protocols
{/*
    public class SendLocalMessageProtocol : AgentProtocol
    {
        static int ChatCountValue;
        private int ChatIdValue;
        protected Agent Holder;
        public override string GetProtocolId()
        { return "send"; }

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

        public SendLocalMessageProtocol(Agent agent) : base(agent)
        {
            //Logger.debug("SendLocalMessageProtocol constructor()");
            //Logger.IncLvl();
            ChatIdValue = ChatCount;
            ChatCountValue++;
            Holder = agent;
            //Logger.DecLvl();
        }

        public override void Restart()
        {
        }

        public virtual void Start()
        { }
        
        public virtual void ReceiveMessage(AgentMessage msg)
        {
            //Logger.debug("SendLocalMessageProtocol.ReceiveMessage()");
            //Logger.IncLvl();
            //Logger.log("Sending message of '" + msg.Sender + "' to '" + msg.Receiver + "'...");

            IMyProgrammableBlock receiverBlock;
            if (msg.Receiver.MatchesPlatform(Holder.Id))
            {
                receiverBlock = Holder.GTS.GetBlockWithName(msg.Receiver.Name) as IMyProgrammableBlock;

                if (receiverBlock == null)
                {
                    //Logger.log("WARNING: Receiver with id '" + msg.Receiver + "' not found locally!");
                }
            }
            else
            {
                //Logger.log("Receiver not local. Trying to find corresponding platform agent.");
                receiverBlock = Holder.GTS.GetBlockWithName(msg.Receiver.Platform) as IMyProgrammableBlock;
                if (receiverBlock == null)
                {
                    if (Id.Platform != Id.Name)
                    {
                        receiverBlock = Holder.GTS.GetBlockWithName(Id.Platform) as IMyProgrammableBlock;
                    }
                    if (receiverBlock == null)
                    {
                        //Logger.log("WARNING: Not registered at any platform! Only local communication possible!");
                    }
                }
            }

            if(receiverBlock == null)
            {
                if(msg.Status != AgentMessage.StatusCodes.CHATNOTFOUND
                    && msg.Status != AgentMessage.StatusCodes.RECEIVERNOTFOUND
                    && msg.Status != AgentMessage.StatusCodes.SERVICENOTFOUND)
                {
                    AgentMessage sendMsg = msg.MakeResponse(
                        Holder.Id,
                        AgentMessage.StatusCodes.RECEIVERNOTFOUND,
                        ""
                        );
                    Holder.SendMessage(ref sendMsg);
                }
                //Logger.DecLvl();
                Stop();
                return;
            }

            if (msg.Receiver.MatchesPlatform(Holder.Id))
            {
                msg.Sender.Platform = "local";
            }

            if(!receiverBlock.TryRun("message \"" + msg.ToString() + "\""))
            {
                Holder.ScheduleMessage(msg);
            }
            Stop();
            //Logger.DecLvl();
        }

        public static void RegisterServices(Agent agent) {
            agent.RegisterService(Id, (holder) => new SendLocalMessageProtocol(holder) );
        }

    }*/

    //EMBED SEScripts.MultiAgentNetwork.MAN.Agents.Agent
    //EMBED SEScripts.MultiAgentNetwork.MAN.Protocols.AgentProtocol
    //EMBED SEScripts.MultiAgentNetwork.MAN.Models.AgentMessage
}
