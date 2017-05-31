﻿namespace SEScripts.Merged
{
public class MAN
{
//WRAPPERBODY

public class GridPlatformAgent : PlatformAgent
{
List<IMyLaserAntenna> LaserAntennas;
List<IMyRadioAntenna> RadioAntennas;
public GridPlatformAgent(MyGridProgram program, List<IMyTerminalBlock> antennas) : base(program)
{
LaserAntennas = new List<IMyLaserAntenna>();
RadioAntennas = new List<IMyRadioAntenna>();
foreach(IMyTerminalBlock antenna in antennas)
{
IMyLaserAntenna laser = antenna as IMyLaserAntenna;
IMyRadioAntenna radio = antenna as IMyRadioAntenna;
if(laser != null)
{
LaserAntennas.Add(laser);
} else if(radio != null)
{
RadioAntennas.Add(radio);
}
}
Id = GenerateId(program.Me);
program.Me.CustomName = Id.Name;
}
public override AgentId GenerateId(IMyProgrammableBlock me)
{
if(!Id.Name.Contains("GridMaster"))
{
me.CustomName = "GridMaster";
return base.GenerateId(me);
}
else
{
return Id;
}
}
public override bool SendMessage(ref AgentMessage message)
{
bool baseSuccess = base.SendMessage(ref message);
if (message.Receiver.Platform == "ALL" || !baseSuccess )
{
return SendToGrid(message) || baseSuccess;
}
return true;
}
public bool SendToGrid(AgentMessage message)
{
//Logger.log("GridPlatformAgent.SendToGrid()");
//Logger.IncLvl();
if (message.Receiver.Platform == Id.Platform || message.Receiver.Platform == "local")
{
return false;
}

if( message.Receiver.Platform == "ALL" || message.Receiver.Platform == "ANY")
{
//Logger.debug("Receiver platform set to local...");
message.Receiver = new AgentId(message.Receiver.Name + "@local");
//Logger.debug("Receiver id was set to: " + message.Receiver.ToString());
}
foreach(IMyRadioAntenna antenna in RadioAntennas)
{
if(antenna.TransmitMessage("message \"" + message.ToString() + "\"", MyTransmitTarget.Everyone))
{
//Logger.log("Sending was successful.");
//Logger.DecLvl();
return true;
}
}
//Logger.log("Could not send message by antenna...");
ScheduleMessage(message);
//Logger.log("Message scheduled");
if(ScheduledMessages.Count > 0)
{
//Logger.log("Schedule refresh");
ScheduleRefresh();
}
//Logger.log("Sent to grid (finished)");
//Logger.DecLvl();
return true;
}
}

//WRAPPER: SEScripts.MultiAgentFramework.MAFWRAPPER
public class PlatformAgent : Agent
{
public int RefreshInterval;
public ServiceRegister PlatformServices;
public List<IMyTextPanel> ReceptionBuffers;
public PlatformAgent(MyGridProgram program) : base(program)
{
PlatformServices = new ServiceRegister();
ReceptionBuffers = new List<IMyTextPanel>();
RefreshInterval = 500;
DataStorage db = DataStorage.Load(program.Storage ?? "");
if (db.Exists<string>("id"))
{
Id = new AgentId(db.Get<string>("id"));
List<IMyTerminalBlock> buffers = new List<IMyTerminalBlock>();
GTS.SearchBlocksOfName("RBUFFER-" + Id.Name, buffers);
foreach (IMyTerminalBlock buffer in buffers)
{
IMyTextPanel tBuffer = buffer as IMyTextPanel;
if (tBuffer != null)
{
ReceptionBuffers.Add(tBuffer);
}
}
}
else
{
Id = GenerateId(program.Me);
}
program.Me.CustomName = Id.Name;
new ServiceRegistrationProtocol.Platform(this).Setup();
//MessageRoutingProtocol.RegisterServices(this);
new ProvideServiceInformationProtocol(this).Setup();
}
public void RegisterBuffers(List<string> bufferNames)
{
foreach (string name in bufferNames)
{
IMyTextPanel buffer = GTS.GetBlockWithName(name) as IMyTextPanel;
if (buffer != null)
{
buffer.CustomName = "RBUFFER-" + Id.Name;
buffer.WritePublicText("");
buffer.CustomData = "";
ReceptionBuffers.Add(buffer);
}
}
}
public void CollectPlatformMessages()
{
foreach (IMyTextPanel buffer in ReceptionBuffers)
{
string text = buffer.CustomData;
buffer.CustomData = "";
List<XML.XMLTree> messages = XML.ParseXML(Parser.UnescapeQuotes(text)).GetAllNodes(node => node.Type == "message");
foreach (XML.XMLTree message in messages)
{
ReceiveMessage((AgentMessage)message);
}
}
}
public override void ReceiveMessage(AgentMessage message)
{
if (message.Receiver.Platform == Id.Name)
{
message.Receiver.Platform = "local";
}
//Logger.log("Looking for agent '" + message.Receiver + "'...");
AgentMessage.StatusCodes status = AssignMessage(message);
//Logger.log("Status: " + status.ToString());
ReceiveMessage(message, status);
}
public override void ReceiveMessage(AgentMessage message, AgentMessage.StatusCodes status)
{
if (message.Receiver.Platform == "local")
{
message.Receiver.Platform = Id.Platform;
}
bool forwarded = false;

if (status == AgentMessage.StatusCodes.RECEIVERNOTFOUND || status == AgentMessage.StatusCodes.PLATFORMNOTFOUND)
{
//Logger.log("no receiver found; looking for service '" + message.Service + "'...");
if (message.Service != null && PlatformServices.ContainsKey(message.Service))
{
//Logger.log("service is known.");
if (message.Receiver.MatchesPlatform(Id))
{
if(PlatformServices.ContainsKey(message.Service))
{
List<PlatformService> pServices = PlatformServices[message.Service].FindAll(
service => message.Receiver.Matches(service.Provider) && message.Sender != service.Provider);
if(message.Receiver.Name == "ALL")
{
foreach(PlatformService pService in pServices)
{
AgentMessage sendMsg = message.Duplicate();
sendMsg.Receiver = pService.Provider;
forwarded |= SendMessage( ref sendMsg);
}
}
else
{
if(pServices.ElementAtOrDefault(0) != null)
{
message.Receiver = pServices[0].Provider;
forwarded = SendMessage(ref message);
}
}
}
}
}
if (!forwarded)
{
base.ReceiveMessage(message, status);
}
}
else
{
base.ReceiveMessage(message, status);
}
}
public override bool SendMessage(ref AgentMessage message)
{
if (base.SendMessage(ref message))
{
//Logger.log("base.SendMessage(message) did succeed.");
return true;
}
else if (message.Sender.Platform == "local")
{
message.Sender.Platform = Id.Name;
}
if (message.Receiver.Platform != Id.Name)
{
//Logger.log("Calling SendToPlatform(message, platform)...");
return SendToPlatform(message, message.Receiver.Platform);
}
else
{
return false;
}
}
public bool SendToPlatform(AgentMessage message, string platform)
{
//Logger.IncLvl();
//Logger.debug("PlatformAgent.SendToPlatform()");
if (platform == "ALL" || platform == "ANY")
{
List<IMyTerminalBlock> buffers = new List<IMyTerminalBlock>();
GTS.SearchBlocksOfName("RBUFFER-", buffers,
(block) => (
block.CustomName != "RBUFFER-" + Id.Name
&& (block as IMyTextPanel) != null)
);
bool success = false;
foreach (IMyTerminalBlock buffer in buffers)
{
string platformName = buffer.CustomName.Replace("RBUFFER-", "");
if (platformName != Id.Name)
{
if( SendToPlatform(message, platformName) )
{
success = true;
if(platform == "ANY")
{
return true;
}
}
}
}
//Logger.DecLvl();
return success;
}
else
{
//Logger.log("Trying to send to platform '" + platform + "'...");
IMyTextPanel buffer = GTS.GetBlockWithName("RBUFFER-" + platform) as IMyTextPanel;
if (buffer == null)
{
//Logger.log("No corresponding buffer found with name 'RBUFFER-" + platform + "'.");
//Logger.DecLvl();
return false;
}
else
{
//Logger.log("Writing message to reception buffer of platform '" + platform + "'...");
buffer.CustomData += message.ToString();
//Logger.DecLvl();
return true;
}
}
}
public static string GenerateSuffix()
{
//Logger.debug("PlatformAgent.GenerateSuffix()");
//Logger.IncLvl();
const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
Random rnd = new Random();
char[] title = new char[8];
for (int i = 0; i < 8; i++)
{
title[i] = chars[rnd.Next(0, chars.Length)];
}
//Logger.DecLvl();
return new string(title);
}
public override void Refresh(TimeSpan elapsedTime)
{
bool refreshScheduled = RefreshScheduled
|| (ElapsedTime.Milliseconds + elapsedTime.Milliseconds >= RefreshInterval);
RefreshScheduled = refreshScheduled;
if (Timer != null)
{
if (RefreshInterval < 1000)
{
Timer.GetActionWithName("TriggerNow").Apply(Timer);
}
else
{
Timer.GetActionWithName("Start").Apply(Timer);
}
}
base.Refresh(elapsedTime);
if (!refreshScheduled)
{
return;
}
CollectPlatformMessages();
}
public virtual AgentId GenerateId(IMyProgrammableBlock me)
{
System.Text.RegularExpressions.Regex braceRegex = new System.Text.RegularExpressions.Regex(@"-{.*}$");
string name;
if (braceRegex.IsMatch(me.CustomName))
{
name = braceRegex.Replace(me.CustomName, "-{" + PlatformAgent.GenerateSuffix() + "}");
}
else
{
name = me.CustomName + "-{" + PlatformAgent.GenerateSuffix() + "}";
}
return new AgentId(name + "@" + name);
}
}

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
//Logger.debug("Agent constructor");
//Logger.IncLvl();
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
//Logger.DecLvl();
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
//Logger.debug("Agent.SetKnowledgeEntry()");
//Logger.IncLvl();
string actualKey = global ? key : chat.GetProtocolId() + "_" + key;
Knowledge[actualKey] = value;
//Logger.DecLvl();
}
public object GetKnowledgeEntry(string key, AgentProtocol chat)
{
return GetKnowledgeEntry(key, chat, false);
}
public object GetKnowledgeEntry(string key, AgentProtocol chat, bool global)
{
//Logger.debug("Agent.GetKnowledgeEntry()");
//Logger.IncLvl();
string actualKey = global ? key : chat.GetProtocolId() + "_" + key;
//Logger.DecLvl();
return Knowledge.GetValueOrDefault(actualKey, null);
}
public void OnEvent(string eventId, AgentProtocol chat)
{
//Logger.debug("Agent.OnEvent( )");
//Logger.IncLvl();
if (!EventListeners.ContainsKey(eventId))
{
EventListeners[eventId] = new List<AgentProtocol>();
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
public virtual void ReceiveMessage(AgentMessage msg)
{
//Logger.debug("Agent.ReceiveMessage(AgentMessage)");
//Logger.IncLvl();
AgentMessage.StatusCodes status = AssignMessage(msg);
ReceiveMessage(msg, status);
//Logger.DecLvl();
}
public virtual void ReceiveMessage(AgentMessage msg, AgentMessage.StatusCodes status)
{
//Logger.debug("Agent.ReceiveMessage(AgentMessage, AgentMessage.StatusCode)");
//Logger.IncLvl();
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
//Logger.log("WARNING: Message Receiver Id does not conform with this agent's Id!");
}
else if (status == AgentMessage.StatusCodes.CHATNOTFOUND)
{
//Logger.log("WARNING: ChatId not found!");
}
else if (status == AgentMessage.StatusCodes.SERVICENOTFOUND)
{
//Logger.log("WARNING: No service with id '" + msg.Service + "' found!");
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
//Logger.DecLvl();
}
protected virtual AgentMessage.StatusCodes AssignMessage(AgentMessage message)
{
//Logger.debug("Agent.AssignMessage()");
//Logger.IncLvl();

if (!Id.MatchesPlatform(message.Receiver))
{
//Logger.log("receiver platform does not match local platform.");
return AgentMessage.StatusCodes.PLATFORMNOTFOUND;
}
else if(!Id.MatchesName(message.Receiver))
{
//Logger.log("receiver does not match this agent.");
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
//Logger.DecLvl();
return AgentMessage.StatusCodes.CHATNOTFOUND;
}
else
{
//Logger.log("Transmit message to chat " + message.ReceiverChatId.ToString());
Chats[message.ReceiverChatId].ReceiveMessage(message);
//Logger.DecLvl();
return AgentMessage.StatusCodes.OK;
}
}
else if (!Services.ContainsKey(message.Service))
{
//Logger.log("WARNING: Service not found");
if (message.Status == AgentMessage.StatusCodes.CHATNOTFOUND)
{
//Logger.log("WARNING: Requested chat id did not exist on '" + (message.Sender.ToString() ?? "") + "'.");
//Logger.DecLvl();
return AgentMessage.StatusCodes.ABORT;
}
else if (message.Status == AgentMessage.StatusCodes.SERVICENOTFOUND)
{
//Logger.log("WARNING: Requested service '" + (message.Service ?? "") + "' did not exist on '" + (message.Sender.ToString() ?? "") + "'.");
//Logger.DecLvl();
return AgentMessage.StatusCodes.ABORT;
}
else if (message.Status == AgentMessage.StatusCodes.SERVICENOTFOUND)
{
//Logger.log("WARNING: An unknown error occured at '" + (message.Sender.ToString() ?? "") + "'.");
//Logger.DecLvl();
return AgentMessage.StatusCodes.ABORT;
}
else
{
//Logger.DecLvl();
return AgentMessage.StatusCodes.SERVICENOTFOUND;
}
}
else
{
//Logger.log("create protocol '" + message.Service + "'.");
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
//Logger.log("Transfer message to chat.");
chat.ReceiveMessage(message);
//Logger.DecLvl();
return AgentMessage.StatusCodes.OK;
}
}
public virtual bool SendMessage(ref AgentMessage msg)
{
using (Logger logger = new Logger("Agent.SendMessage(ref AgentMessage)", Logger.Mode.LOG))
{
//Logger.debug("Agent.SendMessage");
//Logger.IncLvl();
//Logger.log("Sending message of '" + msg.Sender.ToString() + "' to '" + msg.Receiver.ToString() + "'...");
//Logger.log("Requested service: " + msg.Service);
//Logger.log("Message content: " + msg.Content);
//Logger.log("Message status: " + msg.Status.ToString());
IMyProgrammableBlock targetBlock = null;
//Logger.log("comparing receiver platform '" + msg.Receiver.Platform + "' and own platform '" + Id.Platform + "'...");
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
//Logger.DecLvl();
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
//Logger.DecLvl();
return true;
}
}
public void ScheduleMessage(AgentMessage msg)
{
//Logger.debug("Agent.ScheduleMessage()");
//Logger.IncLvl();
ScheduledMessages.Add(msg);
ScheduleRefresh();
//Logger.DecLvl();
}
public void SendScheduledMessages()
{
//Logger.debug("Agent.SendScheduledMessages()");
//Logger.IncLvl();
for (int i = ScheduledMessages.Count - 1; i >= 0; i--)
{
AgentMessage msg = ScheduledMessages[i];
ScheduledMessages.RemoveAt(i);
SendMessage(ref msg);
}
//Logger.DecLvl();
}
public void RegisterService(string id, Func<Agent, AgentProtocol> createChat)
{
RegisterService(id, createChat, new Dictionary<string, string> ());
}
public void RegisterService(string id, Func<Agent, AgentProtocol> createChat, Dictionary<string, string> options)
{
//Logger.debug("Agent.RegisterService()");
//Logger.IncLvl();
Services.Add(id, new Service(
id,
(options.GetValueOrDefault("description") as String) ?? "",
this.Id,
new AgentId(options.GetValueOrDefault("permissions", "ANY@ANY")),
options.ContainsKey("providesui") && options["providesui"] != "false",
createChat)
);
//Logger.DecLvl();
}
public bool AddChat(AgentProtocol chat)
{
//Logger.debug("Agent.AddChat();");
//Logger.IncLvl();
if(Chats.ContainsKey(chat.ChatId))
{
//Logger.DecLvl();
return false;
}
else
{
Chats[chat.ChatId] = chat;
//Logger.DecLvl();
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
//Logger.log("Agent.StopChat() {" + chat.GetProtocolId() + "}");
//Logger.IncLvl();
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
//Logger.DecLvl();
}
public void SetTimer(IMyTimerBlock timer)
{
Timer = timer;
}
public void ScheduleRefresh()
{
//Logger.debug("Agent.ScheduleRefresh()");
//Logger.IncLvl();
RefreshScheduled = true;
if (Timer != null && !Timer.IsCountingDown)
{
Timer.GetActionWithName("Start").Apply(Timer);
}
//Logger.DecLvl();
}
public virtual void Refresh(TimeSpan elapsedTime)
{
//Logger.log("Agent.Refresh()");
//Logger.IncLvl();
ElapsedTimeValue += elapsedTime;
if (!RefreshScheduled)
{
return;
}
RefreshScheduled = false;
SendScheduledMessages();
Event("refresh");
ElapsedTimeValue = new TimeSpan(0);
//Logger.DecLvl();
}
}

public abstract class AgentProtocol
{
static int ChatCountValue;
private int ChatIdValue;
private int PartnerIdValue;
protected Agent Holder;
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
public AgentProtocol(Agent agent)
{
//Logger.debug("AgentProtocol constructor()");
//Logger.IncLvl();
ChatIdValue = ChatCount;
ChatCountValue++;
Holder = agent;
//Logger.DecLvl();
}
public bool TrySetId(int id)
{
//Logger.debug("AgentProtocol.TrySetId()");
//Logger.IncLvl();
if (id == ChatId)
{
//Logger.DecLvl();
return true;
}
else if (id >= ChatCount)
{
ChatCountValue = id + 1;
ChatIdValue = id;
//Logger.DecLvl();
return true;
}
else
{
//Logger.DecLvl();
return false;
}
}
public abstract void Restart();
public virtual void Start() { }
public virtual void Stop()
{
//Logger.debug("AgentProtocol.Stop()");
if (ChatId == ChatCount - 1)
{
ChatCountValue = ChatId;
}
Holder.StopChat(ChatId);
//Logger.DecLvl();
}
public virtual void ReceiveMessage(AgentMessage msg)
{
//Logger.debug("AgentProtocol.ReceiveMessage()");
//Logger.IncLvl();
if (msg.Status == AgentMessage.StatusCodes.CHATIDNOTACCEPTED)
{
string[] contentSplit = msg.Content.Split(':');
if (contentSplit.Length == 2 && contentSplit[0] == "validId")
{
int oldId = ChatId;
int desiredId = -1;
if (!Int32.TryParse(contentSplit[1], out desiredId))
{
//Logger.log("WARNING: Invalid chat id requested!");
Stop();
} else
{
desiredId = Math.Max(ChatCount, desiredId);
if (Holder.UpdateChatId(oldId, desiredId) && TrySetId(desiredId))
{
Restart();
}
else
{
//Logger.log("ERROR: Could not change chat id");
Stop();
}
}
}
}
//Logger.DecLvl();
}
public static string MakeRoute(AgentId aid, string protocolId, string argument)
{
return "man:" + aid + "::" + protocolId + "(" + Parser.Sanitize(argument) + ")";
}
public virtual void Setup() { }
}


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
SetAttribute("sender", value.ToString());
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
get {
Interfaces target;
if(Enum.TryParse<Interfaces>(GetAttribute("targetinterface"), true, out target))
{
return target;
} else
{
return Interfaces.TEXT;
}
}
set
{
SetAttribute("targetinterface", value.ToString());
}
}
public AgentMessage() : base()
{
//Logger.debug("AgentMessage constructor()");
//Logger.IncLvl();
Type = "message";
Attributes = new Dictionary<string, string>();
Content = "";
Status = StatusCodes.OK;
Service = "response";
TargetInterface = Interfaces.TEXT;
//Logger.DecLvl();
}
public AgentMessage(AgentId sender, AgentId receiver, string msg) : this()
{
//Logger.debug("AgentMessage constructor(AgentId, AgentId, string)");
//Logger.IncLvl();
Content = msg;
Sender = sender;
Receiver = receiver;
Status = StatusCodes.OK;
Service = "response";
TargetInterface = Interfaces.TEXT;
//Logger.DecLvl();
}
public AgentMessage(
AgentId sender,
AgentId receiver,
StatusCodes status,
string content,
string requestedService,
int senderchatId
) : this(sender, receiver, status, content, requestedService, senderchatId, -1) { }
public AgentMessage(
AgentId sender,
AgentId receiver,
StatusCodes status,
string content,
string requestedService,
int senderChatId,
int receiverChatId
) : this(sender, receiver, content)
{
//Logger.debug("AgentMessage constructor(AgentId, AgentId, StatusCodes, string, string, int)");
//Logger.IncLvl();
Status = status;
Service = requestedService;
SenderChatId = senderChatId;
ReceiverChatId = receiverChatId;
TargetInterface = Interfaces.TEXT;
//Logger.DecLvl();
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
//Logger.debug("AgentMessage.MakeResponse()");
//Logger.IncLvl();
AgentMessage aMsg = new AgentMessage(
newSender,
Sender,
status,
msg,
"response"
);
aMsg.SenderChatId = ReceiverChatId;
aMsg.ReceiverChatId = SenderChatId;
//Logger.DecLvl();
return aMsg;
}
public AgentMessage Duplicate()
{
AgentMessage msg = new AgentMessage(
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
//Logger.debug("AgentMessage.ToString()");
//Logger.IncLvl();
//Logger.DecLvl();
return ToXML();
}
public string ToXML()
{
//Logger.debug("AgentMessage.ToXML()");
//Logger.IncLvl();
string xml = "<message ";
xml += Parser.PackData(GetValues((node) => true));
xml += "/>";
//Logger.DecLvl();
//Logger.log("sanitize message");
return Parser.Sanitize(xml);
}
public static AgentMessage FromXML(string xml)
{
//Logger.debug("AgentMessage.FromXML()");
//Logger.IncLvl();
SetUp();
xml = Parser.UnescapeQuotes(xml);
XML.XMLTree xmlNode = XML.ParseXML(xml);
AgentMessage msg = xmlNode.GetNode((node) => node.Type == "message") as AgentMessage;
//Logger.DecLvl();
//Logger.log("unescape message");
return msg;
}
public static void SetUp()
{
//Logger.debug("AgentMessage.SetUp()");
//Logger.IncLvl();
if (!XML.NodeRegister.ContainsKey("message"))
{
XML.NodeRegister.Add("message", () => {
return new AgentMessage();
}
);
}
//Logger.DecLvl();
}
}


public class AgentId : IEquatable<AgentId>
{
private string Id;
public string Name
{
get
{
string[] idSplit = Id.Split('@');
return idSplit[0];
}
set
{
string[] idSplit = Id.Split('@');
string platform = (idSplit.Length > 1 ? idSplit[1] : "");
Id = value + "@" + platform;
}
}
public string Platform
{
get
{
string[] idSplit = Id.Split('@');
if (idSplit.Length > 1)
{
return idSplit[1];
}
else
{
return "local";
}
}
set
{
string[] idSplit = Id.Split('@');
Id = idSplit[0] + "@" + value;
}
}
public override string ToString()
{
return Name + "@" + Platform;
}
public AgentId(string id)
{
Id = id;
}
public bool MatchesName(AgentId other)
{
return other.Name == "ANY"
|| other.Name == "ALL"
|| other.Name == Name
|| Name == "ANY"
|| Name == "ALL";
}
public bool MatchesPlatform(AgentId other)
{
return other.Platform == "local"
|| other.Platform == "ALL"
|| other.Platform == "ANY"
|| other.Platform == ""
|| other.Platform == Platform
|| Platform == "ALL"
|| Platform == "ANY";
}
public bool Matches(AgentId other)
{
return MatchesName(other) && MatchesPlatform(other);
}
public bool Equals(AgentId other)
{
return Name == other.Name && (
Platform == other.Platform
|| Platform == "local"
|| other.Platform == "local"
|| Platform == ""
|| other.Platform == "");
}
public override bool Equals(Object obj)
{
if (obj == null)
return false;
AgentId aidObj = obj as AgentId;
if (aidObj == null)
return false;
else
return Equals(aidObj);
}
public static bool operator ==(AgentId id1, AgentId id2)
{
if (((object)id1) == null || ((object)id2) == null)
return Object.Equals(id1, id2);
return id1.Equals(id2);
}
public static bool operator !=(AgentId id1, AgentId id2)
{
if (((object)id1) == null || ((object)id2) == null)
return !Object.Equals(id1, id2);
return !(id1.Equals(id2));
}
}

public class Service : PlatformService
{
public Func<Agent, AgentProtocol> Create;
public Service(string id, string description, AgentId provider, Func<Agent, AgentProtocol> create) :
this(id, description, provider, false, create) { }
public Service(string id, string description, AgentId provider, bool providesUI, Func<Agent, AgentProtocol> create) :
this(id, description, provider, new AgentId("ANY@ANY"), providesUI, create) { }
public Service(string id, string description, AgentId provider, AgentId permissions, Func<Agent, AgentProtocol> create) :
this(id, description, provider, permissions, false, create) { }
public Service(string id, string description, AgentId provider, AgentId permissions, bool providesUI, Func<Agent, AgentProtocol> create)
: base(id, description, provider, permissions, providesUI)
{
Create = create;
}
}

public class PlatformService
{
public string Id;
public string Description;
public AgentId Provider;
private bool IsUIProvider;
public bool ProvidesUI
{
get { return IsUIProvider; }
}
private AgentId PermissionMask;
public PlatformService(string id, string description, AgentId provider) : this(id, description, provider, new AgentId("ANY@ANY"), false) { }
public PlatformService(string id, string description, AgentId provider, AgentId permissionMask) : this(id, description, provider, permissionMask, false) { }
public PlatformService(string id, string description, AgentId provider, AgentId permissionMask, bool providesUI)
{
Id = id;
Description = description;
Provider = provider;
IsUIProvider = providesUI;
PermissionMask = permissionMask;
}
public bool HasPermissions(AgentId agent)
{
return agent.Matches(PermissionMask);
}
public string ToXML()
{
return "<service" + (ProvidesUI ? " providesUI" : "") + " id='" + Id.ToString() + "' description='" + Description + "' provider='" + Provider + "' permissions='" + PermissionMask + "'/>";
}
public string ToString()
{
return ToXML();
}
public static PlatformService FromXMLString(string xml)
{
XML.XMLTree serviceNode = XML.ParseXML(xml).GetNode((node) => node.Type == "service");
return PlatformService.FromXMLNode(serviceNode);
}
public static PlatformService FromXMLNode(XML.XMLTree node)
{
if(node.Type != "service")
{
return null;
}
if (node.GetAttribute("id") == null || node.GetAttribute("provider") == null)
{
return null;
}
return new PlatformService(
node.GetAttribute("id"),
node.GetAttribute("description") ?? "",
new AgentId(node.GetAttribute("provider")),
new AgentId(node.GetAttribute("permissions") ?? "ANY@ANY"),
node.GetAttribute("providesui") != null && node.GetAttribute("providesui") != "false"
);
}
public bool Equals(PlatformService other)
{
return Id == other.Id && Provider == other.Provider;
}
public override bool Equals(Object obj)
{
if (obj == null)
return false;
PlatformService serviceObj = obj as PlatformService;
if (serviceObj == null)
return false;
else
return Equals(serviceObj);
}
public static bool operator ==(PlatformService service1, PlatformService service2)
{
if (((object)service1) == null || ((object)service2) == null)
return Object.Equals(service1, service2);
return service1.Equals(service2);
}
public static bool operator !=(PlatformService service1, PlatformService service2)
{
if (((object)service1) == null || ((object)service2) == null)
return !Object.Equals(service1, service2);
return !(service1.Equals(service2));
}

}

public class PrintProtocol : AgentProtocol
{
public override string GetProtocolId()
{ return "print"; }
public PrintProtocol(Agent agent) : base(agent) { }
public override void ReceiveMessage(AgentMessage msg)
{
//Logger.log("Message received:");
//Logger.IncLvl();
//Logger.log("from: " + msg.Sender);
//Logger.log("content: " + msg.Content);
//Logger.DecLvl();
Stop();
}
public override void Restart() { }
public override void Setup()
{
Holder.RegisterService(GetProtocolId(), (agent) => {
return new PrintProtocol(agent);
});
}
}

class ServiceRegistrationProtocol : AgentProtocol
{
public class Platform : AgentProtocol
{
public override string GetProtocolId()
{ return "register-services"; }
public Platform(Agent agent) : base(agent) { }
public override void ReceiveMessage(AgentMessage msg)
{
//Logger.debug("ServiceRegistrationProtocol.ReceiveMessage(AgentMessage)");
//Logger.IncLvl();
List<XML.XMLTree> services = XML.ParseXML(msg.Content).GetAllNodes((node) => (node.Type == "service"));
PlatformAgent platform = Holder as PlatformAgent;
AgentMessage response;
if (platform == null)
{
response = msg.MakeResponse(
Holder.Id,
AgentMessage.StatusCodes.UNKNOWNERROR,
"ERROR: Agent is no PlatformAgent - service registration not possible!"
);
response.SenderChatId = ChatId;
Holder.SendMessage(ref response);
Stop();
return;
}
AgentId sender = msg.Sender;
sender.Platform = platform.Id.Platform;
PlatformService service;
foreach (XML.XMLTree serviceNode in services)
{
service = PlatformService.FromXMLNode(serviceNode);
//Logger.log("Register service '" + service.Id + "' of '" + sender + "'.");
if (service != null)
{
if (!platform.PlatformServices.ContainsKey(service.Id))
{
platform.PlatformServices[service.Id] = new List<PlatformService>();
}
platform.PlatformServices[service.Id].Add(service);
}
}
response = msg.MakeResponse(
Holder.Id,
AgentMessage.StatusCodes.OK,
"services registered"
);
response.SenderChatId = ChatId;
Holder.SendMessage(ref response);
Stop();
//Logger.DecLvl();
}
public override void Restart() { }
public override void Setup()
{
Holder.RegisterService(GetProtocolId(), (agent) => {
return new Platform(agent);
});
}
}
public override string GetProtocolId()
{ return "complete-service-registration"; }
public ServiceRegistrationProtocol(Agent agent) : base(agent) { }
public override void ReceiveMessage(AgentMessage msg)
{
if (msg.Status == AgentMessage.StatusCodes.OK && msg.Content == "services registered")
{
//Logger.log("Setting agent platform to '" + msg.Sender.Name + "'.");
Holder.Id.Platform = msg.Sender.Name;
Holder.Event("register");
Stop();
}
else
{
base.ReceiveMessage(msg);
}
}
public override void Restart() { }
public override void Setup()
{
Holder.RegisterService(GetProtocolId(), (agent) => {
return new ServiceRegistrationProtocol(agent);
});
}
}

public class ServiceRegister : Dictionary<string, List<PlatformService>>
{
public ServiceRegister Merge(ServiceRegister other)
{
ServiceRegister merged = new ServiceRegister();
foreach(KeyValuePair<string, List<PlatformService>> item in this)
{
merged.Add(item.Key, item.Value);
}
foreach(KeyValuePair<string, List<PlatformService>> item in other)
{
if(merged.ContainsKey(item.Key))
{
merged[item.Key].AddList<PlatformService>(item.Value);
merged[item.Key] = Util.Uniques<PlatformService>(merged[item.Key]);
} else
{
merged[item.Key] = item.Value;
}
}
return merged;
}
public string ToXML()
{
string xml = "<services>";
foreach(KeyValuePair<string, List<PlatformService>> item in this) {
foreach(PlatformService service in item.Value)
{
xml += service.ToXML();
}
}
xml += "</services>";
return xml;
}
}
//!EMBED SEScripts.MultiAgentNetwork.MAN.Protocols.MessageRoutingProtocol

class ProvideServiceInformationProtocol : AgentProtocol
{
public override string GetProtocolId()
{ return "get-services"; }
public ProvideServiceInformationProtocol(Agent agent) : base(agent) { }
public override void ReceiveMessage(AgentMessage msg)
{
if (msg.Status == AgentMessage.StatusCodes.OK)
{
List<PlatformService> services = new List<PlatformService>(Holder.Services.Values);
PlatformAgent platform = Holder as PlatformAgent;
if (platform != null)
{
foreach (List<PlatformService> platformServices in platform.PlatformServices.Values)
{
services.AddRange(platformServices);
}
}
services = Util.Uniques<PlatformService>(services);
string content = "<platformInfo platformname='" + Parser.Sanitize(Holder.Prog.Me.CubeGrid.CustomName) + "'/>";
content += "<services>";
foreach (PlatformService service in services)
{
if((msg.TargetInterface == AgentMessage.Interfaces.TEXT || service.ProvidesUI) )//&& service.HasPermissions(msg.Sender))
{
content += service.ToXML();
}
else
{
if(!service.HasPermissions(msg.Sender))
{
//Logger.log("no permissions: " + service.Id);
}
}
}
content += "</services>";
AgentMessage message = msg.MakeResponse(Holder.Id, AgentMessage.StatusCodes.OK, content);
message.TargetInterface = AgentMessage.Interfaces.UI;
message.SenderChatId = ChatId;
Holder.ScheduleMessage(message);
Stop();
}
else
{
base.ReceiveMessage(msg);
}
}
public override void Restart() { }
public override void Setup()
{
Holder.RegisterService(
GetProtocolId(),
(agent) => {
return new ProvideServiceInformationProtocol(agent);
},
new Dictionary<string, string>
{
{"description", "List Services"}
}
);
}
}

//!EMBED SEScripts.MultiAgentNetwork.MAN.Agents.PlatformAgent
//!EMBED SEScripts.MultiAgentNetwork.MAN.Agents.RegisteredAgent

public class UITerminalAgent : RegisteredAgent
{
public XML.UIController UI;
public IMyTextPanel Screen;
const string loadingPage = "<meta historyDisabled/><hl width='100%'/>Requesting Services...<hl width='100%'/>";
public UITerminalAgent(MyGridProgram program, IMyTextPanel screen) : base(program)
{
//Logger.debug("UITerminalAgent constructor()");
//Logger.IncLvl();
Screen = screen;
//Logger.log("Setting up UI");
UI = XML.UIController.FromXML(loadingPage);
//Logger.log("Setup RequestRouteProtocol");
new RequestRouteProtocol(this).Setup();
//Logger.log("Setup UIServiceIndex");
UIServiceIndexServer indexServer = new UIServiceIndexServer(this);
indexServer.Setup();
indexServer.LoadHomeScreen();
//UI.FollowRoute(new XML.Route(AgentProtocol.MakeRoute(Id, indexServer.GetProtocolId(),"page='refresh'")));
//UpdateScreen();
//Logger.DecLvl();
}
public void LoadXML(string uiString)
{
//Logger.debug("UITerminalAgent.LoadUI()");
//Logger.IncLvl();
UI.LoadXML(uiString);
//Logger.DecLvl();
}
public void LoadUI(XML.XMLTree ui)
{
//Logger.debug("UITerminalAgent.LoadUI()");
//Logger.IncLvl();
UI.LoadUI(ui);
if(UI.HasUserInputBindings)
{
ScheduleRefresh();
}
//Logger.DecLvl();
}
public void UpdateScreen()
{
//Logger.debug("UITerminalAgent.UpdateScreen()");
//Logger.IncLvl();
UI.RenderTo(Screen);
UI.ApplyScreenProperties(Screen);
//Logger.DecLvl();
}
public void Call(List<string> parameters)
{
//Logger.debug("UITerminalAgent.Call()");
//Logger.IncLvl();
UI.Call(parameters);
UpdateScreen();
if (UI.HasUserInputBindings)
{
ScheduleRefresh();
}
//Logger.DecLvl();
}
public override void Refresh(TimeSpan elapsedTime)
{
base.Refresh(elapsedTime);
UI.Call(new List<string> { "refresh" });
if (UI.UpdateUserInput())
{
UpdateScreen();
}
}
}

public class RegisteredAgent : Agent
{
public RegisteredAgent(MyGridProgram program) : base(program)
{
if (Id.Platform != "local" && (GTS.GetBlockWithName(Id.Platform) as IMyProgrammableBlock) == null)
{
Id.Platform = "local";
}
new ServiceRegistrationProtocol(this).Setup();
new PrintPlatformServicesProtocol(this).Setup();
}
public void RegisterWith(string platformName)
{
//Logger.log("RegisteredAgent.RegisterWith()");
//Logger.IncLvl();
AgentId platform = new AgentId(platformName + "@local");
ServiceRegistrationProtocol chat = new ServiceRegistrationProtocol(this);
Chats[chat.ChatId] = chat;
string content = "<services>";
foreach (KeyValuePair<string, Service> service in Services)
{
if(service.Value.ProvidesUI || service.Value.HasPermissions(new AgentId("**@local")))
{
content += service.Value.ToXML();
}
}
content += "</services>";
AgentMessage message = new AgentMessage(
this.Id,
platform,
AgentMessage.StatusCodes.OK,
content,
new ServiceRegistrationProtocol.Platform(this).GetProtocolId(),
chat.ChatId
);
//Logger.log("Registering:");
//Logger.log("msg: " + message.ToXML());
SendMessage(ref message);
//Logger.DecLvl();
}
}

class PrintPlatformServicesProtocol : AgentProtocol
{
int State;
public override string GetProtocolId()
{ return "print-platform-services"; }
public PrintPlatformServicesProtocol(Agent agent) : base(agent)
{
//Logger.log("Create new PrintPlatformServicesProtocol");
State = 0;
}
public override void ReceiveMessage(AgentMessage msg)
{
//Logger.log("PrintPlatformServicesProtocol.ReceiveMessage(message)");
switch (State)
{
case 0:
//Logger.log("Handling state 0");
if (Holder.Id.Platform == "local")
{
//Logger.log("WARNING: PrintPlatformServicesProtocol started, but agent is not registered at any platform!");
Stop();
return;
}
else
{
AgentMessage newMsg = new AgentMessage(
Holder.Id,
new AgentId(Holder.Id.Platform + "@local"),
AgentMessage.StatusCodes.OK,
"",
"get-services",
ChatId
);
newMsg.SenderChatId = ChatId;
Holder.SendMessage(ref newMsg);
State = 1;
}
break;
case 1:
//Logger.log("Handling state 1");
if (msg.Status == AgentMessage.StatusCodes.OK)
{
List<XML.XMLTree> services = XML.ParseXML(msg.Content).GetAllNodes((node) => node.Type == "service");
//Logger.log("Available Platform Services:");
foreach (XML.XMLTree service in services)
{
//Logger.log("  " + service.GetAttribute("id"));
}
}
else
{
base.ReceiveMessage(msg);
//Logger.log("An error occured in protocol PrintPlatformServicesProtocol: " + msg.Status.ToString());
}
Stop();
break;
}
}
public override void Restart()
{
State = 0;
ReceiveMessage(null);
}
public override void Setup()
{
Holder.RegisterService(GetProtocolId(), (agent) => {
return new PrintPlatformServicesProtocol(agent);
});
}
}

class RequestRouteProtocol : AgentProtocol
{
String RouteDefinition;
const string LoadingPage = "<meta refresh historyDisabled backgroundColor='000000' fontColor='CC0000'/><br/><br/><hl/>Loading...<hl/>";
const string DefaultColors = "<meta historyDisabled fontColor='FFFFFF' backgroundColor='000000'/>";
const string Header = "<uicontrols>Browser</uicontrols><hl/>";
public override string GetProtocolId()
{ return "request-route"; }
private XML.UIController UI;
public RequestRouteProtocol(Agent agent) : base(agent)
{
//Logger.debug("RequestRouteProtocol constructor()");
}
public override void Restart()
{
//Logger.debug("RequestRouteProtocol.Restart()");
//Logger.IncLvl();
if (RouteDefinition != null) {
RequestRoute(RouteDefinition);
}
//Logger.DecLvl();
}
public override void ReceiveMessage(AgentMessage msg)
{
//Logger.debug("RequestRouteProtocol.ReceiveMessage()");
//Logger.IncLvl();
if (msg.Status != AgentMessage.StatusCodes.OK)
{
base.ReceiveMessage(msg);
return;
}
if(msg.GetAttribute("uiupdates") != null || msg.GetAttribute("uiupdates") != "false")
{
ListenForUIUpdate(true);
}
UITerminalAgent UIAgent = Holder as UITerminalAgent;
if (UIAgent != null)
{
try
{
XML.XMLTree xml = XML.ParseXML(msg.Content);
UIAgent.LoadUI(xml);
UIAgent.UpdateScreen();
} catch
{
//Logger.log("WARNING: Invalid UI received from " + msg.Sender);
}
//UIAgent.Call(new List<string> { "" });
Stop();
}
else
{
//Logger.log("WARNING: Agent is no UITerminalAgent. Can not display UI.");
base.ReceiveMessage(msg);
Stop();
//Logger.DecLvl();
return;
}
//Logger.DecLvl();
}
public bool RequestRoute(string routeString)
{
//Logger.log("RequestRouteProtocol.RequestRoute()");
//Logger.IncLvl();
using (Logger logger = new Logger("RequestRouteProtocol.RequestRoute()", Logger.Mode.LOG))
{
string mafRoutePattern = @"(?<provider>[\w{}\s_\-#]+@[\w{}\s_\-#]+)::(?<service>[\w\-]+)(\((?<argument>[^)]*)\)){0,1}";
System.Text.RegularExpressions.Regex mafRouteRegex = new System.Text.RegularExpressions.Regex(mafRoutePattern);
System.Text.RegularExpressions.Match mafRouteMatch = mafRouteRegex.Match(routeString);
if (!mafRouteMatch.Success)
{
throw new Exception("WARNING: Route not understood: <<" + routeString + ">>");
//Logger.log("WARNING: Route not understood: <<" + routeString + ">>");
//Logger.DecLvl();
Stop();
return false;
}
ListenForUIUpdate(false);
AgentId id = new AgentId(mafRouteMatch.Groups["provider"].Value);
string service = mafRouteMatch.Groups["service"].Value;
string content = Parser.UnescapeQuotes(mafRouteMatch.Groups["argument"].Value);
UITerminalAgent UIAgent = Holder as UITerminalAgent;
content += " " + (UIAgent?.UI.GetPackedValues() ?? "");
UIAgent?.LoadXML(LoadingPage);
UIAgent?.UpdateScreen();
RouteDefinition = routeString;
AgentMessage request = new AgentMessage(
Holder.Id,
id,
AgentMessage.StatusCodes.OK,
content,
service,
ChatId
);
request.TargetInterface = AgentMessage.Interfaces.UI;
request.SenderChatId = ChatId;
Holder.ScheduleMessage(request);
//Logger.DecLvl();
logger.log("requesting route: " + request.ToXML(), Logger.Mode.LOG);
return true;
}
}
public void ListenForUIUpdate(bool on)
{
if(on)
{
Holder.RegisterService("update-ui", (agent) => new RequestRouteProtocol(agent));
}
else
{
Holder.Services.Remove("update-ui");
}
}
public override void Setup()
{
//Logger.debug("RequestRouteProtocol.RegisterServices()");
//Logger.IncLvl();
/*Holder.RegisterService(
GetProtocolId(),
(agent) => {
return new RequestRouteProtocol(agent);
});*/
XML.Route.RouteHandlers.Add("man", (def, controller) => {
using (Logger logger = new Logger("Handle man route", Logger.Mode.LOG))
{
RequestRouteProtocol request = new RequestRouteProtocol(Holder);
Holder.AddChat(request);
request.UI = controller;
request.RequestRoute(def);
}
});
//Logger.DecLvl();
}
}


public class UIServiceIndexServer : AgentProtocol
{
static string XMLHeader = "<meta fontColor='000000' backgroundColor='999999' fontSize='1.5' $ATTRIBUTES$/><uicontrols>$TITLE$</uicontrols><hl/>";
public override string GetProtocolId()
{
return "get-ui-services-index";
}
private int RefreshTime;
private int ResetTime;

public Dictionary<string, List<PlatformService>> Services
{
get
{
if(Holder.GetKnowledgeEntry("UISERVICES", this) == null)
{
Holder.SetKnowledgeEntry("UISERVICES", new Dictionary<string, List<PlatformService>>(), this);
}
return Holder.GetKnowledgeEntry("UISERVICES", this) as Dictionary<string, List<PlatformService>>;
}
}
public Dictionary<string, string> Platforms
{
get
{
if(Holder.GetKnowledgeEntry("UIPLATFORMS", this) == null)
{
Holder.SetKnowledgeEntry("UIPLATFORMS", new Dictionary<string, string>(), this);
}
return Holder.GetKnowledgeEntry("UIPLATFORMS", this) as Dictionary<string, string>;
}
}
AgentId UIReceiver;
bool HomePageActive
{
get
{
object active = Holder.GetKnowledgeEntry("HomePageActive", this);
return active != null && (bool)active;
}
set
{
Holder.SetKnowledgeEntry("HomePageActive", value, this);
}
}
public UIServiceIndexServer(Agent holder) : base(holder)
{
//Logger.log("UIServiceIndexServer constructor");
RefreshTime = 1250;
ResetTime = 10000;
}
public override void ReceiveMessage(AgentMessage msg)
{
//Logger.IncLvl();
//Logger.debug("UIServicesIndexServer.ReceiveMessage()");
if(msg.Status == AgentMessage.StatusCodes.OK)
{
ReceiveServices(msg);
}
else
{
base.ReceiveMessage(msg);
}
//Logger.DecLvl();
}
private void ReceiveServices(AgentMessage msg)
{
//Logger.debug("UIServicesIndexServer.ReceiveServices()");
//Logger.IncLvl();
XML.XMLTree messageXML;
try
{
//Logger.log("test");
messageXML = XML.ParseXML(msg.Content);
}
catch
{
//Logger.log("WARNING: Invalid message received!");
//Logger.DecLvl();
return;
}
string platformId = msg.Sender.Platform;
string platformName = messageXML.GetNode((node) => node.Type == "platforminfo")?.GetAttribute("platformname");
List<PlatformService> services = new List<PlatformService>();
Services[platformId] = new List<PlatformService>();
Platforms[platformId] = Parser.UnescapeQuotes(platformName ?? platformId);
PlatformService service;
foreach (XML.XMLTree serviceNode in messageXML.GetAllNodes((node) => node.Type == "service"))
{
service = PlatformService.FromXMLNode(serviceNode);
if (service != null)
{
Services[platformId].Add(service);
}
}
/*if(UIReceiver != null && Holder.Services.ContainsKey(GetProtocolId()))
{
AgentMessage fakeRequest = new AgentMessage(UIReceiver, Holder.Id, AgentMessage.StatusCodes.OK, "", GetProtocolId());
Holder.ReceiveMessage(fakeRequest);
}*/
if (HomePageActive) LoadHomeScreen();
//Logger.DecLvl();
}

public override void Restart()
{ }
public override void NotifyEvent(string eventId)
{
//Logger.log("UIServiceIndexServer.NotifyEvent()");
//Logger.IncLvl();
switch (eventId)
{
case "register":
RetrieveServices();
break;
case "refresh":
int tuRefresh = (int)Holder.GetKnowledgeEntry("TimeUntilRefresh", this);
tuRefresh -= Holder.ElapsedTime.Milliseconds;
//Logger.log("Time Till Refresh: " + ttr);
if (tuRefresh < 0)
{
tuRefresh = RefreshTime;
RetrieveServices();
}
int tuReset = (int)Holder.GetKnowledgeEntry("TimeUntilReset", this);
tuReset -= Holder.ElapsedTime.Milliseconds;
if(tuReset < 0)
{
tuReset = ResetTime;
if (!HomePageActive)
{
(Holder as UITerminalAgent)?.UI.ClearUIStack();
LoadHomeScreen();
}

}
Holder.ScheduleRefresh();
Holder.SetKnowledgeEntry("TimeTillRefresh", tuRefresh, this);
Holder.SetKnowledgeEntry("TimeTillReset", tuReset, this);
break;
default:
base.NotifyEvent(eventId);
break;
}
//Logger.DecLvl();
}
public void SetUIReceiver(AgentId receiver)
{
UIReceiver = receiver;
}
private void RetrieveServices()
{
//Logger.debug("UIServiceIndexServer.RetrieveServices()");
//Logger.IncLvl();
AgentMessage request = new AgentMessage(
Holder.Id,
new AgentId("ALL@ALL"),
AgentMessage.StatusCodes.OK,
"",
"get-services",
ChatId);
request.TargetInterface = AgentMessage.Interfaces.UI;
request.SenderChatId = ChatId;
Holder.SendMessage(ref request);
//Logger.DecLvl();
}

public string PageHome()
{
//Logger.debug("UIServiceIndexServer.PageHome()");
//Logger.IncLvl();
StringBuilder xml = new StringBuilder(XMLHeader).Replace("$TITLE$", "Platforms").Replace("$ATTRIBUTES$", "uiServiceIndexHome");
xml.Append("<menu id='platformMenu'>");
xml.Append(GetPlatformMenuitems());
xml.Append("</menu>");
//xml.Append("<menuItem route='" + MakeRoute(Holder.Id, GetProtocolId(), "page='refresh'") + "'>" + "Refresh</menuitem>");
//Logger.DecLvl();
return xml.ToString();
}
private StringBuilder GetPlatformMenuitems()
{
StringBuilder xml = new StringBuilder();
foreach (string key in Platforms.Keys)
{
if(Services[key].Count > 0)
xml.Append("<menuItem id='" + key + "' route='fn:show-platform-services' platform='" + key + "'>" + Platforms[key] + "</menuitem>");
}
return xml;
}
public string PagePlatformServices(string platformId)
{
StringBuilder xml = new StringBuilder(XMLHeader).Replace("$TITLE$", Platforms[platformId] + " Services");
xml.Append("<menu>");
foreach(PlatformService service in Services[platformId])
{
xml.Append("<menuitem route='" + MakeRoute(service.Provider, service.Id, "") + "'>" + service.Description + "</menuitem>");
}
xml.Append("</menu>");
return xml.ToString();
}
public void LoadHomeScreen()
{
//Logger.log("UIServiceIndexServer.LoadHomeScreen()");
//Logger.IncLvl();
UITerminalAgent UIAgent = Holder as UITerminalAgent;
if(UIAgent != null)
{
XML.XMLTree meta = UIAgent.UI.GetNode((node) => node.Type == "meta");
bool isLoaded = (meta?.GetAttribute("uiserviceindexhome") != null);
if (HomePageActive && !isLoaded )
{
//Logger.log("Setting HomPageActive to false");
HomePageActive = false;
}
else
{
XML.XMLTree ui = XML.ParseXML(PageHome());
if (isLoaded)
{
meta.SetAttribute("historydisabled", "true");
//Logger.log("Get id of selected node");
/*XML.XMLTree selected = UIAgent.UI.GetSelectedNode();
//Logger.log("got selected node! (" + selected + ")");
//Logger.log("selected node is " + (selected == null ? "null!" : "of type " + selected.Type));
string selectedId = selected.GetAttribute("id");
//Logger.log("got selected id!")
//string selectedId = null;
//Logger.log("id is: " + selectedId ?? "null" );
if (selectedId != null)
{
//Logger.log("Get node to select in new UI");
XML.XMLTree selectedNode = ui.GetNode((node) => (node.GetAttribute("id") == selectedId));
//Logger.log("found it!");
if (selectedNode != null && selectedNode.IsSelectable())
{
//Logger.log("select node...");
while (!selectedNode.IsSelected())
{
ui.SelectNext();
}
}
}
//Logger.log("done");*/
}
UIAgent.LoadUI(ui);
HomePageActive = true;
UIAgent.UpdateScreen();
Holder.ScheduleRefresh();
}
}
//Logger.DecLvl();
}
public override void Setup()
{
XML.Route.RegisterRouteFunction("show-platform-services", (controller) =>
{
string platformId = controller.GetSelectedNode().GetAttribute("platform");
if (platformId != null)
{
controller.LoadUI(XML.ParseXML(PagePlatformServices(platformId)));
}
});
//Logger.log("Create event listeners");
Holder.SetKnowledgeEntry("TimeUntilRefresh", RefreshTime, this);
Holder.SetKnowledgeEntry("TimeUntilReset", ResetTime, this);
AgentProtocol chat = new UIServiceIndexServer(Holder);
Holder.AddChat(chat);
Holder.OnEvent("register", chat);
Holder.OnEvent("refresh", chat);
Holder.SetKnowledgeEntry("HomePageActive", false, this);
//Logger.DecLvl();
}
}


public class UIServerProtocol : AgentProtocol
{
public override string GetProtocolId()
{ return "serve-ui-page"; }
public string ApplicationId;
public Dictionary<string, string> ResponseData;
public IMyGridTerminalSystem GTS
{
get
{
return Holder.GTS;
}
}
public Dictionary<string, Func<UIServerProtocol, AgentMessage, Dictionary<string, string>, string>> Pages
{
get
{
return GetPageGenerators().Where((page) => page.Key.StartsWith(ApplicationId + "_")).ToDictionary(page => page.Key, page => page.Value);
}
set
{
foreach(KeyValuePair<string, Func<UIServerProtocol, AgentMessage, Dictionary<string, string>, string>> page in value)
{
SetPageGenerator(page.Key, page.Value, false);
}
}
}
public UIServerProtocol(Agent holder) : base(holder)
{
//Logger.debug("UIServerProtocol constructor");
//Logger.IncLvl();
if(!GetPageGenerators().ContainsKey("404"))
{
GetPageGenerators()["404"] = (agent, msg, data) => "Page not found!<hl/><uicontrols>BACK</uicontrols>";
}
ApplicationId = "";
//Logger.DecLvl();
}
public void SetPageGenerator(string id, Func<UIServerProtocol, AgentMessage, Dictionary<string, string>, string> pageGenerator)
{
SetPageGenerator(id, pageGenerator, false);
}
private void SetPageGenerator(string id, Func<UIServerProtocol, AgentMessage, Dictionary<string, string>, string> pageGenerator, bool global)
{
//Logger.debug("UIServerProtocol.SetPageGenerator()");
//Logger.IncLvl();
string pageId = global ? id : ApplicationId + "_" + id;
GetPageGenerators()[pageId] = pageGenerator;
//Logger.DecLvl();
}
public Func<UIServerProtocol, AgentMessage, Dictionary<string, string>, string> GetPageGenerator(string id)
{
return GetPageGenerator(id, false);
}
public string GetPage(string id, AgentMessage msg, Dictionary<string, string> data)
{
return GetPageGenerator(id)(this, msg, data);
}
private Func<UIServerProtocol, AgentMessage, Dictionary<string, string>, string> GetPageGenerator(string id, bool global)
{
//Logger.debug("UIServerProtocol.GetPageGenerator()");
//Logger.IncLvl();
string pageId = global ? id : ApplicationId + "_" + id;
//Logger.DecLvl();
return GetPageGenerators()?.GetValueOrDefault(pageId, GetPageGenerators()["404"] ?? ((a,m,b) => "404") );
}
private Dictionary<string, Func<UIServerProtocol, AgentMessage, Dictionary<string, string>, string>> GetPageGenerators()
{
//Logger.debug("UIServerProtocol.GetPageGenerators()");
//Logger.IncLvl();
if (Holder.GetKnowledgeEntry("UIPAGES", this) == null)
{
Holder.SetKnowledgeEntry("UIPAGES", new Dictionary<string, Func<UIServerProtocol, AgentMessage, Dictionary<string, string>, string>>(), this);
}
//Logger.DecLvl();
return Holder.GetKnowledgeEntry("UIPAGES", this) as Dictionary<string, Func<UIServerProtocol, AgentMessage, Dictionary<string, string>, string>>;
}
public override void ReceiveMessage(AgentMessage msg)
{
//Logger.debug("UIServerProtocol.ReceiveMessage()");
//Logger.IncLvl();
if (msg.Status == AgentMessage.StatusCodes.OK)
{
Dictionary<string, string> data = Parser.GetXMLAttributes(msg.Content);
ResponseData = new Dictionary<string, string>();
string page = GetPage(
data.GetValueOrDefault<string, string>("page") ?? "",
msg,
data);
AgentMessage response = msg.MakeResponse(
Holder.Id,
AgentMessage.StatusCodes.OK,
page);
response.SenderChatId = ChatId;
foreach(KeyValuePair<string, string> entry in ResponseData)
{
if(response.GetAttribute(entry.Key) == null)
{
response.SetAttribute(entry.Key, entry.Value);
}
}
Holder.SendMessage(ref response);
Stop();
}
else
{
base.ReceiveMessage(msg);
}
//Logger.DecLvl();
}
public static bool CreateApplication(Agent holder, string id, string title, Dictionary<string, Func<UIServerProtocol, AgentMessage, Dictionary<string, string>, string>> pages)
{
//Logger.debug("UIServerProtocol.CreateApplication()");
//Logger.IncLvl();
UIServerProtocol setup = new UIServerProtocol(holder);
setup.SelectApplication(id);
setup.Pages = pages;
if(holder.Services.ContainsKey(id))
{
//Logger.DecLvl();
return false;
}
holder.RegisterService(
id,
(agent) =>
{
UIServerProtocol uiServer = new UIServerProtocol(agent);
uiServer.SelectApplication(id);
return uiServer;
},
new Dictionary<string, string>
{
{"description", title},
{"providesui", "true"}
}
);
//Logger.DecLvl();
return true;
}
static public Func<UIServerProtocol, AgentMessage, Dictionary<string, string>, string> SimplePage(string id, string uiContent)
{
return (session, msg, data) => uiContent;
}

public override void Restart() { }

public void SelectApplication(string id)
{
ApplicationId = id;
}
public string MakeApplicationRoute(string page, string argument)
{
return MakeRoute(Holder.Id, ApplicationId, "page='" + page + "' " + argument);
}
}

}

