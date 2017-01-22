class MAF
{

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
string name;
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
System.Text.RegularExpressions.Regex braceRegex = new System.Text.RegularExpressions.Regex(@"-{.*}$");
if (braceRegex.IsMatch(program.Me.CustomName))
{
name = braceRegex.Replace(program.Me.CustomName, "-{" + PlatformAgent.GenerateSuffix() + "}");
}
else
{
name = program.Me.CustomName + "-{" + PlatformAgent.GenerateSuffix() + "}";
}
Id = new AgentId(name + "@" + name);
}
program.Me.SetCustomName(Id.Name);
ServiceRegistrationProtocol.Platform.RegisterServices(this);
MessageRoutingProtocol.RegisterServices(this);
ProvideServiceInformationProtocol.RegisterServices(this);
}
public void RegisterBuffers(List<string> bufferNames)
{
foreach (string name in bufferNames)
{
IMyTextPanel buffer = GTS.GetBlockWithName(name) as IMyTextPanel;
if (buffer != null)
{
buffer.SetCustomName("RBUFFER-" + Id.Name);
buffer.WritePublicText("");
buffer.WritePrivateText("");
ReceptionBuffers.Add(buffer);
}
}
}
public void CollectPlatformMessages()
{
foreach (IMyTextPanel buffer in ReceptionBuffers)
{
string text = buffer.GetPrivateText();
buffer.WritePrivateText("");
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
Logger.log("Looking for agent '" + message.Receiver.Id + "'...");
AgentMessage.StatusCodes status = AssignMessage(message);
Logger.log("Status: " + status.ToString());
ReceiveMessage(message, status);
}
public override void ReceiveMessage(AgentMessage message, AgentMessage.StatusCodes status)
{
if (message.Receiver.Platform == "local")
{
message.Receiver.Platform = Id.Platform;
}
bool forwarded = false;
if (status == AgentMessage.StatusCodes.RECEIVERNOTFOUND)
{
Logger.log("no receiver found; looking for service '" + message.Service + "'...");
if (message.Service != null && PlatformServices.ContainsKey(message.Service))
{
Logger.log("service is known.");
AgentId receiver = null;
if (message.Receiver == new AgentId("ANY@local") || message.Receiver == new AgentId("ANY@ANY"))
{
if (PlatformServices[message.Service].Count > 0)
{
receiver = PlatformServices[message.Service][0].Provider;
}
else if (message.Receiver.Platform != "ANY")
{
receiver = null;
}
}
else
{
receiver = PlatformServices[message.Service].Find(service => service.Provider == message.Receiver).Provider;
}
if (receiver != null)
{
Logger.log("Capable agent found: '" + receiver.Name + "'");
message.Receiver = receiver;
status = AgentMessage.StatusCodes.OK;
}
else
{
Logger.log("No capable agent found.");
}
}
//TODO: Implement forwarding to other platforms;
Logger.log("Trying to forward message... to '" + message.Receiver.Id + "'.");
if (SendMessage(message))
{
Logger.log("message forwarded.");
forwarded = true;
}
else
{
base.ReceiveMessage(message, status);
}
}
else
{
base.ReceiveMessage(message, status);
}
}
public override bool SendMessage(AgentMessage message)
{
if (base.SendMessage(message))
{
Logger.log("base.SendMessage(message) did succeed.");
return true;
}
else if (message.Sender.Platform == "local")
{
message.Sender.Platform = Id.Name;
}
if (message.Receiver.Platform != Id.Name)
{
Logger.log("Calling SendToPlatform(message, platform)...");
return SendToPlatform(message, message.Receiver.Platform);
}
else
{
return false;
}
}
public bool SendToPlatform(AgentMessage message, string platform)
{
if (platform == "ALL")
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
success = success || SendToPlatform(message, platformName);
}
}
return success;
}
else
{
Logger.log("Trying to send to platform '" + platform + "'...");
IMyTextPanel buffer = GTS.GetBlockWithName("RBUFFER-" + platform) as IMyTextPanel;
if (buffer == null)
{
Logger.log("No corresponding buffer found with name 'RBUFFER-" + platform + "'.");
return false;
}
else
{
Logger.log("Writing message to reception buffer of platform '" + platform + "'...");
buffer.WritePrivateText(message.ToString(), true);
return true;
}
}
}
public static string GenerateSuffix()
{
Logger.debug("PlatformAgent.GenerateSuffix()");
Logger.IncLvl();
const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
Random rnd = new Random();
char[] title = new char[8];
for (int i = 0; i < 8; i++)
{
title[i] = chars[rnd.Next(0, chars.Length)];
}
Logger.DecLvl();
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
}

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

