﻿namespace SEScripts.Merged
{
public class UIServiceIndexServer : AgentProtocol
{
static string XMLHeader = "<meta $ATTRIBUTES$/><uicontrols>$TITLE$</uicontrols><hl/>";
public override string GetProtocolId()
{
return "get-ui-services-index";
}
private int RefreshTime;
private XML.XMLTree UIRoot;

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
Logger.log("UIServiceIndexServer constructor");
RefreshTime = 5000;
}
public override void ReceiveMessage(AgentMessage msg)
{
Logger.IncLvl();
Logger.debug("UIServicesIndexServer.ReceiveMessage()");
if(msg.Status == AgentMessage.StatusCodes.OK)
{
ReceiveServices(msg);
}
else
{
base.ReceiveMessage(msg);
}
Logger.DecLvl();
}
private void ReceiveServices(AgentMessage msg)
{
Logger.debug("UIServicesIndexServer.ReceiveServices()");
Logger.IncLvl();
XML.XMLTree messageXML;
try
{
Logger.log("test");
messageXML = XML.ParseXML(msg.Content);
}
catch
{
Logger.log("WARNING: Invalid message received!");
Logger.DecLvl();
return;
}
string platformId = msg.Sender.Platform;
string platformName = messageXML.GetNode((node) => node.Type == "platforminfo")?.GetAttribute("platform");
List<PlatformService> services = new List<PlatformService>();
Services[platformId] = new List<PlatformService>();
Platforms[platformId] = platformName ?? platformId;
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
Logger.DecLvl();
}

public override void Restart()
{ }
public override void NotifyEvent(string eventId)
{
Logger.log("UIServiceIndexServer.NotifyEvent()");
Logger.IncLvl();
switch (eventId)
{
case "register":
RetrieveServices();
break;
case "refresh":
int ttr = (int)Holder.GetKnowledgeEntry("TimeTillRefresh", this);
ttr -= Holder.ElapsedTime.Milliseconds;
Logger.log("Time Till Refresh: " + ttr);
if (ttr < 0)
{
ttr = RefreshTime;
RetrieveServices();
}
Holder.ScheduleRefresh();
Holder.SetKnowledgeEntry("TimeTillRefresh", ttr, this);
break;
default:
base.NotifyEvent(eventId);
break;
}
Logger.DecLvl();
}
public void SetUIReceiver(AgentId receiver)
{
UIReceiver = receiver;
}
private void RetrieveServices()
{
Logger.debug("UIServiceIndexServer.RetrieveServices()");
Logger.IncLvl();
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
Logger.DecLvl();
}

public string PageHome()
{
Logger.debug("UIServiceIndexServer.PageHome()");
Logger.IncLvl();
StringBuilder xml = new StringBuilder(XMLHeader).Replace("$TITLE$", "Platforms").Replace("$ATTRIBUTES$", "uiServiceIndexHome");
xml.Append("<menu id='platformMenu'>");
xml.Append(GetPlatformMenuitems());
xml.Append("</menu>");
//xml.Append("<menuItem route='" + MakeRoute(Holder.Id, GetProtocolId(), "page='refresh'") + "'>" + "Refresh</menuitem>");
Logger.DecLvl();
return xml.ToString();
}
private StringBuilder GetPlatformMenuitems()
{
StringBuilder xml = new StringBuilder();
foreach (string key in Platforms.Keys)
{
xml.Append("<menuItem route='fn:show-platform-services' platform='" + key + "'>" + Platforms[key] + "</menuitem>");
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
Logger.log("UIServiceIndexServer.LoadHomeScreen()");
Logger.IncLvl();
UITerminalAgent UIAgent = Holder as UITerminalAgent;
if(UIAgent != null)
{
XML.XMLTree meta = UIAgent.UI.GetNode((node) => node.Type == "meta");
bool isLoaded = (meta != null && meta.GetAttribute("uiserviceindexhome") != null);
if (HomePageActive && !isLoaded )
{
Logger.log("Setting HomPageActive to false");
HomePageActive = false;
}
else
{
if(isLoaded)
{
meta.SetAttribute("historydisabled", "true");
}
XML.XMLTree ui = XML.ParseXML(PageHome());
UIAgent.LoadUI(ui);
HomePageActive = true;
UIAgent.UpdateScreen();
Holder.ScheduleRefresh();
}
}
Logger.DecLvl();
}
public override void Setup()
{
Logger.debug("UIServiceIndexServer.Setup()");
Logger.IncLvl();
Logger.log("Create ui app");
/*UIServerProtocol.CreateApplication(
Holder,
GetProtocolId(),
"Show Services",
new Dictionary<string, Func<UIServerProtocol, AgentMessage, Dictionary<string, string>, string>>
{
{"", this.PageHomeServer },
{"platform", this.PagePlatformServicesServer },
{"refresh", this.PageRefreshServer }
});*/
XML.Route.RegisterRouteFunction("show-platform-services", (controller) =>
{
string platformId = controller.GetSelectedNode().GetAttribute("platform");
if (platformId != null)
{
controller.LoadUI(XML.ParseXML(PagePlatformServices(platformId)));
}
});
Logger.log("Create event listeners");
Holder.SetKnowledgeEntry("TimeTillRefresh", RefreshTime, this);
AgentProtocol chat = new UIServiceIndexServer(Holder);
Holder.AddChat(chat);
Holder.OnEvent("register", chat);
Holder.OnEvent("refresh", chat);
Holder.SetKnowledgeEntry("HomePageActive", false, this);
Logger.DecLvl();
}
}



}