public class DataStorage : XML.DataStore
{
private Dictionary<string, Type> String2Type;
private Dictionary<Type, string> Type2String;
private Dictionary<string, string> StringEntries;
private Dictionary<string, int> IntegerEntries;
private Dictionary<string, float> FloatEntries;
private static DataStorage Instance;
private DataStorage() : base()
{
String2Type = new Dictionary<string, Type> {
{"string", typeof(String) },
{"int", typeof(int) },
{"float", typeof(float) }
};
Type2String = new Dictionary<Type, string> {
{ typeof(String), "string" },
{ typeof(int), "int" },
{ typeof(float), "float" }
};
Type = "data";
Attributes = new Dictionary<string, string>();
StringEntries = new Dictionary<string, string>();
IntegerEntries = new Dictionary<string, int>();
FloatEntries = new Dictionary<string, float>();
}
public static DataStorage GetInstance()
{
if (Instance == null)
{
Instance = new DataStorage();
}
return Instance;
}
public void Save(out string data)
{
UpdateAttributes();
DataStorage.SetUp();
data = "<data " + Parser.PackData(GetValues((node) => true)) + "/>";
}
private void UpdateAttributes()
{
foreach (KeyValuePair<string, string> entry in StringEntries)
{
Attributes[entry.Key] = "string:" + entry.Value;
}
foreach (KeyValuePair<string, int> entry in IntegerEntries)
{
Attributes[entry.Key] = "int:" + entry.Value.ToString();
}
foreach (KeyValuePair<string, float> entry in FloatEntries)
{
Attributes[entry.Key] = "float:" + entry.Value.ToString();
}
}
public static DataStorage Load(string Storage)
{
DataStorage.SetUp();
XML.XMLTree xml = XML.ParseXML(Storage);
if (xml != null)
{
DataStorage ds = xml.GetNode((node) => node.Type == "data") as DataStorage;
if (ds != null)
{
Instance = ds;
}
}
return DataStorage.GetInstance();
}
public void Set<T>(string key, T value)
{
Type type = GetEntryType(key);
if (type != null && type != typeof(T))
{
throw new Exception("ERROR: An entry for key '" + key + "' does already exist, but is of type '" + type.ToString() + "'!");
}
if (typeof(T) == typeof(string))
{
StringEntries[key] = (string)(object)value;
}
else if (typeof(T) == typeof(int))
{
IntegerEntries[key] = (int)(object)value;
}
else if (typeof(T) == typeof(float))
{
FloatEntries[key] = (float)(object)value;
}
}
public Type GetEntryType(string key)
{
if (StringEntries.ContainsKey(key))
{
return typeof(string);
}
else if (IntegerEntries.ContainsKey(key))
{
return typeof(int);
}
else if (FloatEntries.ContainsKey(key))
{
return typeof(float);
}
else
{
return null;
}
}
public T Get<T>(string key)
{
if (!Exists1<T>(key))
{
throw new Exception("No entry found for key '" + key + "' of type '" + typeof(T).ToString() + "'!");
}
if (typeof(T) == typeof(string))
{
return (T)(object)StringEntries[key];
}
else if (typeof(T) == typeof(int))
{
return (T)(object)IntegerEntries[key];
}
else if (typeof(T) == typeof(float))
{
return (T)(object)FloatEntries[key];
}
else
{
throw new Exception("Error: Invalid Type at DataStore.Get<Type>(string key)!");
}
}
public bool Exists(string key)
{
return (Exists1<string>(key) || Exists1<int>(key) || Exists1<float>(key));
}
public bool Exists1<T>(string key)
{
if (typeof(T) == typeof(string))
{
return StringEntries.ContainsKey(key);
}
else if (typeof(T) == typeof(int))
{
return IntegerEntries.ContainsKey(key);
}
else if (typeof(T) == typeof(float))
{
return FloatEntries.ContainsKey(key);
}
return false;
}
public List<string> GetKeys()
{
List<string> keys = new List<string>(
StringEntries.Keys.Count +
IntegerEntries.Keys.Count +
FloatEntries.Keys.Count
);
keys.AddRange(StringEntries.Keys);
keys.AddRange(IntegerEntries.Keys);
keys.AddRange(FloatEntries.Keys);
return keys;
}
public static void SetUp()
{
if (!XML.NodeRegister.ContainsKey("data"))
{
XML.NodeRegister.Add("data", () => DataStorage.GetInstance());
}
}
public override void SetAttribute(string key, string value)
{
if (StringEntries == null || IntegerEntries == null || FloatEntries == null)
{
base.SetAttribute(key, value);
return;
}
Type type = typeof(string);
string[] valueSplit = value.Split(':');
if (valueSplit.Length > 1 && String2Type.ContainsKey(valueSplit[0]))
{
type = String2Type[valueSplit[0]];
valueSplit[0] = "";
value = String.Join(":", valueSplit).Substring(1);
}
if (type == typeof(string))
{
Set(key, value);
}
else if (type == typeof(int))
{
int intValue;
if (Int32.TryParse(value, out intValue))
{
Set(key, intValue);
}
}
else if (type == typeof(float))
{
float floatValue;
if (Single.TryParse(value, out floatValue))
{
Set(key, floatValue);
}
}
}
}