public abstract class AgentProtocol
{
static int ChatCountValue;
private int ChatIdValue;
protected Agent Holder;
public static string Id
{
get { return null; }
}
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
public AgentProtocol(Agent agent)
{
Logger.debug("AgentProtocol constructor()");
Logger.IncLvl();
ChatIdValue = ChatCount;
ChatCountValue++;
Holder = agent;
Logger.DecLvl();
}
public bool TrySetId(int id)
{
if (id == ChatId)
{
return true;
}
else if (id >= ChatCount)
{
ChatCountValue = id + 1;
ChatIdValue = id;
return true;
}
else
{
return false;
}
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
public virtual void ReceiveMessage(AgentMessage msg)
{
if (msg.Status == AgentMessage.StatusCodes.CHATIDNOTACCEPTED)
{
string[] contentSplit = msg.Content.Split(':');
if (contentSplit.Length == 2 && contentSplit[0] == "validId")
{
int chatId = -1;
if (Int32.TryParse(contentSplit[1], out chatId))
{
TrySetId(Math.Max(ChatCount, chatId));
Restart();
}
else
{
Logger.log("ERROR: Could not change chat id");
}
}
}
}
public static void RegisterServices(Agent agent) { }
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

public class AgentId : IEquatable<AgentId>
{
public string Id;
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
public AgentId(string id)
{
Id = id;
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
public Service(string id, string description, AgentId provider, bool providesUI, Func<Agent, AgentProtocol> create) : base(id, description, provider, providesUI)
{
Create = create;
}
}

public class PlatformService
{
public string Id;
public string Description;
public AgentId Provider;
private bool isUIProvider;
public bool ProvidesUI
{
get { return isUIProvider; }
}
public PlatformService(string id, string description, AgentId provider) : this(id, description, provider, false) { }
public PlatformService(string id, string description, AgentId provider, bool providesUI)
{
Id = id;
Description = description;
Provider = provider;
isUIProvider = providesUI;
}
public string ToXML()
{
return "<service id='" + Id.ToString() + "' description='" + Description + "' provider='" + Provider.Id + "'/>";
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
public new static string Id
{
get { return "print"; }
}
public PrintProtocol(Agent agent) : base(agent) { }
public override void ReceiveMessage(AgentMessage msg)
{
Logger.log("Message received:");
Logger.IncLvl();
Logger.log("from: " + msg.Sender.Id);
Logger.log("content: " + msg.Content);
Logger.DecLvl();
Stop();
}
public override void Restart() { }
public static new void RegisterServices(Agent holder)
{
holder.RegisterService(Id, (agent) => {
return new PrintProtocol(agent);
});
}
}

class ServiceRegistrationProtocol : AgentProtocol
{
public class Platform : AgentProtocol
{
public new static string Id
{
get { return "register-services"; }
}
public Platform(Agent agent) : base(agent) { }
public override void ReceiveMessage(AgentMessage msg)
{
Logger.debug("ServiceRegistrationProtocol.ReceiveMessage(AgentMessage)");
Logger.IncLvl();
List<XML.XMLTree> services = XML.ParseXML(msg.Content).GetAllNodes((node) => (node.Type == "service"));
PlatformAgent platform = Holder as PlatformAgent;
if (platform == null)
{
Holder.SendMessage(
msg.MakeResponse(
Holder.Id,
AgentMessage.StatusCodes.UNKNOWNERROR,
"ERROR: Agent is no PlatformAgent - service registration not possible!"
)
);
Stop();
return;
}
foreach (XML.XMLTree service in services)
{
string serviceId = service.GetAttribute("id");
string serviceDesc = service.GetAttribute("description") ?? "";
AgentId sender = msg.Sender;
sender.Platform = platform.Id.Platform;
Logger.log("Register service '" + serviceId + "' of '" + sender.Id + "'.");
if (serviceId != null)
{
if (!platform.PlatformServices.ContainsKey(serviceId))
{
platform.PlatformServices[serviceId] = new List<PlatformService>();
}
platform.PlatformServices[serviceId].Add(new PlatformService(serviceId, serviceDesc, sender));
}
}
Holder.SendMessage(msg.MakeResponse(
Holder.Id,
AgentMessage.StatusCodes.OK,
"services registered"
));
Stop();
Logger.DecLvl();
}
public override void Restart() { }
public static new void RegisterServices(Agent holder)
{
holder.RegisterService(Id, (agent) => {
return new Platform(agent);
});
}
}
public new static string Id
{
get { return "complete-service-registration"; }
}
public ServiceRegistrationProtocol(Agent agent) : base(agent) { }
public override void ReceiveMessage(AgentMessage msg)
{
if (msg.Status == AgentMessage.StatusCodes.OK && msg.Content == "services registered")
{
Logger.log("Setting agent platform to '" + msg.Sender.Name + "'.");
Holder.Id.Platform = msg.Sender.Name;
Stop();
}
else
{
base.ReceiveMessage(msg);
}
}
public override void Restart() { }
public static new void RegisterServices(Agent holder)
{
holder.RegisterService(Id, (agent) => {
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

class MessageRoutingProtocol : AgentProtocol
{
static public bool IsPlatform = false;
public new static string Id
{
get { return "route-message"; }
}
public MessageRoutingProtocol(Agent agent) : base(agent) { }
public override void ReceiveMessage(AgentMessage msg) { }
public override void Restart() { }
public static new void RegisterServices(Agent holder)
{
holder.RegisterService(Id, (agent) => {
return new MessageRoutingProtocol(agent);
});
}
}

class ProvideServiceInformationProtocol : AgentProtocol
{
public new static string Id
{
get { return "get-services"; }
}
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
string content = "<services>";
foreach (PlatformService service in services)
{
content += service.ToXML();
}
content += "</services>";
AgentMessage message = new AgentMessage(
Holder.Id,
msg.Sender,
AgentMessage.StatusCodes.OK,
content,
"response",
ChatId
);
Holder.SendMessage(message);
Stop();
}
else
{
base.ReceiveMessage(msg);
}
}
public override void Restart() { }
public static new void RegisterServices(Agent holder)
{
holder.RegisterService(Id, (agent) => {
return new ProvideServiceInformationProtocol(agent);
});
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
ServiceRegistrationProtocol.RegisterServices(this);
PrintPlatformServicesProtocol.RegisterServices(this);
}
public void RegisterWith(string platformName)
{
AgentId platform = new AgentId(platformName + "@local");
ServiceRegistrationProtocol chat = new ServiceRegistrationProtocol(this);
Chats[chat.ChatId] = chat;
string content = "<services>";
foreach (string service in Services.Keys)
{
content += "<service id='" + service + "'/>";
}
content += "</services>";
AgentMessage message = new AgentMessage(
this.Id,
platform,
AgentMessage.StatusCodes.OK,
content,
ServiceRegistrationProtocol.Platform.Id,
chat.ChatId
);
SendMessage(message);
}
}

class PrintPlatformServicesProtocol : AgentProtocol
{
int State;
public new static string Id
{
get { return "print-platform-services"; }
}
public PrintPlatformServicesProtocol(Agent agent) : base(agent)
{
Logger.log("Create new PrintPlatformServicesProtocol");
State = 0;
}
public override void ReceiveMessage(AgentMessage msg)
{
Logger.log("PrintPlatformServicesProtocol.ReceiveMessage(message)");
switch (State)
{
case 0:
Logger.log("Handling state 0");
if (Holder.Id.Platform == "local")
{
Logger.log("WARNING: PrintPlatformServicesProtocol started, but agent is not registered at any platform!");
Stop();
return;
}
else
{
Holder.SendMessage(new AgentMessage(
Holder.Id,
new AgentId(Holder.Id.Platform + "@local"),
AgentMessage.StatusCodes.OK,
"",
"get-services",
ChatId
));
State = 1;
}
break;
case 1:
Logger.log("Handling state 1");
if (msg.Status == AgentMessage.StatusCodes.OK)
{
List<XML.XMLTree> services = XML.ParseXML(msg.Content).GetAllNodes((node) => node.Type == "service");
Logger.log("Available Platform Services:");
foreach (XML.XMLTree service in services)
{
Logger.log("  " + service.GetAttribute("id"));
}
}
else
{
base.ReceiveMessage(msg);
Logger.log("An error occured in protocol PrintPlatformServicesProtocol: " + msg.Status.ToString());
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
public static new void RegisterServices(Agent holder)
{
holder.RegisterService(Id, (agent) => {
return new PrintPlatformServicesProtocol(agent);
});
}
}

}

class DataStorage : XML.DataStore
{
private Dictionary<string, Type> String2Type;
private Dictionary<Type, string> Type2String;
private Dictionary<string, string> StringEntries;
private Dictionary<string, int> IntegerEntries;
private Dictionary<string, float> FloatEntries;
private static DataStorage Instance;
private DataStorage() : base()
{
Logger.log("DataStore constructor()");
Logger.IncLvl();
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
Logger.DecLvl();
}
public static DataStorage GetInstance()
{
Logger.log("DataStore.GetInstance()");
Logger.IncLvl();
if (Instance == null)
{
Instance = new DataStorage();
}
Logger.DecLvl();
return Instance;
}
public void Save(out string data)
{
Logger.log("DataStore.Save(string)");
Logger.IncLvl();
UpdateAttributes();
DataStorage.SetUp();
data = "<data " + Parser.PackData(GetValues((node) => true)) + "/>";
Logger.DecLvl();
}
private void UpdateAttributes()
{
Logger.log("DataStore.UpdateAttributes()");
Logger.IncLvl();
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
Logger.DecLvl();
}
public static DataStorage Load(string Storage)
{
Logger.log("DataStore.Load()");
Logger.IncLvl();
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
Logger.DecLvl();
return DataStorage.GetInstance();
}
public void Set<T>(string key, T value)
{
Logger.log("DataStore.Set<T>(string, T)");
Logger.IncLvl();
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
Logger.DecLvl();
}
public Type GetEntryType(string key)
{
Logger.log("DataStore.GetEntryType(string)");
Logger.IncLvl();
if (StringEntries.ContainsKey(key))
{
Logger.DecLvl();
return typeof(string);
}
else if (IntegerEntries.ContainsKey(key))
{
Logger.DecLvl();
return typeof(int);
}
else if (FloatEntries.ContainsKey(key))
{
Logger.DecLvl();
return typeof(float);
}
else
{
Logger.DecLvl();
return null;
}
}
public T Get<T>(string key)
{
Logger.log("DataStore.Get(string)");
Logger.IncLvl();
if (!Exists<T>(key))
{
throw new Exception("No entry found for key '" + key + "' of type '" + typeof(T).ToString() + "'!");
}
if (typeof(T) == typeof(string))
{
Logger.DecLvl();
return (T)(object)StringEntries[key];
}
else if (typeof(T) == typeof(int))
{
Logger.DecLvl();
return (T)(object)IntegerEntries[key];
}
else if (typeof(T) == typeof(float))
{
Logger.DecLvl();
return (T)(object)FloatEntries[key];
}
else
{
throw new Exception("Error: Invalid Type at DataStore.Get<Type>(string key)!");
}
}
public bool Exists(string key)
{
Logger.log("DataStore.Exists(string)");
return (Exists<string>(key) || Exists<int>(key) || Exists<float>(key));
}
public bool Exists<T>(string key)
{
Logger.log("Exists<T>(string)");
Logger.IncLvl();
if (typeof(T) == typeof(string))
{
Logger.DecLvl();
return StringEntries.ContainsKey(key);
}
else if (typeof(T) == typeof(int))
{
Logger.DecLvl();
return IntegerEntries.ContainsKey(key);
}
else if (typeof(T) == typeof(float))
{
Logger.DecLvl();
return FloatEntries.ContainsKey(key);
}
Logger.DecLvl();
return false;
}
public List<string> GetKeys()
{
Logger.log("DataStore.GetKeys()");
Logger.IncLvl();
List<string> keys = new List<string>(
StringEntries.Keys.Count +
IntegerEntries.Keys.Count +
FloatEntries.Keys.Count
);
keys.AddRange(StringEntries.Keys);
keys.AddRange(IntegerEntries.Keys);
keys.AddRange(FloatEntries.Keys);
Logger.DecLvl();
return keys;
}
public static void SetUp()
{
Logger.log("DataStore.SetUp()");
Logger.IncLvl();
if (!XML.NodeRegister.ContainsKey("data"))
{
XML.NodeRegister.Add("data", () => DataStorage.GetInstance());
}
Logger.DecLvl();
}
public override void SetAttribute(string key, string value)
{
Logger.log("DataStore.SetAttribute(string, string)");
Logger.IncLvl();
if (StringEntries == null || IntegerEntries == null || FloatEntries == null)
{
base.SetAttribute(key, value);
Logger.DecLvl();
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
Logger.DecLvl();
}
}


public static class Logger
{
static IMyTextPanel DebugPanel;
static public bool DEBUG = false;
static int offset = 0;
public static void log(string msg)
{
if (DebugPanel == null)
{
return;
}
string prefix = "";
for (int i = 0; i < offset; i++)
{
prefix += "  ";
}
DebugPanel.WritePublicText(prefix + msg + "\n", true);
//P.Echo(prefix + msg);
}
public static void debug(string msg)
{
if (!DEBUG)
{
return;
}
log(msg);
}
public static void IncLvl()
{
offset += 2;
}
public static void DecLvl()
{
offset = offset - 2;
}
}

public static class Parser
{
public static string PackData(Dictionary<string, string> data)
{
string dataString = "";
foreach (string key in data.Keys)
{
dataString += key + "=\"" + data[key] + "\" ";
}
return dataString;
}
public static string Sanitize(string xmlDefinition)
{
return xmlDefinition.Replace("\"", "\\\"").Replace("'", "\\'");
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
Logger.debug("GetNextUnescaped():");
Logger.IncLvl();
int end = start + count - 1;
int needlePos = haystack.IndexOfAny(needles, start, end - start + 1);
while (needlePos > 0 && haystack[needlePos - 1] == '\\')
{
needlePos = haystack.IndexOfAny(needles, needlePos + 1, end - needlePos);
}
Logger.DecLvl();
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
Logger.debug("GetNextOutsideQuotes():");
Logger.IncLvl();
char[] quoteChars = new char[] { '\'', '"' };
int needlePos = -1;
int quoteEnd = -1;
int quoteStart;
Logger.debug("needle: |" + new string(needles) + "|");
Logger.debug("haystack: |" + haystack + "|");
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
Logger.debug("quoteStart position: " + quoteStart.ToString()
+ ", quoteEnd position: " + quoteEnd.ToString());
if (quoteStart == -1)
{
Logger.debug("searching for needle in:: " + haystack.Substring(quoteEnd + 1));
needlePos = GetNextUnescaped(needles, haystack, quoteEnd + 1);
}
else
{
Logger.debug("found start quote: " + haystack.Substring(quoteStart));
Logger.debug("searching for needle in: "
+ haystack.Substring(quoteEnd + 1, quoteStart - quoteEnd));
needlePos = GetNextUnescaped(
needles,
haystack,
quoteEnd + 1,
quoteStart - quoteEnd - 1
);
if (needlePos != -1)
{
Logger.debug("found needle: " + haystack.Substring(needlePos));
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
Logger.debug("found end quote: " + haystack.Substring(quoteEnd));
}
}
Logger.debug("yay!");
Logger.DecLvl();
return needlePos;
}
public static List<String> ParamString2List(string arg)
{
Logger.debug("Parser.ParamString2List()");
Logger.IncLvl();
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
Logger.DecLvl();
return argList;
}
public static Dictionary<string, string> GetXMLAttributes(string attributeString)
{
Logger.debug("GetXMLAttributes():");
Logger.IncLvl();
Logger.debug("attribute string is: <" + attributeString + ">");
Dictionary<string, string> attributes = new Dictionary<string, string>();
char[] quoteChars = new char[] { '\'', '"' };
List<string> attributeList = ParamString2List(attributeString);
int equalSign;
foreach (string attribute in attributeList)
{
equalSign = attribute.IndexOf('=');
if (equalSign == -1)
{
attributes[attribute.Substring(0).ToLower()] = "true";
}
else
{
attributes[attribute.Substring(0, equalSign).ToLower()] =
attribute.Substring(equalSign + 1).Trim(quoteChars);
}
}
Logger.debug("attribute dict: {");
Logger.IncLvl();
foreach (string key in attributes.Keys)
{
Logger.debug(key + ": " + attributes[key]);
}
Logger.debug("}");
Logger.DecLvl();
Logger.DecLvl();
return attributes;
}
}

public static class TextUtils
{
public enum FONT { DEFAULT, MONOSPACE };
public static bool DEBUG = true;
private static FONT selectedFont = FONT.DEFAULT;
private static Dictionary<FONT, Dictionary<char, int>> LetterWidths = new Dictionary<FONT, Dictionary<char, int>>{
{
FONT.DEFAULT,
new Dictionary<char, int> {
{' ', 8 }, { '!', 8 }, { '"', 10}, {'#', 19}, {'$', 20}, {'%', 24}, {'&', 20}, {'\'', 6}, {'(', 9}, {')', 9}, {'*', 11}, {'+', 18}, {',', 9}, {'-', 10}, {'.', 9}, {'/', 14}, {'0', 19}, {'1', 9}, {'2', 19}, {'3', 17}, {'4', 19}, {'5', 19}, {'6', 19}, {'7', 16}, {'8', 19}, {'9', 19}, {':', 9}, {';', 9}, {'<', 18}, {'=', 18}, {'>', 18}, {'?', 16}, {'@', 25}, {'A', 21}, {'B', 21}, {'C', 19}, {'D', 21}, {'E', 18}, {'F', 17}, {'G', 20}, {'H', 20}, {'I', 8}, {'J', 16}, {'K', 17}, {'L', 15}, {'M', 26}, {'N', 21}, {'O', 21}, {'P', 20}, {'Q', 21}, {'R', 21}, {'S', 21}, {'T', 17}, {'U', 20}, {'V', 20}, {'W', 31}, {'X', 19}, {'Y', 20}, {'Z', 19}, {'[', 9}, {'\\', 12}, {']', 9}, {'^', 18}, {'_', 15}, {'`', 8}, {'a', 17}, {'b', 17}, {'c', 16}, {'d', 17}, {'e', 17}, {'f', 9}, {'g', 17}, {'h', 17}, {'i', 8}, {'j', 8}, {'k', 17}, {'l', 8}, {'m', 27}, {'n', 17}, {'o', 17}, {'p', 17}, {'q', 17}, {'r', 10}, {'s', 17}, {'t', 9}, {'u', 17}, {'v', 15}, {'w', 27}, {'x', 15}, {'y', 17}, {'z', 16}, {'{', 9}, {'|', 6}, {'}', 9}, {'~', 18}, {' ', 8}, {'¡', 8}, {'¢', 16}, {'£', 17}, {'¤', 19}, {'¥', 19}, {'¦', 6}, {'§', 20}, {'¨', 8}, {'©', 25}, {'ª', 10}, {'«', 15}, {'¬', 18}, {'­', 10}, {'®', 25}, {'¯', 8}, {'°', 12}, {'±', 18}, {'²', 11}, {'³', 11}, {'´', 8}, {'µ', 17}, {'¶', 18}, {'·', 9}, {'¸', 8}, {'¹', 11}, {'º', 10}, {'»', 15}, {'¼', 27}, {'½', 29}, {'¾', 28}, {'¿', 16}, {'À', 21}, {'Á', 21}, {'Â', 21}, {'Ã', 21}, {'Ä', 21}, {'Å', 21}, {'Æ', 31}, {'Ç', 19}, {'È', 18}, {'É', 18}, {'Ê', 18}, {'Ë', 18}, {'Ì', 8}, {'Í', 8}, {'Î', 8}, {'Ï', 8}, {'Ð', 21}, {'Ñ', 21}, {'Ò', 21}, {'Ó', 21}, {'Ô', 21}, {'Õ', 21}, {'Ö', 21}, {'×', 18}, {'Ø', 21}, {'Ù', 20}, {'Ú', 20}, {'Û', 20}, {'Ü', 20}, {'Ý', 17}, {'Þ', 20}, {'ß', 19}, {'à', 17}, {'á', 17}, {'â', 17}, {'ã', 17}, {'ä', 17}, {'å', 17}, {'æ', 28}, {'ç', 16}, {'è', 17}, {'é', 17}, {'ê', 17}, {'ë', 17}, {'ì', 8}, {'í', 8}, {'î', 8}, {'ï', 8}, {'ð', 17}, {'ñ', 17}, {'ò', 17}, {'ó', 17}, {'ô', 17}, {'õ', 17}, {'ö', 17}, {'÷', 18}, {'ø', 17}, {'ù', 17}, {'ú', 17}, {'û', 17}, {'ü', 17}, {'ý', 17}, {'þ', 17}, {'ÿ', 17}, {'Ā', 20}, {'ā', 17}, {'Ă', 21}, {'ă', 17}, {'Ą', 21}, {'ą', 17}, {'Ć', 19}, {'ć', 16}, {'Ĉ', 19}, {'ĉ', 16}, {'Ċ', 19}, {'ċ', 16}, {'Č', 19}, {'č', 16}, {'Ď', 21}, {'ď', 17}, {'Đ', 21}, {'đ', 17}, {'Ē', 18}, {'ē', 17}, {'Ĕ', 18}, {'ĕ', 17}, {'Ė', 18}, {'ė', 17}, {'Ę', 18}, {'ę', 17}, {'Ě', 18}, {'ě', 17}, {'Ĝ', 20}, {'ĝ', 17}, {'Ğ', 20}, {'ğ', 17}, {'Ġ', 20}, {'ġ', 17}, {'Ģ', 20}, {'ģ', 17}, {'Ĥ', 20}, {'ĥ', 17}, {'Ħ', 20}, {'ħ', 17}, {'Ĩ', 8}, {'ĩ', 8}, {'Ī', 8}, {'ī', 8}, {'Į', 8}, {'į', 8}, {'İ', 8}, {'ı', 8}, {'Ĳ', 24}, {'ĳ', 14}, {'Ĵ', 16}, {'ĵ', 8}, {'Ķ', 17}, {'ķ', 17}, {'Ĺ', 15}, {'ĺ', 8}, {'Ļ', 15}, {'ļ', 8}, {'Ľ', 15}, {'ľ', 8}, {'Ŀ', 15}, {'ŀ', 10}, {'Ł', 15}, {'ł', 8}, {'Ń', 21}, {'ń', 17}, {'Ņ', 21}, {'ņ', 17}, {'Ň', 21}, {'ň', 17}, {'ŉ', 17}, {'Ō', 21}, {'ō', 17}, {'Ŏ', 21}, {'ŏ', 17}, {'Ő', 21}, {'ő', 17}, {'Œ', 31}, {'œ', 28}, {'Ŕ', 21}, {'ŕ', 10}, {'Ŗ', 21}, {'ŗ', 10}, {'Ř', 21}, {'ř', 10}, {'Ś', 21}, {'ś', 17}, {'Ŝ', 21}, {'ŝ', 17}, {'Ş', 21}, {'ş', 17}, {'Š', 21}, {'š', 17}, {'Ţ', 17}, {'ţ', 9}, {'Ť', 17}, {'ť', 9}, {'Ŧ', 17}, {'ŧ', 9}, {'Ũ', 20}, {'ũ', 17}, {'Ū', 20}, {'ū', 17}, {'Ŭ', 20}, {'ŭ', 17}, {'Ů', 20}, {'ů', 17}, {'Ű', 20}, {'ű', 17}, {'Ų', 20}, {'ų', 17}, {'Ŵ', 31}, {'ŵ', 27}, {'Ŷ', 17}, {'ŷ', 17}, {'Ÿ', 17}, {'Ź', 19}, {'ź', 16}, {'Ż', 19}, {'ż', 16}, {'Ž', 19}, {'ž', 16}, {'ƒ', 19}, {'Ș', 21}, {'ș', 17}, {'Ț', 17}, {'ț', 9}, {'ˆ', 8}, {'ˇ', 8}, {'ˉ', 6}, {'˘', 8}, {'˙', 8}, {'˚', 8}, {'˛', 8}, {'˜', 8}, {'˝', 8}, {'Ё', 19}, {'Ѓ', 16}, {'Є', 18}, {'Ѕ', 21}, {'І', 8}, {'Ї', 8}, {'Ј', 16}, {'Љ', 28}, {'Њ', 21}, {'Ќ', 19}, {'Ў', 17}, {'Џ', 18}, {'А', 19}, {'Б', 19}, {'В', 19}, {'Г', 15}, {'Д', 19}, {'Е', 18}, {'Ж', 21}, {'З', 17}, {'И', 19}, {'Й', 19}, {'К', 17}, {'Л', 17}, {'М', 26}, {'Н', 18}, {'О', 20}, {'П', 19}, {'Р', 19}, {'С', 19}, {'Т', 19}, {'У', 19}, {'Ф', 20}, {'Х', 19}, {'Ц', 20}, {'Ч', 16}, {'Ш', 26}, {'Щ', 29}, {'Ъ', 20}, {'Ы', 24}, {'Ь', 19}, {'Э', 18}, {'Ю', 27}, {'Я', 20}, {'а', 16}, {'б', 17}, {'в', 16}, {'г', 15}, {'д', 17}, {'е', 17}, {'ж', 20}, {'з', 15}, {'и', 16}, {'й', 16}, {'к', 17}, {'л', 15}, {'м', 25}, {'н', 16}, {'о', 16}, {'п', 16}, {'р', 17}, {'с', 16}, {'т', 14}, {'у', 17}, {'ф', 21}, {'х', 15}, {'ц', 17}, {'ч', 15}, {'ш', 25}, {'щ', 27}, {'ъ', 16}, {'ы', 20}, {'ь', 16}, {'э', 14}, {'ю', 23}, {'я', 17}, {'ё', 17}, {'ђ', 17}, {'ѓ', 16}, {'є', 14}, {'ѕ', 16}, {'і', 8}, {'ї', 8}, {'ј', 7}, {'љ', 22}, {'њ', 25}, {'ћ', 17}, {'ќ', 16}, {'ў', 17}, {'џ', 17}, {'Ґ', 15}, {'ґ', 13}, {'–', 15}, {'—', 31}, {'‘', 6}, {'’', 6}, {'‚', 6}, {'“', 12}, {'”', 12}, {'„', 12}, {'†', 20}, {'‡', 20}, {'•', 15}, {'…', 31}, {'‰', 31}, {'‹', 8}, {'›', 8}, {'€', 19}, {'™', 30}, {'−', 18}, {'∙', 8}, {'□', 21}, {'', 40}, {'', 40}, {'', 40}, {'', 40}, {'', 41}, {'', 41}, {'', 32}, {'', 32}, {'', 40}, {'', 40}, {'', 34}, {'', 34}, {'', 40}, {'', 40}, {'', 40}, {'', 41}, {'', 32}, {'', 41}, {'', 32}, {'', 40}, {'', 40}, {'', 40}, {'', 40}, {'', 40}, {'', 40}, {'', 40}, {'', 40}
}
},
{
FONT.MONOSPACE,
new Dictionary<char, int> {
{' ', 24 }, { '!', 24 }, { '"', 24}, {'#', 24}, {'$', 24}, {'%', 24}, {'&', 24}, {'\'', 24}, {'(', 24}, {')', 24}, {'*', 24}, {'+', 24}, {',', 24}, {'-', 24}, {'.', 24}, {'/', 24}, {'0', 24}, {'1', 24}, {'2', 24}, {'3', 24}, {'4', 24}, {'5', 24}, {'6', 24}, {'7', 24}, {'8', 24}, {'9', 24}, {':', 24}, {';', 24}, {'<', 24}, {'=', 24}, {'>', 24}, {'?', 24}, {'@', 24}, {'A', 24}, {'B', 24}, {'C', 24}, {'D', 24}, {'E', 24}, {'F', 24}, {'G', 24}, {'H', 24}, {'I', 24}, {'J', 24}, {'K', 24}, {'L', 24}, {'M', 24}, {'N', 24}, {'O', 24}, {'P', 24}, {'Q', 24}, {'R', 24}, {'S', 24}, {'T', 24}, {'U', 24}, {'V', 24}, {'W', 24}, {'X', 24}, {'Y', 24}, {'Z', 24}, {'[', 24}, {'\\', 24}, {']', 24}, {'^', 24}, {'_', 24}, {'`', 24}, {'a', 24}, {'b', 24}, {'c', 24}, {'d', 24}, {'e', 24}, {'f', 24}, {'g', 24}, {'h', 24}, {'i', 24}, {'j', 24}, {'k', 24}, {'l', 24}, {'m', 24}, {'n', 24}, {'o', 24}, {'p', 24}, {'q', 24}, {'r', 24}, {'s', 24}, {'t', 24}, {'u', 24}, {'v', 24}, {'w', 24}, {'x', 24}, {'y', 24}, {'z', 24}, {'{', 24}, {'|', 24}, {'}', 24}, {'~', 24}, {' ', 24}, {'¡', 24}, {'¢', 24}, {'£', 24}, {'¤', 24}, {'¥', 24}, {'¦', 24}, {'§', 24}, {'¨', 24}, {'©', 24}, {'ª', 24}, {'«', 24}, {'¬', 24}, {'­', 24}, {'®', 24}, {'¯', 24}, {'°', 24}, {'±', 24}, {'²', 24}, {'³', 24}, {'´', 24}, {'µ', 24}, {'¶', 24}, {'·', 24}, {'¸', 24}, {'¹', 24}, {'º', 24}, {'»', 24}, {'¼', 24}, {'½', 24}, {'¾', 24}, {'¿', 24}, {'À', 24}, {'Á', 24}, {'Â', 24}, {'Ã', 24}, {'Ä', 24}, {'Å', 24}, {'Æ', 24}, {'Ç', 24}, {'È', 24}, {'É', 24}, {'Ê', 24}, {'Ë', 24}, {'Ì', 24}, {'Í', 24}, {'Î', 24}, {'Ï', 24}, {'Ð', 24}, {'Ñ', 24}, {'Ò', 24}, {'Ó', 24}, {'Ô', 24}, {'Õ', 24}, {'Ö', 24}, {'×', 24}, {'Ø', 24}, {'Ù', 24}, {'Ú', 24}, {'Û', 24}, {'Ü', 24}, {'Ý', 24}, {'Þ', 24}, {'ß', 24}, {'à', 24}, {'á', 24}, {'â', 24}, {'ã', 24}, {'ä', 24}, {'å', 24}, {'æ', 24}, {'ç', 24}, {'è', 24}, {'é', 24}, {'ê', 24}, {'ë', 24}, {'ì', 24}, {'í', 24}, {'î', 24}, {'ï', 24}, {'ð', 24}, {'ñ', 24}, {'ò', 24}, {'ó', 24}, {'ô', 24}, {'õ', 24}, {'ö', 24}, {'÷', 24}, {'ø', 24}, {'ù', 24}, {'ú', 24}, {'û', 24}, {'ü', 24}, {'ý', 24}, {'þ', 24}, {'ÿ', 24}, {'Ā', 24}, {'ā', 24}, {'Ă', 24}, {'ă', 24}, {'Ą', 24}, {'ą', 24}, {'Ć', 24}, {'ć', 24}, {'Ĉ', 24}, {'ĉ', 24}, {'Ċ', 24}, {'ċ', 24}, {'Č', 24}, {'č', 24}, {'Ď', 24}, {'ď', 24}, {'Đ', 24}, {'đ', 24}, {'Ē', 24}, {'ē', 24}, {'Ĕ', 24}, {'ĕ', 24}, {'Ė', 24}, {'ė', 24}, {'Ę', 24}, {'ę', 24}, {'Ě', 24}, {'ě', 24}, {'Ĝ', 24}, {'ĝ', 24}, {'Ğ', 24}, {'ğ', 24}, {'Ġ', 24}, {'ġ', 24}, {'Ģ', 24}, {'ģ', 24}, {'Ĥ', 24}, {'ĥ', 24}, {'Ħ', 24}, {'ħ', 24}, {'Ĩ', 24}, {'ĩ', 24}, {'Ī', 24}, {'ī', 24}, {'Į', 24}, {'į', 24}, {'İ', 24}, {'ı', 24}, {'Ĳ', 24}, {'ĳ', 24}, {'Ĵ', 24}, {'ĵ', 24}, {'Ķ', 24}, {'ķ', 24}, {'Ĺ', 24}, {'ĺ', 24}, {'Ļ', 24}, {'ļ', 24}, {'Ľ', 24}, {'ľ', 24}, {'Ŀ', 24}, {'ŀ', 24}, {'Ł', 24}, {'ł', 24}, {'Ń', 24}, {'ń', 24}, {'Ņ', 24}, {'ņ', 24}, {'Ň', 24}, {'ň', 24}, {'ŉ', 24}, {'Ō', 24}, {'ō', 24}, {'Ŏ', 24}, {'ŏ', 24}, {'Ő', 24}, {'ő', 24}, {'Œ', 24}, {'œ', 24}, {'Ŕ', 24}, {'ŕ', 24}, {'Ŗ', 24}, {'ŗ', 24}, {'Ř', 24}, {'ř', 24}, {'Ś', 24}, {'ś', 24}, {'Ŝ', 24}, {'ŝ', 24}, {'Ş', 24}, {'ş', 24}, {'Š', 24}, {'š', 24}, {'Ţ', 24}, {'ţ', 24}, {'Ť', 24}, {'ť', 24}, {'Ŧ', 24}, {'ŧ', 24}, {'Ũ', 24}, {'ũ', 24}, {'Ū', 24}, {'ū', 24}, {'Ŭ', 24}, {'ŭ', 24}, {'Ů', 24}, {'ů', 24}, {'Ű', 24}, {'ű', 24}, {'Ų', 24}, {'ų', 24}, {'Ŵ', 24}, {'ŵ', 24}, {'Ŷ', 24}, {'ŷ', 24}, {'Ÿ', 24}, {'Ź', 24}, {'ź', 24}, {'Ż', 24}, {'ż', 24}, {'Ž', 24}, {'ž', 24}, {'ƒ', 24}, {'Ș', 24}, {'ș', 24}, {'Ț', 24}, {'ț', 24}, {'ˆ', 24}, {'ˇ', 24}, {'ˉ', 24}, {'˘', 24}, {'˙', 24}, {'˚', 24}, {'˛', 24}, {'˜', 24}, {'˝', 24}, {'Ё', 24}, {'Ѓ', 24}, {'Є', 24}, {'Ѕ', 24}, {'І', 24}, {'Ї', 24}, {'Ј', 24}, {'Љ', 24}, {'Њ', 24}, {'Ќ', 24}, {'Ў', 24}, {'Џ', 24}, {'А', 24}, {'Б', 24}, {'В', 24}, {'Г', 24}, {'Д', 24}, {'Е', 24}, {'Ж', 24}, {'З', 24}, {'И', 24}, {'Й', 24}, {'К', 24}, {'Л', 24}, {'М', 24}, {'Н', 24}, {'О', 24}, {'П', 24}, {'Р', 24}, {'С', 24}, {'Т', 24}, {'У', 24}, {'Ф', 24}, {'Х', 24}, {'Ц', 24}, {'Ч', 24}, {'Ш', 24}, {'Щ', 24}, {'Ъ', 24}, {'Ы', 24}, {'Ь', 24}, {'Э', 24}, {'Ю', 24}, {'Я', 24}, {'а', 24}, {'б', 24}, {'в', 24}, {'г', 24}, {'д', 24}, {'е', 24}, {'ж', 24}, {'з', 24}, {'и', 24}, {'й', 24}, {'к', 24}, {'л', 24}, {'м', 24}, {'н', 24}, {'о', 24}, {'п', 24}, {'р', 24}, {'с', 24}, {'т', 24}, {'у', 24}, {'ф', 24}, {'х', 24}, {'ц', 24}, {'ч', 24}, {'ш', 24}, {'щ', 24}, {'ъ', 24}, {'ы', 24}, {'ь', 24}, {'э', 24}, {'ю', 24}, {'я', 24}, {'ё', 24}, {'ђ', 24}, {'ѓ', 24}, {'є', 24}, {'ѕ', 24}, {'і', 24}, {'ї', 24}, {'ј', 24}, {'љ', 24}, {'њ', 24}, {'ћ', 24}, {'ќ', 24}, {'ў', 24}, {'џ', 24}, {'Ґ', 24}, {'ґ', 24}, {'–', 24}, {'—', 24}, {'‘', 24}, {'’', 24}, {'‚', 24}, {'“', 24}, {'”', 24}, {'„', 24}, {'†', 24}, {'‡', 24}, {'•', 24}, {'…', 24}, {'‰', 24}, {'‹', 24}, {'›', 24}, {'€', 24}, {'™', 24}, {'−', 24}, {'∙', 24}, {'□', 24}, {'', 24}, {'', 24}, {'', 24}, {'', 24}, {'', 24}, {'', 24}, {'', 24}, {'', 24}, {'', 24}, {'', 24}, {'', 24}, {'', 24}, {'', 24}, {'', 24}, {'', 24}, {'', 24}, {'', 24}, {'', 24}, {'', 24}, {'', 24}, {'', 24}, {'', 24}, {'', 24}, {'', 24}, {'', 24}, {'', 24}, {'', 24}
}
}
};
public enum PadMode { LEFT, RIGHT };
public enum RoundMode { FLOOR, CEIL };
public static void SelectFont(FONT f)
{
selectedFont = f;
}
public static int GetTextWidth(string text)
{
if (DEBUG)
{
Logger.debug("TextUtils.GetTextWidth()");
}
Logger.IncLvl();
int width = 0;
text = text.Replace("\r", "");
string[] lines = text.Split('\n');
foreach (string line in lines)
{
width = Math.Max(width, GetLineWidth(line.ToCharArray()));
}
Logger.DecLvl();
return width;
}
private static int GetLineWidth(char[] line)
{
if (DEBUG)
{
Logger.debug("TextUtils.GetLineWidth()");
}
Logger.IncLvl();
int width = 0;
if (line.Length == 0)
{
return width;
}
foreach (char c in line)
{
//Logger.debug("adding character width of '" + c.ToString() + "'");
width += LetterWidths[selectedFont][c] + 1;
}
Logger.DecLvl();
return width - 1;
}
public static string RemoveLastTrailingNewline(string text)
{
if (DEBUG)
{
Logger.debug("TextUtils.RemoveLastTrailingNewline");
}
Logger.IncLvl();
Logger.DecLvl();
return (text.Length > 1 && text[text.Length - 1] == '\n') ? text.Remove(text.Length - 1) : text;
}
public static string RemoveFirstTrailingNewline(string text)
{
if (DEBUG)
Logger.debug("TextUtils.RemoveFirstTrailingNewline");
Logger.IncLvl();
Logger.DecLvl();
return (text.Length > 1 && text[0] == '\n') ? text.Remove(0) : text;
}
public static string CenterText(string text, int totalWidth)
{
if (DEBUG)
Logger.debug("TextUtils.CenterText()");
Logger.IncLvl();
if (DEBUG)
{
Logger.debug("text is " + text);
Logger.debug("width is " + totalWidth.ToString());
}
string result = "";
string[] lines = text.Split('\n');
int width;
foreach (string line in lines)
{
width = GetLineWidth(line.ToCharArray());
result += CreateStringOfLength(" ", (totalWidth - width) / 2) + line + CreateStringOfLength(" ", (totalWidth - width) / 2) + "\n";
}
result = RemoveLastTrailingNewline(result);
Logger.DecLvl();
return result;
}
public static string CreateStringOfLength(string constituent, int length)
{
return CreateStringOfLength(constituent, length, RoundMode.FLOOR);
}
public static string CreateStringOfLength(string constituent, int length, RoundMode mode)
{
if (DEBUG)
Logger.debug("TextUtils.CreateStringOfLength()");
Logger.IncLvl();
int lengthOfConstituent = GetLineWidth(constituent.ToCharArray());
if (mode == RoundMode.CEIL)
{
length += lengthOfConstituent;
}
//return new String(constituent, constituentMultiplier);
string result = "";
if (length < lengthOfConstituent)
{
Logger.DecLvl();
return "";
}
for (int i = -1; i < length; i = i + lengthOfConstituent + 1)
{
result += constituent;
}
Logger.DecLvl();
return result;
}
public static string PadString(string line, int totalWidth, PadMode mode, string padString)
{
if (DEBUG)
Logger.debug("TextUtils.PadString()");
Logger.IncLvl();
if (mode == PadMode.LEFT)
{
Logger.DecLvl();
return CreateStringOfLength(padString, totalWidth - GetLineWidth(line.ToCharArray())) + line;
}
else if (mode == PadMode.RIGHT)
{
Logger.DecLvl();
return line + CreateStringOfLength(padString, totalWidth - GetLineWidth(line.ToCharArray()));
}
Logger.DecLvl();
return line;
}
public static string PadText(string text, int totalWidth, PadMode mode)
{
return PadText(text, totalWidth, mode, " ");
}
public static string PadText(string text, int totalWidth, PadMode mode, string padString)
{
if (DEBUG)
Logger.debug("TextUtils.PadText()");
Logger.IncLvl();
string[] lines = text.Split('\n');
string result = "";
foreach (string line in lines)
{
result += PadString(line, totalWidth, mode, padString) + "\n";
}
Logger.DecLvl();
return result.Trim(new char[] { '\n' });
}
}

public static class XML
{
public static Dictionary<string, Func<XML.XMLTree>> NodeRegister = new Dictionary<string, Func<XML.XMLTree>> {
{"root", () => { return new XML.RootNode(); } },
{"menu", () => { return new XML.Menu(); } },
{"menuitem", () => { return new XML.MenuItem(); } },
{"progressbar", () => { return new XML.ProgressBar(); } },
{"container", () => { return new XML.Container(); } },
{"hl", () => { return new XML.HorizontalLine(); } },
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
Logger.debug("XML.CreateNode()");
Logger.IncLvl();
type = type.ToLower();
if (NodeRegister.ContainsKey(type))
{
Logger.DecLvl();
return NodeRegister[type]();
}
else
{
Logger.DecLvl();
return new Generic(type);
}
}
public static XMLTree ParseXML(string xmlString)
{
char[] spaceChars = { ' ', '\n' };
Logger.debug("ParseXML");
Logger.IncLvl();
RootNode root = new RootNode();
XMLTree currentNode = root;
string type;
Logger.debug("Enter while loop");
while (xmlString.Length > 0)
{
if (xmlString[0] == '<')
{
Logger.debug("found tag");
if (xmlString[1] == '/')
{
Logger.debug("tag is end tag");
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
Logger.debug("tag is start tag");
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
Logger.debug("add new node of type '" + newNode.Type + "=" + type + "' to current node");
currentNode.AddChild(newNode);
Logger.debug("added new node to current node");
if (spacePos != -1 && spacePos < bracketPos)
{
string attrString = xmlString.Substring(typeLength + 2, bracketPos - typeLength - 2);
attrString = attrString.TrimEnd(new char[] { '/' });
Logger.debug("get xml attributes. attribute string: '" + attrString + "/" + xmlString + "'");
Dictionary<string, string> attributes =
Parser.GetXMLAttributes(attrString);
Logger.debug("got xml attributes");
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
if (newNode.Render(0) != null)
{
currentNode.AddChild(newNode);
}
xmlString = bracketPos == -1 ? "" : xmlString.Substring(bracketPos);
}
}
Logger.debug("parsing finished");
Logger.DecLvl();
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
public XMLTree()
{
PreventDefaults = new List<string>();
Parent = null;
Children = new List<XMLTree>();
Selectable = false;
ChildrenAreSelectable = false;
Selected = false;
SelectedChild = -1;
Activated = false;
Attributes = new Dictionary<string, string>();
Type = "NULL";
// set attribute defaults
SetAttribute("alignself", "left");
SetAttribute("aligntext", "left");
SetAttribute("selected", "false");
SetAttribute("selectable", "false");
SetAttribute("flowdirection", "vertical");
}
public bool IsSelectable()
{
Logger.debug(Type + ": IsSelectable():");
Logger.IncLvl();
Logger.DecLvl();
return Selectable || ChildrenAreSelectable;
}
public bool IsSelected()
{
Logger.debug(Type + ": IsSelected():");
Logger.IncLvl();
Logger.DecLvl();
return Selected;
}
public XMLTree GetSelectedSibling()
{
Logger.debug(Type + ": GetSelectedSibling():");
Logger.IncLvl();
if (!Selected)
{
Logger.DecLvl();
return null;
//throw new Exception(
//    "Node is not selected. You can only get the selected Node from one of it's parent nodes!");
}
if (SelectedChild == -1)
{
Logger.DecLvl();
return this;
}
else
{
Logger.DecLvl();
return Children[SelectedChild].GetSelectedSibling();
}
}
public virtual void AddChild(XMLTree child)
{
Logger.debug(Type + ": AddChild():");
Logger.IncLvl();
Children.Add(child);
child.SetParent(this as XMLParentNode);
UpdateSelectability(child);
Logger.DecLvl();
}
public void SetParent(XMLParentNode parent)
{
Logger.debug(Type + ": SetParent():");
Logger.IncLvl();
Parent = parent;
Logger.DecLvl();
}
public XMLParentNode GetParent()
{
Logger.debug(Type + ": GetParent():");
Logger.IncLvl();
Logger.DecLvl();
return Parent;
}
public XMLTree GetChild(int i)
{
Logger.debug(Type + ": GetChild():");
Logger.IncLvl();
Logger.DecLvl();
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
Logger.debug(Type + ": UpdateSelectability():");
Logger.IncLvl();
bool ChildrenWereSelectable = ChildrenAreSelectable;
ChildrenAreSelectable = ChildrenAreSelectable || child.IsSelectable();
if (Parent != null && (Selectable || ChildrenAreSelectable) != (Selectable || ChildrenWereSelectable))
{
Parent.UpdateSelectability(this);
}
Logger.DecLvl();
}
public bool SelectFirst()
{
Logger.debug(Type + ": SelectFirst():");
Logger.IncLvl();
if (SelectedChild != -1)
{
Children[SelectedChild].Unselect();
}
SelectedChild = -1;
bool success = (Selectable || ChildrenAreSelectable) ? SelectNext() : false;
Logger.DecLvl();
return success;
}
public bool SelectLast()
{
Logger.debug(Type + ": SelectLast():");
Logger.IncLvl();
if (SelectedChild != -1)
{
Children[SelectedChild].Unselect();
}
SelectedChild = -1;
Logger.DecLvl();
return (Selectable || ChildrenAreSelectable) ? SelectPrevious() : false;
}
public void Unselect()
{
Logger.debug(Type + ": Unselect():");
Logger.IncLvl();
if (SelectedChild != -1)
{
Children[SelectedChild].Unselect();
}
Selected = false;
Activated = false;
Logger.DecLvl();
}
public virtual bool SelectNext()
{
Logger.debug(Type + ": SelectNext():");
Logger.IncLvl();
bool WasSelected = IsSelected();
if (SelectedChild == -1 || !Children[SelectedChild].SelectNext())
{
Logger.debug(Type + ": find next child to select...");
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
}
Logger.DecLvl();
return Selected;
}
public virtual bool SelectPrevious()
{
Logger.debug(Type + ": SelectPrevious():");
Logger.IncLvl();
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
}
Logger.DecLvl();
return Selected;
}
public virtual void OnSelect() { }
public virtual string GetAttribute(string key)
{
Logger.debug(Type + ": GetAttribute(" + key + "):");
Logger.IncLvl();
if (Attributes.ContainsKey(key))
{
Logger.DecLvl();
return Attributes[key];
}
Logger.DecLvl();
return null;
}
public virtual void SetAttribute(string key, string value)
{
Logger.debug(Type + ": SetAttribute():");
Logger.IncLvl();
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
if (key == "activated")
{
bool shouldBeActivated = value == "true";
Activated = shouldBeActivated;
}
Attributes[key] = value;
Logger.DecLvl();
}
public void KeyPress(string keyCode)
{
Logger.debug(Type + ": _KeyPress():");
Logger.IncLvl();
Logger.debug("button: " + keyCode);
OnKeyPressed(keyCode);
if (Parent != null && !PreventDefaults.Contains(keyCode))
{
Parent.KeyPress(keyCode);
}
Logger.DecLvl();
}
public virtual void OnKeyPressed(string keyCode)
{
Logger.debug(Type + ": OnKeyPressed()");
Logger.IncLvl();
switch (keyCode)
{
case "ACTIVATE":
ToggleActivation();
break;
default:
break;
}
Logger.DecLvl();
}
public virtual void ToggleActivation()
{
Logger.debug(Type + ": ToggleActivation()");
Logger.IncLvl();
Activated = !Activated;
Logger.DecLvl();
}
public void PreventDefault(string keyCode)
{
Logger.debug(Type + ": PreventDefault()");
Logger.IncLvl();
if (!PreventDefaults.Contains(keyCode))
{
PreventDefaults.Add(keyCode);
}
Logger.DecLvl();
}
public void AllowDefault(string keyCode)
{
Logger.debug(Type + ": AllowDefault()");
Logger.IncLvl();
if (PreventDefaults.Contains(keyCode))
{
PreventDefaults.Remove(keyCode);
}
Logger.DecLvl();
}
public void FollowRoute(Route route)
{
Logger.debug(Type + ": FollowRoute");
Logger.IncLvl();
if (Parent != null)
{
Parent.FollowRoute(route);
}
Logger.DecLvl();
}
public virtual Dictionary<string, string> GetValues(Func<XMLTree, bool> filter)
{
Logger.debug(Type + ": GetValues()");
Logger.IncLvl();
Dictionary<string, string> dict = new Dictionary<string, string>();
string name = GetAttribute("name");
string value = GetAttribute("value");
if (name != null && value != null)
{
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
Logger.DecLvl();
return dict;
}
public int GetWidth(int maxWidth)
{
Logger.debug(Type + ".GetWidth()");
Logger.IncLvl();
string attributeWidthValue = GetAttribute("width");
if (attributeWidthValue == null)
{
Logger.DecLvl();
return 0;
//return maxWidth;
}
else
{
if (attributeWidthValue[attributeWidthValue.Length - 1] == '%')
{
Logger.debug("is procent value (" + Single.Parse(attributeWidthValue.Substring(0, attributeWidthValue.Length - 1)).ToString() + ")");
Logger.DecLvl();
return (int)(Single.Parse(attributeWidthValue.Substring(0, attributeWidthValue.Length - 1)) / 100f * maxWidth);
}
else if (maxWidth == 0)
{
Logger.DecLvl();
return Int32.Parse(attributeWidthValue);
}
else
{
Logger.DecLvl();
return Math.Min(maxWidth, Int32.Parse(attributeWidthValue));
}
}
}
public string Render(int availableWidth)
{
Logger.debug(Type + ".Render()");
Logger.IncLvl();
List<string> segments = new List<string>();
int width = GetWidth(availableWidth);
PreRender(ref segments, width, availableWidth);
RenderText(ref segments, width, availableWidth);
string renderString = PostRender(segments, width, availableWidth);
Logger.DecLvl();
return renderString;
}
protected virtual void PreRender(ref List<string> segments, int width, int availableWidth)
{
Logger.debug(Type + ".PreRender()");
Logger.IncLvl();
Logger.DecLvl();
}
protected virtual void RenderText(ref List<string> segments, int width, int availableWidth)
{
Logger.debug(Type + ".RenderText()");
Logger.IncLvl();
for (int i = 0; i < Children.Count; i++)
{
if (GetAttribute("flowdirection") == "vertical")
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
Logger.DecLvl();
}
protected virtual string PostRender(List<string> segments, int width, int availableWidth)
{
Logger.debug(Type + ".PostRender()");
Logger.IncLvl();
string renderString = "";
string flowdir = GetAttribute("flowdirection");
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
Logger.log("Center element...");
renderString = TextUtils.CenterText(renderString, availableWidth);
}
else if (alignSelf == "right")
{
Logger.log("Aligning element right...");
renderString = TextUtils.PadText(renderString, availableWidth, TextUtils.PadMode.RIGHT);
}
}
Logger.DecLvl();
return renderString;
}
protected virtual string RenderChild(XMLTree child, int availableWidth)
{
Logger.log(Type + ".RenderChild()");
Logger.IncLvl();
Logger.DecLvl();
return child.Render(availableWidth);
}
}

public interface XMLParentNode
{
XMLParentNode GetParent();
void UpdateSelectability(XMLTree child);
void KeyPress(string keyCode);
void FollowRoute(Route route);
bool SelectNext();
}

public class TextNode : XMLTree
{
public string Content;
public TextNode(string content) : base()
{
Type = "textnode";
Content = content.Replace("\n", "");
Content = Content.Trim(new char[] { '\n', ' ', '\r' });
if (Content == "")
{
Content = null;
}
}
protected override void RenderText(ref List<string> segments, int width, int availableWidth) { }
protected override string PostRender(List<string> segments, int width, int availableWidth)
{
return Content;
}
}

public class Route
{
string Definition;
static Dictionary<string, Action<UIController>> UIFactories =
new Dictionary<string, Action<UIController>>();
public Route(string definition)
{
Logger.debug("Route constructor():");
Logger.IncLvl();
Definition = definition;
Logger.debug("xml string is: " + Definition.Substring(4));
Logger.DecLvl();
}
public void Follow(UIController controller)
{
Logger.debug("Route: GetUI():");
Logger.IncLvl();
XMLTree ui = null;
if (Definition == "revert")
{
controller.RevertUI();
}
else if (Definition.Substring(0, 4) == "xml:")
{
ui = ParseXML(Parser.UnescapeQuotes(Definition.Substring(4)));
controller.LoadUI(ui);
}
else if (Definition.Substring(0, 3) == "fn:" && UIFactories.ContainsKey(Definition.Substring(3)))
{
UIFactories[Definition.Substring(3)](controller);
}
Logger.DecLvl();
}
static public void RegisterRouteFunction(string id, Action<UIController> fn)
{
UIFactories[id] = fn;
}
}

public class UIController : XMLParentNode
{
XMLTree ui;
public Stack<XMLTree> UIStack;
public string Type;
public UIController(XMLTree rootNode)
{
Logger.debug("UIController constructor()");
Logger.IncLvl();
Type = "CTRL";
UIStack = new Stack<XMLTree>();
ui = rootNode;
ui.SetParent(this);
if (GetSelectedNode() == null && ui.IsSelectable())
{
ui.SelectFirst();
}
Logger.DecLvl();
}
public static UIController FromXML(string xml)
{
Logger.debug("UIController FromXMLString()");
Logger.IncLvl();
XMLTree rootNode = XML.ParseXML(xml);
Logger.DecLvl();
return new UIController(rootNode);
}
public void ApplyScreenProperties(IMyTextPanel screen)
{
Logger.debug("UIController.ApplyScreenProperties()");
Logger.IncLvl();
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
Logger.DecLvl();
}
public void Call(List<string> parameters)
{
Logger.debug("UIController.Main()");
Logger.IncLvl();
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
break;
case "revert":
RevertUI();
break;
default:
break;
}
Logger.DecLvl();
return;
}
public void LoadXML(string xmlString)
{
LoadUI(XML.ParseXML(xmlString));
}
public void LoadUI(XMLTree rootNode)
{
Logger.debug("UIController: LoadUI():");
Logger.IncLvl();
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
Logger.DecLvl();
}
public void ClearUIStack()
{
UIStack = new Stack<XMLTree>();
}
public void RevertUI()
{
Logger.log("UIController: RevertUI():");
Logger.IncLvl();
if (UIStack.Count == 0)
{
Logger.log("Error: Can't revert: UI stack is empty.");
Logger.DecLvl();
return;
}
ui = UIStack.Pop();
ui.SetParent(this);
Logger.DecLvl();
}
public string Render()
{
Logger.debug("UIController: Render():");
Logger.IncLvl();
Logger.DecLvl();
return ui.Render(0);
}
public void RenderTo(IMyTextPanel panel)
{
Logger.debug("UIController.RenderTo()");
Logger.IncLvl();
int panelWidth = 0;
string panelType = panel.DetailedInfo.Split('\n')[0];
Logger.debug("Type: " + panelType);
switch (panelType)
{
case "Type: Text Panel":
panelWidth = 658;
break;
case "Type: LCD Panel":
panelWidth = 658;
break;
case "Wide LCD Panel":
panelWidth = 1316;
break;
}
int width = (int)(((float)panelWidth) / panel.GetValue<Single>("FontSize"));
Logger.debug("font size: " + panel.GetValue<Single>("FontSize").ToString());
Logger.debug("resulting width: " + width.ToString());
string text = ui.Render(width);
panel.WritePublicText(text);
Logger.DecLvl();
}
public void KeyPress(string keyCode)
{
Logger.debug("UIController: KeyPress():");
Logger.IncLvl();
switch (keyCode)
{
case "LEFT/ABORT":
RevertUI();
break;
}
Logger.DecLvl();
}
public XMLTree GetSelectedNode()
{
Logger.debug("UIController: GetSelectedNode():");
Logger.IncLvl();
XMLTree sibling = ui.GetSelectedSibling();
Logger.DecLvl();
return sibling;
}
public XMLTree GetNode(Func<XMLTree, bool> filter)
{
Logger.debug("UIController: GetNode()");
Logger.IncLvl();
Logger.DecLvl();
return ui.GetNode(filter);
}
public List<XMLTree> GetAllNodes(Func<XMLTree, bool> filter)
{
Logger.debug("UIController: GetAllNodes()");
Logger.IncLvl();
Logger.DecLvl();
return ui.GetAllNodes(filter);
}
public void UpdateSelectability(XMLTree child) { }
public void FollowRoute(Route route)
{
Logger.debug("UIController: FollowRoute():");
Logger.IncLvl();
route.Follow(this);
Logger.DecLvl();
}
public XMLParentNode GetParent()
{
return null;
}
public Dictionary<string, string> GetValues()
{
Logger.debug("UIController.GetValues()");
Logger.IncLvl();
return GetValues((node) => true);
}
public Dictionary<string, string> GetValues(Func<XMLTree, bool> filter)
{
Logger.debug("UIController.GetValues()");
Logger.IncLvl();
if (ui == null)
{
Logger.DecLvl();
return null;
}
Logger.DecLvl();
return ui.GetValues(filter);
}
public string GetPackedValues(Func<XMLTree, bool> filter)
{
return Parser.PackData(GetValues(filter));
}
public string GetPackedValues()
{
Logger.debug("UIController.GetPackedValues()");
Logger.IncLvl();
Logger.DecLvl();
return GetPackedValues(node => true);
}
public bool SelectNext()
{
return ui.SelectNext();
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
Logger.debug("UIFactory constructor");
Logger.IncLvl();
if (uiList == null)
{
UIs = new List<UIController>();
}
UIs = uiList;
Logger.DecLvl();
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
Type = type;
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
Logger.debug(Type + ": Add child():");
Logger.IncLvl();
if (child.Type != "menuitem" && child.IsSelectable())
{
Logger.DecLvl();
throw new Exception(
"ERROR: Only children of type <menupoint> or children that are not selectable are allowed!"
+ " (type was: <" + child.Type + ">)");
}
base.AddChild(child);
Logger.DecLvl();
}
protected override string RenderChild(XMLTree child, int width)
{
string renderString = "";
string prefix = "     ";
if (child.Type == "menuitem")
{
renderString += (child.IsSelected() ? ">> " : prefix);
}
renderString += base.RenderChild(child, width);
return renderString;
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
Logger.debug(Type + ": SetAttribute():");
Logger.IncLvl();
switch (key)
{
case "route":
Logger.debug("prepare to set route...");
SetRoute(new Route(value));
if (TargetRoute == null)
{
Logger.debug("Failure!");
}
else
{
Logger.debug("Success!");
}
break;
default:
base.SetAttribute(key, value);
break;
}
Logger.DecLvl();
}
public override void OnKeyPressed(string keyCode)
{
Logger.debug(Type + ": OnKeyPressed():");
switch (keyCode)
{
case "RIGHT/SUBMIT":
if (TargetRoute != null)
{
Logger.debug("Follow Target Route!");
FollowRoute(TargetRoute);
}
else
{
Logger.debug("No route set!");
}
break;
}
base.OnKeyPressed(keyCode);
Logger.DecLvl();
}
public void SetRoute(Route route)
{
TargetRoute = route;
}
}

public class ProgressBar : XMLTree
{
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
Logger.debug(Type + ".IncreaseFillLevel()");
Logger.IncLvl();
FillLevel += StepSize;
Logger.DecLvl();
}
public void DecreaseFillLevel()
{
Logger.debug(Type + ".DecreaseFillLevel()");
Logger.IncLvl();
FillLevel -= StepSize;
Logger.DecLvl();
}
public override void OnKeyPressed(string keyCode)
{
Logger.debug(Type + ": OnKeyPressed():");
Logger.IncLvl();
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
Logger.DecLvl();
}
protected override void RenderText(ref List<string> segments, int width, int availableWidth)
{
Logger.debug(Type + ".RenderText()");
Logger.IncLvl();
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
Logger.DecLvl();
}
}

public class Container : XMLTree
{
public Container() : base()
{
Type = "container";
}
}

public class HorizontalLine : XMLTree
{
public HorizontalLine() : base()
{
Type = "hl";
SetAttribute("width", "100%");
}
protected override void RenderText(ref List<string> segments, int width, int availableWidth)
{
segments.Add(TextUtils.CreateStringOfLength("_", width, TextUtils.RoundMode.CEIL));
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
private void RetrieveController()
{
XMLParentNode ancestor = this;
while (ancestor.GetParent() != null)
{
ancestor = ancestor.GetParent();
}
Controller = ancestor as UIController;
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
RetrieveController();
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
protected override string PostRender(List<string> segments, int width, int availableWidth)
{
if (Controller == null)
{
RetrieveController();
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
}
}

public class TextInput : XMLTree
{
int CursorPosition;
public TextInput()
{
Type = "textinput";
Selectable = true;
CursorPosition = -1;
PreventDefault("LEFT/ABORT");
PreventDefault("RIGHT/SUBMIT");
SetAttribute("maxlength", "10");
SetAttribute("value", "");
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
IncreaseLetter();
break;
case "DOWN":
DecreaseLetter();
break;
default:
base.OnKeyPressed(keyCode);
break;
}
}
private void IncreaseLetter()
{
if (CursorPosition == -1)
{
return;
}
char[] value = GetAttribute("value").ToCharArray();
char letter = value[CursorPosition];
switch (letter)
{
case ' ':
value[CursorPosition] = 'a';
break;
case 'z':
value[CursorPosition] = 'A';
break;
case 'Z':
value[CursorPosition] = '0';
break;
case '9':
value[CursorPosition] = ' ';
break;
default:
value[CursorPosition] = (char)(((int)value[CursorPosition]) + 1);
break;
}
SetAttribute("value", new string(value));
}
private void DecreaseLetter()
{
if (CursorPosition == -1)
{
return;
}
char[] value = GetAttribute("value").ToCharArray();
char letter = value[CursorPosition];
switch (letter)
{
case ' ':
value[CursorPosition] = '9';
break;
case '0':
value[CursorPosition] = 'Z';
break;
case 'a':
value[CursorPosition] = ' ';
break;
case 'A':
value[CursorPosition] = 'z';
break;
default:
value[CursorPosition] = (char)(((int)value[CursorPosition]) - 1);
break;
}
SetAttribute("value", new string(value));
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
SetAttribute("value", GetAttribute("value") + " ");
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
protected override void RenderText(ref List<string> segments, int width, int availableWidth)
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
/*char[] valueChr = (" " + value).ToCharArray();
valueChr[0] = (char) 187;
value = new string(valueChr);*/
value = "_" + value;
}
segments.Add((IsSelected() ? new string(new char[] { (char)187 }) : "  ") + " " + value);
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
protected override void PreRender(ref List<string> segments, int width, int availableWidth)
{
segments.Add(IsSelected() ? "[[  " : "[   ");
base.PreRender(ref segments, width, availableWidth);
}
protected override string PostRender(List<string> segments, int width, int availableWidth)
{
segments.Add(IsSelected() ? "  ]]" : "   ]");
return base.PostRender(segments, width, availableWidth);
}
}

public class Break : TextNode
{
public Break() : base("")
{
Type = "br";
}
protected override void RenderText(ref List<string> segments, int width, int availableWidth) { }
protected override string PostRender(List<string> segments, int width, int availableWidth)
{
return "";
}
}

public class Space : XMLTree
{
public Space() : base()
{
Logger.debug("Space constructor()");
Logger.IncLvl();
Type = "space";
SetAttribute("width", "0");
Logger.DecLvl();
}
protected override void RenderText(ref List<string> segments, int width, int availableWidth)
{
Logger.debug(Type + ".RenderText()");
Logger.IncLvl();
segments.Add(TextUtils.CreateStringOfLength(" ", width));
Logger.DecLvl();
}
}

public class Hidden : XMLTree
{
public Hidden() : base()
{
Type = "hidden";
}
protected override string PostRender(List<string> segments, int width, int availableWidth)
{
return null;
}
}

public class HiddenData : DataStore
{
public HiddenData() : base()
{
Type = "hiddendata";
}
protected override string PostRender(List<string> segments, int width, int availableWidth)
{
return null;
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
}
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
