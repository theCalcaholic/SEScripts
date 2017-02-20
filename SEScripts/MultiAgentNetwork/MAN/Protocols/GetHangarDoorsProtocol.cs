using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SEScripts.MultiAgentNetwork.MAN.Agents;
using SEScripts.MultiAgentNetwork.MAN.Models;

namespace SEScripts.MultiAgentNetwork.MAN.Protocols
{
    public class GetHangarDoorsProtocol : AgentProtocol
    {
        static List<string> HangarDoorGroups = new List<string>();

        public override string GetProtocolId()
        { return "get-hangar-doors"; }

        public GetHangarDoorsProtocol(Agent agent) : base(agent) { }
        public override void Restart() {}

        public override void ReceiveMessage(AgentMessage msg)
        {
            if (msg.Status != AgentMessage.StatusCodes.OK)
            {
                base.ReceiveMessage(msg);
                return;
            }

            AgentMessage response;
            if(msg.TargetInterface == AgentMessage.Interfaces.UI)
            {
                string content = "<menu>";
                foreach(string groupName in HangarDoorGroups)
                {
                    content += "<menuitem route='" + MakeRoute(Holder.Id, GetProtocolId(), groupName) + "'>" + groupName + "</menuitem>";
                }
                content += "</menu>";

                response = msg.MakeResponse(Holder.Id, AgentMessage.StatusCodes.OK, content);
            }
            else
            {
                response = msg.MakeResponse(Holder.Id, AgentMessage.StatusCodes.OK, string.Join(",", HangarDoorGroups));
            }

            response.SenderChatId = ChatId;
            Holder.SendMessage(ref response);
            Stop();
        }

        public override void Setup()
        {
            Holder.RegisterService(
                GetProtocolId(),
                (holder) => new GetHangarDoorsProtocol(holder),
                new Dictionary<string, string>
                {
                    {"description", "List Hangars" },
                    {"permissions", "ANY@ANY" },
                    {"providesui", "true" }
                });
        }

        public static void AddHangarDoorGroup(string groupName)
        {
            HangarDoorGroups.Add(groupName);
        }

        public static void RemoveHangarDoorGroup(string groupName)
        {
            HangarDoorGroups.Remove(groupName);
        }
    }
}