public class Logger : IDisposable
{
public enum Mode { DEBUG, LOG, ERROR, WARNING, CONSOLE};
private static StringBuilder Log = new StringBuilder();
static public bool DEBUG = false;
protected static StringBuilder Prefix = new StringBuilder();
protected Program Prog;
protected Mode logMode;
private bool disposed;
public static string Output
{
get { return Log.ToString(); }
}
public Logger(string message) : this(message, Mode.DEBUG) {}
public Logger(string message, Mode mode) : this(message, mode, null) {}
public Logger(string message, Mode mode, Program program)
{
disposed = false;
if (!DEBUG && mode == Mode.DEBUG)
return;
Prog = program;
logMode = mode;
log(message, logMode);
IncLvl();
}
public void log(string message, Mode mode)
{
log(new StringBuilder(message), mode);
}
public void log(StringBuilder message, Mode mode)
{
StringBuilder msg = new StringBuilder().Append(Prefix);
if (logMode != Mode.LOG && logMode != Mode.CONSOLE)
msg.Append(logMode.ToString()).Append(": ");
msg.Append(message);
Log.Append(msg).Append("\n");
if (logMode == Mode.CONSOLE)
{
if(Prog != null)
Prog?.Echo(msg.ToString());
}
}
private void IncLvl()
{
Prefix.Append("  ");
}
private void DecLvl()
{
if( Prefix.Length >= 2)
Prefix.Remove(Prefix.Length - 2, 2);
}

public virtual void Dispose()
{
if (!disposed)
{
DecLvl();
}
disposed = true;
}
public static void Clear()
{
Log = new StringBuilder();
}
}

public static class Parser
{
public static string PackData(Dictionary<string, string> data)
{
StringBuilder dataString = new StringBuilder();
foreach (string key in data.Keys)
{
dataString.Append(key + "=\"" + data[key] + "\" ");
}
return dataString.ToString();
}
public static string Sanitize(string xmlDefinition)
{
//using (new Logger("Parser.Sanitize"))
//{
return xmlDefinition.Replace("\"", "\\\"").Replace("'", "\\'");
//}
}
public static string UnescapeQuotes(string xmlDefinition)
{
return xmlDefinition.Replace("\\\"", "\"").Replace("\\'", "'");
}
public static int GetNextUnescaped(char[] needles, string haystack)
{
return GetNextUnescaped(needles, haystack, 0);
}
public static int GetNextUnescaped(char[] needles, string haystack, int start)
{
return GetNextUnescaped(needles, haystack, start, haystack.Length - start);
}
public static int GetNextUnescaped(char[] needles, string haystack, int start, int count)
{
////Logger.debug("GetNextUnescaped():");
////Logger.IncLvl();
int end = start + count - 1;
int needlePos = haystack.IndexOfAny(needles, start, end - start + 1);
while (needlePos > 0 && haystack[needlePos - 1] == '\\')
{
needlePos = haystack.IndexOfAny(needles, needlePos + 1, end - needlePos);
}
////Logger.DecLvl();
return needlePos;
}
public static int GetNextOutsideQuotes(char needle, string haystack)
{
return GetNextOutsideQuotes(new char[] { needle }, haystack);
}
public static int GetNextOutsideQuotes(char needle, string haystack, bool ignoreEscapedQuotes)
{
return GetNextOutsideQuotes(new char[] { needle }, haystack, ignoreEscapedQuotes);
}
public static int GetNextOutsideQuotes(char[] needles, string haystack)
{
return GetNextOutsideQuotes(needles, haystack, true);
}
public static int GetNextOutsideQuotes(
char[] needles,
string haystack,
bool ignoreEscapedQuotes
)
{
//using (Logger logger = new Logger("GetNextOutsideQuotes():"))
//{
char[] quoteChars = new char[] { '\'', '"' };
int needlePos = -1;
int quoteEnd = -1;
int quoteStart;
while (needlePos == -1)
{
if (ignoreEscapedQuotes)
{
quoteStart = GetNextUnescaped(quoteChars, haystack, quoteEnd + 1);
}
else
{
quoteStart = haystack.IndexOfAny(quoteChars, quoteEnd + 1);
}
if (quoteStart == -1)
{
needlePos = GetNextUnescaped(needles, haystack, quoteEnd + 1);
}
else
{
needlePos = GetNextUnescaped(
needles,
haystack,
quoteEnd + 1,
quoteStart - quoteEnd - 1
);
if (needlePos != -1)
{
//Logger.log("found needle: " + haystack.Substring(needlePos), Logger.Mode.DEBUG);
}
if (ignoreEscapedQuotes)
{
quoteEnd = GetNextUnescaped(
new char[] { haystack[quoteStart] },
haystack,
quoteStart + 1
);
}
else
{
quoteEnd = haystack.IndexOf(haystack[quoteStart], quoteStart + 1);
}
}
}
return needlePos;
//}
}
public static List<String> ParamString2List(string arg)
{
//using (new Logger("Parser.ParamString2List(string)"))
//{
arg = arg.Trim() + " ";
List<string> argList = new List<string>();
char[] quoteChars = new char[] { '\'', '"' };
int spacePos = -1;
while (spacePos != arg.Length - 1)
{
arg = arg.Substring(spacePos + 1);
spacePos = Parser.GetNextOutsideQuotes(new char[] { ' ', '\n' }, arg);
argList.Add(arg.Substring(0, spacePos).Trim(quoteChars));
}
return argList;
//}
}
public static Dictionary<string, string> GetXMLAttributes(string attributeString)
{
//using (new Logger("Parser.GetXMLAttributes(string)"))
//{
Dictionary<string, string> attributes = new Dictionary<string, string>();
char[] quoteChars = new char[] { '\'', '"' };
List<string> attributeList = ParamString2List(attributeString);
int equalChar;
foreach (string attribute in attributeList)
{
equalChar = attribute.IndexOf('=');
if (equalChar == -1)
{
attributes[attribute.Substring(0).ToLower()] = "true";
}
else
{
attributes[attribute.Substring(0, equalChar).ToLower()] =
attribute.Substring(equalChar + 1).Trim(quoteChars);
}
}
return attributes;
//}
}
}

public static class TextUtils
{
public enum FONT { DEFAULT, MONOSPACE };
public static bool DEBUG = true;
private static FONT selectedFont = FONT.DEFAULT;
private static Dictionary<char, int> LetterWidths = new Dictionary<char, int>{
{ ' ', 8 }, { '!', 8 }, { '"', 10}, {'#', 19}, {'$', 20}, {'%', 24}, {'&', 20}, {'\'', 6}, {'(', 9}, {')', 9}, {'*', 11}, {'+', 18}, {',', 9}, {'-', 10}, {'.', 9}, {'/', 14}, {'0', 19}, {'1', 9}, {'2', 19}, {'3', 17}, {'4', 19}, {'5', 19}, {'6', 19}, {'7', 16}, {'8', 19}, {'9', 19}, {':', 9}, {';', 9}, {'<', 18}, {'=', 18}, {'>', 18}, {'?', 16}, {'@', 25}, {'A', 21}, {'B', 21}, {'C', 19}, {'D', 21}, {'E', 18}, {'F', 17}, {'G', 20}, {'H', 20}, {'I', 8}, {'J', 16}, {'K', 17}, {'L', 15}, {'M', 26}, {'N', 21}, {'O', 21}, {'P', 20}, {'Q', 21}, {'R', 21}, {'S', 21}, {'T', 17}, {'U', 20}, {'V', 20}, {'W', 31}, {'X', 19}, {'Y', 20}, {'Z', 19}, {'[', 9}, {'\\', 12}, {']', 9}, {'^', 18}, {'_', 15}, {'`', 8}, {'a', 17}, {'b', 17}, {'c', 16}, {'d', 17}, {'e', 17}, {'f', 9}, {'g', 17}, {'h', 17}, {'i', 8}, {'j', 8}, {'k', 17}, {'l', 8}, {'m', 27}, {'n', 17}, {'o', 17}, {'p', 17}, {'q', 17}, {'r', 10}, {'s', 17}, {'t', 9}, {'u', 17}, {'v', 15}, {'w', 27}, {'x', 15}, {'y', 17}, {'z', 16}, {'{', 9}, {'|', 6}, {'}', 9}, {'~', 18}, {' ', 8}, {'¡', 8}, {'¢', 16}, {'£', 17}, {'¤', 19}, {'¥', 19}, {'¦', 6}, {'§', 20}, {'¨', 8}, {'©', 25}, {'ª', 10}, {'«', 15}, {'¬', 18}, {'­', 10}, {'®', 25}, {'¯', 8}, {'°', 12}, {'±', 18}, {'²', 11}, {'³', 11}, {'´', 8}, {'µ', 17}, {'¶', 18}, {'·', 9}, {'¸', 8}, {'¹', 11}, {'º', 10}, {'»', 15}, {'¼', 27}, {'½', 29}, {'¾', 28}, {'¿', 16}, {'À', 21}, {'Á', 21}, {'Â', 21}, {'Ã', 21}, {'Ä', 21}, {'Å', 21}, {'Æ', 31}, {'Ç', 19}, {'È', 18}, {'É', 18}, {'Ê', 18}, {'Ë', 18}, {'Ì', 8}, {'Í', 8}, {'Î', 8}, {'Ï', 8}, {'Ð', 21}, {'Ñ', 21}, {'Ò', 21}, {'Ó', 21}, {'Ô', 21}, {'Õ', 21}, {'Ö', 21}, {'×', 18}, {'Ø', 21}, {'Ù', 20}, {'Ú', 20}, {'Û', 20}, {'Ü', 20}, {'Ý', 17}, {'Þ', 20}, {'ß', 19}, {'à', 17}, {'á', 17}, {'â', 17}, {'ã', 17}, {'ä', 17}, {'å', 17}, {'æ', 28}, {'ç', 16}, {'è', 17}, {'é', 17}, {'ê', 17}, {'ë', 17}, {'ì', 8}, {'í', 8}, {'î', 8}, {'ï', 8}, {'ð', 17}, {'ñ', 17}, {'ò', 17}, {'ó', 17}, {'ô', 17}, {'õ', 17}, {'ö', 17}, {'÷', 18}, {'ø', 17}, {'ù', 17}, {'ú', 17}, {'û', 17}, {'ü', 17}, {'ý', 17}, {'þ', 17}, {'ÿ', 17}, {'Ā', 20}, {'ā', 17}, {'Ă', 21}, {'ă', 17}, {'Ą', 21}, {'ą', 17}, {'Ć', 19}, {'ć', 16}, {'Ĉ', 19}, {'ĉ', 16}, {'Ċ', 19}, {'ċ', 16}, {'Č', 19}, {'č', 16}, {'Ď', 21}, {'ď', 17}, {'Đ', 21}, {'đ', 17}, {'Ē', 18}, {'ē', 17}, {'Ĕ', 18}, {'ĕ', 17}, {'Ė', 18}, {'ė', 17}, {'Ę', 18}, {'ę', 17}, {'Ě', 18}, {'ě', 17}, {'Ĝ', 20}, {'ĝ', 17}, {'Ğ', 20}, {'ğ', 17}, {'Ġ', 20}, {'ġ', 17}, {'Ģ', 20}, {'ģ', 17}, {'Ĥ', 20}, {'ĥ', 17}, {'Ħ', 20}, {'ħ', 17}, {'Ĩ', 8}, {'ĩ', 8}, {'Ī', 8}, {'ī', 8}, {'Į', 8}, {'į', 8}, {'İ', 8}, {'ı', 8}, {'Ĳ', 24}, {'ĳ', 14}, {'Ĵ', 16}, {'ĵ', 8}, {'Ķ', 17}, {'ķ', 17}, {'Ĺ', 15}, {'ĺ', 8}, {'Ļ', 15}, {'ļ', 8}, {'Ľ', 15}, {'ľ', 8}, {'Ŀ', 15}, {'ŀ', 10}, {'Ł', 15}, {'ł', 8}, {'Ń', 21}, {'ń', 17}, {'Ņ', 21}, {'ņ', 17}, {'Ň', 21}, {'ň', 17}, {'ŉ', 17}, {'Ō', 21}, {'ō', 17}, {'Ŏ', 21}, {'ŏ', 17}, {'Ő', 21}, {'ő', 17}, {'Œ', 31}, {'œ', 28}, {'Ŕ', 21}, {'ŕ', 10}, {'Ŗ', 21}, {'ŗ', 10}, {'Ř', 21}, {'ř', 10}, {'Ś', 21}, {'ś', 17}, {'Ŝ', 21}, {'ŝ', 17}, {'Ş', 21}, {'ş', 17}, {'Š', 21}, {'š', 17}, {'Ţ', 17}, {'ţ', 9}, {'Ť', 17}, {'ť', 9}, {'Ŧ', 17}, {'ŧ', 9}, {'Ũ', 20}, {'ũ', 17}, {'Ū', 20}, {'ū', 17}, {'Ŭ', 20}, {'ŭ', 17}, {'Ů', 20}, {'ů', 17}, {'Ű', 20}, {'ű', 17}, {'Ų', 20}, {'ų', 17}, {'Ŵ', 31}, {'ŵ', 27}, {'Ŷ', 17}, {'ŷ', 17}, {'Ÿ', 17}, {'Ź', 19}, {'ź', 16}, {'Ż', 19}, {'ż', 16}, {'Ž', 19}, {'ž', 16}, {'ƒ', 19}, {'Ș', 21}, {'ș', 17}, {'Ț', 17}, {'ț', 9}, {'ˆ', 8}, {'ˇ', 8}, {'ˉ', 6}, {'˘', 8}, {'˙', 8}, {'˚', 8}, {'˛', 8}, {'˜', 8}, {'˝', 8}, {'Ё', 19}, {'Ѓ', 16}, {'Є', 18}, {'Ѕ', 21}, {'І', 8}, {'Ї', 8}, {'Ј', 16}, {'Љ', 28}, {'Њ', 21}, {'Ќ', 19}, {'Ў', 17}, {'Џ', 18}, {'А', 19}, {'Б', 19}, {'В', 19}, {'Г', 15}, {'Д', 19}, {'Е', 18}, {'Ж', 21}, {'З', 17}, {'И', 19}, {'Й', 19}, {'К', 17}, {'Л', 17}, {'М', 26}, {'Н', 18}, {'О', 20}, {'П', 19}, {'Р', 19}, {'С', 19}, {'Т', 19}, {'У', 19}, {'Ф', 20}, {'Х', 19}, {'Ц', 20}, {'Ч', 16}, {'Ш', 26}, {'Щ', 29}, {'Ъ', 20}, {'Ы', 24}, {'Ь', 19}, {'Э', 18}, {'Ю', 27}, {'Я', 20}, {'а', 16}, {'б', 17}, {'в', 16}, {'г', 15}, {'д', 17}, {'е', 17}, {'ж', 20}, {'з', 15}, {'и', 16}, {'й', 16}, {'к', 17}, {'л', 15}, {'м', 25}, {'н', 16}, {'о', 16}, {'п', 16}, {'р', 17}, {'с', 16}, {'т', 14}, {'у', 17}, {'ф', 21}, {'х', 15}, {'ц', 17}, {'ч', 15}, {'ш', 25}, {'щ', 27}, {'ъ', 16}, {'ы', 20}, {'ь', 16}, {'э', 14}, {'ю', 23}, {'я', 17}, {'ё', 17}, {'ђ', 17}, {'ѓ', 16}, {'є', 14}, {'ѕ', 16}, {'і', 8}, {'ї', 8}, {'ј', 7}, {'љ', 22}, {'њ', 25}, {'ћ', 17}, {'ќ', 16}, {'ў', 17}, {'џ', 17}, {'Ґ', 15}, {'ґ', 13}, {'–', 15}, {'—', 31}, {'‘', 6}, {'’', 6}, {'‚', 6}, {'“', 12}, {'”', 12}, {'„', 12}, {'†', 20}, {'‡', 20}, {'•', 15}, {'…', 31}, {'‰', 31}, {'‹', 8}, {'›', 8}, {'€', 19}, {'™', 30}, {'−', 18}, {'∙', 8}, {'□', 21}, {'', 40}, {'', 40}, {'', 40}, {'', 40}, {'', 41}, {'', 41}, {'', 32}, {'', 32}, {'', 40}, {'', 40}, {'', 34}, {'', 34}, {'', 40}, {'', 40}, {'', 40}, {'', 41}, {'', 32}, {'', 41}, {'', 32}, {'', 40}, {'', 40}, {'', 40}, {'', 40}, {'', 40}, {'', 40}, {'', 40}, {'', 40}
};
public enum PadMode { LEFT, RIGHT, BOTH };
public enum RoundMode { FLOOR, CEIL };
public static void SelectFont(FONT f)
{
selectedFont = f;
}
public static void Reset()
{
selectedFont = FONT.DEFAULT;
}
public static int GetCharWidth(char c)
{
if(selectedFont == FONT.MONOSPACE)
{
return 24;
}

int v = 0;
if (LetterWidths.ContainsKey(c)) return LetterWidths[c];
return 8;
}
public static int GetTextWidth(string text)
{
return GetTextWidth(text, 0, text.Length);
}
public static int GetTextWidth(string text, int start, int length)
{
//using (new SimpleProfiler("TextUtils.GetTextWidth(StringBuilder, int, int)"))
//{
string[] lines = text.Substring(start, length).Split('\n');
if (start + length > text.Length)
{
throw new Exception("ERROR: stringbuilder slice exceeds the stringbuilders length!");
}
text = text.Replace("\r", "");
int width = 0;
int lineWidth = 0;
foreach(string line in lines)
{
if (selectedFont == FONT.MONOSPACE)
{
lineWidth = (line.Length * 25);
}
else
{
lineWidth = line.Select(c => LetterWidths.GetValueOrDefault(c, 6)).Sum() + line.Length;
}
width = Math.Max(lineWidth - 1, width);
lineWidth = 0;
}
return Math.Max(width, lineWidth - 1);
//}
}
/*public static string RemoveLastTrailingNewline(string text)
{
//Logger.debug("TextUtils.RemoveLastTrailingNewline");
//Logger.IncLvl();
//Logger.DecLvl();
return (text.Length > 1 && text[text.Length - 1] == '\n') ? text.Remove(text.Length - 1) : text;
}
public static string RemoveFirstTrailingNewline(string text)
{
//Logger.debug("TextUtils.RemoveFirstTrailingNewline");
//Logger.IncLvl();
//Logger.DecLvl();
return (text.Length > 1 && text[0] == '\n') ? text.Remove(0) : text;
}*/
public static StringBuilder CreateStringOfLength(char constituent, int length)
{
return CreateStringOfLength(constituent, length, RoundMode.FLOOR);
}
public static StringBuilder CreateStringOfLength(char constituent, int length, RoundMode mode)
{
//using (new SimpleProfiler("TextUtils.CreateStringOfLength(string, int, RoundMode)"))
//{
int lengthOfConstituent = GetCharWidth(constituent);
if (mode == RoundMode.CEIL)
{
//Logger.log("Ceil mode", Logger.Mode.DEBUG);
length += lengthOfConstituent;
}
StringBuilder result = new StringBuilder();
if (length < lengthOfConstituent)
{
return new StringBuilder();
}
int numberOfChars = (length + 1) / (lengthOfConstituent + 1);
return new StringBuilder(new string(constituent, numberOfChars));
//}
}
public static StringBuilder PadText(string text, int totalWidth, PadMode mode)
{
return PadText(text, totalWidth, mode, ' ');
}
public static StringBuilder PadText(string text, int totalWidth, PadMode mode, char padChar)
{
//using (new SimpleProfiler("TextUtils.PadText(StringBuilder, int, PadMode, string)"))
//{
string[] lines = text.Split('\n');
StringBuilder result = new StringBuilder();
StringBuilder padding = new StringBuilder();
int width;
int lineStart;
int lineEnd = -1;
foreach (string line in lines)
{
width = GetTextWidth(line) + 1;
if (mode == PadMode.BOTH)
{
padding = CreateStringOfLength(padChar, (totalWidth - width) / 2);
result.Append(padding);
result.Append(line);
result.Append(padding);
}
else
{
padding = CreateStringOfLength(padChar, totalWidth - width);
if (mode == PadMode.LEFT)
{
result.Append(padding);
result.Append(line);
}
else
{
result.Append(line);
result.Append(padding);
}
}
result.Append("\n");
}
if (result.Length > 0)
result.Remove(result.Length - 1, 1);
return result;
}
//}
}

public static class XML
{
public static Dictionary<string, Func<XMLTree>> NodeRegister = new Dictionary<string, Func<XMLTree>> {
{"root", () => { return new RootNode(); } },
{"menu", () => { return new Menu(); } },
{"menuitem", () => { return new MenuItem(); } },
{"progressbar", () => { return new ProgressBar(); } },
{"hl", () => { return new HorizontalLine(); } },
{"vl", () => { return new VerticalLine(); } },
{"uicontrols", () => { return new UIControls(); } },
{"textinput", () => { return new TextInput(); } },
{"submitbutton", () => { return new SubmitButton(); } },
{"br", () => { return new Break(); } },
{"space", () => { return new Space(); } },
{"hidden", () => { return new Hidden(); } },
{"hiddendata", () => { return new Hidden(); } },
{"meta", () => { return new MetaNode(); } }
};
public static XMLTree CreateNode(string type)
{
//Logger.debug("XML.CreateNode()");
//Logger.IncLvl();
type = type.ToLower();
if (NodeRegister.ContainsKey(type))
{
//Logger.DecLvl();
return NodeRegister[type]();
}
else
{
//Logger.DecLvl();
return new Generic(type);
}
}
public static XMLTree ParseXML(string xmlString)
{
char[] spaceChars = { ' ', '\n' };
//Logger.debug("ParseXML");
//Logger.IncLvl();
RootNode root = new RootNode();
XMLTree currentNode = root;
string type;
////Logger.debug("Enter while loop");
while (xmlString.Length > 0)
{
if (xmlString[0] == '<')
{
////Logger.debug("found tag");
if (xmlString[1] == '/')
{
////Logger.debug("tag is end tag");
int spacePos = xmlString.IndexOfAny(spaceChars);
int bracketPos = xmlString.IndexOf('>');
int typeLength = (spacePos == -1 ? bracketPos : Math.Min(spacePos, bracketPos)) - 2;
type = xmlString.Substring(2, typeLength).ToLower();
if (type != currentNode.Type)
{
throw new Exception("Invalid end tag ('" + type + "(!= " + currentNode.Type + "') found (node has been ended but not started)!");
}
currentNode = currentNode.GetParent() as XMLTree;
xmlString = xmlString.Substring(bracketPos + 1);
}
else
{
////Logger.debug("tag is start tag");
int spacePos = xmlString.IndexOfAny(spaceChars);
int bracketPos = Parser.GetNextOutsideQuotes('>', xmlString);
int typeLength = (spacePos == -1 ? bracketPos : Math.Min(spacePos, bracketPos)) - 1;
type = xmlString.Substring(1, typeLength).ToLower().TrimEnd(new char[] { '/' });
XMLTree newNode = XML.CreateNode(type);
if (newNode == null)
{
int closingBracketPos = xmlString.IndexOf("<");
int textLength = closingBracketPos == -1 ? xmlString.Length : closingBracketPos;
newNode = new XML.TextNode(xmlString.Substring(0, textLength).Trim());
}
////Logger.debug("add new node of type '" + newNode.Type + "=" + type + "' to current node");
currentNode.AddChild(newNode);
////Logger.debug("added new node to current node");
if (spacePos != -1 && spacePos < bracketPos)
{
string attrString = xmlString.Substring(typeLength + 2, bracketPos - typeLength - 2);
attrString = attrString.TrimEnd(new char[] { '/' });
////Logger.debug("get xml attributes. attribute string: '" + attrString + "/" + xmlString + "'");
Dictionary<string, string> attributes =
Parser.GetXMLAttributes(attrString);
////Logger.debug("got xml attributes");
foreach (string key in attributes.Keys)
{
newNode.SetAttribute(key, attributes[key]);
}
}
if (newNode.Type == "textnode" || bracketPos == -1 || xmlString[bracketPos - 1] != '/')
{
currentNode = newNode;
}
xmlString = xmlString.Substring(bracketPos + 1);
}
}
else
{
int bracketPos = xmlString.IndexOf("<");
int textLength = bracketPos == -1 ? xmlString.Length : bracketPos;
XMLTree newNode = new XML.TextNode(xmlString.Substring(0, textLength).Trim());
if (true || newNode.GetRenderBox(-1, -1) != null)
{
currentNode.AddChild(newNode);
}
xmlString = bracketPos == -1 ? "" : xmlString.Substring(bracketPos);
}
}

return root;
}

public class RootNode : XMLTree
{
public RootNode() : base()
{
Type = "root";
PreventDefault("UP");
PreventDefault("DOWN");
}
public override string GetAttribute(string key)
{
XMLTree meta = GetNode((node) => { return node.Type == "meta"; });
string value;
if (meta != null)
{
value = meta.GetAttribute(key);
}
else
{
value = base.GetAttribute(key);
}
switch (key)
{
case "width":
if (value == null)
{
value = "100%";
}
break;
}
return value;
}
public override void SetAttribute(string key, string value)
{
XMLTree meta = GetNode((node) => { return node.Type == "meta"; });
if (meta != null)
{
meta.SetAttribute(key, value);
}
else
{
base.SetAttribute(key, value);
}
}
public override void UpdateSelectability(XMLTree child)
{
base.UpdateSelectability(child);
if (IsSelectable() && !IsSelected())
{
SelectFirst();
}
}
public override bool SelectNext()
{
if (IsSelectable() && !base.SelectNext())
{
return SelectNext();
}
return true;
}
public override bool SelectPrevious()
{
if (!base.SelectPrevious())
{
return SelectPrevious();
}
return true;
}
public override void OnKeyPressed(string keyCode)
{
switch (keyCode)
{
case "UP":
SelectPrevious();
break;
case "DOWN":
SelectNext();
break;
}
}
}

public abstract class XMLTree : XMLParentNode
{
public string Type;
private XMLParentNode Parent;
private List<string> PreventDefaults;
protected List<XMLTree> Children;
protected bool Selectable;
protected bool ChildrenAreSelectable;
private bool Selected;
protected int SelectedChild;
protected bool Activated;
protected Dictionary<string, string> Attributes;
private bool _hasUserInputBindings;
private RenderBox _renderCache;
public bool HasUserInputBindings
{
get { return _hasUserInputBindings; }
set
{
_hasUserInputBindings = value;
if(Parent != null && HasUserInputBindings)
{
Parent.HasUserInputBindings = true;
}
}
}
public int NumberOfChildren
{
get
{
return Children.Count;
}
}
protected bool RerenderRequired;
public virtual RenderBox GetRenderBox(int maxWidth, int maxHeight)
{
//using (Logger logger = new Logger("XMLTree<" + Type + ">.GetRenderBox(int, int)", Logger.Mode.LOG))
//{
//Logger.debug("XMLTree.GetRenderCache(int)");
//Logger.IncLvl();
/*if(_renderCache != null)
{
return _renderCache;
}*/
RenderBoxTree cache = new RenderBoxTree();
cache.type = Type;
//Console.WriteLine(Type);
//logger.log("1", Logger.Mode.LOG);
RenderBox childCache;
foreach (XMLTree child in Children)
{
//TODO: Problems with relative height/width values
childCache = child.GetRenderBox(maxWidth, maxHeight);
//logger.log("-", Logger.Mode.LOG);
cache.Add(childCache);
}
//logger.log("2", Logger.Mode.LOG);
UpdateRenderCacheProperties(cache, maxWidth, maxHeight);
//_renderCache = cache;
return cache;
//}
}
protected void UpdateRenderCacheProperties(RenderBox cache, int maxWidth, int maxHeight)
{
//using (Logger logger = new Logger("XMLTree<" + Type + ">.UpdateRenderCacheProperties(NodeBox, int)", Logger.Mode.LOG))
//{
//Logger.IncLvl();
cache.Flow = GetAttribute("flow") == "horizontal" ? RenderBox.FlowDirection.HORIZONTAL : RenderBox.FlowDirection.VERTICAL;
switch (GetAttribute("alignself"))
{
case "right":
cache.Align = RenderBox.TextAlign.RIGHT;
break;
case "center":
cache.Align = RenderBox.TextAlign.CENTER;
break;
default:
cache.Align = RenderBox.TextAlign.LEFT;
break;
}
cache.MinWidth = Math.Max(0, ResolveSize(GetAttribute("minwidth"), maxWidth));
cache.MaxWidth = ResolveSize(GetAttribute("maxwidth"), maxWidth);
cache.DesiredWidth = ResolveSize(GetAttribute("width"), maxWidth);
int forcedWidth = ResolveSize(GetAttribute("forcewidth"), maxWidth);
if (forcedWidth != -1)
{
cache.MinWidth = forcedWidth;
cache.MaxWidth = forcedWidth;
}
cache.MinHeight = Math.Max(0, ResolveSize(GetAttribute("minheight"), maxHeight));
cache.MaxHeight = ResolveSize(GetAttribute("maxheight"), maxHeight);
cache.DesiredHeight = ResolveSize(GetAttribute("height"), maxHeight);
int forcedHeight = ResolveSize(GetAttribute("forceheight"), maxWidth);
if (forcedHeight != -1)
{
//logger.log("Apply forced height (" + forcedHeight + ")", Logger.Mode.LOG);
cache.MinHeight = forcedHeight;
cache.MaxHeight = forcedHeight;
}
//cache.Height = CalculateWidth(GetAttribute("height"), -1);
//}
}
public static int ResolveSize(string widthString, int maxWidth)
{
//using (Logger logger = new Logger("XMLTree.ResolvePercentage(string, int)", Logger.Mode.LOG))
//{
if(widthString != null)
widthString = widthString?.Trim();
float fWidth;
if (widthString != null && widthString[widthString.Length - 1] == '%' && Single.TryParse(widthString.Substring(0, widthString.Length - 1), out fWidth))
{
if (maxWidth == -1)
return -1;
return (int)(fWidth / 100f * Math.Max(0, maxWidth));
}
else
{
int iWidth = -1;
if (Int32.TryParse(widthString, out iWidth))
return iWidth;
return -1;
}
//}
}
public XMLTree()
{
//using (Logger logger = new Logger("XMLTree constructor", Logger.Mode.LOG))
//{
HasUserInputBindings = false;
PreventDefaults = new List<string>();
Parent = null;
Children = new List<XMLTree>();
Selectable = false;
ChildrenAreSelectable = false;
Selected = false;
SelectedChild = -1;
Activated = false;
Attributes = new Dictionary<string, string>();
RerenderRequired = true;
Type = "NULL";
// set attribute defaults
SetAttribute("alignself", "left");
SetAttribute("aligntext", "left");
SetAttribute("selected", "false");
SetAttribute("selectable", "false");
SetAttribute("flow", "vertical");
//}
}
public bool IsSelectable()
{
//Logger.debug(Type + ": IsSelectable():");
//Logger.IncLvl();
//Logger.DecLvl();
return Selectable || ChildrenAreSelectable;
}
public bool IsSelected()
{
//Logger.debug(Type + ": IsSelected():");
//Logger.IncLvl();
//Logger.DecLvl();
return Selected;
}
public XMLTree GetSelectedSibling()
{
//Logger.debug(Type + ": GetSelectedSibling():");
//Logger.IncLvl();
if (!Selected)
{
//Logger.DecLvl();
return null;
//throw new Exception(
//    "Node is not selected. You can only get the selected Node from one of it's parent nodes!");
}
if (SelectedChild == -1)
{
//Logger.DecLvl();
return this;
}
else
{
//Logger.DecLvl();
return Children[SelectedChild].GetSelectedSibling();
}
}
public virtual void AddChild(XMLTree child)
{
//Logger.debug(Type + ": AddChild():");
//Logger.IncLvl();
AddChildAt(Children.Count, child);
//Logger.DecLvl();
}
public virtual void AddChildAt(int position, XMLTree child)
{
//Logger.debug(Type + ":AddChildAt()");
//Logger.IncLvl();
if( position > Children.Count )
{
throw new Exception("XMLTree.AddChildAt - Exception: position must be less than number of children!");
}
RerenderRequired = true;
Children.Insert(position, child);
child.SetParent(this as XMLParentNode);
UpdateSelectability(child);
//Logger.DecLvl();
}
public void SetParent(XMLParentNode parent)
{
//Logger.debug(Type + ": SetParent():");
//Logger.IncLvl();
Parent = parent;
if(HasUserInputBindings && Parent != null)
{
Parent.HasUserInputBindings = true;
}
//Logger.DecLvl();
}
public XMLParentNode GetParent()
{
//Logger.debug(Type + ": GetParent():");
//Logger.IncLvl();
//Logger.DecLvl();
return Parent;
}
public XMLTree GetChild(int i)
{
//Logger.debug(Type + ": GetChild():");
//Logger.IncLvl();
//Logger.DecLvl();
return i < Children.Count ? Children[i] : null;
}
public XMLTree GetNode(Func<XMLTree, bool> filter)
{
if (filter(this))
{
return this;
}
else
{
XMLTree child = GetChild(0);
XMLTree childResult;
for (int i = 1; child != null; i++)
{
childResult = child.GetNode(filter);
if (childResult != null)
{
return childResult;
}
child = GetChild(i);
}
}
return null;
}
public List<XMLTree> GetAllNodes(Func<XMLTree, bool> filter)
{
List<XMLTree> nodeList = new List<XMLTree>();
GetAllNodes(filter, ref nodeList);
return nodeList;
}
private void GetAllNodes(Func<XMLTree, bool> filter, ref List<XMLTree> nodeList)
{
if (filter(this))
{
nodeList.Add(this);
}
XMLTree child = GetChild(0);
for (int i = 1; child != null; i++)
{
child.GetAllNodes(filter, ref nodeList);
child = GetChild(i);
}
}
public virtual void UpdateSelectability(XMLTree child)
{
//Logger.debug(Type + ": UpdateSelectability():");
//Logger.IncLvl();
bool ChildrenWereSelectable = ChildrenAreSelectable;
ChildrenAreSelectable = ChildrenAreSelectable || child.IsSelectable();
if ((Selectable || ChildrenAreSelectable) != (Selectable || ChildrenWereSelectable))
{
RerenderRequired = true;
//Logger.debug("update parent selectability");
if(Parent != null)
Parent.UpdateSelectability(this);
//Logger.debug("parent selectability updated");
}
//Logger.DecLvl();
}
public bool SelectFirst()
{
//Logger.debug(Type + ": SelectFirst():");
//Logger.IncLvl();
if (SelectedChild != -1)
{
Children[SelectedChild].Unselect();
}
SelectedChild = -1;
bool success = (Selectable || ChildrenAreSelectable) ? SelectNext() : false;
//Logger.DecLvl();
return success;
}
public bool SelectLast()
{
//Logger.debug(Type + ": SelectLast():");
//Logger.IncLvl();
if (SelectedChild != -1)
{
Children[SelectedChild].Unselect();
}
SelectedChild = -1;
//Logger.DecLvl();
return (Selectable || ChildrenAreSelectable) ? SelectPrevious() : false;
}
public void Unselect()
{
//Logger.debug(Type + ": Unselect():");
//Logger.IncLvl();
if (SelectedChild != -1)
{
Children[SelectedChild].Unselect();
}
Selected = false;
Activated = false;
//Logger.DecLvl();
}
public virtual bool SelectNext()
{
//Logger.debug(Type + ": SelectNext():");
//Logger.IncLvl();
bool WasSelected = IsSelected();
if (SelectedChild == -1 || !Children[SelectedChild].SelectNext())
{
//Logger.debug(Type + ": find next child to select...");
SelectedChild++;
while ((SelectedChild < Children.Count && (!Children[SelectedChild].SelectFirst())))
{
SelectedChild++;
}
if (SelectedChild == Children.Count)
{
SelectedChild = -1;
Selected = Selectable && !Selected;
}
else
{
Selected = true;
}
}
if (!Selected)
{
Unselect();
}
if (!WasSelected && IsSelected())
{
OnSelect();
RerenderRequired = true;
}
//Logger.DecLvl();
return Selected;
}
public virtual bool SelectPrevious()
{
//Logger.debug(Type + ": SelectPrevious():");
//Logger.IncLvl();
bool WasSelected = IsSelected();
if (SelectedChild == -1) { SelectedChild = Children.Count; }
if (SelectedChild == Children.Count || !Children[SelectedChild].SelectPrevious())
{
SelectedChild--;
while (SelectedChild > -1 && !Children[SelectedChild].SelectLast())
{
SelectedChild--;
}
if (SelectedChild == -1)
{
Selected = Selectable && !Selected;
}
else
{
Selected = true;
}
}
if (!Selected)
{
Unselect();
}
if (!WasSelected && IsSelected())
{
OnSelect();
RerenderRequired = true;
}
//Logger.DecLvl();
return Selected;
}
public virtual void OnSelect() { }
public virtual string GetAttribute(string key)
{
//Logger.debug(Type + ": GetAttribute(" + key + "):");
//Logger.IncLvl();
if (Attributes.ContainsKey(key))
{
//Logger.DecLvl();
return Attributes[key];
}
else if( key == "flowdirection" && Attributes.ContainsKey("flow"))
{
return Attributes["flow"];
}
//Logger.DecLvl();
return null;
}
public virtual void SetAttribute(string key, string value)
{
//Logger.debug(Type + ": SetAttribute():");
//Logger.IncLvl();
if (key == "selectable")
{
bool shouldBeSelectable = value == "true";
if (Selectable != shouldBeSelectable)
{
Selectable = shouldBeSelectable;
if (Parent != null)
{
Parent.UpdateSelectability(this);
}
}
}
else if (key == "activated")
{
bool shouldBeActivated = value == "true";
Activated = shouldBeActivated;
}
else if(key == "inputbinding")
{
HasUserInputBindings = true;
if(Parent != null)
{
Parent.HasUserInputBindings = true;
}
}
else if (key == "flow")
{
if(value == "horizontal")
{
//RenderCach.Flow = NodeBox.FlowDirection.HORIZONTAL;
}
else
{
//base.Flow = NodeBox.FlowDirection.VERTICAL;
}
RerenderRequired = true;
}
else if (key == "align")
{
switch(value)
{
case "right":
//base.Align = NodeBox.TextAlign.RIGHT;
break;
case "center":
//base.Align = NodeBox.TextAlign.CENTER;
break;
default:
//base.Align = NodeBox.TextAlign.LEFT;
break;
}
RerenderRequired = true;
}
else if (key == "width")
{
int width;
if(Int32.TryParse(value, out width))
{
//base.DesiredWidth = width;
}
}
Attributes[key] = value;
//Logger.DecLvl();
}
public XMLParentNode RetrieveRoot()
{
XMLParentNode ancestor = this;
while (ancestor.GetParent() != null)
{
ancestor = ancestor.GetParent();
}
return ancestor;
}
public void KeyPress(string keyCode)
{
//Logger.debug(Type + ": _KeyPress():");
//Logger.IncLvl();
//Logger.debug("button: " + keyCode);
OnKeyPressed(keyCode);
if (Parent != null && !PreventDefaults.Contains(keyCode))
{
Parent.KeyPress(keyCode);
}
//Logger.DecLvl();
}
public virtual void OnKeyPressed(string keyCode)
{
//Logger.debug(Type + ": OnKeyPressed()");
//Logger.IncLvl();
switch (keyCode)
{
case "ACTIVATE":
ToggleActivation();
break;
default:
break;
}
//Logger.DecLvl();
}
public virtual void ToggleActivation()
{
//Logger.debug(Type + ": ToggleActivation()");
//Logger.IncLvl();
Activated = !Activated;
//Logger.DecLvl();
}
public void PreventDefault(string keyCode)
{
//Logger.debug(Type + ": PreventDefault()");
//Logger.IncLvl();
if (!PreventDefaults.Contains(keyCode))
{
PreventDefaults.Add(keyCode);
}
//Logger.DecLvl();
}
public void AllowDefault(string keyCode)
{
//Logger.debug(Type + ": AllowDefault()");
//Logger.IncLvl();
if (PreventDefaults.Contains(keyCode))
{
PreventDefaults.Remove(keyCode);
}
//Logger.DecLvl();
}
public void FollowRoute(Route route)
{
//Logger.debug(Type + ": FollowRoute");
//Logger.IncLvl();
if (Parent != null)
{
Parent.FollowRoute(route);
}
//Logger.DecLvl();
}
public virtual Dictionary<string, string> GetValues(Func<XMLTree, bool> filter)
{
//Logger.log(Type + ": GetValues()");
//Logger.IncLvl();
Dictionary<string, string> dict = new Dictionary<string, string>();
string name = GetAttribute("name");
string value = GetAttribute("value");
if (name != null && value != null)
{
//Logger.log($"Added entry {{{name}: {value}}}");
dict[name] = value;
}
Dictionary<string, string> childDict;
foreach (XMLTree child in Children)
{
childDict = child.GetValues(filter);
foreach (string key in childDict.Keys)
{
if (!dict.ContainsKey(key))
{
dict[key] = childDict[key];
}
}
}
//Logger.DecLvl();
return dict;
}
/*public int GetWidth(int maxWidth)
{
//Logger.debug(Type + ".GetWidth()");
//Logger.IncLvl();
string attributeWidthValue = GetAttribute("width");
if (attributeWidthValue == null)
{
//Logger.DecLvl();
return 0;
//return maxWidth;
}
else
{
if (attributeWidthValue[attributeWidthValue.Length - 1] == '%')
{
//Logger.debug("is procent value (" + Single.Parse(attributeWidthValue.Substring(0, attributeWidthValue.Length - 1)).ToString() + ")");
//Logger.DecLvl();
return (int)(Single.Parse(attributeWidthValue.Substring(0, attributeWidthValue.Length - 1)) / 100f * maxWidth);
}
else if (maxWidth == 0)
{
//Logger.DecLvl();
return Int32.Parse(attributeWidthValue);
}
else
{
//Logger.DecLvl();
return Math.Min(maxWidth, Int32.Parse(attributeWidthValue));
}
}
}
public string RenderOld(int availableWidth)
{
//Logger.debug(Type + ".Render()");
//Logger.IncLvl();
List<string> segments = new List<string>();
int width = GetWidth(availableWidth);
PreRender(ref segments, width, availableWidth);
RenderText(ref segments, width, availableWidth);
string renderString = PostRender(segments, width, availableWidth);
//Logger.DecLvl();
return renderString;
}
public NodeBox Render(int availableWidth)
{
return Render(availableWidth, 0);
}
public NodeBox Render(int availableWidth, int availableHeight)
{
//Logger.debug(Type + ".Render()");
//Logger.IncLvl();
//Logger.DecLvl();
return Cache;
}
protected virtual void PreRender(ref List<string> segments, int width, int availableWidth)
{
//Logger.debug(Type + ".PreRender()");
//Logger.IncLvl();
//Logger.DecLvl();
}
protected virtual void RenderText(ref List<string> segments, int width, int availableWidth)
{
//Logger.debug(Type + ".RenderText()");
//Logger.IncLvl();
for (int i = 0; i < Children.Count; i++)
{
if (GetAttribute("flow") == "vertical")
{
string childString = RenderChild(Children[i], width);
if (childString != null)
{
if (i > 0 && Children[i - 1].Type == "textnode" && (Children[i].Type == "textnode" || Children[i].Type == "br"))
{
segments[segments.Count - 1] += childString;
}
else
{
segments.Add(childString);
}
}
else
{
}
}
else
{
string childString = RenderChild(Children[i], width);
if (childString != null)
{
availableWidth -= TextUtils.GetTextWidth(childString);
segments.Add(childString);
}
}
}
//Logger.DecLvl();
}
protected virtual string PostRender(List<string> segments, int width, int availableWidth)
{
//Logger.debug(Type + ".PostRender()");
//Logger.IncLvl();
string renderString = "";
string flowdir = GetAttribute("flow");
string alignChildren = GetAttribute("alignchildren");
string alignSelf = GetAttribute("alignself");
int totalWidth = 0;
foreach (string segment in segments)
{
int lineWidth = TextUtils.GetTextWidth(segment);
if (lineWidth > totalWidth)
{
totalWidth = lineWidth;
}
}
totalWidth = Math.Min(availableWidth, Math.Max(width, totalWidth));
if (flowdir == "vertical")
{
for (int i = 0; i < segments.Count; i++)
{
switch (alignChildren)
{
case "right":
segments[i] = TextUtils.PadText(segments[i], totalWidth, TextUtils.PadMode.LEFT);
break;
case "center":
segments[i] = TextUtils.CenterText(segments[i], totalWidth);
break;
default:
segments[i] = TextUtils.PadText(segments[i], totalWidth, TextUtils.PadMode.RIGHT);
break;
}
}
renderString = String.Join("\n", segments.ToArray());
}
else
{
renderString = String.Join("", segments.ToArray());
}
if (availableWidth - totalWidth > 0)
{
if (alignSelf == "center")
{
//Logger.log("Center element...");
renderString = TextUtils.CenterText(renderString, availableWidth);
}
else if (alignSelf == "right")
{
//Logger.log("Aligning element right...");
renderString = TextUtils.PadText(renderString, availableWidth, TextUtils.PadMode.RIGHT);
}
}
//Logger.DecLvl();
return renderString;
}

protected virtual string RenderChild(XMLTree child, int availableWidth)
{
//Logger.log(Type + ".RenderChild()");
//Logger.IncLvl();
//Logger.DecLvl();
return child.Render(availableWidth);
}*/
public void DetachChild(XMLTree child)
{
Children.Remove(child);
RerenderRequired = true;
}
public void Detach()
{
if(GetParent() != null)
{
GetParent().DetachChild(this);
}
}
/*protected virtual void BuildRenderCache()
{
//Logger.debug(Type + ".BuildRenderCache()");
//Logger.IncLvl();
//base.Clear();
NodeBoxTree box = this;
foreach (XMLTree child in Children)
{
box.Add(child);
}
RerenderRequired = false;
//Logger.DecLvl();
}*/
public virtual string Render(int maxWidth, int maxHeight)
{
//using (Logger logger = new Logger("XMLTree<" + Type + ">.Render(int, int)", Logger.Mode.LOG))
//{
//Logger.debug(Type + ".Render(int)");
//Logger.IncLvl();
//Logger.log("RENDERING::PREPARE");
RenderBox cache = GetRenderBox(maxWidth, maxHeight);
//logger.log("Rendering::START", Logger.Mode.LOG);
//Logger.log("RENDERING::START");
string result = cache.Render(maxWidth, maxHeight);
//Logger.DecLvl();
return result;
//}
}
public string Render()
{
return Render(-1, -1);
}
}

public interface XMLParentNode
{
bool HasUserInputBindings { get; set; }
XMLParentNode GetParent();
void UpdateSelectability(XMLTree child);
void KeyPress(string keyCode);
void FollowRoute(Route route);
bool SelectNext();
void DetachChild(XMLTree child);
}

public class TextNode : XMLTree
{
public string Content;
public TextNode(string content) : base()
{
//Logger.debug("TextNode constructor()");
//Logger.IncLvl();
//Logger.debug("content: " + content);
Type = "textnode";
Content = content;
Content.Replace("\n", "");
Content = Content.Trim(new char[] { '\n', ' ', '\r' });
//Logger.debug("final content: " + Content);
RerenderRequired = true;
//Logger.DecLvl();
}
public override RenderBox GetRenderBox(int maxWidth, int maxHeight)
{
//using (new Logger("XMLTree<" + Type + ">.GetRenderBox(int, int)"))
//{
//Logger.debug("TextNode.GetRenderCache(int)");
//Logger.IncLvl();
RenderBox cache = new RenderBoxLeaf(Content);
cache.type = Type;
//Logger.DecLvl();
return cache;
//}
}
//protected override void RenderText(ref List<string> segments, int width, int availableWidth) { }
/*protected override string PostRender(List<string> segments, int width, int availableWidth)
{
return Content;
}*/
/*protected override void BuildRenderCache()
{
//Logger.debug(Type + ".BuildRenderCache()");
//Logger.IncLvl();
RerenderRequired = false;
//Logger.DecLvl();
}*/
}

public class Route
{
static public Dictionary<string, Action<string, UIController>> RouteHandlers = new Dictionary<string, Action<string, UIController>>
{
{
"revert", (def, controller) => { controller.RevertUI(); }
},
{
"xml", (def, controller) =>
{
XMLTree ui = XML.ParseXML(Parser.UnescapeQuotes(def));
controller.LoadUI(ui);
}
},
{
"fn", (def, controller) =>
{
if(UIFactories.ContainsKey(def))
{
UIFactories[def](controller);
}
}
}
};
static Dictionary<string, Action<UIController>> UIFactories =
new Dictionary<string, Action<UIController>>();
string Definition;
public Route(string definition)
{
//Logger.debug("Route constructor():");
//Logger.IncLvl();
Definition = definition;
//Logger.DecLvl();
}
public void Follow(UIController controller)
{
using (Logger logger = new Logger("Route.Follow(UIController)", Logger.Mode.LOG))
{
logger.log("route def: " + Definition, Logger.Mode.LOG);
//Logger.debug("Route.Follow()");
//Logger.IncLvl();
string[] DefTypeAndValue = Definition.Split(new char[] { ':' }, 2);
if (Route.RouteHandlers.ContainsKey(DefTypeAndValue[0].ToLower()))
{
Route.RouteHandlers[DefTypeAndValue[0].ToLower()](
DefTypeAndValue.Length >= 2 ? DefTypeAndValue[1] : null, controller
);
}
else
{
logger.log("route not understood.", Logger.Mode.WARNING);
}
}
//Logger.DecLvl();
}
static public void RegisterRouteFunction(string id, Action<UIController> fn)
{
UIFactories[id] = fn;
}
}

public class UIController : XMLParentNode
{
public XMLTree ui;
public Stack<XMLTree> UIStack;
public string Type;
bool UserInputActive;
IMyTerminalBlock UserInputSource;
TextInputMode UserInputMode;
List<XMLTree> UserInputBindings;
string InputDataCache;
public bool HasUserInputBindings
{
get { return UserInputActive && UserInputSource != null && UserInputBindings.Count > 0; }
set { }
}
public enum TextInputMode { PUBLIC_TEXT, CUSTOM_DATA }
public enum FONT
{
Debug, Red, Green, Blue, White, DarkBlue, UrlNormal, UrlHighlight, ErrorMessageBoxCaption, ErrorMessageBoxText,
InfoMessageBoxCaption, InfoMessageBoxText, ScreenCaption, GameCredits, LoadingScreen, BuildInfo, BuildInfoHighlight,
Monospace, MONO = Monospace, DEFAULT = Debug
}
Dictionary<FONT, long> Fonts = new Dictionary<FONT, long> {
{FONT.Debug, 151057691},
{FONT.Red, -795103743 },
{FONT.Green, -161094011 },
{FONT.Blue, 1920284339 },
{FONT.White, 48665683 },
{FONT.DarkBlue, 1919824171 },
{FONT.UrlNormal, 992097699 },
{FONT.UrlHighlight, -807552222 },
{FONT.ErrorMessageBoxCaption, 1458347610 },
{FONT.ErrorMessageBoxText, 895781166 },
{FONT.InfoMessageBoxCaption, 837834442 },
{FONT.InfoMessageBoxText, 1833612699 },
{FONT.ScreenCaption, 1216738022 },
{FONT.GameCredits, -1859174863 },
{FONT.LoadingScreen, 741958017 },
{FONT.BuildInfo, 1184185815 },
{FONT.BuildInfoHighlight, -270950170 },
{FONT.Monospace, 1147350002 }
};
public UIController(XMLTree rootNode)
{
//Logger.debug("UIController constructor()");
//Logger.IncLvl();
Type = "CTRL";
UIStack = new Stack<XMLTree>();
UserInputBindings = new List<XMLTree>();
UserInputActive = false;
InputDataCache = "";
ui = rootNode;
ui.SetParent(this);
if (GetSelectedFont() == Fonts[FONT.MONO])
TextUtils.SelectFont(TextUtils.FONT.MONOSPACE);
else
TextUtils.SelectFont(TextUtils.FONT.DEFAULT);
if (GetSelectedNode() == null && ui.IsSelectable())
{
ui.SelectFirst();
}
CollectUserInputBindings();
//Logger.DecLvl();
}
public static UIController FromXML(string xml)
{
//Logger.debug("UIController FromXMLString()");
//Logger.IncLvl();
XMLTree rootNode = XML.ParseXML(xml);
//Logger.DecLvl();
return new UIController(rootNode);
}
public void ApplyScreenProperties(IMyTextPanel screen)
{
//Logger.debug("UIController.ApplyScreenProperties()");
//Logger.IncLvl();
if (ui.GetAttribute("fontcolor") != null)
{
string colorString = ui.GetAttribute("fontcolor");
colorString = "FF" +
colorString.Substring(colorString.Length - 2, 2)
+ colorString.Substring(colorString.Length - 4, 2)
+ colorString.Substring(colorString.Length - 6, 2);
Color fontColor = new Color(
uint.Parse(colorString, System.Globalization.NumberStyles.AllowHexSpecifier)
);
screen.SetValue<Color>("FontColor", fontColor);
}
if (ui.GetAttribute("fontsize") != null)
{
screen.SetValue<Single>("FontSize", Single.Parse(ui.GetAttribute("fontsize")));
}
if (ui.GetAttribute("backgroundcolor") != null)
{
string colorString = ui.GetAttribute("backgroundcolor");
colorString = "FF" +
colorString.Substring(colorString.Length - 2, 2)
+ colorString.Substring(colorString.Length - 4, 2)
+ colorString.Substring(colorString.Length - 6, 2);
Color fontColor = new Color(
uint.Parse(colorString, System.Globalization.NumberStyles.AllowHexSpecifier));
screen.SetValue<Color>("BackgroundColor", fontColor);
}

screen.SetValue<long>("Font", GetSelectedFont());
//Logger.DecLvl();
}
public void Call(List<string> parameters)
{
//Logger.debug("UIController.Main()");
//Logger.IncLvl();
switch (parameters[0])
{
case "key":
XMLTree selectedNode = GetSelectedNode();
if (selectedNode != null)
{
selectedNode.KeyPress(parameters[1].ToUpper());
}
break;
case "refresh":
string refresh = ui.GetAttribute("refresh");
if (refresh != null)
{
FollowRoute(new Route(refresh));
}
UpdateUserInput();
break;
case "revert":
RevertUI();
break;
default:
break;
}
//Logger.DecLvl();
return;
}
public void LoadXML(string xmlString)
{
LoadUI(XML.ParseXML(xmlString));
}
public void LoadUI(XMLTree rootNode)
{
//Logger.debug("UIController: LoadUI():");
//Logger.IncLvl();
if (ui.GetAttribute("historydisabled") == null || ui.GetAttribute("historydisabled") != "true")
{
UIStack.Push(ui);
}
if (rootNode.GetAttribute("revert") != null && rootNode.GetAttribute("revert") == "true")
{
RevertUI();
}
else
{
ui = rootNode;
ui.SetParent(this);
}
UserInputBindings = new List<XMLTree>();
CollectUserInputBindings();
//Logger.DecLvl();
}
public void ClearUIStack()
{
UIStack = new Stack<XMLTree>();
}
public void RevertUI()
{
//Logger.log("UIController: RevertUI():");
//Logger.IncLvl();
if (UIStack.Count == 0)
{
//Logger.log("Error: Can't revert: UI stack is empty.");
//Logger.DecLvl();
return;
}
ui = UIStack.Pop();
ui.SetParent(this);
//Logger.DecLvl();
}
public string Render()
{
if (GetSelectedFont() == Fonts[FONT.MONO])
TextUtils.SelectFont(TextUtils.FONT.MONOSPACE);
else
TextUtils.SelectFont(TextUtils.FONT.DEFAULT);
return ui.Render(-1, -1);
}
public void RenderTo(IMyTextPanel panel)
{
//Logger.debug("UIController.RenderTo()");
//Logger.IncLvl();
int panelWidth = -1;
string panelType = panel.BlockDefinition.SubtypeId;
//Logger.debug("Type: " + panelType);
if (panelType == "LargeTextPanel" || panelType == "SmallTextPanel")
{
panelWidth = 658;
}
else if(panelType == "LargeLCDPanel" || panelType == "SmallLCDPanel")
{
panelWidth = 658;
}
else if(panelType == "SmallLCDPanelWide" || panelType == "LargeLCDPanelWide")
{
panelWidth = 1316;
}
else if(panelType == "LargeBlockCorner_LCD_1" || panelType == "LargeBlockCorner_LCD_2"
|| panelType == "SmallBlockCorner_LCD_1" || panelType == "SmallBlockCorner_LCD_2")
{ }
else if(panelType == "LargeBlockCorner_LCD_Flat_1" || panelType == "LargeBlockCorner_LCD_Flat_2"
|| panelType == "SmallBlockCorner_LCD_Flat_1" || panelType == "SmallBlockCorner_LCD_Flat_2")
{ }
int width = panelWidth == -1 ? -1 : (int)(((float)panelWidth) / panel.GetValue<Single>("FontSize"));
//TODO: Get height of screen
int height = 20;
ApplyScreenProperties(panel);
using (new Logger("RENDERING...", Logger.Mode.LOG))
{
string text = ui.Render(width, height);
panel.WritePublicText(text);
}
}
public long GetSelectedFont()
{
string font = ui.GetAttribute("fontfamily");
if (font != null)
{
FONT fontName;
long fontValue;
if (long.TryParse(font, out fontValue))
{
return fontValue;
}
else if (Enum.TryParse<FONT>(font, out fontName))
{
return Fonts.GetValueOrDefault(fontName, -1);
}
}
return -1;
}
public void KeyPress(string keyCode)
{
//Logger.debug("UIController: KeyPress():");
//Logger.IncLvl();
switch (keyCode)
{
case "LEFT/ABORT":
RevertUI();
break;
}
//Logger.DecLvl();
}
public XMLTree GetSelectedNode()
{
//Logger.debug("UIController: GetSelectedNode():");
//Logger.IncLvl();
XMLTree sibling = ui.GetSelectedSibling();
//Logger.DecLvl();
return sibling;
}
public XMLTree GetNode(Func<XMLTree, bool> filter)
{
//Logger.debug("UIController: GetNode()");
//Logger.IncLvl();
//Logger.DecLvl();
return ui.GetNode(filter);
}
public List<XMLTree> GetAllNodes(Func<XMLTree, bool> filter)
{
//Logger.debug("UIController: GetAllNodes()");
//Logger.IncLvl();
//Logger.DecLvl();
return ui.GetAllNodes(filter);
}
public void UpdateSelectability(XMLTree child) { }
public void FollowRoute(Route route)
{
//Logger.debug("UIController: FollowRoute():");
//Logger.IncLvl();
route.Follow(this);
//Logger.DecLvl();
}
public XMLParentNode GetParent()
{
return null;
}
public Dictionary<string, string> GetValues()
{
//Logger.debug("UIController.GetValues()");
//Logger.IncLvl();
return GetValues((node) => true);
}
public Dictionary<string, string> GetValues(Func<XMLTree, bool> filter)
{
//Logger.debug("UIController.GetValues()");
//Logger.IncLvl();
if (ui == null)
{
//Logger.DecLvl();
return null;
}
//Logger.DecLvl();
return ui.GetValues(filter);
}
public string GetPackedValues(Func<XMLTree, bool> filter)
{
return Parser.PackData(GetValues(filter)).ToString();
}
public void DetachChild(XMLTree xml)
{
if(xml == ui)
{
ui = null;
}
}
public string GetPackedValues()
{
//Logger.debug("UIController.GetPackedValues()");
//Logger.IncLvl();
//Logger.DecLvl();
return GetPackedValues(node => true);
}
public bool SelectNext()
{
return ui.SelectNext();
}
public void SetUserInputSource(IMyTerminalBlock sourceBlock, TextInputMode mode)
{
if(mode == TextInputMode.PUBLIC_TEXT && (sourceBlock as IMyTextPanel) == null)
{
throw new Exception("Only Text Panels can be used as user input if PUBLIC_TEXT mode is selected!");
}
UserInputSource = sourceBlock;
UserInputMode = mode;
}
public void EnableUserInput()
{
UserInputActive = true;
}
public void DisableUserInput()
{
UserInputActive = false;
}
public void RegisterInputBinding(XMLTree node)
{
UserInputBindings.Add(node);
}
public bool UpdateUserInput()
{
using (new Logger("UIController.RefreshUserInput()", Logger.Mode.LOG))
{
//Logger.IncLvl();
if (!UserInputActive || UserInputSource == null)
{
return false;
}
// get input data
string inputData = null;
switch (UserInputMode)
{
case TextInputMode.CUSTOM_DATA:
inputData = UserInputSource?.CustomData;
break;
case TextInputMode.PUBLIC_TEXT:
inputData = (UserInputSource as IMyTextPanel)?.GetPublicText();
break;
}
bool inputHasChanged = true;
if (inputData == null || inputData == InputDataCache)
{
inputHasChanged = false;
}
//Logger.debug("input has " + (inputHasChanged ? "" : "not ") + "changed");
//Logger.debug("Iterating input bindings (" + UserInputBindings.Count + " bindings registered).");
// update ui input bindings
string binding;
string fieldValue;
foreach (XMLTree node in UserInputBindings)
{
binding = node.GetAttribute("inputbinding");
if (binding != null)
{
//Logger.debug("binding found at " + node.Type + " node for field: " + binding);
fieldValue = node.GetAttribute(binding.ToLower());
//Logger.debug("field is " + (fieldValue ?? "EMPTY") + ".");
if (!inputHasChanged && fieldValue != null && fieldValue != InputDataCache)
{
//Logger.debug("applying field value: " + fieldValue);
inputData = fieldValue;
inputHasChanged = true;
}
else if (inputHasChanged)
{
//Logger.debug("Updating field value to input: " + inputData);
node.SetAttribute(binding.ToLower(), inputData);
}
}
}
if (inputHasChanged)
{
InputDataCache = inputData;
}
// update user input device
switch (UserInputMode)
{
case TextInputMode.CUSTOM_DATA:
if (UserInputSource != null)
{
UserInputSource.CustomData = InputDataCache;
}
break;
case TextInputMode.PUBLIC_TEXT:
(UserInputSource as IMyTextPanel)?.WritePublicText(InputDataCache);
break;
}
return inputHasChanged;
}
}
private void CollectUserInputBindings()
{
using (Logger logger = new Logger("UIController.CollectUserInputBindings()", Logger.Mode.LOG))
{
XMLTree node;
Queue<XMLParentNode> nodes = new Queue<XMLParentNode>();
nodes.Enqueue(ui);
while (nodes.Count != 0)
{
node = nodes.Dequeue() as XMLTree;
if (!node.HasUserInputBindings)
{
logger.log("node has no userinputbindings", Logger.Mode.LOG);
}
if (node != null && node.HasUserInputBindings)
{
logger.log("Checking " + node.Type + " node...", Logger.Mode.LOG);
for (int i = 0; i < node.NumberOfChildren; i++)
{
nodes.Enqueue(node.GetChild(i));
}
if (node.GetAttribute("inputbinding") != null)
{
RegisterInputBinding(node);
}
}
}
}
}
}

public abstract class UIFactory
{
private int Count;
private int Max;
private List<UIController> UIs;
public UIFactory() : this(null) { }
public UIFactory(List<UIController> uiList)
{
//Logger.debug("UIFactory constructor");
//Logger.IncLvl();
if (uiList == null)
{
UIs = new List<UIController>();
}
UIs = uiList;
//Logger.DecLvl();
}
public abstract XMLTree Render(UIController controller);
protected void UpdateUIs(XMLTree renderedUI)
{
foreach (UIController ui in UIs)
{
ui.LoadUI(renderedUI);
}
}
}

public class Generic : XMLTree
{
public Generic(string type) : base()
{
Type = type.ToLower();
}
}

public class Menu : XMLTree
{
public Menu() : base()
{
Type = "menu";
}
public override void AddChild(XMLTree child)
{
//using (new Logger("Menu.AddChile(XMLTree)"))
//{
if (child.Type != "menuitem" && child.IsSelectable())
{
throw new Exception(
"ERROR: Only children of type <menupoint> or children that are not selectable are allowed!"
+ " (type was: <" + child.Type + ">)");
}
base.AddChild(child);
//}
}
/*protected override string RenderChild(XMLTree child, int width)
{
string renderString = "";
string prefix = "     ";
if (child.Type == "menuitem")
{
renderString += (child.IsSelected() ? ">> " : prefix);
}
renderString += base.RenderChild(child, width);
return renderString;
}*/
public override Dictionary<string, string> GetValues(Func<XMLTree, bool> filter)
{
Dictionary<string, string> values = base.GetValues(filter);
string name = GetAttribute("name");
string value = GetChild(SelectedChild).GetAttribute("value");
if (filter(this) && IsSelected() && name != null && value != null)
{
values[name] = value;
}
return values;
}
public override RenderBox GetRenderBox(int maxWidth, int maxHeight)
{
using (new Logger("Menu.GetRenderBox(int, int)"))
{
RenderBoxLeaf prefix = new RenderBoxLeaf();
prefix.MinHeight = 1;
prefix.type = Type + "_prefix";
RenderBoxLeaf prefixSelected = new RenderBoxLeaf(">> ");
prefixSelected.type = Type + "_prefixSelected";
int prefixWidth = prefixSelected.MinWidth;
prefix.MaxWidth = prefixWidth;
prefix.MinWidth = prefixWidth;
prefixSelected.MaxWidth = prefixWidth;
RenderBoxTree cache = new RenderBoxTree();
UpdateRenderCacheProperties(cache, maxWidth, maxHeight);
cache.type = Type;
RenderBoxTree menuPoint;
foreach (XMLTree child in Children)
{
menuPoint = new RenderBoxTree();
menuPoint.type = Type + "_menupoint";
menuPoint.Flow = RenderBox.FlowDirection.HORIZONTAL;
if (child.IsSelected())
{
menuPoint.Add(prefixSelected);
}
else
{
menuPoint.Add(prefix);
}
RenderBox childBox = child.GetRenderBox(maxWidth, maxHeight);
menuPoint.Add(childBox);
cache.Add(menuPoint);
}
return cache;
}
}
}

public class MenuItem : XMLTree
{
Route TargetRoute;
public MenuItem() : this(null) { }
public MenuItem(Route targetRoute) : base()
{
Type = "menuitem";
Selectable = true;
SetRoute(targetRoute);
PreventDefault("RIGHT/SUBMIT");
}
public override void SetAttribute(string key, string value)
{
////Logger.debug(Type + ": SetAttribute():");
////Logger.IncLvl();
switch (key)
{
case "route":
////Logger.debug("prepare to set route...");
SetRoute(new Route(value));
if (TargetRoute == null)
{
////Logger.debug("Failure!");
}
else
{
////Logger.debug("Success!");
}
break;
default:
base.SetAttribute(key, value);
break;
}
////Logger.DecLvl();
}
public override void OnKeyPressed(string keyCode)
{
//Logger.debug(Type + ": OnKeyPressed():");
using (Logger logger = new Logger("MenuItem.OnKeyPressed(string)", Logger.Mode.LOG))
{
switch (keyCode)
{
case "RIGHT/SUBMIT":
if (TargetRoute != null)
{
//Logger.debug("Follow Target Route!");
FollowRoute(TargetRoute);
}
else
{
logger.log("target route is null!", Logger.Mode.WARNING);
//Logger.debug("No route set!");
}
break;
}
base.OnKeyPressed(keyCode);
//Logger.DecLvl();
}
}
public void SetRoute(Route route)
{
TargetRoute = route;
}

}

public class ProgressBar : XMLTree
{
RenderBox emptyBar;
RenderBox filledBar;
float StepSize
{
get
{
float stepSize;
if (!Single.TryParse(GetAttribute("stepsize"), out stepSize))
{
return 0.1f;
}
return stepSize;
}
set
{
string valueString = Math.Max(0.001f, Math.Min(0.009f, value)).ToString();
if (valueString.Length > 5)
{
valueString += valueString.Substring(0, 5);
}
SetAttribute("stepsize", valueString);
}
}
public float FillLevel
{
get
{
float fillLevel;
if (!Single.TryParse(GetAttribute("value"), out fillLevel))
{
return 0.0f;
}
if (fillLevel < 0 || fillLevel > 1)
return 0.0f;
return fillLevel;
}
set
{
string valueString = Math.Max(0f, Math.Min(1f, value)).ToString();
if (valueString.Length > 5)
{
valueString = valueString.Substring(0, 5);
}
SetAttribute("value", valueString);
}
}
public ProgressBar() : this(0f)
{
}
public ProgressBar(float fillLevel) : this(fillLevel, false)
{
}
public ProgressBar(float fillLevel, bool selectable) : base()
{
Type = "progressbar";
PreventDefault("LEFT/ABORT");
PreventDefault("RIGHT/SUBMIT");
SetAttribute("width", "500");
SetAttribute("filledstring", "|");
SetAttribute("emptystring", "'");
SetAttribute("value", fillLevel.ToString());
SetAttribute("stepsize", "0.05");
SetAttribute("selectable", selectable ? "true" : "false");
}
public void IncreaseFillLevel()
{
//Logger.debug(Type + ".IncreaseFillLevel()");
//Logger.IncLvl();
FillLevel += StepSize;
//Logger.DecLvl();
}
public void DecreaseFillLevel()
{
//Logger.debug(Type + ".DecreaseFillLevel()");
//Logger.IncLvl();
FillLevel -= StepSize;
//Logger.DecLvl();
}
public override void OnKeyPressed(string keyCode)
{
//Logger.debug(Type + ": OnKeyPressed():");
//Logger.IncLvl();
switch (keyCode)
{
case "LEFT/ABORT":
DecreaseFillLevel();
break;
case "RIGHT/SUBMIT":
IncreaseFillLevel();
break;
}
base.OnKeyPressed(keyCode);
//Logger.DecLvl();
}
/*protected override void RenderText(ref List<string> segments, int width, int availableWidth)
{
//Logger.debug(Type + ".RenderText()");
//Logger.IncLvl();
string suffix = IsSelected() ? ">" : "  ";
string prefix = IsSelected() ? "<" : "  ";
string renderString = prefix + "[";
float fillLevel = FillLevel;
string fillString = GetAttribute("filledstring");
string emptyString = GetAttribute("emptystring");
int innerWidth = (width - 2 * TextUtils.GetTextWidth("[]"));
renderString += TextUtils.CreateStringOfLength(fillString, (int)(innerWidth * fillLevel));
renderString += TextUtils.CreateStringOfLength(emptyString, (int)(innerWidth * (1 - fillLevel)));
renderString += "]" + suffix;
segments.Add(renderString);
//Logger.DecLvl();
}*/
public override RenderBox GetRenderBox(int maxWidth, int maxHeight)
{
//Logger.debug("ProgressBar.GetRenderCache(int)");
//Logger.IncLvl();
RenderBoxTree cache = new RenderBoxTree();
cache.type = Type;
int outerWidth = TextUtils.GetTextWidth(IsSelected() ? "<[]>" : " [] ") + 2;
RenderBox prefix = new RenderBoxLeaf(
(IsSelected() ? "<" : " ") + "[");
prefix.MaxWidth = prefix.MinWidth;
RenderBox suffix = new RenderBoxLeaf(
"]" + (IsSelected() ? ">" : " "));
suffix.MaxWidth = suffix.MinWidth;
cache.Add(prefix);
filledBar = new RenderBoxLeaf();
filledBar.PadChar = GetAttribute("filledstring")[0];
filledBar.MinHeight = 1;
cache.Add(filledBar);
emptyBar = new RenderBoxLeaf();
emptyBar.MinHeight = 1;
emptyBar.PadChar = GetAttribute("emptystring")[0];
cache.Add(emptyBar);
cache.Add(suffix);
int width = ResolveSize(GetAttribute("minwidth"), maxWidth);
if (width >= outerWidth)
{
filledBar.MinWidth = (int)((width - outerWidth) * FillLevel);
emptyBar.MinWidth = (int)((width - outerWidth) * (1 - FillLevel));
}
width = ResolveSize(GetAttribute("maxwidth"), maxWidth);
if (width >= outerWidth)
{
filledBar.MaxWidth = (int) ((width - outerWidth) * FillLevel);
emptyBar.MaxWidth = (int)((width - outerWidth) * (1 - FillLevel));
}
width = ResolveSize(GetAttribute("width"), maxWidth);
if (width >= outerWidth)
{
filledBar.DesiredWidth = (int)((width - outerWidth) * FillLevel);
emptyBar.DesiredWidth = (int)((width - outerWidth) * (1 - FillLevel));
}
width = ResolveSize(GetAttribute("forcewidth"), maxWidth);
if (width >= outerWidth)
{
int forcedWidth = (int)((width - outerWidth) * FillLevel);
filledBar.MaxWidth = forcedWidth;
filledBar.MinWidth = forcedWidth;
forcedWidth = (int)((width - outerWidth) * (1 - FillLevel));
emptyBar.MinWidth = forcedWidth;
emptyBar.MaxWidth = forcedWidth;
}
UpdateRenderCacheProperties(cache, maxWidth, maxHeight);
//Logger.log("filledBar: ");
//Logger.debug = false;
//Logger.log("  fillLevel: " + FillLevel);
//Logger.log("  min width: " + filledBar.MinWidth);
//Logger.log("  max width: " + filledBar.MaxWidth);
//Logger.log("  desired width: " + filledBar.DesiredWidth);
//Logger.log("  minheight: " + filledBar.MinHeight);
//Logger.debug = true;
//Logger.log("  actual width: " + filledBar.GetActualWidth(maxWidth));
cache.Flow = RenderBox.FlowDirection.HORIZONTAL;
//GetAttribute("flow") == "horizontal" ? NodeBox.FlowDirection.HORIZONTAL : NodeBox.FlowDirection.VERTICAL;
switch (GetAttribute("alignself"))
{
case "right":
cache.Align = RenderBox.TextAlign.RIGHT;
break;
case "center":
cache.Align = RenderBox.TextAlign.CENTER;
break;
default:
cache.Align = RenderBox.TextAlign.LEFT;
break;
}
//Logger.DecLvl();
return cache;
}
}
//!EMBED SEScripts.XUI.XML.Container

public class HorizontalLine : XMLTree
{
public HorizontalLine() : base()
{
Type = "hl";
SetAttribute("width", "100%");
SetAttribute("minheight", "1");
SetAttribute("maxheight", "1");
}
/*protected override void RenderText(ref List<string> segments, int width, int availableWidth)
{
segments.Add(TextUtils.CreateStringOfLength("_", width, TextUtils.RoundMode.CEIL));
}*/
public override RenderBox GetRenderBox(int maxWidth, int maxHeight)
{
//using (new Logger("HorizontalLine.GetRenderBox()"))
//{
RenderBox cache = new RenderBoxLeaf();
cache.type = Type;
//cache.Add("_");
cache.PadChar = '_';
UpdateRenderCacheProperties(cache, maxWidth, maxHeight);
return cache;
//}
}
}

public class VerticalLine : XMLTree
{
public VerticalLine() : base()
{
Type = "vl";
SetAttribute("height", "100%");
//SetAttribute("minwidth", "4");
}
/*protected override void RenderText(ref List<string> segments, int width, int availableWidth)
{
segments.Add(TextUtils.CreateStringOfLength("_", width, TextUtils.RoundMode.CEIL));
}*/
public override RenderBox GetRenderBox(int maxWidth, int maxHeight)
{
//using (new Logger("VerticalLine.GetRenderBox()"))
//{
RenderBox cache = new RenderBoxLeaf();
cache.PadChar = '|';
cache.type = Type;
//cache.Add("_");
UpdateRenderCacheProperties(cache, maxWidth, maxHeight);
cache.MinWidth = TextUtils.GetCharWidth('|');
return cache;
//}
}
}

public class UIControls : XMLTree
{
UIController Controller;
public UIControls() : base()
{
Type = "uicontrols";
Controller = null;
SetAttribute("selectable", "false");
}
private void UpdateController()
{
Controller = RetrieveRoot() as UIController;
SetAttribute("selectable", (Controller != null && Controller.UIStack.Count > 0) ? "true" : "false");
if (IsSelectable())
{
PreventDefault("LEFT/ABORT");
PreventDefault("RIGHT/SUBMIT");
}
else
{
AllowDefault("LEFT/ABORT");
AllowDefault("RIGHT/SUBMIT");
}
GetParent().UpdateSelectability(this);
if (IsSelected() && !IsSelectable())
{
GetParent().SelectNext();
}
}
public override void OnKeyPressed(string keyCode)
{
if (Controller == null)
{
UpdateController();
}
switch (keyCode)
{
case "LEFT/ABORT":
case "RIGHT/SUBMIT":
if (Controller != null && Controller.UIStack.Count > 0)
{
Controller.RevertUI();
}
break;
}
}
/*protected override string PostRender(List<string> segments, int width, int availableWidth)
{
if (Controller == null)
{
UpdateController();
}
string prefix;
if (!IsSelectable())
{
prefix = "";
}
else
{
prefix = IsSelected() ? "<<" : TextUtils.CreateStringOfLength(" ", TextUtils.GetTextWidth("<<"));
}
string renderString = base.PostRender(segments, width, availableWidth);
int prefixSpacesCount = TextUtils.CreateStringOfLength(" ", TextUtils.GetTextWidth(prefix)).Length;
string tmpPrefix = "";
for (int i = 0; i < prefixSpacesCount; i++)
{
if ((renderString.Length - 1) < i || renderString[i] != ' ')
{
tmpPrefix += " ";
}
}
renderString = prefix + (tmpPrefix + renderString).Substring(prefixSpacesCount);
return renderString;
//renderString = prefix + renderString;
}*/
public override RenderBox GetRenderBox(int maxWidth, int maxHeight)
{
//using (new Logger("XMLTree<" + Type + ">.GetRenderBox(int, int)"))
//{
//Logger.debug("UIControls.GetRenderCache(int)");
//Logger.IncLvl();
RenderBoxTree cache = new RenderBoxTree();
cache.type = Type;
if (Controller == null)
{
UpdateController();
}
if (IsSelectable())
{
RenderBox childCache = new RenderBoxLeaf(IsSelected() ?
new StringBuilder("<<") :
TextUtils.CreateStringOfLength(' ', TextUtils.GetTextWidth("<<")));
childCache.MaxWidth = childCache.MinWidth;
cache.Add(childCache);
}
RenderBoxTree contentCache = new RenderBoxTree();
contentCache.Flow = GetAttribute("flow") == "horizontal" ? RenderBox.FlowDirection.HORIZONTAL : RenderBox.FlowDirection.VERTICAL;
foreach (XMLTree child in Children)
{
contentCache.Add(child.GetRenderBox(maxWidth, maxHeight));
}
cache.Add(contentCache);
UpdateRenderCacheProperties(cache, maxWidth, maxHeight);
cache.Flow = RenderBox.FlowDirection.HORIZONTAL;

return cache;
}
//}
}

public class TextInput : XMLTree
{
int CursorPosition;
public TextInput()
{
//Logger.log("TextInput constructor()");
//Logger.IncLvl();
Type = "textinput";
Selectable = true;
CursorPosition = -1;
PreventDefault("LEFT/ABORT");
PreventDefault("RIGHT/SUBMIT");
SetAttribute("maxlength", "10");
SetAttribute("value", "");
SetAttribute("allowedchars", " a-z0-9");
//Logger.DecLvl();
}
public override void OnKeyPressed(string keyCode)
{
switch (keyCode)
{
case "LEFT/ABORT":
DecreaseCursorPosition();
break;
case "RIGHT/SUBMIT":
IncreaseCursorPosition();
break;
case "UP":
DecreaseLetter();
break;
case "DOWN":
IncreaseLetter();
break;
default:
base.OnKeyPressed(keyCode);
break;
}
}
public override void SetAttribute(string key, string value)
{
if(key == "allowedchars")
{
if(!System.Text.RegularExpressions.Regex.IsMatch(value,
@"([^-\\]-[^-\\]|[^-\\]|\\-|\\\\)*"))
{
throw new Exception("Invalid format of allowed characters!");
}

}
if (key == "value")
using (new Logger("set value: " + value)) { }
base.SetAttribute(key, value);
}
private void IncreaseLetter()
{
//Logger.log("TextInput.IncreaseLetter()");
//Logger.IncLvl();
if (CursorPosition == -1)
{
return;
}
char[] value = GetAttribute("value").ToCharArray();
char letter = value[CursorPosition];
string[] charSets = GetAllowedCharSets();
for (int i = 0; i < charSets.Length; i++)
{
if ((charSets[i].Length == 1 && charSets[i][0] == value[CursorPosition])
|| (charSets[i].Length == 3 && charSets[i][2] == value[CursorPosition]))
{
//Logger.log("letter outside class, setting to: " + charSets[i == 0 ? charSets.Length - 1 : i - 1][0] + ". (chars[" + ((i + 1) % charSets.Length) + "])");
value[CursorPosition] = charSets[(i + 1) % charSets.Length][0];
SetAttribute("value", new string(value));
//Logger.DecLvl();
return;
}
}
//Logger.log("letter inside class, setting to: " + (char)(((int)value[CursorPosition]) + 1));
value[CursorPosition] = (char)(((int)value[CursorPosition]) + 1);
SetAttribute("value", new string(value));
//Logger.DecLvl();
}
private void DecreaseLetter()
{
//Logger.log("TextInput.DecreaseLetter()");
//Logger.IncLvl();
if (CursorPosition == -1)
{
return;
}
char[] value = GetAttribute("value").ToCharArray();
char[] chars = GetAttribute("allowedchars").ToCharArray();
string[] charSets = GetAllowedCharSets();
for(int i = 0; i < charSets.Length; i++)
{
if(charSets[i][0] == value[CursorPosition])
{
int index = (i == 0 ? charSets.Length - 1 : i - 1);
//Logger.log("letter outside class, setting to: " + charSets[index][charSets[index].Length - 1] + ". (chars[" + (index) + "])");
value[CursorPosition] = charSets[index][charSets[index].Length - 1];
SetAttribute("value", new string(value));
return;
}
}
//Logger.log("letter inside class, setting to: " + (char)(((int)value[CursorPosition]) - 1));
value[CursorPosition] = (char)(((int)value[CursorPosition]) - 1);
SetAttribute("value", new string(value));
//Logger.DecLvl();
}
private string[] GetAllowedCharSets()
{
string charString = GetAttribute("allowedchars");
System.Text.RegularExpressions.MatchCollection matches =
System.Text.RegularExpressions.Regex.Matches(charString, @"[^-\\]-[^-\\]|[^-\\]|\\-|\\\\");
string[] charSets = new string[matches.Count];
int i = 0;
foreach (System.Text.RegularExpressions.Match match in matches)
{
string matchString = match.ToString();
if (matchString == "\\-")
{
charSets[i] = "-";
}
else if (matchString == "\\\\")
{
charSets[i] = "\\";
}
else
{
charSets[i] = matchString;
}
i++;
}
//P.Echo("Char sets found: " + string.Join(",", charSets));
return charSets;
}
private void IncreaseCursorPosition()
{
if (CursorPosition < Single.Parse(GetAttribute("maxlength")) - 1)
{
CursorPosition++;
}
else
{
CursorPosition = 0;
DecreaseCursorPosition();
KeyPress("DOWN");
}
if (CursorPosition != -1)
{
PreventDefault("UP");
PreventDefault("DOWN");
}
if (CursorPosition >= GetAttribute("value").Length)
{
string[] charSets = GetAllowedCharSets();
SetAttribute("value", GetAttribute("value") + charSets[0][0]);
}
}
private void DecreaseCursorPosition()
{
if (CursorPosition > -1)
{
CursorPosition--;
}
if (CursorPosition == -1)
{
AllowDefault("UP");
AllowDefault("DOWN");
}
}
/*protected override void RenderText(ref List<string> segments, int width, int availableWidth)
{
string value = GetAttribute("value");
if (CursorPosition != -1)
{
value = value.Substring(0, CursorPosition)
+ "|" + value.Substring(CursorPosition, 1) + "|"
+ value.Substring(CursorPosition + 1);
}
else if (value.Length == 0)
{
value = "_" + value;
}
segments.Add((IsSelected() ? new string(new char[] { (char)187 }) : "  ") + " " + value);
}*/
public override RenderBox GetRenderBox(int maxWidth, int maxHeight)
{
using (Logger logger = new Logger("TextInput.GetRenderCache(int)", Logger.Mode.LOG))
{
RenderBoxTree cache = new RenderBoxTree();
UpdateRenderCacheProperties(cache, maxWidth, maxHeight);
RenderBoxLeaf content = new RenderBoxLeaf();
cache.type = Type;
cache.Flow = RenderBox.FlowDirection.HORIZONTAL;
cache.Add(content);
content.Add((IsSelected() ? new string(new char[] { (char)187 }) : " ") + " ");
content.MinWidth = TextUtils.GetTextWidth(new string(new char[] { (char)187, ' ' }));
string value = GetAttribute("value");
logger.log("value: " + value, Logger.Mode.LOG);
if (CursorPosition != -1)
{
content.Add(value.Substring(0, CursorPosition));
content.Add("|");
content.Add(value.Substring(CursorPosition, 1));
content.Add("|");
content.Add(value.Substring(CursorPosition + 1));
}
else
{
if (value.Length == 0)
content.Add("_");
content.Add(value);
}
/*
for(int i = 0; i < cache.Count; i++)
{
cache[i].MaxWidth = cache[i].MinWidth;
}*/
logger.log("height: " + cache.GetActualHeight(maxHeight), Logger.Mode.LOG);
return cache;
}
}
}

public abstract class DataStore : XMLTree
{
public DataStore() : base() { }
public override Dictionary<string, string> GetValues(Func<XMLTree, bool> filter)
{
Dictionary<string, string> dict = base.GetValues(filter);
if (!filter(this))
{
return dict;
}
foreach (KeyValuePair<string, string> data in Attributes)
{
if (!dict.ContainsKey(data.Key))
{
dict[data.Key] = data.Value;
}
}
return dict;
}
}


public class SubmitButton : MenuItem
{
public SubmitButton()
{
Type = "submitbutton";
SetAttribute("flowdirection", "horizontal");
}
/*protected override void PreRender(ref List<string> segments, int width, int availableWidth)
{
segments.Add(IsSelected() ? "[[  " : "[   ");
base.PreRender(ref segments, width, availableWidth);
}*/
/*protected override string PostRender(List<string> segments, int width, int availableWidth)
{
segments.Add(IsSelected() ? "  ]]" : "   ]");
return base.PostRender(segments, width, availableWidth);
}*/
public override RenderBox GetRenderBox(int maxWidth, int maxHeight)
{
//Logger.debug("SubmitButton.GetRenderCache(int)");
//Logger.IncLvl();
RenderBoxTree cache = new RenderBoxTree();
cache.type = Type;
RenderBoxLeaf childCache = new RenderBoxLeaf(IsSelected() ? "[[  " : "[   ");
childCache.MaxWidth = childCache.MinWidth;
cache.Add(childCache);
RenderBoxTree contentCache = new RenderBoxTree();
contentCache.Flow = GetAttribute("flow") == "horizontal" ? RenderBox.FlowDirection.HORIZONTAL : RenderBox.FlowDirection.VERTICAL;
foreach (XMLTree child in Children)
{
contentCache.Add(child.GetRenderBox(maxWidth, maxHeight));
}
cache.Add(contentCache);
childCache = new RenderBoxLeaf(IsSelected() ? "  ]]" : "   ]");
childCache.MaxWidth = childCache.MinWidth;
cache.Add(childCache);
UpdateRenderCacheProperties(cache, maxWidth, maxHeight);
cache.Flow = RenderBox.FlowDirection.HORIZONTAL;
//Logger.DecLvl();
return cache;
}
}

public class Break : TextNode
{
public Break() : base("")
{
Type = "br";
}
//protected override void RenderText(ref List<string> segments, int width, int availableWidth) { }
/*protected override string PostRender(List<string> segments, int width, int availableWidth)
{
return "";
}*/
public override RenderBox GetRenderBox(int maxWidth, int maxHeight)
{
//using (new Logger("Break.GetRenderBox(int, int)"))
//{
RenderBox cache = new RenderBoxLeaf("\n");
cache.type = Type;
cache.MaxHeight = (GetParent() as XMLTree)?.GetAttribute("flow") == "horizontal" ? 1 : 0;
cache.MaxWidth = 0;
return cache;
//}
}
}

public class Space : XMLTree
{
public Space() : base()
{
//Logger.debug("Space constructor()");
//Logger.IncLvl();
Type = "space";
SetAttribute("width", "0");
//Logger.DecLvl();
}
/*protected override void RenderText(ref List<string> segments, int width, int availableWidth)
{
//Logger.debug(Type + ".RenderText()");
//Logger.IncLvl();
segments.Add(TextUtils.CreateStringOfLength(" ", width));
//Logger.DecLvl();
}*/
public override RenderBox GetRenderBox(int maxWidth, int maxHeight)
{
//Logger.debug("GetRenderCache(int)");
//Logger.IncLvl();
RenderBox cache = new RenderBoxLeaf();
cache.type = Type;
cache.MinHeight = 1;
int width = ResolveSize(GetAttribute("width"), maxWidth);
cache.MinWidth = width;
cache.MaxWidth = width;
//Logger.DecLvl();
return cache;
}
}

public class Hidden : XMLTree
{
public Hidden() : base()
{
Type = "hidden";
}
/*protected override string PostRender(List<string> segments, int width, int availableWidth)
{
return null;
}*/
public override RenderBox GetRenderBox(int maxWidth, int maxHeight)
{
//using (new Logger("Hidden.GetRenderCache(int)", Logger.Mode.LOG))
//{
RenderBox cache = new RenderBoxTree();
cache.type = Type;
cache.MaxWidth = 0;
cache.MaxHeight = 0;
return cache;
//}
}
}

public class HiddenData : DataStore
{
public HiddenData() : base()
{
Type = "hiddendata";
}
/*protected override string PostRender(List<string> segments, int width, int availableWidth)
{
return null;
}*/
public override RenderBox GetRenderBox(int maxWidth, int maxHeight)
{
//using (new Logger("hiddenData.GetRenderCache(int)"))
//{
RenderBox cache = new RenderBoxTree();
cache.type = Type;
cache.MaxWidth = 0;
cache.MaxHeight = 0;
return cache;
//}
}
}

class MetaNode : Hidden
{
public MetaNode() : base()
{
Type = "meta";
}
public override Dictionary<string, string> GetValues(Func<XMLTree, bool> filter)
{
if (filter(this))
{
return Attributes;
}
else
{
return new Dictionary<string, string>();
}
}
public override void SetAttribute(string key, string value)
{
long fontValue;
if (key.ToLower() == "fontfamily" && long.TryParse(value, out fontValue))
{
if (fontValue == 1147350002)
TextUtils.SelectFont(TextUtils.FONT.MONOSPACE);
else
TextUtils.SelectFont(TextUtils.FONT.DEFAULT);
}
base.SetAttribute(key, value);
}
}

public abstract class RenderBox
{
protected bool minHeightIsCached;
protected bool minWidthIsCached;
protected int minHeightCache;
protected int minWidthCache;
public bool DEBUG = false;
public char PadChar;
public enum TextAlign { LEFT, RIGHT, CENTER }
public enum FlowDirection { HORIZONTAL, VERTICAL }
//public abstract int Height { get; set; }
public abstract void Add(string box);
public abstract void Add(StringBuilder box);
public abstract void AddAt(int position, string box);
public abstract void AddAt(int position, StringBuilder box);
public abstract StringBuilder GetLine(int index);
public abstract StringBuilder GetLine(int index, int maxWidth, int maxHeight);
public abstract void Clear();
private RenderBox.FlowDirection _Flow;
private RenderBox.TextAlign _Align;
protected int _MinWidth;
protected int _MaxWidth;
protected int _DesiredWidth;
protected int _MinHeight;
protected int _MaxHeight;
protected int _DesiredHeight;
public RenderBox Parent;
public string type;
public int GetActualWidth(int maxWidth)
{
using (Logger logger = new Logger("RenderBox.GetActualWidth(int)", Logger.Mode.LOG))
{
logger.log("Type: " + type, Logger.Mode.LOG);
if (this as RenderBoxLeaf != null)
logger.log("content: |" + (this as RenderBoxLeaf).Content + "|", Logger.Mode.LOG);
logger.log("implicit max width: " + maxWidth, Logger.Mode.LOG);
logger.log("explicit max width: " + MaxWidth, Logger.Mode.LOG);
logger.log("min width: " + MinWidth, Logger.Mode.LOG);
logger.log("desired width: " + DesiredWidth, Logger.Mode.LOG);
if (MaxWidth != -1)
maxWidth = (maxWidth == -1 ? MaxWidth : Math.Min(MaxWidth, maxWidth));
if (maxWidth == -1)
{
//Logger.debug("actual width equals min width");
//Logger.DecLvl();
return Math.Max(MinWidth, DesiredWidth);
}
else
{
int desired;
if (DesiredWidth == -1)
{
//Logger.debug("actual width equals max width");
desired = Math.Min(MinWidth, maxWidth);
}
else
{
//Logger.debug("actual width equals desired width, but if desired<min -> width=min and if desired>max -> width = max");
desired = Math.Max(MinWidth, DesiredWidth);
}
//Logger.DecLvl();
logger.log("actual width: " + Math.Min(desired, maxWidth), Logger.Mode.LOG);
return Math.Min(desired, maxWidth);
}
}
}
public int GetActualHeight(int maxHeight)
{
using (Logger logger = new Logger("RenderBox.GetActualHeight(int)", Logger.Mode.LOG))
{
logger.log("Type: " + type, Logger.Mode.LOG);
//logger.log("implicit max height: " + maxHeight, Logger.Mode.LOG);
//logger.log("explicit max height: " + MaxHeight, Logger.Mode.LOG);
//Logger.debug("NodeBox.GetActualHeight(int)");
//Logger.IncLvl();
if (MaxHeight != -1)
maxHeight = (maxHeight == -1 ? MaxHeight : Math.Min(MaxHeight, maxHeight));
if (maxHeight == -1)
{
//logger.log("actual height equals min height", Logger.Mode.LOG);
//Logger.DecLvl();
return DesiredHeight == -1 ? MinHeight : Math.Min(MinHeight, DesiredHeight);
}
else
{
int desired = DesiredHeight == -1 ? MinHeight : Math.Max(MinHeight, DesiredHeight);
//Logger.DecLvl();
logger.log("actual height: " + Math.Min(desired, maxHeight) + " (min( " + desired + ", " + maxHeight + ")", Logger.Mode.LOG);
return Math.Min(desired, maxHeight);
}
}
}
public RenderBox.TextAlign Align
{
get { return _Align; }
set
{
_Align = value;
}
}
public virtual RenderBox.FlowDirection Flow
{
get { return _Flow; }
set {
_Flow = value;
ClearCache();
}
}
public virtual int MinWidth
{
get
{
/*using (new SimpleProfiler("RenderBox.MinWidth.get"))
//{
//Logger.debug("NodeBox.MinWidth.get()");
//Logger.IncLvl();
//Logger.debug("minwidth = " + _MinWidth);
//Logger.DecLvl();*/
return _MinWidth;
//}
}
set
{
//using (new SimpleProfiler("RenderBox.MinWidth.get"))
//{
//Logger.debug("NodeBox.MinWidth.set()");
//Logger.IncLvl();
//Logger.debug("minwidth = " + value);
_MinWidth = Math.Max(0, value);
ClearCache();
//Logger.DecLvl();
//}
}
}
public int DesiredWidth
{
get
{
//Logger.debug("NodeBox.DesiredWidth.get()");
//Logger.IncLvl();
//Logger.debug("desiredwidth = " + _DesiredWidth);
//Logger.DecLvl();
return _DesiredWidth;
}
set
{
//Logger.debug("NodeBox.DesiredWidth.set()");
//Logger.IncLvl();
//Logger.debug("desiredwidth = " + value);
_DesiredWidth = value;
//Logger.DecLvl();
}
}
public int MaxWidth
{
get
{
//Logger.debug("NodeBox.MaxWidth.get()");
//Logger.IncLvl();
//Logger.debug("maxwidth = " + _MaxWidth);
//Logger.DecLvl();
return _MaxWidth;
}
set
{
//Logger.debug("NodeBox.MaxWidth.set()");
//Logger.IncLvl();
//Logger.debug("maxwidth = " + value);
_MaxWidth = value;
//Logger.DecLvl();
}
}
public virtual int MinHeight
{
get
{
//using (new SimpleProfiler("RenderBox.MinHeight.get"))
//{
//Logger.debug("NodeBox.MinHeight.get()");
//Logger.IncLvl();
//Logger.debug("minheight = " + _MinHeight);
//Logger.DecLvl();
return _MinHeight;
//}
}
set
{
//using (new SimpleProfiler("RenderBox.MinHeight.set"))
//{
//Logger.debug("NodeBox.MinHeight.set()");
//Logger.IncLvl();
//Logger.debug("minheight = " + value);
_MinHeight = Math.Max(0, value);
ClearCache();
//Logger.DecLvl();
//}
}
}
public int DesiredHeight
{
get
{
//Logger.debug("NodeBox.DesiredHeight.get()");
//Logger.IncLvl();
//Logger.debug("desiredheight = " + _DesiredHeight);
//Logger.DecLvl();
return _DesiredHeight;
}
set
{
//Logger.debug("NodeBox.DesiredHeight.set()");
//Logger.IncLvl();
//Logger.debug("desiredheight = " + value);
_DesiredHeight = value;
//Logger.DecLvl();
}
}
public int MaxHeight
{
get
{
//Logger.debug("NodeBox.MaxHeight.get()");
//Logger.IncLvl();
//Logger.debug("maxheight = " + _MaxHeight);
//Logger.DecLvl();
return _MaxHeight;
}
set
{
//Logger.debug("NodeBox.MaxHeight.set()");
//Logger.IncLvl();
//Logger.debug("maxheight = " + value);
//using (Logger logger = new Logger("RenderBox.MaxHeight.set", Logger.Mode.LOG))
//{
//logger.log("value: " + value, Logger.Mode.LOG);
_MaxHeight = value;
ClearCache();
//}
//Logger.DecLvl();
}
}
public RenderBox()
{
//using (new Logger("RenderBox.__construct()", Logger.Mode.LOG))
//{
//Logger.debug("NodeBox constructor()");
PadChar = ' ';
_Flow = RenderBox.FlowDirection.VERTICAL;
_Align = RenderBox.TextAlign.LEFT;
_MinWidth = 0;
_MaxWidth = -1;
_DesiredWidth = -1;
_MinHeight = 0;
_MaxHeight = -1;
_DesiredHeight = -1;
minHeightIsCached = false;
minWidthIsCached = false;
//}
}
public IEnumerable<StringBuilder> GetLines(int maxWidth, int maxHeight)
{
//using (new SimpleProfiler("RenderBox.GetLines(int, int)"))
//{
//Logger.debug("NodeBox.GetRenderedLines()");
//Logger.IncLvl();
int height = GetActualHeight(maxHeight);
for (int i = 0; i < height; i++)
{
yield return GetLine(i, maxWidth, maxHeight);
}
//Logger.DecLvl();
//}
}
public IEnumerable<StringBuilder> GetLines()
{
//using (new SimpleProfiler("RenderBox.GetLines()"))
//{
//Logger.debug("NodeBox.GetRenderedLines()");
//Logger.IncLvl();
int height = GetActualHeight(-1);
for (int i = 0; i < height; i++)
{
yield return GetLine(i, -1, -1);
}
//Logger.DecLvl();
//}
}
protected void AlignLine(ref StringBuilder line)
{
//using (new SimpleProfiler("RenderBox.AlignLine(ref StringBuilder)"))
//{
AlignLine(ref line, -1);
//}
}
protected void AlignLine(ref StringBuilder line, int maxWidth)
{
//using (Logger logger = new Logger("RenderBox.AlignLine(ref StringBuilder, int)", Logger.Mode.LOG))
//{
//Logger.debug("NodeBox.AlignLine()");
//Logger.IncLvl();
//Logger.debug("max width is " + maxWidth);
int actualWidth = GetActualWidth(maxWidth);
//logger.log("actualWidth: " + actualWidth, Logger.Mode.LOG);
//Logger.debug("actual width is " + actualWidth);
//Logger.debug("line width is " + TextUtils.GetTextWidth(line));
//Logger.debug("line is: |" + line + "|");
int remainingWidth = actualWidth - TextUtils.GetTextWidth(line.ToString());
//Logger.debug("remaining width is " + remainingWidth);
if (remainingWidth > 0) // line is not wide enough; padding necessary
{
////Logger.debug("line is so far: |" + line.ToString() + "|");
//Logger.debug("padding...");
switch (Align)
{
case TextAlign.CENTER:
line = TextUtils.PadText(line.ToString(), actualWidth, TextUtils.PadMode.BOTH, PadChar);
break;
case TextAlign.RIGHT:
line = TextUtils.PadText(line.ToString(), actualWidth, TextUtils.PadMode.LEFT, PadChar);
break;
default:
line = TextUtils.PadText(line.ToString(), actualWidth, TextUtils.PadMode.RIGHT, PadChar);
break;
}
////Logger.debug("line is so far: |" + line.ToString() + "|");
}
else if (remainingWidth < 0)
{
//Logger.debug("clipping");
line = new StringBuilder(line.ToString());
while (remainingWidth < 0)
{
remainingWidth += TextUtils.GetTextWidth(new string(new char[] { line[line.Length - 1] })) + 1;
line.Remove(line.Length - 1, 1);
}
}
else
{
//Logger.debug("neither padding nor clipping...");
}
//Logger.debug("aligned line is: {" + line + "}");
//Logger.DecLvl();
//}
}
public string Render(int maxWidth, int maxHeight)
{
//using (Logger logger = new Logger("RenderBox.Render(" + maxWidth + ", " + maxHeight + ")", Logger.Mode.LOG))
//{
StringBuilder result = new StringBuilder();
int i = 0;
foreach (StringBuilder line in GetLines(maxWidth, maxHeight))
{
//logger.log("rendering line " + (i++), Logger.Mode.LOG);
result.Append(line);
result.Append("\n");
}
if (result.Length > 0)
result.Remove(result.Length - 1, 1);
return result.ToString();
//}
}
public void ClearCache()
{
minHeightIsCached = false;
minWidthIsCached = false;
if(Parent != null)
Parent?.ClearCache();
}
}

public class RenderBoxLeaf : RenderBox
{
public string Content;
public override RenderBox.FlowDirection Flow
{
get { return RenderBox.FlowDirection.VERTICAL; }
set { }
}
public override int MinHeight
{
get
{
//using (new SimpleProfiler("RenderBoxLeaf.MinHeight.get"))
//{
//Logger.debug("NodeBoxLeaf.MinHeight.get");
//Logger.IncLvl();
if (minHeightIsCached && false)
return minHeightCache;
if (Content.Length > 0)
{
minHeightCache = Math.Max(_MinHeight, 1);
}
else
{
minHeightCache = _MinHeight;
}
if(MaxHeight != -1)
{
minHeightCache = Math.Min(minHeightCache, MaxHeight);
}
minHeightIsCached = true;
//Logger.debug("minheight = " + minHeightCache);
//Logger.DecLvl();
return minHeightCache;
//}
}
set
{
//using (new SimpleProfiler("RenderBoxLeaf.MinHeight.set"))
//{
//Logger.debug("NodeBoxLeaf.MinHeight.set");
//Logger.IncLvl();
//Logger.debug("minheight = " + value);
_MinHeight = value;
ClearCache();
//Logger.DecLvl();
//}
}
}
public override int MinWidth
{
get
{
//using (new SimpleProfiler("RenderBoxLeaf.MinWidth.get"))
//{
//Logger.debug("NodeBoxLeaf.MinWidth.get");
//Logger.IncLvl();
if (minWidthIsCached && false)
return minWidthCache;
minWidthCache = MinHeight == 0 ? 0 : Math.Max(TextUtils.GetTextWidth(Content), _MinWidth);
//Logger.debug("minwidth = " + minWidth);
minWidthIsCached = true;
//Logger.DecLvl();
return minWidthCache;
//}
}
set
{
//using (new SimpleProfiler("RenderBoxLeaf.MinWidth.set"))
//{
//Logger.debug("NodeBoxLeaf.MinWidth.set()");
//Logger.IncLvl();
//Logger.debug("minwidth = " + value);
_MinWidth = value;
ClearCache();
//Logger.DecLvl();
//}
}
}
public RenderBoxLeaf()
{
//using (new Logger("RenderBoxLeaf.__construct()", Logger.Mode.LOG))
//{
//Logger.debug("NodeBoxLeaf constructor()");
Content = "";
ClearCache();
//}
}
public RenderBoxLeaf(StringBuilder content) : this()
{
//using (new Logger("RenderBoxLeaf.__construct(StringBuilder)", Logger.Mode.LOG))
//{
//Logger.debug("NodeBoxLeaf constructor(StringBuilder)");
//Logger.IncLvl();
Add(content);
//Logger.DecLvl();
//}
}
public RenderBoxLeaf(string content) : this(new StringBuilder(content))
{ }
public override void AddAt(int position, StringBuilder box)
{
//using (new SimpleProfiler("RenderBoxLeaf.AddAt(int, StringBuilder)"))
//{
//Logger.debug("NodeBoxLeaf.AddAt(int, StringBuilder)");
//Logger.IncLvl();
/*box.Replace("\n", "");
box.Replace("\r", "");*/
if (position == 0)
{
Content = box.ToString() + Content;
}
else
{
Content += box;
}
ClearCache();
//Logger.DecLvl();
//}
}
public override void Add(StringBuilder box)
{
//using (new SimpleProfiler("RenderBoxLeaf.Add(StringBuilder)"))
//{
//Logger.debug("NodeBoxLeaf.Add(StringBuilder)");
//Logger.IncLvl();
AddAt(1, box);
//Logger.DecLvl();
//}
}
public override void AddAt(int position, string box)
{
//using (new SimpleProfiler("RenderBoxLeaf.AddAt(int, string)"))
//{
//Logger.debug("NodeBoxLeaf.AddAt(int, string)");
//Logger.IncLvl();
AddAt(position, new StringBuilder(box));
//Logger.DecLvl();
//}
}
public override void Add(string box)
{
//using (new SimpleProfiler("RenderBoxLeaf.Add(string)"))
//{
//Logger.debug("NodeBoxLeaf.Add(string)");
//Logger.IncLvl();
Add(new StringBuilder(box));
//Logger.DecLvl();
//}
}
public override StringBuilder GetLine(int index)
{
return GetLine(index, -1, -1);
}
public override StringBuilder GetLine(int index, int maxWidth, int maxHeight)
{
//using (Logger logger = new Logger("RenderBoxLeaf.GetLine(int, int, int)", Logger.Mode.LOG))
//{
//logger.log("type: " + type, Logger.Mode.LOG);
//logger.log("index: " + index, Logger.Mode.LOG);
//logger.log("maxwidth: " + maxWidth, Logger.Mode.LOG);
StringBuilder line;
if (index == 0)
{
line = new StringBuilder(Content.ToString());
}
else
{
line = new StringBuilder();
}
AlignLine(ref line, maxWidth);
//Logger.debug("line is {" + line + "}");
//Logger.log("instructions: " + (P.Runtime.CurrentInstructionCount - instructions) + " -> " + P.Runtime.CurrentInstructionCount + "/" + P.Runtime.MaxInstructionCount)
//Logger.DecLvl();
return line;
//}
}
public override void Clear()
{
//using (new SimpleProfiler("RenderBoxLeaf.Clear()"))
//{
Content = "";
ClearCache();
//}
}
}

public class RenderBoxTree : RenderBox
{
List<RenderBox> Boxes;
public RenderBox this[int i]
{
get
{
return Boxes[i];
}
set
{
Boxes[i] = value;
}
}
public int Count
{
get
{
//using (new SimpleProfiler("RenderBoxTree.Count.get"))
//{
//Logger.debug("NodeBoxTree.Count.get");
return Boxes.Count;
//}
}
}
public override int MinHeight
{
get
{
//using (new SimpleProfiler("RenderBoxTree.MinHeight.get"))
//{
//Logger.debug("NodeBoxTree.MinHeight.get");
//Logger.IncLvl();
if (minHeightIsCached)
return minHeightCache;
//int minHeight = (Flow != FlowDirection.HORIZONTAL ? 0 : _MinHeight);
int minHeight = 0;
int boxMinHeight;
foreach (RenderBox box in Boxes)
{
if (Flow == RenderBox.FlowDirection.HORIZONTAL)
{
minHeight = Math.Max(minHeight, box.MinHeight);
}
else
{
boxMinHeight = box.MinHeight;
if (boxMinHeight > 0)
{
minHeight += boxMinHeight;
//Logger.debug("min height + " + boxMinHeight);
}
}
}
//Logger.debug("minheight = " + minHeight.ToString());
minHeightCache = Math.Max(0, Math.Max(_MinHeight, minHeight));
minHeightIsCached = true;
//Logger.DecLvl();
return minHeightCache;
//}
}
}
public override int MinWidth
{
get
{
//using (new SimpleProfiler("RenderBoxTree.MinWidth.get"))
//{
//Logger.debug("NodeBoxTree.MinWidth.get");
//Logger.IncLvl();
if (minWidthIsCached)
return minWidthCache;
int minWidth = (Flow == RenderBox.FlowDirection.HORIZONTAL ? 0 : _MinWidth);
int boxMinWidth;
foreach (RenderBox box in Boxes)
{
if (Flow == RenderBox.FlowDirection.HORIZONTAL)
{
boxMinWidth = box.MinWidth;
if (boxMinWidth > 0)
{
minWidth++;
minWidth += boxMinWidth;
//Logger.debug("min width + " + boxMinWidth);
}
}
else
{
minWidth = Math.Max(box.MinWidth, minWidth);
}
}
if (Flow == RenderBox.FlowDirection.HORIZONTAL)
minWidth = Math.Max(_MinWidth, minWidth - 1);
//Logger.debug("minwidth = " + minWidth);
minWidthCache = Math.Max(minWidth, 0);
minWidthIsCached = true;
//Logger.DecLvl();
return minWidthCache;
//}
}
}
public RenderBoxTree() : base()
{
//using (new Logger("RenderBoxTree.__construct()", Logger.Mode.LOG))
//{
//Logger.debug("NodeBoxTree constructor()");
Boxes = new List<RenderBox>();
//}
}
public override void Add(string box)
{
//using (new SimpleProfiler("RenderBoxTree.Add(string)"))
//{
AddAt(Boxes.Count, box);
//}
}
public override void AddAt(int position, string box)
{
//using (new SimpleProfiler("RenderBoxTree.AddAt(int, string)"))
//{
//Logger.debug("NodeBoxTree.AddAt(int, string)");
//Logger.IncLvl();
AddAt(position, new RenderBoxLeaf(box));
//Logger.DecLvl();
//}
}
public override void Add(StringBuilder box)
{
//using (new SimpleProfiler("RenderBoxTree.Add(StringBuilder)"))
//{
AddAt(Boxes.Count, box);
//}
}
public override void AddAt(int position, StringBuilder box)
{
//using (new SimpleProfiler("RenderBoxTree.AddAt(int, StringBuilder)"))
//{
//Logger.debug("NodeBoxTree.AddAt(int, StringBuilder)");
//Logger.IncLvl();
AddAt(position, new RenderBoxLeaf(box));
//Logger.DecLvl();
//}
}
public void AddAt(int position, RenderBox box)
{
//using (new SimpleProfiler("RenderBoxTree.AddAt(int, RenderBox)"))
//{
//Logger.debug("NodeBoxTree.AddAt(int, NodeBox)");
//Logger.IncLvl();
Boxes.AddOrInsert<RenderBox>(box, position);
box.Parent = this;
ClearCache();
//Logger.DecLvl();
//}
}
public void Add(RenderBox box)
{
//using (new SimpleProfiler("RenderBoxTree.Add(RenderBox)"))
//{
//Logger.debug("NodeBoxTree.Add(NodeBox)");
//Logger.IncLvl();
AddAt(Boxes.Count, box);
//Logger.DecLvl();
//}
}
public override StringBuilder GetLine(int index)
{
return GetLine(index, -1, -1);
}
public override StringBuilder GetLine(int index, int maxWidth, int maxHeight)
{
//using (Logger logger = new Logger("RenderBoxTree.GetLine(int, int, int)", Logger.Mode.LOG))
//{
//Logger.debug("NodeBoxTree.GetLine(int, int)");
//Logger.IncLvl();
StringBuilder line = new StringBuilder();
int floatingMaxHeight = Math.Min(maxHeight, MaxHeight);
if (floatingMaxHeight != -1)
floatingMaxHeight = Math.Max(floatingMaxHeight - MinHeight, 0) - 1;
int boxMinHeight;
int boxHeight;
int boxMaxHeight;
//bool foundLine = false;
if (Flow == RenderBox.FlowDirection.VERTICAL)
{
foreach (RenderBox box in Boxes)
{
boxMinHeight = box.MinHeight;
boxMaxHeight = floatingMaxHeight + boxMinHeight + 1;
boxHeight = box.GetActualHeight(boxMaxHeight);
if (index < boxHeight)
{
line = box.GetLine(index, maxWidth, boxMaxHeight);
//Logger.debug("child box width is " + TextUtils.GetTextWidth(line));
//foundLine = true;
break;
}
else
{
//logger.log("Decreasing index by " + boxHeight, Logger.Mode.LOG);
index -= boxHeight;
if(floatingMaxHeight != -1)
floatingMaxHeight = Math.Max(0, floatingMaxHeight - boxHeight + boxMinHeight);
}
}
}
else
{
int floatingMaxWidth;
if (maxWidth != -1)
floatingMaxWidth = (MaxWidth == -1) ? maxWidth : Math.Min(maxWidth, MaxWidth);
else
floatingMaxWidth = MaxWidth;
if (floatingMaxWidth != -1)
floatingMaxWidth = Math.Max(floatingMaxWidth - MinWidth, 0) - 1;
StringBuilder nextLine;
int boxMinWidth;
foreach (RenderBox box in Boxes)
{
boxMinWidth = box.MinWidth;
nextLine = box.GetLine(index, 1 + floatingMaxWidth + boxMinWidth, maxHeight);
if (floatingMaxWidth != -1)
floatingMaxWidth = Math.Max(0, floatingMaxWidth - TextUtils.GetTextWidth(nextLine.ToString()) + boxMinWidth);
line.Append(nextLine);
//Logger.debug("child box width is: " + TextUtils.GetTextWidth(nextLine));
}
//foundLine = index < Height;
}
AlignLine(ref line, maxWidth);
//Logger.debug("line is: {" + line + "}");
//Logger.DecLvl();
return line;
//}
}
public override void Clear()
{
//Logger.debug("NodeBoxTree.Clear()");
//Logger.IncLvl();
Boxes.Clear();
ClearCache();
//Logger.DecLvl();
}
}
}
//!EMBED SEScripts.Lib.SimpleProfiler
}

public static class Util
{
public static List<T> Uniques<T>(List<T> list) {
List<T> listOut = new List<T>();
bool duplicate;
foreach(T itemIn in list)
{
duplicate = false;
foreach (T itemOut in listOut)
{
if( itemOut.Equals(itemIn) )
{
duplicate = true;
}
}
if(!duplicate)
{
listOut.Add(itemIn);
}
}
return listOut;
}
}

}