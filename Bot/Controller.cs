using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Timers;
using System.Windows.Forms;
using Bot.Custom;
using Bot.Enums;
using Bot.Helpers;
using static System.String;
using Technologies = Bot.Enums.Technologies;
using Timer = System.Timers.Timer;

namespace Bot
{
    public class Controller
    {
        #region Attributes

        /// <summary>
        /// Currently selected Town ID.
        /// </summary>
        private string m_CurrentTownID { get; set; } = "";

        /// <summary>
        /// Current selected Town.
        /// </summary>
        private Town m_Town { get; set; }

        /// <summary>
        /// HttpHandler for Grepolis
        /// </summary>
        private readonly HttpHandler m_HttpHandler = new HttpHandler();

        /// <summary>
        /// Count the Retrys.
        /// </summary>
        private int m_RetryCount = 0;

        /// <summary>
        /// Count of reconnect trys.
        /// </summary>
        private int m_ReconnectCount = 0;

        /// <summary>
        /// How often the bot cycled.
        /// </summary>
        private int m_CycleCount = 0;

        /// <summary>
        /// True when bot running.
        /// </summary>
        private bool m_BotRunning = false;

        /// <summary>
        /// Queue which contains the actions the Bot should execute.
        /// </summary>
        private LinkedList<ControllerStates> m_ControllerQueue { get; } = new LinkedList<ControllerStates>();

        /// <summary>
        /// Queue wich contains data for the m_ControllerQueue actions
        /// </summary>
        private LinkedList<Dictionary<string, string>> m_ControllerQueueDataQueue { get; } = new LinkedList<Dictionary<string, string>>();

        /// <summary>
        /// Singleton.
        /// </summary>
        public static Controller Instance { get; } = new Controller();

        /// <summary>
        /// Controller State.
        /// </summary>
        public ControllerStates State { get; set; } = ControllerStates.LoginToGrepolisPart1;

        /// <summary>
        /// Bot logged in or not.
        /// </summary>
        public bool LoggedIn { get; set; } = false;

        /// <summary>
        /// State of verified Login.
        /// </summary>
        public bool LoginVerified { get; set; }

        /// <summary>
        /// Grepolis H. Needed for every Request.
        /// </summary>
        public string H { get; set; }

        /// <summary>
        /// Grepolis Player.
        /// </summary>
        public Player Player { get; } = new Player();

        /// <summary>
        /// Server Time recieved from server.
        /// </summary>
        public string ServerTime { get; set; }

        /// <summary>
        /// Server time with length 13 for requests.
        /// </summary>
        public string ExpandedServerTime => ExpandServerTime(ServerTime);

        /// <summary>
        /// Notification ID.
        /// </summary>
        public string Nlreq_ID { get; set; } = "0"; //Currently only used for notifications, in the past for http requests as well. Circa 2.82 replaced by "nl_init":true for all server requests.

        #region Timers

        /// <summary>
        /// Request Timer for delayed Server Requests.
        /// </summary>
        private readonly Timer m_RequestTimer = new Timer();

        /// <summary>
        /// Timer to delay the reconnect.
        /// </summary>
        private readonly AdvTimer m_ReconnectTimer = new AdvTimer();

        /// <summary>
        /// Timer that starts the Refresh Cycle and execute the ControllerQueue Actions.
        /// </summary>
        private readonly AdvTimer m_RefreshTimer = new AdvTimer();

        /// <summary>
        /// Timer for Farmer check and looting.
        /// </summary>
        private readonly AdvTimer m_FarmerTimer = new AdvTimer();

        /// <summary>
        /// Timer for Building Queues.
        /// </summary>
        private readonly AdvTimer m_BuildTimer = new AdvTimer();

        /// <summary>
        /// Timer for Unit Queues.
        /// </summary>
        private readonly AdvTimer m_UnitTimer = new AdvTimer();

        /// <summary>
        /// Timer for trading.
        /// </summary>
        private readonly AdvTimer m_TradingTimer = new AdvTimer();

        #endregion Timers

        #region Events

        /// <summary>
        /// Event Handler for RequestFailed Event.
        /// </summary>
        public delegate void RequestFailedEventHandler(object sender, CustomArgs ca);

        /// <summary>
        /// Event fires when Request failed.
        /// </summary>
        public event RequestFailedEventHandler RequestFailedEvent;

        /// <summary>
        /// Event Handler for RequestSuccessful Event.
        /// </summary>
        public delegate void RequestSuccessfulEventHandler(object sender, CustomArgs ca);

        /// <summary>
        /// Event fires when Request successful.
        /// </summary>
        public event RequestSuccessfulEventHandler RequestSuccessfulEvent;

        /// <summary>
        /// Event Handler for LogEvent.
        /// </summary>
        public delegate void LogEventHandler(object sender, CustomArgs ca);

        /// <summary>
        /// Event fires when something to Log on Log Tab.
        /// </summary>
        public event LogEventHandler LogEvent;

        /// <summary>
        /// Event Handler for LoginSuccessfulEvent.
        /// </summary>
        public delegate void LoginSuccessfulEventHandler(object sender, CustomArgs ca);

        /// <summary>
        /// Event fires after Controller login.
        /// </summary>
        public event LoginSuccessfulEventHandler LoginSuccessfulEvent;

        /// <summary>
        /// Event Handler for RefreshTimerUpdatedEvent.
        /// </summary>
        public delegate void RefreshTimerUpdatedEventHandler(object sender, CustomArgs ca);

        /// <summary>
        /// Event fires to refresh the RefreshTimer on GUI.
        /// </summary>
        public event RefreshTimerUpdatedEventHandler RefreshTimerUpdatedEvent;

        /// <summary>
        /// Event Handler for FarmerTimerUpdatedEvent.
        /// </summary>
        public delegate void FarmerTimerUpdatedEventHandler(object sender, CustomArgs ca);

        /// <summary>
        /// Event fires to refresh the FarmerTimer on GUI.
        /// </summary>
        public event FarmerTimerUpdatedEventHandler FarmerTimerUpdatedEvent;

        /// <summary>
        /// Event Handler for TradingTimerUpdatedEvent.
        /// </summary>
        public delegate void TradingTimerUpdatedEventHandler(object sender, CustomArgs ca);

        /// <summary>
        /// Event fires to refresh the TradingTimer on GUI.
        /// </summary>
        public event TradingTimerUpdatedEventHandler TradingTimerUpdatedEvent;

        /// <summary>
        /// Event Handler for ReconnectTimerUpdatedEvent.
        /// </summary>
        public delegate void ReconnectTimerUpdatedEventHandler(object sender, CustomArgs ca);

        /// <summary>
        /// Event fires to refresh the ReconnectTimer on GUI.
        /// </summary>
        public event ReconnectTimerUpdatedEventHandler ReconnectTimerUpdatedEvent;

        /// <summary>
        /// Event Handler for StartReconnectEvent.
        /// </summary>
        public delegate void StartReconnectEventHandler(object sender, CustomArgs ca);

        /// <summary>
        /// Event fires to start the reconnect sequence.
        /// </summary>
        public event StartReconnectEventHandler StartReconnectEvent;

        /// <summary>
        /// Event Handler for LoggedOutEvent;
        /// </summary>
        public delegate void LoggedOutEventHandler(object sender, CustomArgs ca);

        /// <summary>
        /// Event fires when Grepolis logout.
        /// </summary>
        public event LoggedOutEventHandler LoggedOutEvent;

        /// <summary>
        /// Event Handler for BotStartedEvent.
        /// </summary>
        public delegate void BotStartedEventHandler(object sender, CustomArgs ca);

        /// <summary>
        /// Event fires when Bot started.
        /// </summary>
        public event BotStartedEventHandler BotStartedEvent;

        /// <summary>
        /// Event Handler for BotStoppedEvent.
        /// </summary>
        public delegate void BotStoppedEventHandler(object sender, CustomArgs ca);

        /// <summary>
        /// Event fires when Bot stopped.
        /// </summary>
        public event BotStoppedEventHandler BotStoppedEvent;

        /// <summary>
        /// Event Handler for BotCyclingStartedEvent.
        /// </summary>
        public delegate void BotCyclingStartedEventHandler(object sender, CustomArgs ca);

        /// <summary>
        /// Event fires when Bot started cycling.
        /// </summary>
        public event BotCyclingStartedEventHandler BotCyclingStartedEvent;

        /// <summary>
        /// Event Handler for BotCyclingStoppedEvent.
        /// </summary>
        public delegate void BotCyclingStoppedEventHandler(object sender, CustomArgs ca);

        /// <summary>
        /// Event fires when Bot stopped cycling.
        /// </summary>
        public event BotCyclingStoppedEventHandler BotCyclingStoppedEvent;

        /// <summary>
        /// Event Handler for BuildTimerUpdatedEvent.
        /// </summary>
        public delegate void BuildTimerUpdatedEventHandler(object sender, CustomArgs ca);

        /// <summary>
        /// Event fires to refresh the BuildTimer on GUI.
        /// </summary>
        public event BuildTimerUpdatedEventHandler BuildTimerUpdatedEvent;

        /// <summary>
        /// Event Handler for UnitTimerUpdatedEvent.
        /// </summary>
        public delegate void UnitTimerUpdatedEventHandler(object sender, CustomArgs ca);

        /// <summary>
        /// Event fires to refresh the UnitTimer on GUI.
        /// </summary>
        public event UnitTimerUpdatedEventHandler UnitTimerUpdatedEvent;

        #endregion Events

        #endregion Attributes

        #region Constructor

        private Controller()
        {
            InitTimers();
            InitEventHandler();
        }

        #endregion Constructor

        #region Functions

        #region Initialization

        /// <summary>
        /// Initialize all timers.
        /// </summary>
        private void InitTimers()
        {
            m_RequestTimer.Enabled = true;
        }

        /// <summary>
        /// Initialize all Event Handler.
        /// </summary>
        private void InitEventHandler()
        {
            m_HttpHandler.UploadValuesCompleted += HttpHandlerOnUploadValuesCompleted;
            m_HttpHandler.DownloadStringCompleted += HttpHandlerOnDownloadStringCompleted;
            RequestFailedEvent += OnRequestFailedEvent;
            RequestSuccessfulEvent += OnRequestSuccessfulEvent;

            m_RequestTimer.Elapsed += RequestTimerOnElapsed;

            m_ReconnectTimer.InternalTimer.Elapsed += ReconnectTimerOnElapsed;
            m_ReconnectTimer.TimerDoneEvent += ReconnectTimerOnTimerDoneEvent;

            m_RefreshTimer.InternalTimer.Elapsed += RefreshTimerOnElapsed;
            m_RefreshTimer.TimerDoneEvent += RefreshTimerOnTimerDoneEvent;

            m_FarmerTimer.InternalTimer.Elapsed += FarmerTimerOnElapsed;

            m_TradingTimer.InternalTimer.Elapsed += TradingTimerOnElapsed;

            m_BuildTimer.InternalTimer.Elapsed += BuildTimerOnElapsed;

            m_UnitTimer.InternalTimer.Elapsed += UnitTimerOnElapsed;
        }

        /// <summary>
        /// Copy Cookies from BotWebBrowser to Controller.
        /// </summary>
        public void InitCookies(WebBrowser p_WebBrowser)
        {
            try
            {
                var l_HostName = p_WebBrowser.Url.Scheme + Uri.SchemeDelimiter + p_WebBrowser.Url.Host;
                var l_HostUri = new Uri(l_HostName);
                var l_Container = CookieHelper.GetUriCookieContainer(l_HostUri);
                var l_CookieCollection = l_Container.GetCookies(l_HostUri);
                m_HttpHandler.CookieContainer.Add(l_CookieCollection);

                /*Save cookies to file
                string l_Cookies = "";
                for (int i = 0; i < l_CookieCollection.Count; i++)
                {
                    l_Cookies += l_CookieCollection[i].Name + "=" + l_CookieCollection[i].Value + ";";
                }
                l_IOHandler.writeCookies(l_Cookies);
                */
            }
            catch (Exception ex)
            {
                Logger.WriteExceptionToLog(ex.Message);
            }
        }

        #endregion Initialization

        #region Login, Start, Pause, Resume, Reconnect, Randomizing

        /// <summary>
        /// Called to Login the Controller.
        /// </summary>
        public void LoginToGrepolis()
        {
            CallBotStartedEvent();
            m_CurrentTownID = Player.DefaultTownID;

            m_RequestTimer.Interval = 10; //Fast requests to Login.

            m_ControllerQueue.AddLast(ControllerStates.UpdateTowns); //Update towns first!
            m_ControllerQueue.AddLast(ControllerStates.UpdateGameData);
            m_ControllerQueue.AddLast(ControllerStates.BuildingBuildData);
            m_ControllerQueue.AddLast(ControllerStates.UpdateUnits);
            m_RequestTimer.Start();
        }

        /// <summary>
        /// Starts the Bot.
        /// </summary>
        public void StartBot()
        {
            CallBotStartedEvent();

            //Start cycle imidiatly after starting bot.
            m_RefreshTimer.Duration = 0.15;
            m_FarmerTimer.Duration = 0.1;
            m_TradingTimer.Duration = 0.1;
            m_BuildTimer.Duration = 0.1;
            m_UnitTimer.Duration = 0.1;

            m_RefreshTimer.Start();
            m_FarmerTimer.Start();
            m_TradingTimer.Start();
            m_BuildTimer.Start();
            m_UnitTimer.Start();
        }

        /// <summary>
        /// Pause the bot.
        /// </summary>
        public void PauseBot()
        {
            CallBotStoppedEvent();

            m_RefreshTimer.Pause();
            m_FarmerTimer.Pause();
            m_TradingTimer.Pause();
            m_BuildTimer.Pause();
            m_UnitTimer.Pause();
        }

        public void ResumeBot()
        {
            CallBotStartedEvent();

            m_RefreshTimer.Resume();
            m_FarmerTimer.Resume();
            m_TradingTimer.Resume();
            m_BuildTimer.Resume();
            m_UnitTimer.Resume();
        }

        /// <summary>
        /// Called to start the reconnect sequence.
        /// </summary>
        private void StartReconnect()
        {
            CallLogEvent("Stopped bot.");
            //Stop timers
            m_RefreshTimer.Stop();
            m_FarmerTimer.Stop();
            m_TradingTimer.Stop();
            m_BuildTimer.Stop();
            m_UnitTimer.Stop();

            State = ControllerStates.LoginToGrepolisPart1; //Start from beginning

            LoggedIn = false;
            CallBotStoppedEvent();
            LoggedOutEvent?.Invoke(this, new CustomArgs());

            m_ControllerQueue.Clear();
            m_ControllerQueueDataQueue.Clear();

            m_ReconnectCount++;

            m_ReconnectTimer.Duration = Randomizer.RandomizeReconnectTimer();
            m_ReconnectTimer.Start();
        }

        /// <summary>
        /// Randomize all Timer Intervals of Done timers.
        /// </summary>
        private void RandomizeTimers()
        {
            m_RequestTimer.Interval = Randomizer.RandomizeRequestTimer();

            if (m_RefreshTimer.Done)
            {
                m_RefreshTimer.Duration = Randomizer.RandomizeRefreshTimer();
                m_RefreshTimer.Ready = true;
            }

            if (m_FarmerTimer.Done)
            {
                m_FarmerTimer.Duration = Randomizer.RandomizeFarmerTimer();
                m_FarmerTimer.Ready = true;
            }

            if (m_TradingTimer.Done)
            {
                m_TradingTimer.Duration = Randomizer.RandomizeTradingTimer();
                m_TradingTimer.Ready = true;
            }

            if (m_BuildTimer.Done)
            {
                m_BuildTimer.Duration = Randomizer.RandomizeBuildTimer();
                m_BuildTimer.Ready = true;
            }

            if (m_UnitTimer.Done)
            {
                m_UnitTimer.Duration = Randomizer.RandomizeUnitTimer();
                m_UnitTimer.Ready = true;
            }
                
        }

        #endregion Login, Start, Pause, Resume, Reconnect, Randomizing

        #region Town Cycle

        /// <summary>
        /// Cycles through the town.
        /// </summary>
        private void TownCycle(Town p_Town)
        {
            try
            {
                //skip if town has conqueror
                if (p_Town.HasConqueror)
                    return;

                //Handle town switch
                m_ControllerQueue.AddLast(ControllerStates.SwitchTown);
                m_ControllerQueueDataQueue.AddLast(new Dictionary<string, string>() {{"townid", p_Town.TownID}});

                if (m_CycleCount % p_Town.Priority  != 0) //always check hole town by first run
                    return;

                //Handle farming
                if (Settings.MasterFarmingEnabled && m_FarmerTimer.Done)
                {
                    //Only locate farmers. After this request check if something to loot because bot doesnt have acutal data of farmers at this moment.
                    m_ControllerQueue.AddLast(ControllerStates.LocateFarmers);
                }

                //Handle culture
                //Only look at actual status. starting enqueue after this request.
                m_ControllerQueue.AddLast(ControllerStates.UpdateCulturalInfo);

                //Handle build
                if (Settings.MasterBuildingEnabled && m_BuildTimer.Done)
                {
                    m_ControllerQueue.AddLast(ControllerStates.CheckMainBuilding); //OpenMainBuilding
                }

                //Handle units
                if (Settings.MasterUnitEnabled && m_UnitTimer.Done && Settings.SkipUnitQueuePop <= p_Town.PopulationAvailable)
                {
                    //Handle army units
                    if (p_Town.Buildings.Single(x => x.DevName == Buildings.barracks).Level > 0)
                    {
                        m_ControllerQueue.AddLast(ControllerStates.OpenBarracksWindow);
                    }
                    //Handle navy units
                    if (p_Town.Buildings.Single(x => x.DevName == Buildings.docks).Level > 0)
                    {
                        m_ControllerQueue.AddLast(ControllerStates.OpenDocksWindow);
                    }
                }

                //Handle trading
                if (Settings.MasterTradingEnabled && m_TradingTimer.Done && p_Town.TradeMode == TradingModes.Send &&
                    p_Town.TradeEnabled)
                {
                    //Get real market level because level contains ingamebuilding queue
                    var l_MarketLevel = p_Town.Buildings.Single(x => x.DevName == Buildings.market).Level;
                    if (p_Town.IngameBuildingQueue.Any(x => x.DevName == Buildings.market))
                    {
                        l_MarketLevel -= p_Town.IngameBuildingQueue.Count(x => x.DevName.Equals(Buildings.market));
                    }

                    //Skip if marketlevel is lower then 10
                    if (l_MarketLevel >= 10)
                    {
                        //Only called to check trade because dont know the resources at this moment.
                        m_ControllerQueue.AddLast(ControllerStates.CheckTrade);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.WriteToLog(ex.Message);
            }
        }

        /// <summary>
        /// Called when bot finished cycle naturaly.
        /// </summary>
        private void BotCycleFinished()
        {
            m_ControllerQueue.RemoveFirst(); //remove FinishedCycle from Queue
            BotCyclingStoppedEvent?.Invoke(this, new CustomArgs());
            CallLogEvent("Finished Cycle!");
            m_CycleCount++; //Update Cycle count.
            if (m_BotRunning)
            {
                m_RefreshTimer.Start();
                //Only start the timers that were done when cycle started
                if (m_FarmerTimer.Ready)
                    m_FarmerTimer.Start();
                if (m_TradingTimer.Ready)
                    m_TradingTimer.Start();
                if(m_BuildTimer.Ready)
                    m_BuildTimer.Start();
                if (m_UnitTimer.Ready)
                    m_UnitTimer.Start();
            }
        }

        #endregion Town Cycle

        #region Requests

        /// <summary>
        /// POST Request.
        /// </summary>
        private void PostRequest(Uri p_Uri, NameValueCollection p_PostData)
        {
            try
            {
                m_HttpHandler.Headers.Add("X-Requested-With", "XMLHttpRequest");
                m_HttpHandler.UploadValuesAsync(p_Uri, p_PostData);
                m_HttpHandler.Headers.Remove("X-Requested-With");
            }
            catch (Exception ex)
            {
                Logger.WriteExceptionToLog(ex.Message);
            }
        }

        /// <summary>
        /// GET Request.
        /// </summary>
        private void GetRequest(Uri p_Uri)
        {
            m_HttpHandler.Headers.Add("X-Requested-With", "XMLHttpRequest");
            m_HttpHandler.DownloadStringAsync(p_Uri);
            m_HttpHandler.Headers.Remove("X-Requested-With");
        }

        /// <summary>
        /// For request the Servertime length has to be 13
        /// </summary>
        private static string ExpandServerTime(string p_ServerTime)
        {
            if (p_ServerTime.Length > 0)
            {
                while (p_ServerTime.Length < 13)
                    p_ServerTime = p_ServerTime + "0";
            }
            return p_ServerTime;
        }

        /// <summary>
        /// Manages which Request to call. Called by RequestTimerOnTick.
        /// </summary>
        private void StateManager()
        {
            //If bot is paused or queue empty
            if (!m_BotRunning || m_ControllerQueue.Count == 0)
                return;

            State = m_ControllerQueue.First(); //Get the action to do.

            switch (State)
            {
                //Login
                case ControllerStates.UpdateGameData:
                    UpdateGameDataRequest();
                    break;

                case ControllerStates.BuildingBuildData:
                    BuildingBuildDatasRequest();
                    break;

                case ControllerStates.UpdateTowns:
                    UpdateTownsRequest();
                    break;

                case ControllerStates.UpdateUnits:
                    UpdateUnitsRequest();
                    //If reconnect
                    if (m_ReconnectCount > 0)
                        m_ReconnectCount = 0; //Reset reconnect counter
                    else
                        CallBotStoppedEvent(); //Stop bot after login
                    break;
                //Switch town
                case ControllerStates.SwitchTown:
                    SwitchTownRequest();
                    break;
                //Farmer
                case ControllerStates.LocateFarmers:
                    LocateFarmersRequest();
                    break;

                case ControllerStates.OpenFarmerWindow:
                    OpenFarmerWindowRequest();
                    break;

                case ControllerStates.GetTownSpecificData:
                    GetTownSpecificDataRequest();
                    break;

                case ControllerStates.LootFarmer:
                    LootFarmerRequest();
                    break;
                //Trade
                case ControllerStates.CheckTrade:
                    CheckTradeRequest();
                    break;

                case ControllerStates.SendResources:
                    SendResourcesRequest();
                    break;
                //Cultur
                case ControllerStates.UpdateCulturalInfo:
                    UpdateCulturalInfoRequest();
                    break;

                case ControllerStates.StartCulturalFestival:
                    StartCulturalFestivalRequest();
                    break;
                //Build
                case ControllerStates.CheckMainBuilding:
                    CheckMainBuildingRequest();
                    break;

                case ControllerStates.CheckBuildingQueue:
                    CheckBuildingQueueRequest();
                    break;

                case ControllerStates.CheckBuildingQueueTeardown:
                    CheckBuildingQueueTeardownRequest();
                    break;
                //Units
                case ControllerStates.OpenBarracksWindow:
                    OpenBarracksWindowRequest();
                    break;
                case ControllerStates.CheckLandUnitQueue:
                    CheckLandUnitQueueRequest();
                    break;
                case ControllerStates.OpenDocksWindow:
                    OpenDocksWindowRequest();
                    break;
                case ControllerStates.CheckNavyUnitQueue:
                    CheckNavyUnitQueueRequest();
                    break;
                //Finish
                case ControllerStates.FinishedCycle:
                    BotCycleFinished();
                    break;
            }
        }

        /// <summary>
        /// Manages which Request to Retry.
        /// </summary>
        private void RetryManager()
        {
            if (m_HttpHandler.IsBusy)
            {
                m_HttpHandler.CancelAsync();
                CallLogEvent("Active Request canceled.");
            }

            if (m_RetryCount < Settings.MaxRetryCount && m_BotRunning)
            {
                m_RetryCount++;
                CallLogEvent("Request failed. Retry " + State.ToString("G") + ".");
                m_RequestTimer.Start();
            }
            else
            {
                CallBotStoppedEvent();
                CallLogEvent("Too many retrys.");
                BotCyclingStoppedEvent?.Invoke(this, new CustomArgs());
            }
        }

        /// <summary>
        /// Only Login. The main update function.
        /// </summary>
        private void UpdateGameDataRequest()
        {
            try
            {
                var l_Url = "https://" + Settings.GrepolisWorldServer + "/game/data?town_id=" + Player.DefaultTownID +
                            "&action=get&h=" + H;
                var l_Uri = new Uri(l_Url);
                //Map parameter relating to window size of browser. We emulate a normal sized window with x:7 and y:5
                PostRequest(l_Uri, new NameValueCollection() { { "json", "{\"types\":[{\"type\":\"map\",\"param\":{\"x\":7,\"y\":5}},{\"type\":\"bar\"},{\"type\":\"backbone\"}],\"town_id\":" + Player.DefaultTownID + ",\"nl_init\":false}" } });

                CallLogEvent("Updating Game Data.");
            }
            catch (Exception ex)
            {
                Logger.WriteExceptionToLog(ex.Message);
            }
        }

        /// <summary>
        /// Only Login. Request for the Building Data of all Towns.
        /// </summary>
        private void BuildingBuildDatasRequest()
        {
            try
            {
                var l_Json = "{\"collections\":{\"BuildingBuildDatas\":[]},\"town_id\":" + m_CurrentTownID +
                             ",\"nl_init\":false}";

                l_Json = Uri.EscapeDataString(l_Json);

                var l_Url = "https://" + Settings.GrepolisWorldServer + "/game/frontend_bridge?town_id=" +
                            m_CurrentTownID + "&action=refetch&h=" + H + "&json=" + l_Json + "&_=" +
                            ExpandedServerTime;

                GetRequest(new Uri(l_Url));

                CallLogEvent("Updating Building Data.");
            }
            catch (Exception ex)
            {
                Logger.WriteExceptionToLog(ex.Message);
            }
        }

        /// <summary>
        /// Only Login. Request for all Town Data.
        /// </summary>
        private void UpdateTownsRequest()
        {
            try
            {
                var l_Json = "{\"collections\":{\"Towns\":[]},\"town_id\":" + m_CurrentTownID +
                             ",\"nl_init\":false}";
                l_Json = Uri.EscapeDataString(l_Json);
                var l_Url = "https://" + Settings.GrepolisWorldServer + "/game/frontend_bridge?town_id=" +
                            m_CurrentTownID + "&action=refetch&h=" + H + "&json=" + l_Json + "&_=" +
                            ExpandedServerTime;
                GetRequest(new Uri(l_Url));
                CallLogEvent("Updating Town Data.");
            }
            catch (Exception ex)
            {
                Logger.WriteExceptionToLog(ex.Message);
            }
        }

        /// <summary>
        /// Only Login. Request for all Units in all Towns.
        /// </summary>
        private void UpdateUnitsRequest()
        {
            try
            {
                var l_Json = "{\"collections\":{\"Units\":[]},\"town_id\":" + m_CurrentTownID +
                             ",\"nl_init\":false}";
                l_Json = Uri.EscapeDataString(l_Json);
                var l_Url = "https://" + Settings.GrepolisWorldServer + "/game/frontend_bridge?town_id=" +
                            m_CurrentTownID + "&action=refetch&h=" + H + "&json=" + l_Json + "&_=" +
                            ExpandedServerTime;
                GetRequest(new Uri(l_Url));
                CallLogEvent("Updating Unit Data.");
            }
            catch (Exception ex)
            {
                Logger.WriteExceptionToLog(ex.Message);
            }
        }

        /// <summary>
        /// Request for switching towns.
        /// </summary>
        private void SwitchTownRequest()
        {
            try
            {
                var l_Town = Player.Towns.Single(x => x.TownID == m_ControllerQueueDataQueue.First()["townid"]);
                var l_Url = "https://" + Settings.GrepolisWorldServer + "/game/frontend_bridge?town_id=" + l_Town.TownID +
                            "&action=execute&h=" + H;
                PostRequest(new Uri(l_Url), new NameValueCollection() { { "json",
                        "{\"model_url\":\"CommandsMenuBubble/" + Player.PlayerID + "\",\"action_name\":\"forceUpdate\",\"arguments\":{},\"town_id\":" + l_Town.TownID + ",\"nl_init\":true}" } });
                CallLogEvent("Switching to " + l_Town.TownName);
            }
            catch (Exception ex)
            {
                Logger.WriteExceptionToLog(ex.Message);
            }
        }

        /// <summary>
        /// Request to locate and update the farmers of a single town.
        /// </summary>
        private void LocateFarmersRequest()
        {
            try
            {
                //skip if town has conqueror
                if (m_Town.HasConqueror)
                {
                    m_ControllerQueue.RemoveFirst();
                    m_RequestTimer.Start();
                    return;
                }

                var l_Json = "{\"chunks\":[{\"x\":" + m_Town.ChunkX + ",\"y\":" + m_Town.ChunkY +
                             ",\"timestamp\":0}],\"town_id\":" +
                             m_Town.TownID + ",\"nl_init\":true}";
                l_Json = Uri.EscapeDataString(l_Json);

                var l_Url = "https://" + Settings.GrepolisWorldServer + "/game/map_data?town_id=" +
                            m_Town.TownID + "&action=get_chunks&h=" + H + "&json=" + l_Json + "&_=" +
                            ExpandedServerTime;
                GetRequest(new Uri(l_Url));
                CallLogEvent(m_Town.TownName + ": Locating Farmers.");
            }
            catch (Exception ex)
            {
                Logger.WriteExceptionToLog(ex.Message);
            }
        }

        /// <summary>
        /// Request to emulate Opening the Farmer Window. Response is useless.
        /// </summary>
        private void OpenFarmerWindowRequest()
        {
            try
            {
                //json:{"window_type":"farm_town","tab_type":"index","known_data":{"models":["PlayerKillpoints","PremiumFeatures"],"collections":["FarmTownPlayerRelations","FarmTowns","Towns"],"templates":[]},"arguments":{"farm_town_id":XXX},"town_id":XXX,"nl_init":true}
                //Get Farmer
                var l_Farmer =
                    Player.Towns.Single(x => x.TownID == m_CurrentTownID)
                        .Farmers.Single(x => x.ID == m_ControllerQueueDataQueue.First()["farmid"]);
                var l_Json =
                    "{\"window_type\":\"farm_town\",\"tab_type\":\"index\",\"known_data\":{\"models\":[\"PlayerKillpoints\",\"PremiumFeatures\"],\"collections\":[\"FarmTownPlayerRelations\",\"FarmTowns\",\"Towns\"],\"templates\":[]},\"arguments\":{\"farm_town_id\":" +
                    l_Farmer.ID + "},\"town_id\":" + m_CurrentTownID + ",\"nl_init\":true}";
                l_Json = Uri.EscapeDataString(l_Json);

                var l_Url = "https://" + Settings.GrepolisWorldServer + "/game/frontend_bridge?town_id=" + m_CurrentTownID +
                            "&action=fetch&h=" + H + "&json=" + l_Json + "&_=" + ExpandedServerTime;
                GetRequest(new Uri(l_Url));
                CallLogEvent(m_Town.TownName + ": Demanding resources from " + l_Farmer.Name + ".");
            }
            catch (Exception ex)
            {
                Logger.WriteExceptionToLog(ex.Message);
            }
        }

        /// <summary>
        /// Request for information about the opend Farmer.
        /// </summary>
        private void GetTownSpecificDataRequest()
        {
            try
            {
                //{"model_url":"FarmTownPlayerRelation/XXXXX","action_name":"getTownSpecificData","arguments":{"farm_town_id":1324},"town_id":5108,"nl_init":true}
                var l_Farmer = m_Town.Farmers.Single(x => x.ID == m_ControllerQueueDataQueue.First()["farmid"]);
                var l_Url = "https://" + Settings.GrepolisWorldServer + "/game/frontend_bridge?town_id=" + m_CurrentTownID +
                            "&action=execute&h=" + H;
                PostRequest(new Uri(l_Url), new NameValueCollection() { {"json", "{\"model_url\":\"FarmTownPlayerRelation/" + l_Farmer.BattlePointsFarmID +
                             "\",\"action_name\":\"getTownSpecificData\",\"arguments\":{\"farm_town_id\":" + l_Farmer.ID +
                             "},\"town_id\":" + m_CurrentTownID + ",\"nl_init\":true}"} });
                //CallLogEvent("Demanding resources from " + l_Farmer.Name + "(2)");
            }
            catch (Exception ex)
            {
                Logger.WriteExceptionToLog(ex.Message);
            }
        }

        /// <summary>
        /// Request for looting resources from farmer.
        /// </summary>
        private void LootFarmerRequest()
        {
            try
            {
                //{"model_url":"FarmTownPlayerRelation/24886","action_name":"claim","arguments":{"farm_town_id":1323,"type":"resources","option":1},"town_id":5108,"nl_init":true}
                var l_Farmer = m_Town.Farmers.Single(x => x.ID == m_ControllerQueueDataQueue.First()["farmid"]);
                var l_Url = "https://" + Settings.GrepolisWorldServer + "/game/frontend_bridge?town_id=" + m_CurrentTownID +
                            "&action=execute&h=" + H;
                PostRequest(new Uri(l_Url), new NameValueCollection() { {"json", "{\"model_url\":\"FarmTownPlayerRelation/" + l_Farmer.BattlePointsFarmID +
                             "\",\"action_name\":\"claim\",\"arguments\":{\"farm_town_id\":" + l_Farmer.ID +
                             ",\"type\":\"resources\",\"option\":" + m_ControllerQueueDataQueue.First()["option"] + "},\"town_id\":" + m_CurrentTownID + ",\"nl_init\":true}"} });
                //CallLogEvent("Demanding resources from " + l_Farmer.Name + "(3)");
            }
            catch (Exception ex)
            {
                Logger.WriteExceptionToLog(ex.Message);
            }
        }

        /// <summary>
        /// Request for checking resources and deciding if town trades.
        /// </summary>
        private void CheckTradeRequest()
        {
            try
            {
                //skip if town has conqueror
                if (m_Town.HasConqueror)
                {
                    m_ControllerQueue.RemoveFirst();
                    m_RequestTimer.Start();
                    return;
                }

                //Get nearest towns
                var l_NearestTowns = Player.GetTownsSortedByDistance(m_Town.TownID);

                Town l_TargetTown = null;

                foreach (var l_NearestTownid in l_NearestTowns)
                {
                    var l_NearestTown = Player.Towns.Single(x => x.TownID == l_NearestTownid);

                    if (!l_NearestTown.HasEnoughResources &&
                        !l_NearestTown.HasConqueror &&
                        l_NearestTown.TradeEnabled &&
                        (l_NearestTown.TradeMode == TradingModes.Receive ||
                         l_NearestTown.TradeMode == TradingModes.Spycave &&
                         l_NearestTown.GetDistance(m_Town.IslandX, m_Town.IslandY) < m_Town.TradeMaxDistance))
                    {
                        l_TargetTown = l_NearestTown;
                        break;
                    }
                }

                //if no town found
                if (l_TargetTown == null)
                {
                    CallLogEvent(m_Town.TownName + ": No towns for trading found.");
                    m_ControllerQueue.RemoveFirst();
                    m_RequestTimer.Start();
                    return;
                }

                //Create server request
                //id = town that receives
                //town_id = sender
                m_ControllerQueueDataQueue.AddFirst(new Dictionary<string, string>() { { "receivertownid", l_TargetTown.TownID } });

                var l_Json = "{\"id\":" + l_TargetTown.TownID + ",\"town_id\":" + m_Town.TownID + ",\"nl_init\":true}";
                l_Json = Uri.EscapeDataString(l_Json);

                var l_Url = "https://" + Settings.GrepolisWorldServer + "/game/town_info?town_id=" + m_CurrentTownID +
                            "&action=trading&h=" + H + "&json=" + l_Json + "&_=" + ExpandedServerTime;
                GetRequest(new Uri(l_Url));
                CallLogEvent(m_Town.TownName + ": Starting trade between " + m_Town.TownName + " and " + l_TargetTown.TownName + ".");
            }
            catch (Exception ex)
            {
                Logger.WriteExceptionToLog(ex.Message);
            }
        }

        /// <summary>
        /// Request for sending resources to another town.
        /// </summary>
        private void SendResourcesRequest()
        {
            try
            {
                var l_TargetTown =
                    Player.Towns.Single(x => x.TownID == m_ControllerQueueDataQueue.First()["receivertownid"]); //Receiver

                //Get available resources to send
                var l_MaxSendWood = Math.Max(0, m_Town.Wood - m_Town.TradeWoodRemaining);
                var l_MaxSendStone = Math.Max(0, m_Town.Stone - m_Town.TradeStoneRemaining);
                var l_MaxSendIron = Math.Max(0, m_Town.Iron - m_Town.TradeIronRemaining);

                //Check if enough resources are available
                if ((m_Town.FreeTradeCapacity <= m_Town.TradeMinSendAmount) ||
                    (l_MaxSendWood <= 100 && l_MaxSendStone <= 100 && l_MaxSendIron <= 100))
                {
                    CallLogEvent(m_Town.TownName + ": Not enough resources to send.");
                    m_ControllerQueueDataQueue.RemoveFirst();
                    m_ControllerQueue.RemoveFirst();
                    m_RequestTimer.Start();
                    return;
                }

                //Check if receiver town needs resources
                var l_MaxNeededWood = Math.Max(0,
                      ((l_TargetTown.Storage * l_TargetTown.TradePercentageWarehouse) / 100) - l_TargetTown.Wood +
                        l_TargetTown.WoodInc);
                var l_MaxNeededStone = Math.Max(0,
                    ((l_TargetTown.Storage * l_TargetTown.TradePercentageWarehouse) / 100) -
                    (l_TargetTown.Stone + l_TargetTown.StoneInc));
                var l_MaxNeededIron = Math.Max(0,
                    ((l_TargetTown.Storage * l_TargetTown.TradePercentageWarehouse) / 100) -
                    (l_TargetTown.Iron + l_TargetTown.IronInc));

                if ((l_MaxNeededWood + l_MaxNeededStone + l_MaxNeededIron <= m_Town.TradeMinSendAmount) ||
                    (l_MaxNeededWood <= 100 && l_MaxNeededStone <= 100 && l_MaxNeededIron <= 100))
                {
                    CallLogEvent(m_Town.TownName + ": Receiver " + l_TargetTown.TownName + " has enough resources.");
                    m_ControllerQueueDataQueue.RemoveFirst();
                    m_ControllerQueue.RemoveFirst();
                    m_RequestTimer.Start();
                    return;
                }

                //Calculate amount of resources to send
                var l_SendWood = Math.Min(l_MaxSendWood, l_MaxNeededWood);
                var l_SendStone = Math.Min(l_MaxSendStone, l_MaxNeededStone);
                var l_SendIron = Math.Min(l_MaxSendIron, l_MaxNeededIron);

                while (true)
                {
                    if (l_SendWood + l_SendStone + l_SendIron > m_Town.FreeTradeCapacity)
                    {
                        if (l_SendWood >= l_SendStone && l_SendWood >= l_SendIron)
                            l_SendWood -= 100;
                        else if (l_SendStone >= l_SendWood && l_SendStone >= l_SendIron)
                            l_SendStone -= 100;
                        else
                            l_SendIron -= 100;
                    }
                    else
                    {
                        l_SendWood = Math.Max(0, l_SendWood);
                        l_SendStone = Math.Max(0, l_SendStone);
                        l_SendIron = Math.Max(0, l_SendIron);
                        //Ready to send resources. Exit loop.
                        break;
                    }
                }

                if (l_SendWood + l_SendStone + l_SendIron <= m_Town.TradeMinSendAmount)
                {
                    //Not enough *specific* resources to send. Sender could still have enough of a resource that another town needs.
                    CallLogEvent(m_Town.TownName + ": Not enough resources for " + l_TargetTown.TownName +
                                 " / trade capacity. (Trade capacity: " + m_Town.FreeTradeCapacity + ", Wood: " +
                                 m_Town.Wood + ", Stone: " + m_Town.Stone + ", Silver: " + m_Town.Iron +
                                 ")");
                    m_ControllerQueueDataQueue.RemoveFirst();
                    m_ControllerQueue.RemoveFirst();
                    m_RequestTimer.Start();
                    return;
                }

                var l_Url = "https://" + Settings.GrepolisWorldServer + "/game/town_info?town_id=" + m_CurrentTownID +
                            "&action=trade&h=" + H;
                PostRequest(new Uri(l_Url),
                    new NameValueCollection()
                    {
                        {
                            "json",
                            "{\"id\":" + l_TargetTown.TownID + ",\"wood\":" +
                            l_SendWood + ",\"stone\":" + l_SendStone + ",\"iron\":" +
                            l_SendIron + ",\"town_id\":" + m_CurrentTownID +
                            ",\"nl_init\":true}"
                        }
                    });

                CallLogEvent(m_Town.TownName + ": Sending resources to " +
                             l_TargetTown.TownName + ". (Wood: " + l_SendWood + "/" +
                             (l_TargetTown.Wood +
                              l_TargetTown.WoodInc) + ", Stone: " + l_SendStone +
                             "/" +
                             (l_TargetTown.Stone +
                              l_TargetTown.StoneInc) + ", Silver: " + l_SendIron +
                             "/" +
                             (l_TargetTown.Iron +
                              l_TargetTown.IronInc) + ")");

                m_ControllerQueueDataQueue.RemoveFirst();
            }
            catch (Exception ex)
            {
                Logger.WriteToLog(ex.Message);
            }
        }

        /// <summary>
        /// Request for updating the information about the state of cultural festivals.
        /// </summary>
        private void UpdateCulturalInfoRequest()
        {
            try
            {
                //skip if town has conqueror
                if (m_Town.HasConqueror)
                {
                    m_ControllerQueue.RemoveFirst();
                    m_RequestTimer.Start();
                    return;
                }

                var l_Json =
                    "{\"town_id\":" + m_CurrentTownID + ",\"nl_init\":true}";
                l_Json = Uri.EscapeDataString(l_Json);

                var l_Url = "https://" + Settings.GrepolisWorldServer + "/game/building_place?town_id=" + m_CurrentTownID +
                            "&action=culture&h=" + H + "&json=" + l_Json + "&_=" + ExpandedServerTime;
                GetRequest(new Uri(l_Url));
            }
            catch (Exception ex)
            {
                Logger.WriteExceptionToLog(ex.Message);
            }
        }

        /// <summary>
        /// Request for starting a cultural festival.
        /// </summary>
        private void StartCulturalFestivalRequest()
        {
            try
            {
                var l_Event = m_ControllerQueueDataQueue.First()["event"];
                var l_Url = "https://" + Settings.GrepolisWorldServer + "/game/building_place?town_id=" + m_CurrentTownID +
                            "&action=start_celebration&h=" + H;
                PostRequest(new Uri(l_Url),
                    new NameValueCollection()
                    {
                        {
                            "json",
                            "{\"celebration_type\":\"" + l_Event + "\",\"town_id\":" + m_CurrentTownID +
                            ",\"nl_init\":true}"
                        }
                    });
            }
            catch (Exception ex)
            {
                Logger.WriteExceptionToLog(ex.Message);
            }
        }

        /// <summary>
        /// Open the main building and update building data.
        /// </summary>
        private void CheckMainBuildingRequest()
        {
            try
            {
                //skip if town has conqueror
                if (m_Town.HasConqueror)
                {
                    m_ControllerQueue.RemoveFirst();
                    m_RequestTimer.Start();
                    return;
                }

                var l_Json =
                    "{\"town_id\":" + m_CurrentTownID + ",\"nl_init\":true}";
                l_Json = Uri.EscapeDataString(l_Json);

                var l_Url = "https://" + Settings.GrepolisWorldServer + "/game/building_main?town_id=" + m_CurrentTownID +
                            "&action=index&h=" + H + "&json=" + l_Json + "&_=" + ExpandedServerTime;
                GetRequest(new Uri(l_Url));
            }
            catch (Exception ex)
            {
                Logger.WriteExceptionToLog(ex.Message);
            }
        }

        /// <summary>
        /// Decide what to build.
        /// </summary>
        private void CheckBuildingQueueRequest()
        {
            try
            {
                //Building for town enabled?
                if (!m_Town.BuildingQueueEnabled)
                {
                    CallLogEvent(m_Town.TownName + ": Building queue is disabled.");
                    m_ControllerQueue.RemoveFirst();
                    m_RequestTimer.Start();
                    return;
                }

                Buildings? l_Building = null;
                Buildings? l_Teardown = null;

                //Check if a Farm is needed
                if (m_Town.PopulationAvailable <= Settings.BuildFarmBelow)
                {
                    if (m_Town.IngameBuildingQueue.All(x => x.DevName != Buildings.farm) &&
                        m_Town.Buildings.Single(x => x.DevName == Buildings.farm).Upgradable &&
                        m_Town.Buildings.Single(x => x.DevName == Buildings.farm).Level + 1 <=
                        m_Town.Buildings.Single(x => x.DevName == Buildings.farm).MaxLevelCombined)
                    {
                        l_Building = Buildings.farm;
                    }
                }

                if (l_Building == null)
                {
                    //Build target on and not advanced queue or building queue empty
                    if (m_Town.BuildingTargetEnabled && (!Settings.AdvancedQueue || m_Town.BotBuildingQueue.Count == 0))
                    {
                        //Automatic mode activad
                        var l_LowestLevel = 50;
                        var l_HighestLevel = 0;
                        int l_Level;
                        Buildings l_BuildingTemp;
                        foreach (var building in m_Town.Buildings)
                        {
                            l_BuildingTemp = building.DevName;
                            l_Level = building.Level + 1;

                            if (building.Upgradable)
                            {
                                if (l_Level <= building.TargetLevel)
                                {
                                    if (l_Level < l_LowestLevel)
                                    {
                                        l_Building = l_BuildingTemp;
                                        l_LowestLevel = l_Level;
                                    }
                                }
                            }
                        }
                        //Search building to demolish when no upgradable building is found
                        //Highest level building is downgraded first
                        //Requirement for demolishing is Senate (main) building lvl 10
                        if (l_Building == null &&
                            m_Town.Buildings.Single(x => x.DevName == Buildings.main).Level >= 10 &&
                            m_Town.DowngradeEnabled)
                        {
                            foreach (var building in m_Town.Buildings)
                            {
                                l_BuildingTemp = building.DevName;
                                //l_Level = m_Player.Towns[m_CurrentTownIntern].Buildings[i].NextLevel -1;//Uses next level to take buildings currently in the ingame queue into account
                                l_Level = building.Level;
                                if (building.Teardownable)
                                {
                                    if (l_Level > building.TargetLevel)
                                    {
                                        if (l_Level > l_HighestLevel)
                                        {
                                            l_Teardown = l_BuildingTemp;
                                            l_HighestLevel = l_Level;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    //else use queue
                    else
                    {
                        //Normal mode activated
                        if (m_Town.BotBuildingQueue.Count > 0)
                        {
                            //Check if max level is already reached
                            //Delete all that reached max level
                            var l_NextBuilding = m_Town.BotBuildingQueue.First();
                            while (m_Town.Buildings.Single(x => x.DevName == l_NextBuilding).Level + 1 >
                                   m_Town.Buildings.Single(x => x.DevName == l_NextBuilding).MaxLevelCombined)
                            {
                                m_Town.BotBuildingQueue.RemoveAt(0);
                                if (m_Town.BotBuildingQueue.Count > 0)
                                    l_NextBuilding = m_Town.BotBuildingQueue.First();
                                else
                                    break;
                            }
                        }

                        if (m_Town.BotBuildingQueue.Count > 0)
                        {
                            //Check if upgradable
                            var l_NextBuilding = m_Town.BotBuildingQueue.First();
                            if (m_Town.Buildings.Single(x => x.DevName == l_NextBuilding).Upgradable)
                            {
                                l_Building = l_NextBuilding;
                            }
                        }
                    }
                }

                if (l_Building == null && l_Teardown == null)
                {
                    CallLogEvent(m_Town.TownName + ": Nothing to build.");
                    m_ControllerQueue.RemoveFirst();
                    m_RequestTimer.Start();
                    return;
                }

                //Adding different controller state to queue for teardown.
                if (l_Teardown != null)
                {
                    CallLogEvent(m_Town.TownName + ": Demolishing " + l_Teardown + " .");
                    m_ControllerQueue.RemoveFirst();
                    m_ControllerQueue.AddFirst(ControllerStates.CheckBuildingQueueTeardown);
                    m_ControllerQueueDataQueue.AddFirst(new Dictionary<string, string>()
                    {
                        {"building", l_Teardown.ToString()}
                    });
                    m_RequestTimer.Start();
                    return;
                }

                CallLogEvent(m_Town.TownName + ": Adding " + l_Building + " to ingame queue.");

                var l_Url = "https://" + Settings.GrepolisWorldServer + "/game/frontend_bridge?town_id=" +
                            m_CurrentTownID +
                            "&action=execute&h=" + H;
                PostRequest(new Uri(l_Url),
                    new NameValueCollection()
                    {
                        {
                            "json",
                            "{\"model_url\":\"BuildingOrder\",\"action_name\":\"buildUp\",\"arguments\":{\"building_id\":\"" +
                            l_Building + "\"},\"town_id\":" + m_CurrentTownID + ",\"nl_init\":true}"
                        }
                    });
            }
            catch (Exception ex)
            {
                Logger.WriteExceptionToLog(ex.Message);
            }
        }

        /// <summary>
        /// Teardown a building.
        /// </summary>
        private void CheckBuildingQueueTeardownRequest()
        {
            try
            {
                var l_Teardown = m_ControllerQueueDataQueue.First()["building"];

                var l_Url = "https://" + Settings.GrepolisWorldServer + "/game/frontend_bridge?town_id=" +
                            m_CurrentTownID +
                            "&action=execute&h=" + H;
                PostRequest(new Uri(l_Url),
                    new NameValueCollection
                    {
                        {
                            "json",
                            "{\"model_url\":\"BuildingOrder\",\"action_name\":\"tearDown\",\"arguments\":{\"building_id\":\"" +
                            l_Teardown + "\"},\"town_id\":" + m_CurrentTownID + ",\"nl_init\":true}"
                        }
                    });


                m_ControllerQueueDataQueue.RemoveFirst();
            }
            catch (Exception ex)
            {
                Logger.WriteExceptionToLog(ex.Message);
            }
        }

        /// <summary>
        /// Request for opening the Barracks window.
        /// </summary>
        private void OpenBarracksWindowRequest()
        {
            try
            {
                //skip if town has conqueror
                if (m_Town.HasConqueror)
                {
                    m_ControllerQueue.RemoveFirst();
                    m_RequestTimer.Start();
                    return;
                }

                var l_Json =
                    "{\"town_id\":" + m_CurrentTownID + ",\"nl_init\":true}";
                l_Json = Uri.EscapeDataString(l_Json);

                var l_Url = "https://" + Settings.GrepolisWorldServer + "/game/building_barracks?town_id=" + m_CurrentTownID +
                            "&action=index&h=" + H + "&json=" + l_Json + "&_=" + ExpandedServerTime;

                CallLogEvent(m_Town.TownName + ": Checking land unit queue.");

                GetRequest(new Uri(l_Url));
            }
            catch (Exception ex)
            {
                Logger.WriteExceptionToLog(ex.Message);
            }
        }

        /// <summary>
        /// Request for building land units.
        /// </summary>
        private void CheckLandUnitQueueRequest()
        {
            try
            {
                //Check if it's possible to queue an unit at this moment
                if (m_Town.UnitQueueEnabled && m_Town.LandUnitQueueSize < 7 && m_Town.LandUnitQueueSize < Settings.QueueLimit)
                {
                    //Check which unit you need to queue
                    ArmyUnit l_Unit = null;
                    var l_Trainable = -1;

                    foreach (var l_ArmyUnit in m_Town.ArmyUnits)
                    {
                        if (!l_ArmyUnit.IsFromBarracks) continue;
                        if (l_ArmyUnit.QueueBot <= 0) continue;
                        if (l_ArmyUnit.QueueBot - (l_ArmyUnit.TotalAmount + l_ArmyUnit.QueueGame) <= 0 ||
                            l_ArmyUnit.MaxBuild <= l_Trainable) continue;

                        l_Unit = l_ArmyUnit;
                        l_Trainable = l_ArmyUnit.MaxBuild;
                    }

                    if (l_Unit != null)
                    {
                        //Check how many you can queue
                        var l_Amount = l_Unit.QueueBot - (l_Unit.TotalAmount + l_Unit.QueueGame);
                        if (l_Unit.MaxBuild < l_Amount) //if you can build less units than needed build maxbuild
                        {
                            l_Amount = l_Unit.MaxBuild;
                            //check if total queue pop is enough
                            var l_QueuePop = l_Amount * l_Unit.Population;
                            if (l_QueuePop < Settings.MinUnitQueuePop) //cant build enough units
                                l_Amount = 0;//When l_Amount is 0 it will skip the queue
                        }
                        //Final check
                        if (l_Amount > 0)
                        { 
                            //http://###.grepolis.com/game/building_barracks?action=build&town_id=#####&h=###########
                            //l_Content.Add("json", "{\"unit_id\":\"" + m_Player.Towns[m_CurrentTownIntern].ArmyUnits[l_UnitIndex].Name + "\",\"amount\":" + l_Amount.ToString() + ",\"town_id\":\"" + m_Player.Towns[m_CurrentTownIntern].TownID + "\",\"nlreq_id\":" + m_Nlreq_id + "}");//json={"unit_id":"archer","amount":#,"town_id":"#####","nlreq_id":##}

                            CallLogEvent(m_Town.TownName + ": Adding " + l_Unit.Name + " (" + l_Amount + ") to the ingame queue.");

                            var l_Url = "https://" + Settings.GrepolisWorldServer + "/game/building_barracks?town_id=" +
                                        m_CurrentTownID +
                                        "&action=build&h=" + H;
                            PostRequest(new Uri(l_Url),
                                new NameValueCollection()
                                {
                                    {
                                        "json",
                                        "{\"unit_id\":\"" + l_Unit.Name + "\",\"amount\":" + l_Amount + ",\"town_id\":" + m_CurrentTownID + ",\"nl_init\":true}"
                                    }
                                });
                            return;
                        }
                        //Unit queue skipped. Max trainable units is 0.
                        CallLogEvent(m_Town.TownName + ": Unit queue skipped (Not enough resources or population).");
                    }
                    else
                    {
                        //Unit queue skipped. Cannot find trainable units.
                        CallLogEvent(m_Town.TownName + ": Unit queue skipped (Can't find trainable units.)");
                    }
                }
                else
                {
                    CallLogEvent(m_Town.TownName + ": Army queue is disabled for this town or queue is full.");
                }
                m_ControllerQueue.RemoveFirst();
                m_RequestTimer.Start();
            }
            catch (Exception ex)
            {
                Logger.WriteExceptionToLog(ex.Message);
            }
        }

        /// <summary>
        /// Open the docks window.
        /// </summary>
        public void OpenDocksWindowRequest()
        {
            try
            {
                //skip if town has conqueror
                if (m_Town.HasConqueror)
                {
                    m_ControllerQueue.RemoveFirst();
                    m_RequestTimer.Start();
                    return;
                }

                var l_Json =
                    "{\"town_id\":" + m_CurrentTownID + ",\"nl_init\":true}";
                l_Json = Uri.EscapeDataString(l_Json);

                var l_Url = "https://" + Settings.GrepolisWorldServer + "/game/building_docks?town_id=" + m_CurrentTownID +
                            "&action=index&h=" + H + "&json=" + l_Json + "&_=" + ExpandedServerTime;

                CallLogEvent(m_Town.TownName + ": Checking navy unit queue.");

                GetRequest(new Uri(l_Url));
            }
            catch (Exception ex)
            {
                Logger.WriteExceptionToLog(ex.Message);
            }
        }

        /// <summary>
        /// Request for building navy units.
        /// </summary>
        public void CheckNavyUnitQueueRequest()
        {
            try
            {
                //Check if it's possible to queue an unit at this moment
                if (m_Town.UnitQueueEnabled && m_Town.NavyUnitQueueSize < 7 && m_Town.NavyUnitQueueSize < Settings.QueueLimit)
                {
                    //Check which unit you need to queue
                    ArmyUnit l_Unit = null;
                    var l_Trainable = -1;

                    foreach (var l_ArmyUnit in m_Town.ArmyUnits)
                    {
                        if (l_ArmyUnit.IsFromBarracks) continue;
                        if (l_ArmyUnit.QueueBot <= 0) continue;
                        if (l_ArmyUnit.QueueBot - (l_ArmyUnit.TotalAmount + l_ArmyUnit.QueueGame) <= 0 ||
                            l_ArmyUnit.MaxBuild <= l_Trainable) continue;

                        l_Unit = l_ArmyUnit;
                        l_Trainable = l_ArmyUnit.MaxBuild;
                    }

                    if (l_Unit != null)
                    {
                        //Check how many you can queue
                        var l_Amount = l_Unit.QueueBot - (l_Unit.TotalAmount + l_Unit.QueueGame);
                        if (l_Unit.MaxBuild < l_Amount) //if you can build less units than needed build maxbuild
                        {
                            l_Amount = l_Unit.MaxBuild;
                            //check if total queue pop is enough
                            var l_QueuePop = l_Amount * l_Unit.Population;
                            if (l_QueuePop < Settings.MinUnitQueuePop) //cant build enough units
                                l_Amount = 0;//When l_Amount is 0 it will skip the queue
                        }
                        //Final check
                        if (l_Amount > 0)
                        {
                            
                            //http://###.grepolis.com/game/building_barracks?action=build&town_id=#####&h=###########
                            //l_Content.Add("json", "{\"unit_id\":\"" + m_Player.Towns[m_CurrentTownIntern].ArmyUnits[l_UnitIndex].Name + "\",\"amount\":" + l_Amount.ToString() + ",\"town_id\":\"" + m_Player.Towns[m_CurrentTownIntern].TownID + "\",\"nlreq_id\":" + m_Nlreq_id + "}");//json={"unit_id":"archer","amount":#,"town_id":"#####","nlreq_id":##}

                            CallLogEvent(m_Town.TownName + ": Adding " + l_Unit.Name + " (" + l_Amount + ") to the ingame queue.");

                            var l_Url = "https://" + Settings.GrepolisWorldServer + "/game/building_docks?town_id=" +
                                        m_CurrentTownID +
                                        "&action=build&h=" + H;
                            PostRequest(new Uri(l_Url),
                                new NameValueCollection()
                                {
                                    {
                                        "json",
                                        "{\"unit_id\":\"" + l_Unit.Name + "\",\"amount\":" + l_Amount + ",\"town_id\":" + m_CurrentTownID + ",\"nl_init\":true}"
                                    }
                                });
                            return;
                        }
                        //Unit queue skipped. Max trainable units is 0.
                        CallLogEvent(m_Town.TownName + ": Navy queue skipped (Not enough resources or population).");
                    }
                    else
                    {
                        //Unit queue skipped. Cannot find trainable units.
                        CallLogEvent(m_Town.TownName + ": Navy queue skipped (Can't find trainable units).");
                    }
                }
                else
                {
                    CallLogEvent(m_Town.TownName + ": Navy queue is disabled for this town or queue is full.");
                }
                m_ControllerQueue.RemoveFirst();
                m_RequestTimer.Start();
            }
            catch (Exception ex)
            {
                Logger.WriteExceptionToLog(ex.Message);
            }
        }

        #endregion Requests

        #region Response

        /// <summary>
        /// Handles which method should parse the server response received from Grepolis server.
        /// </summary>
        private void ResponseManager(string p_Response)
        {
            if (m_HttpHandler.IsBusy)
            {
                m_HttpHandler.CancelAsync();
                CallLogEvent("Active Request canceled.");
            }

            if (p_Response.Contains("”"))
                p_Response = p_Response.Replace("”", "\"");

            var l_ValidCode = ValidateResponse(p_Response);
            if (l_ValidCode != 1)
            {
                ProcessValidatedResponse(l_ValidCode);
                return;
            }

            RequestSuccessfulEvent?.Invoke(this, new CustomArgs());

            UpdateServerTime(p_Response);

            switch (State)
            {
                case ControllerStates.UpdateGameData:
                    UpdateGameDataResponse(p_Response);
                    break;

                case ControllerStates.BuildingBuildData:
                    BuildingBuildDatasResponse(p_Response);
                    break;

                case ControllerStates.UpdateTowns:
                    UpdateTownsResponse(p_Response);
                    break;

                case ControllerStates.UpdateUnits:
                    UpdateUnitsResponse(p_Response);
                    //With this request the Login is complete.
                    CallLogEvent("Login successful.");
                    LoginSuccessfulEvent?.Invoke(this, new CustomArgs());
                    LoggedIn = true;
                    break;

                case ControllerStates.SwitchTown:
                    SwitchTownResponse(p_Response);
                    break;

                case ControllerStates.LocateFarmers:
                    LocateFarmersResponse(p_Response);
                    break;

                case ControllerStates.OpenFarmerWindow:
                    OpenFarmerWindowResponse(p_Response);
                    break;

                case ControllerStates.GetTownSpecificData:
                    GetTownSpecificDataResponse(p_Response);
                    break;

                case ControllerStates.LootFarmer:
                    LootFarmerResponse(p_Response);
                    break;

                case ControllerStates.CheckTrade:
                    CheckTradeResponse(p_Response);
                    break;

                case ControllerStates.SendResources:
                    SendResourcesResponse(p_Response);
                    break;

                case ControllerStates.UpdateCulturalInfo:
                    UpdateCulturalInfoResponse(p_Response);
                    break;

                case ControllerStates.StartCulturalFestival:
                    StartCulturalFestivalResponse(p_Response);
                    break;

                case ControllerStates.CheckMainBuilding:
                    CheckMainBuildingResponse(p_Response);
                    break;

                case ControllerStates.CheckBuildingQueue:
                    CheckBuildingQueueResponse(p_Response);
                    break;
                case ControllerStates.CheckBuildingQueueTeardown:
                    CheckBuildingQueueTeardownResponse(p_Response);
                    break;
                case ControllerStates.OpenBarracksWindow:
                    OpenBarracksWindowResponse(p_Response);
                    break;
                case ControllerStates.CheckLandUnitQueue:
                    CheckLandUnitQueueResponse(p_Response);
                    break;
                case ControllerStates.OpenDocksWindow:
                    OpenDocksWindowResponse(p_Response);
                    break;
                case ControllerStates.CheckNavyUnitQueue:
                    CheckNavyUnitQueueResponse(p_Response);
                    break;
            }

            AddNotifications(p_Response);

            //Start Request Timer for next Request.
            m_RequestTimer.Start();
        }

        /// <summary>
        /// Validate Response
        /// Codes:
        /// 1 = OK
        /// 2 = Reconnect necessarily
        /// 3 = Server error
        /// 4 = sec_check_failed
        /// </summary>
        /// <returns>Reponse Code</returns>
        private int ValidateResponse(string p_Response)
        {
            int l_ValidCode;
            //58 {"redirect":"http:\/\/beta.grepolis.com\/start?nosession"}8|10 _srvtime1303484209
            if (!p_Response.Contains("/start?action=login") && !p_Response.Contains("start?nosession"))//Checks if you're still connected with the server
            {
                if (p_Response.Contains(": startIndex"))//Error caused by game engine
                {
                    l_ValidCode = 3;
                }
                else if (p_Response.Contains("start?sec_check_failed"))//What causes this error??
                {
                    l_ValidCode = 4;
                }
                else if (p_Response.Contains("{\"error\":\""))
                {
                    l_ValidCode = 5; //Ingame error
                }
                else
                {
                    //Add future checks here
                    l_ValidCode = 1;
                }
            }
            else//you're disconnected from server
            {
                l_ValidCode = 2;
            }

            return l_ValidCode;
        }

        /// <summary>
        /// Handles what to do with failed Responses.
        /// </summary>
        private void ProcessValidatedResponse(int p_ValidCode)
        {
            switch (p_ValidCode)
            {
                case 2://Reconnect necessarily
                    CallLogEvent("Server response indicated that you were no longer logged in. " + State.ToString("G"));
                    StartReconnect(); //1
                    break;

                case 3://Game server error, reconnect necessarily
                    CallLogEvent("Server response indicated that there was a gameserver error. " + State.ToString("G"));
                    StartReconnect(); //1
                    break;

                case 4:
                    CallLogEvent("Server response indicated the following error: sec_check_failed. " + State.ToString("G"));
                    StartReconnect(); //2
                    break;

                case 5:
                    RetryManager();
                    CallLogEvent("Ingame error in " + State.ToString("G"));
                    break;
            }
        }

        /// <summary>
        /// Updates the Server time in Controller and current Town.
        /// </summary>
        private void UpdateServerTime(string p_Response)
        {
            try
            {
                const string l_Search = "_srvtime\":";
                var l_Index = p_Response.IndexOf(l_Search, 0, StringComparison.Ordinal);
                ServerTime = p_Response.Substring(l_Index + l_Search.Length, 10);
                ServerTime = Join("", Regex.Split(ServerTime, "[^\\d]"));
                //Player.Towns.Single(x => x.TownID == m_CurrentTownID).ServerTime = ServerTime;
            }
            catch (Exception ex)
            {
                Logger.WriteExceptionToLog(ex.Message);
            }
        }

        /// <summary>
        /// Loads all Notification from the Response.
        /// </summary>
        private void AddNotifications(string p_Response)
        {
            try
            {
                var l_Search = "\"notifications\":[";
                var l_Index = p_Response.IndexOf(l_Search, 0, StringComparison.Ordinal);
                if (l_Index != -1)
                {
                    var l_Response = p_Response.Substring(l_Index, p_Response.Length - l_Index);
                    l_Index = l_Response.IndexOf(l_Search, 0, StringComparison.Ordinal);
                    string l_Notifications;
                    if (l_Response.Contains("}],"))
                    {
                        //Uses LastIndexOf because hero data also uses "}],"
                        //If there is another "}]," after the notifications data set it might cause issues.
                        //Solution, search for "}],\"" using IndexOf
                        l_Notifications = l_Response.Substring(l_Index + l_Search.Length,
                            l_Response.LastIndexOf("}],", StringComparison.Ordinal) - (l_Index + l_Search.Length));
                        l_Notifications += "}";
                    }
                    else
                        l_Notifications = l_Response.Substring(l_Index + l_Search.Length,
                            l_Response.IndexOf("]", l_Index + l_Search.Length, StringComparison.Ordinal) -
                            (l_Index + l_Search.Length));
                    if (l_Notifications.Length > 0)
                    {
                        l_Search = "\"id\":";
                        l_Index = l_Notifications.IndexOf(l_Search, 0, StringComparison.Ordinal);
                        while (l_Index != -1)
                        {
                            var l_Notify_id = l_Notifications.Substring(l_Index + l_Search.Length,
                                l_Notifications.IndexOf(",", l_Index + l_Search.Length, StringComparison.Ordinal) -
                                (l_Index + l_Search.Length));
                            l_Search = "\"time\":";
                            l_Index = l_Notifications.IndexOf(l_Search, l_Index, StringComparison.Ordinal);
                            var l_Time = l_Notifications.Substring(l_Index + l_Search.Length,
                                l_Notifications.IndexOf(",", l_Index + l_Search.Length, StringComparison.Ordinal) -
                                (l_Index + l_Search.Length));
                            l_Search = "\"type\":\"";
                            l_Index = l_Notifications.IndexOf(l_Search, l_Index, StringComparison.Ordinal);
                            var l_Type = l_Notifications.Substring(l_Index + l_Search.Length,
                                l_Notifications.IndexOf("\"", l_Index + l_Search.Length, StringComparison.Ordinal) -
                                (l_Index + l_Search.Length));
                            //param_str
                            string l_Subject;
                            if (l_Type.Equals("building_finished"))
                            {
                                l_Search = "building_name\\\":\\\"";
                                l_Index = l_Notifications.IndexOf(l_Search, l_Index, StringComparison.Ordinal);
                                var l_BuildingLocal = l_Notifications.Substring(l_Index + l_Search.Length,
                                    l_Notifications.IndexOf("\\\"", l_Index + l_Search.Length, StringComparison.Ordinal) -
                                    (l_Index + l_Search.Length));
                                l_BuildingLocal = Parser.FixSpecialCharacters(l_BuildingLocal);
                                l_Search = "town_name\\\":\\\"";
                                l_Index = l_Notifications.IndexOf(l_Search, l_Index, StringComparison.Ordinal);
                                var l_TownName = l_Notifications.Substring(l_Index + l_Search.Length,
                                    l_Notifications.IndexOf("\\\"", l_Index + l_Search.Length, StringComparison.Ordinal) -
                                    (l_Index + l_Search.Length));
                                l_TownName = Parser.FixSpecialCharacters(l_TownName);
                                l_Search = "new_level\\\":";
                                l_Index = l_Notifications.IndexOf(l_Search, l_Index, StringComparison.Ordinal);
                                var l_Level = l_Notifications.Substring(l_Index + l_Search.Length,
                                    l_Notifications.IndexOf(",\\", l_Index + l_Search.Length, StringComparison.Ordinal) -
                                    (l_Index + l_Search.Length));
                                l_Subject = "Expansion completed: " + l_BuildingLocal + " (" + l_Level + ") in " + l_TownName;
                            }
                            else if (l_Type.Equals("newaward"))
                            {
                                l_Search = "name\":\"";
                                l_Index = l_Notifications.IndexOf(l_Search, l_Index, StringComparison.Ordinal);
                                l_Subject = "Achievement: " +
                                            l_Notifications.Substring(l_Index + l_Search.Length,
                                                l_Notifications.IndexOf("\",", l_Index + l_Search.Length,
                                                    StringComparison.Ordinal) - (l_Index + l_Search.Length));
                            }
                            else
                            {
                                l_Search = "\"subject\":";
                                l_Index = l_Notifications.IndexOf(l_Search, l_Index, StringComparison.Ordinal);
                                l_Subject = l_Notifications.Substring(l_Index + l_Search.Length,
                                    l_Notifications.IndexOf("}", l_Index + l_Search.Length, StringComparison.Ordinal) -
                                    (l_Index + l_Search.Length));
                                if (l_Subject.StartsWith("\""))
                                    l_Subject = l_Subject.Substring(1, l_Subject.Length - 1);
                                if (l_Subject.EndsWith("\""))
                                    l_Subject = l_Subject.Substring(0, l_Subject.Length - 1);
                            }
                            l_Subject = Parser.FixSpecialCharacters(l_Subject);

                            //Add notification
                            /*
                            if (l_Type.Equals("botcheck"))
                            {
                                m_Player.addNotification(m_ServerTime, l_Notify_id, l_Time, l_Type, "Captcha notification detected");
                                logEvent("Captcha notification detected");
                                m_CaptchaDetectedTime = DateTime.Now;

                                captchaDetectedSequence(true);
                            }
                            else*/
                            if (!l_Type.Equals("backbone") && !l_Type.Equals("systemmessage"))
                            {
                                Player.AddNotification(ServerTime, l_Notify_id, l_Time, l_Type, l_Subject);
                            }
                            //Search next notification
                            l_Search = "\"id\":";
                            l_Index = l_Notifications.IndexOf(l_Search, l_Index, StringComparison.Ordinal);
                        }
                    }
                    //Update Nlreq_id
                    Nlreq_ID = Player.GetLatestNotificationID();
                }
            }
            catch (Exception ex)
            {
                Logger.WriteExceptionToLog(ex.Message);
            }
        }

        /// <summary>
        /// Update some generell Town information from notification.
        /// </summary>
        private void UpdateTownDataFromNotification(string p_Response)
        {
            if (p_Response.Contains("resources_last_update"))
            {
                var l_Search = "resources_last_update\\\":";
                var l_Index = p_Response.IndexOf(l_Search, 0, StringComparison.Ordinal);
                var l_ResourcesLastUpdate = p_Response.Substring(l_Index + l_Search.Length,
                    p_Response.IndexOf(",", l_Index + l_Search.Length, StringComparison.Ordinal) -
                    (l_Index + l_Search.Length));
                l_Search = "available_population\\\":";
                l_Index = p_Response.IndexOf(l_Search, 0, StringComparison.Ordinal);
                var l_Population = p_Response.Substring(l_Index + l_Search.Length,
                    p_Response.IndexOf(",", l_Index + l_Search.Length, StringComparison.Ordinal) -
                    (l_Index + l_Search.Length));
                l_Search = "has_conqueror\\\":";
                l_Index = p_Response.IndexOf(l_Search, l_Index, StringComparison.Ordinal);
                var l_HasConqueror = p_Response.Substring(l_Index + l_Search.Length,
                    p_Response.IndexOf(",", l_Index + l_Search.Length, StringComparison.Ordinal) -
                    (l_Index + l_Search.Length)).Equals("true");
                l_Search = "last_wood\\\":";
                l_Index = p_Response.IndexOf(l_Search, l_Index, StringComparison.Ordinal);
                var l_Wood = p_Response.Substring(l_Index + l_Search.Length,
                    p_Response.IndexOf(",", l_Index + l_Search.Length, StringComparison.Ordinal) -
                    (l_Index + l_Search.Length));
                l_Search = "last_stone\\\":";
                l_Index = p_Response.IndexOf(l_Search, l_Index, StringComparison.Ordinal);
                var l_Stone = p_Response.Substring(l_Index + l_Search.Length,
                    p_Response.IndexOf(",", l_Index + l_Search.Length, StringComparison.Ordinal) -
                    (l_Index + l_Search.Length));
                l_Search = "last_iron\\\":";
                l_Index = p_Response.IndexOf(l_Search, l_Index, StringComparison.Ordinal);
                var l_Iron = p_Response.Substring(l_Index + l_Search.Length,
                    p_Response.IndexOf(",", l_Index + l_Search.Length, StringComparison.Ordinal) -
                    (l_Index + l_Search.Length));
                l_Search = "\\\"storage\\\":";
                l_Index = p_Response.IndexOf(l_Search, 0, StringComparison.Ordinal);
                var l_Storage = p_Response.Substring(l_Index + l_Search.Length,
                    p_Response.IndexOf(",", l_Index + l_Search.Length, StringComparison.Ordinal) -
                    (l_Index + l_Search.Length));

                l_Search = "production\\\":";
                l_Index = p_Response.IndexOf(l_Search, 0, StringComparison.Ordinal);
                l_Search = "\\\"wood\\\":";
                l_Index = p_Response.IndexOf(l_Search, l_Index, StringComparison.Ordinal);
                var l_WoodProduction = p_Response.Substring(l_Index + l_Search.Length,
                    p_Response.IndexOf(",", l_Index + l_Search.Length, StringComparison.Ordinal) -
                    (l_Index + l_Search.Length));
                l_Search = "\\\"stone\\\":";
                l_Index = p_Response.IndexOf(l_Search, l_Index, StringComparison.Ordinal);
                var l_StoneProduction = p_Response.Substring(l_Index + l_Search.Length,
                    p_Response.IndexOf(",", l_Index + l_Search.Length, StringComparison.Ordinal) -
                    (l_Index + l_Search.Length));
                l_Search = "\\\"iron\\\":";
                l_Index = p_Response.IndexOf(l_Search, l_Index, StringComparison.Ordinal);
                var l_IronProduction = p_Response.Substring(l_Index + l_Search.Length,
                    p_Response.IndexOf("}", l_Index + l_Search.Length, StringComparison.Ordinal) -
                    (l_Index + l_Search.Length));

                m_Town.ResourcesLastUpdate = l_ResourcesLastUpdate;
                m_Town.PopulationAvailable = int.Parse(l_Population);
                m_Town.HasConqueror = l_HasConqueror;
                m_Town.Wood = int.Parse(l_Wood);
                m_Town.Stone = int.Parse(l_Stone);
                m_Town.Iron = int.Parse(l_Iron);
                m_Town.WoodProduction = int.Parse(l_WoodProduction);
                m_Town.StoneProduction = int.Parse(l_StoneProduction);
                m_Town.IronProduction = int.Parse(l_IronProduction);
                m_Town.Storage = int.Parse(l_Storage);
            }
        }

        /// <summary>
        /// Handles the server response for UpdateGameDataRequest().
        /// </summary>
        private void UpdateGameDataResponse(string p_Response)
        {
            try
            {
                //Villiage Count -> count of village list
                /*var l_Search = "\"villages\":";
                var l_Index = p_Response.IndexOf(l_Search, 0, StringComparison.Ordinal);
                var l_Villages = p_Response.Substring(l_Index + l_Search.Length,
                    p_Response.IndexOf(",", l_Index + l_Search.Length, StringComparison.Ordinal) -
                    (l_Index + l_Search.Length));*/

                //Culture Level etc.
                var l_CulturalLevel = 2;

                var l_Search = "\"cultural_points\":";
                var l_Index = p_Response.IndexOf(l_Search, 0, StringComparison.Ordinal);
                var l_CulturalPoints = p_Response.Substring(l_Index + l_Search.Length,
                    p_Response.IndexOf(",", l_Index + l_Search.Length, StringComparison.Ordinal) -
                    (l_Index + l_Search.Length));
                var l_CulturalPointsNext = (int)((3.0 / 2.0) * (Math.Pow(l_CulturalLevel + 1, 2.0) + (-3.0 * (l_CulturalLevel + 1) + 2.0)));
                while (int.Parse(l_CulturalPoints) >= l_CulturalPointsNext)
                {
                    l_CulturalLevel++;
                    l_CulturalPointsNext =
                        (int)((3.0 / 2.0) * (Math.Pow(l_CulturalLevel + 1, 2.0) + (-3.0 * (l_CulturalLevel + 1) + 2.0)));
                }

                Player.CulturalPointsCurrent = int.Parse(l_CulturalPoints);
                Player.CulturalPointsMax = l_CulturalPointsNext;
                Player.CultureLevel = l_CulturalLevel;

                //Player.CulturalCitiesStr = "Cities: " + l_Villages + "/" + l_CulturalLevel;

                //Player.CulturalPointsStr = l_CulturalPoints + "/" + l_CulturalPointsNext;

                //PlayerLedger (Gold)
                l_Search = "{\"model_class_name\":\"PlayerLedger\",\"data\":{";
                l_Index = p_Response.IndexOf(l_Search, 0, StringComparison.Ordinal);
                l_Search = "\"gold\":";
                l_Index = p_Response.IndexOf(l_Search, l_Index, StringComparison.Ordinal);
                var l_Gold = p_Response.Substring(l_Index + l_Search.Length,
                    p_Response.IndexOf(",", l_Index + l_Search.Length, StringComparison.Ordinal) -
                    (l_Index + l_Search.Length));
                Player.Gold = int.Parse(l_Gold);

                //PlayerGods
                l_Search = "{\"model_class_name\":\"PlayerGods\",\"data\":{";
                var l_IndexPlayerGods = p_Response.IndexOf(l_Search, 0, StringComparison.Ordinal);
                //zeus
                l_Search = "\"zeus\":{\"current\":";
                l_Index = p_Response.IndexOf(l_Search, l_IndexPlayerGods, StringComparison.Ordinal);
                var l_Favor = p_Response.Substring(l_Index + l_Search.Length,
                    p_Response.IndexOf(",", l_Index + l_Search.Length, StringComparison.Ordinal) -
                    (l_Index + l_Search.Length));
                Player.FavorZeus = int.Parse(l_Favor.Split('.')[0]);
                l_Search = "\"production\":";
                l_Index = p_Response.IndexOf(l_Search, l_Index, StringComparison.Ordinal);
                Player.FavorZeusProduction = p_Response.Substring(l_Index + l_Search.Length,
                    p_Response.IndexOf(",", l_Index + l_Search.Length, StringComparison.Ordinal) -
                    (l_Index + l_Search.Length));
                //poseidon
                l_Search = "\"poseidon\":{\"current\":";
                l_Index = p_Response.IndexOf(l_Search, l_IndexPlayerGods, StringComparison.Ordinal);
                l_Favor = p_Response.Substring(l_Index + l_Search.Length,
                    p_Response.IndexOf(",", l_Index + l_Search.Length, StringComparison.Ordinal) -
                    (l_Index + l_Search.Length));
                Player.FavorPoseidon = int.Parse(l_Favor.Split('.')[0]);
                l_Search = "\"production\":";
                l_Index = p_Response.IndexOf(l_Search, l_Index, StringComparison.Ordinal);
                Player.FavorPoseidonProduction = p_Response.Substring(l_Index + l_Search.Length,
                    p_Response.IndexOf(",", l_Index + l_Search.Length, StringComparison.Ordinal) -
                    (l_Index + l_Search.Length));
                //hera
                l_Search = "\"hera\":{\"current\":";
                l_Index = p_Response.IndexOf(l_Search, l_IndexPlayerGods, StringComparison.Ordinal);
                l_Favor = p_Response.Substring(l_Index + l_Search.Length,
                    p_Response.IndexOf(",", l_Index + l_Search.Length, StringComparison.Ordinal) -
                    (l_Index + l_Search.Length));
                Player.FavorHera = int.Parse(l_Favor.Split('.')[0]);
                l_Search = "\"production\":";
                l_Index = p_Response.IndexOf(l_Search, l_Index, StringComparison.Ordinal);
                Player.FavorHeraProduction = p_Response.Substring(l_Index + l_Search.Length,
                    p_Response.IndexOf(",", l_Index + l_Search.Length, StringComparison.Ordinal) -
                    (l_Index + l_Search.Length));
                //athena
                l_Search = "\"athena\":{\"current\":";
                l_Index = p_Response.IndexOf(l_Search, l_IndexPlayerGods, StringComparison.Ordinal);
                l_Favor = p_Response.Substring(l_Index + l_Search.Length,
                    p_Response.IndexOf(",", l_Index + l_Search.Length, StringComparison.Ordinal) -
                    (l_Index + l_Search.Length));
                Player.FavorAthena = int.Parse(l_Favor.Split('.')[0]);
                l_Search = "\"production\":";
                l_Index = p_Response.IndexOf(l_Search, l_Index, StringComparison.Ordinal);
                Player.FavorAthenaProduction = p_Response.Substring(l_Index + l_Search.Length,
                    p_Response.IndexOf(",", l_Index + l_Search.Length, StringComparison.Ordinal) -
                    (l_Index + l_Search.Length));
                //athena
                l_Search = "\"hades\":{\"current\":";
                l_Index = p_Response.IndexOf(l_Search, l_IndexPlayerGods, StringComparison.Ordinal);
                l_Favor = p_Response.Substring(l_Index + l_Search.Length,
                    p_Response.IndexOf(",", l_Index + l_Search.Length, StringComparison.Ordinal) -
                    (l_Index + l_Search.Length));
                Player.FavorHades = int.Parse(l_Favor.Split('.')[0]);
                l_Search = "\"production\":";
                l_Index = p_Response.IndexOf(l_Search, l_Index, StringComparison.Ordinal);
                Player.FavorHadesProduction = p_Response.Substring(l_Index + l_Search.Length,
                    p_Response.IndexOf(",", l_Index + l_Search.Length, StringComparison.Ordinal) -
                    (l_Index + l_Search.Length));
                //artemis
                l_Search = "\"artemis\":{\"current\":";
                l_Index = p_Response.IndexOf(l_Search, l_IndexPlayerGods, StringComparison.Ordinal);
                if (l_Index != -1) //Not all server have artemis
                {
                    l_Favor = p_Response.Substring(l_Index + l_Search.Length,
                        p_Response.IndexOf(",", l_Index + l_Search.Length, StringComparison.Ordinal) -
                        (l_Index + l_Search.Length));
                    Player.FavorArtemis = int.Parse(l_Favor.Split('.')[0]);
                    l_Search = "\"production\":";
                    l_Index = p_Response.IndexOf(l_Search, l_Index, StringComparison.Ordinal);
                    Player.FavorArtemisProduction = p_Response.Substring(l_Index + l_Search.Length,
                        p_Response.IndexOf(",", l_Index + l_Search.Length, StringComparison.Ordinal) -
                        (l_Index + l_Search.Length));
                }

                //Killpoints
                l_Search = "{\"model_class_name\":\"PlayerKillpoints\",\"data\":{";
                var l_IndexKillpoints = p_Response.IndexOf(l_Search, 0, StringComparison.Ordinal);
                l_Search = "\"att\":";
                l_Index = p_Response.IndexOf(l_Search, l_IndexKillpoints, StringComparison.Ordinal);
                Player.AttackKillPoints =
                    int.Parse(p_Response.Substring(l_Index + l_Search.Length,
                        p_Response.IndexOf(",", l_Index + l_Search.Length, StringComparison.Ordinal) -
                        (l_Index + l_Search.Length)));
                l_Search = "\"def\":";
                l_Index = p_Response.IndexOf(l_Search, l_IndexKillpoints, StringComparison.Ordinal);
                Player.DefendKillPoints =
                    int.Parse(p_Response.Substring(l_Index + l_Search.Length,
                        p_Response.IndexOf(",", l_Index + l_Search.Length, StringComparison.Ordinal) -
                        (l_Index + l_Search.Length)));
                l_Search = "\"used\":";
                l_Index = p_Response.IndexOf(l_Search, l_IndexKillpoints, StringComparison.Ordinal);
                Player.UsedKillPoints =
                    int.Parse(p_Response.Substring(l_Index + l_Search.Length,
                        p_Response.IndexOf(",", l_Index + l_Search.Length, StringComparison.Ordinal) -
                        (l_Index + l_Search.Length)));

                //CommandsMenuBubble
                l_Search = "{\"model_class_name\":\"CommandsMenuBubble\"";
                l_Index = p_Response.IndexOf(l_Search, 0, StringComparison.Ordinal);
                l_Search = "\"incoming_attacks_total\":";
                l_Index = p_Response.IndexOf(l_Search, l_Index, StringComparison.Ordinal);
                Player.IncomingAttacks =
                    int.Parse(p_Response.Substring(l_Index + l_Search.Length,
                        p_Response.IndexOf(",", l_Index + l_Search.Length, StringComparison.Ordinal) -
                        (l_Index + l_Search.Length)));

                //Researches
                l_Search = "{\"class_name\":\"TownResearches\",\"data\":[";
                l_Index = p_Response.IndexOf(l_Search, 0, StringComparison.Ordinal);
                var l_IndexEnd = p_Response.IndexOf("]}", l_Index, StringComparison.Ordinal);
                l_Search = "Researches";
                l_Index = p_Response.IndexOf(l_Search, l_Index + 20, StringComparison.Ordinal);
                var l_ResponseSub = p_Response.Substring(l_Index,
                    p_Response.IndexOf("}}", l_Index, StringComparison.Ordinal) - l_Index);
                while (l_Index != -1 && l_Index < l_IndexEnd)
                {
                    l_Search = "\"id\":";
                    var l_IndexSub = l_ResponseSub.IndexOf(l_Search, 0, StringComparison.Ordinal);

                    var l_TownID = l_ResponseSub.Substring(l_IndexSub + l_Search.Length);

                    var l_Town = Player.Towns.Single(x => x.TownID == l_TownID); // Select town

                    l_Town.Research.Technologies.Single(x => x.ID == Technologies.slinger).Researched =
                        l_ResponseSub.Contains("\"slinger\":true");
                    l_Town.Research.Technologies.Single(x => x.ID == Technologies.archer).Researched =
                        l_ResponseSub.Contains("\"archer\":true");
                    l_Town.Research.Technologies.Single(x => x.ID == Technologies.hoplite).Researched =
                        l_ResponseSub.Contains("\"hoplite\":true");
                    l_Town.Research.Technologies.Single(x => x.ID == Technologies.town_guard).Researched =
                        l_ResponseSub.Contains("\"town_guard\":true");
                    l_Town.Research.Technologies.Single(x => x.ID == Technologies.diplomacy).Researched =
                        l_ResponseSub.Contains("\"diplomacy\":true");
                    l_Town.Research.Technologies.Single(x => x.ID == Technologies.espionage).Researched =
                        l_ResponseSub.Contains("\"espionage\":true");
                    l_Town.Research.Technologies.Single(x => x.ID == Technologies.booty).Researched =
                        l_ResponseSub.Contains("\"booty\":true");
                    l_Town.Research.Technologies.Single(x => x.ID == Technologies.pottery).Researched =
                        l_ResponseSub.Contains("\"pottery\":true");
                    l_Town.Research.Technologies.Single(x => x.ID == Technologies.architecture).Researched =
                        l_ResponseSub.Contains("\"architecture\":true");
                    l_Town.Research.Technologies.Single(x => x.ID == Technologies.instructor).Researched =
                        l_ResponseSub.Contains("\"instructor\":true");
                    l_Town.Research.Technologies.Single(x => x.ID == Technologies.bireme).Researched =
                        l_ResponseSub.Contains("\"bireme\":true");
                    l_Town.Research.Technologies.Single(x => x.ID == Technologies.building_crane).Researched =
                        l_ResponseSub.Contains("\"building_crane\":true");
                    l_Town.Research.Technologies.Single(x => x.ID == Technologies.meteorology).Researched =
                        l_ResponseSub.Contains("\"meteorology\":true");
                    l_Town.Research.Technologies.Single(x => x.ID == Technologies.chariot).Researched =
                        l_ResponseSub.Contains("\"chariot\":true");
                    l_Town.Research.Technologies.Single(x => x.ID == Technologies.attack_ship).Researched =
                        l_ResponseSub.Contains("\"attack_ship\":true");
                    l_Town.Research.Technologies.Single(x => x.ID == Technologies.conscription).Researched =
                        l_ResponseSub.Contains("\"conscription\":true");
                    l_Town.Research.Technologies.Single(x => x.ID == Technologies.shipwright).Researched =
                        l_ResponseSub.Contains("\"shipwright\":true");
                    l_Town.Research.Technologies.Single(x => x.ID == Technologies.demolition_ship).Researched =
                        l_ResponseSub.Contains("\"demolition_ship\":true");
                    l_Town.Research.Technologies.Single(x => x.ID == Technologies.catapult).Researched =
                        l_ResponseSub.Contains("\"catapult\":true");
                    l_Town.Research.Technologies.Single(x => x.ID == Technologies.cryptography).Researched =
                        l_ResponseSub.Contains("\"cryptography\":true");
                    l_Town.Research.Technologies.Single(x => x.ID == Technologies.democracy).Researched =
                        l_ResponseSub.Contains("\"democracy\":true");
                    l_Town.Research.Technologies.Single(x => x.ID == Technologies.colonize_ship).Researched =
                        l_ResponseSub.Contains("\"colonize_ship\":true");
                    l_Town.Research.Technologies.Single(x => x.ID == Technologies.small_transporter).Researched =
                        l_ResponseSub.Contains("\"small_transporter\":true");
                    l_Town.Research.Technologies.Single(x => x.ID == Technologies.plow).Researched =
                        l_ResponseSub.Contains("\"plow\":true");
                    l_Town.Research.Technologies.Single(x => x.ID == Technologies.berth).Researched =
                        l_ResponseSub.Contains("\"berth\":true");
                    l_Town.Research.Technologies.Single(x => x.ID == Technologies.trireme).Researched =
                        l_ResponseSub.Contains("\"trireme\":true");
                    l_Town.Research.Technologies.Single(x => x.ID == Technologies.phalanx).Researched =
                        l_ResponseSub.Contains("\"phalanx\":true");
                    l_Town.Research.Technologies.Single(x => x.ID == Technologies.breach).Researched =
                        l_ResponseSub.Contains("\"breach\":true");
                    l_Town.Research.Technologies.Single(x => x.ID == Technologies.mathematics).Researched =
                        l_ResponseSub.Contains("\"mathematics\":true");
                    l_Town.Research.Technologies.Single(x => x.ID == Technologies.ram).Researched =
                        l_ResponseSub.Contains("\"ram\":true");
                    l_Town.Research.Technologies.Single(x => x.ID == Technologies.cartography).Researched =
                        l_ResponseSub.Contains("\"cartography\":true");
                    l_Town.Research.Technologies.Single(x => x.ID == Technologies.take_over).Researched =
                        l_ResponseSub.Contains("\"take_over\":true");
                    l_Town.Research.Technologies.Single(x => x.ID == Technologies.stone_storm).Researched =
                        l_ResponseSub.Contains("\"stone_storm\":true");
                    l_Town.Research.Technologies.Single(x => x.ID == Technologies.temple_looting).Researched =
                        l_ResponseSub.Contains("\"temple_looting\":true");
                    l_Town.Research.Technologies.Single(x => x.ID == Technologies.divine_selection).Researched =
                        l_ResponseSub.Contains("\"divine_selection\":true");
                    l_Town.Research.Technologies.Single(x => x.ID == Technologies.combat_experience).Researched =
                        l_ResponseSub.Contains("\"combat_experience\":true");
                    l_Town.Research.Technologies.Single(x => x.ID == Technologies.strong_wine).Researched =
                        l_ResponseSub.Contains("\"strong_wine\":true");
                    l_Town.Research.Technologies.Single(x => x.ID == Technologies.set_sail).Researched =
                        l_ResponseSub.Contains("\"set_sail\":true");

                    //next Town Data Set
                    l_Search = "Researches";
                    l_Index = p_Response.IndexOf(l_Search, l_Index + l_Search.Length, StringComparison.Ordinal);
                    l_ResponseSub = p_Response.Substring(l_Index,
                        p_Response.IndexOf("}}", l_Index, StringComparison.Ordinal) - l_Index);
                }

                //Collections.RunningPowers
                Player.ResetCastedPowers();
                l_Search = "{\"class_name\":\"RunningPowers\"";
                l_Index = p_Response.IndexOf(l_Search, 0, StringComparison.Ordinal);
                l_IndexEnd = p_Response.IndexOf("]}", l_Index, StringComparison.Ordinal);
                l_Search = "\"power_id\":\"";
                l_Index = p_Response.IndexOf(l_Search, l_Index, StringComparison.Ordinal);
                while (l_Index < l_IndexEnd && l_Index != -1)
                {
                    var l_CastedPower = p_Response.Substring(l_Index + l_Search.Length,
                        p_Response.IndexOf("\"", l_Index + l_Search.Length, StringComparison.Ordinal) -
                        (l_Index + l_Search.Length));
                    l_Index = p_Response.IndexOf(l_Search, l_Index, StringComparison.Ordinal);
                    l_Search = "\"town_id\":";
                    l_Index = p_Response.IndexOf(l_Search, l_Index, StringComparison.Ordinal);
                    var l_TownID = p_Response.Substring(l_Index + l_Search.Length,
                        p_Response.IndexOf(",", l_Index + l_Search.Length, StringComparison.Ordinal) -
                        (l_Index + l_Search.Length));
                    Player.Towns.Single(x => x.TownID == l_TownID).CastedPowers.Add(l_CastedPower);
                    //next power
                    l_Search = "\"power_id\":\"";
                    l_Index = p_Response.IndexOf(l_Search, l_Index, StringComparison.Ordinal);
                }

                //Collections.BuildingOrders
                l_Search = "{\"class_name\":\"BuildingOrders\",\"data\":[";
                l_Index = p_Response.IndexOf(l_Search, 0, StringComparison.Ordinal);
                l_IndexEnd = p_Response.IndexOf("]", l_Index + l_Search.Length, StringComparison.Ordinal);
                l_Search = "\"town_id\":";
                l_Index = p_Response.IndexOf(l_Search, l_Index, StringComparison.Ordinal);
                Player.ResetBuildingQueue();
                while (l_Index < l_IndexEnd && l_Index != -1)
                {
                    var l_TownID = p_Response.Substring(l_Index + l_Search.Length,
                        p_Response.IndexOf(",", l_Index + l_Search.Length, StringComparison.Ordinal) -
                        (l_Index + l_Search.Length));
                    l_Search = "\"building_type\":\"";
                    l_Index = p_Response.IndexOf(l_Search, l_Index, StringComparison.Ordinal);
                    var l_Building = p_Response.Substring(l_Index + l_Search.Length,
                        p_Response.IndexOf("\"", l_Index + l_Search.Length, StringComparison.Ordinal) -
                        (l_Index + l_Search.Length));
                    var l_BuildingEnum = (Buildings)Enum.Parse(typeof(Buildings), l_Building);

                    l_Search = "\"to_be_completed_at\":";
                    l_Index = p_Response.IndexOf(l_Search, l_Index, StringComparison.Ordinal);
                    var l_CompletedAt = p_Response.Substring(l_Index + l_Search.Length,
                        p_Response.IndexOf(",", l_Index + l_Search.Length, StringComparison.Ordinal) -
                        (l_Index + l_Search.Length));

                    l_Search = "\"created_at\":";
                    l_Index = p_Response.IndexOf(l_Search, l_Index, StringComparison.Ordinal);
                    var l_CreatedAt = p_Response.Substring(l_Index + l_Search.Length,
                        p_Response.IndexOf(",", l_Index + l_Search.Length, StringComparison.Ordinal) -
                        (l_Index + l_Search.Length));

                    Player.Towns.Single(x => x.TownID == l_TownID).IngameBuildingQueue.Add(new BuildingQueueBuilding(l_BuildingEnum, l_CreatedAt, l_CompletedAt));
                    //next building order
                    l_Search = "\"town_id\":";
                    l_Index = p_Response.IndexOf(l_Search, l_Index, StringComparison.Ordinal);
                }
                Player.OrderAllIngameQueues(); //bring in correct order by created at attribute

                //Collections.Trades
                l_Search = "{\"class_name\":\"Trades\",\"data\":[";
                l_Index = p_Response.IndexOf(l_Search, 0, StringComparison.Ordinal);
                l_IndexEnd = p_Response.IndexOf("]", l_Index + l_Search.Length, StringComparison.Ordinal);
                l_Search = "\"origin_town_id\":";
                l_Index = p_Response.IndexOf(l_Search, l_Index, StringComparison.Ordinal);
                var l_IndexStart = l_Index;
                Player.ResetTrades();
                while (l_Index < l_IndexEnd && l_Index != -1)
                {
                    var l_Trade = new Trade();

                    l_Trade.Origin_Town_ID = p_Response.Substring(l_Index + l_Search.Length,
                        p_Response.IndexOf(",", l_Index + l_Search.Length, StringComparison.Ordinal) -
                        (l_Index + l_Search.Length));
                    l_Search = "\"destination_town_id\":";
                    l_Index = p_Response.IndexOf(l_Search, l_IndexStart, StringComparison.Ordinal);
                    l_Trade.Destination_Town_ID = p_Response.Substring(l_Index + l_Search.Length,
                        p_Response.IndexOf(",", l_Index + l_Search.Length, StringComparison.Ordinal) -
                        (l_Index + l_Search.Length));
                    l_Search = "\"id\":";
                    l_Index = p_Response.IndexOf(l_Search, l_IndexStart, StringComparison.Ordinal);
                    l_Trade.ID = p_Response.Substring(l_Index + l_Search.Length,
                        p_Response.IndexOf(",", l_Index + l_Search.Length, StringComparison.Ordinal) -
                        (l_Index + l_Search.Length));
                    l_Search = "\"wood\":";
                    l_Index = p_Response.IndexOf(l_Search, l_IndexStart, StringComparison.Ordinal);
                    l_Trade.Wood = int.Parse(p_Response.Substring(l_Index + l_Search.Length,
                        p_Response.IndexOf(",", l_Index + l_Search.Length, StringComparison.Ordinal) -
                        (l_Index + l_Search.Length)));
                    l_Search = "\"stone\":";
                    l_Index = p_Response.IndexOf(l_Search, l_IndexStart, StringComparison.Ordinal);
                    l_Trade.Stone = int.Parse(p_Response.Substring(l_Index + l_Search.Length,
                        p_Response.IndexOf(",", l_Index + l_Search.Length, StringComparison.Ordinal) -
                        (l_Index + l_Search.Length)));
                    l_Search = "\"iron\":";
                    l_Index = p_Response.IndexOf(l_Search, l_IndexStart, StringComparison.Ordinal);
                    l_Trade.Iron = int.Parse(p_Response.Substring(l_Index + l_Search.Length,
                        p_Response.IndexOf(",", l_Index + l_Search.Length, StringComparison.Ordinal) -
                        (l_Index + l_Search.Length)));
                    l_Search = "\"in_exchange\":";
                    l_Index = p_Response.IndexOf(l_Search, l_IndexStart, StringComparison.Ordinal);
                    l_Trade.In_Exchange = p_Response.Substring(l_Index + l_Search.Length,
                        p_Response.IndexOf(",", l_Index + l_Search.Length, StringComparison.Ordinal) -
                        (l_Index + l_Search.Length));
                    //add trade
                    Player.Trades.Add(l_Trade);
                    //next trade
                    l_Search = "\"origin_town_id\":";
                    l_Index = p_Response.IndexOf(l_Search, l_IndexStart + l_Search.Length, StringComparison.Ordinal);
                    l_IndexStart = l_Index;
                }

                //Collections.RemainingUnitOrders
                l_Search = "{\"class_name\":\"RemainingUnitOrders\",\"data\":[";
                l_Index = p_Response.IndexOf(l_Search, 0, StringComparison.Ordinal);
                l_IndexEnd = p_Response.IndexOf("]", l_Index + l_Search.Length, StringComparison.Ordinal);
                l_Search = "\"UnitOrder\"";
                l_Index = p_Response.IndexOf(l_Search, l_Index, StringComparison.Ordinal);
                l_IndexStart = l_Index;
                Player.ResetUnitQueue();
                while (l_Index < l_IndexEnd && l_Index != -1)
                {
                    l_Search = "\"town_id\":";
                    l_Index = p_Response.IndexOf(l_Search, l_IndexStart, StringComparison.Ordinal);
                    var l_TownID = p_Response.Substring(l_Index + l_Search.Length,
                        p_Response.IndexOf(",", l_Index + l_Search.Length, StringComparison.Ordinal) -
                        (l_Index + l_Search.Length));
                    l_Search = "\"unit_type\":\"";
                    l_Index = p_Response.IndexOf(l_Search, l_IndexStart, StringComparison.Ordinal);
                    var l_UnitType = p_Response.Substring(l_Index + l_Search.Length,
                        p_Response.IndexOf("\",", l_Index + l_Search.Length, StringComparison.Ordinal) -
                        (l_Index + l_Search.Length));
                    l_Search = "\"kind\":\"";
                    l_Index = p_Response.IndexOf(l_Search, l_IndexStart, StringComparison.Ordinal);
                    var l_Kind = p_Response.Substring(l_Index + l_Search.Length,
                        p_Response.IndexOf("\",", l_Index + l_Search.Length, StringComparison.Ordinal) -
                        (l_Index + l_Search.Length));
                    l_Search = "\"units_left\":";
                    l_Index = p_Response.IndexOf(l_Search, l_IndexStart, StringComparison.Ordinal);
                    var l_UnitsLeft = p_Response.Substring(l_Index + l_Search.Length,
                        p_Response.IndexOf(",", l_Index + l_Search.Length, StringComparison.Ordinal) -
                        (l_Index + l_Search.Length));

                    Player.Towns.Single(x => x.TownID == l_TownID)
                        .ArmyUnits.Single(x => x.Name.ToString("G") == l_UnitType)
                        .QueueGame += int.Parse(l_UnitsLeft);
                    if (l_Kind.Equals("ground"))
                        Player.Towns.Single(x => x.TownID == l_TownID).LandUnitQueueSize += 1;
                    else if (l_Kind.Equals("naval"))
                        Player.Towns.Single(x => x.TownID == l_TownID).NavyUnitQueueSize += 1;

                    //next queue order
                    l_Search = "\"UnitOrder\"";
                    l_Index = p_Response.IndexOf(l_Search, l_IndexStart + l_Search.Length, StringComparison.Ordinal);
                    l_IndexStart = l_Index;
                }

                //FarmTowns

                l_ResponseSub = p_Response.Substring(0,
                    p_Response.IndexOf("map_chunk_pixel_size", StringComparison.Ordinal)); //Substring only for map data
                l_Search = "\"relation_status\":";
                l_Index = l_ResponseSub.IndexOf(l_Search, 0, StringComparison.Ordinal);

                while (l_Index != -1)
                {
                    while (!l_ResponseSub[l_Index].Equals('{'))
                        l_Index--;
                    l_Search = "\"id\":";
                    l_Index = l_ResponseSub.IndexOf(l_Search, l_Index, StringComparison.Ordinal);
                    //Get farmers id
                    var l_ID = l_ResponseSub.Substring(l_Index + l_Search.Length,
                        l_ResponseSub.IndexOf(",", l_Index + l_Search.Length, StringComparison.Ordinal) -
                        (l_Index + l_Search.Length));
                    l_Search = "\"name\":\"";
                    l_Index = l_ResponseSub.IndexOf(l_Search, l_Index, StringComparison.Ordinal);
                    //Get farmers name
                    var l_Name = l_ResponseSub.Substring(l_Index + l_Search.Length,
                        l_ResponseSub.IndexOf("\",", l_Index + l_Search.Length, StringComparison.Ordinal) -
                        (l_Index + l_Search.Length));
                    l_Name = Parser.FixSpecialCharacters(l_Name);
                    l_Search = "\"expansion_stage\":";
                    l_Index = l_ResponseSub.IndexOf(l_Search, l_Index, StringComparison.Ordinal);
                    //Get expansion state
                    var l_ExpansionState = int.Parse(l_ResponseSub.Substring(l_Index + l_Search.Length,
                        l_ResponseSub.IndexOf(",", l_Index + l_Search.Length, StringComparison.Ordinal) -
                        (l_Index + l_Search.Length)));
                    l_Search = "\"x\":";
                    l_Index = l_ResponseSub.IndexOf(l_Search, l_Index, StringComparison.Ordinal);
                    //Get farmers island x coord
                    var l_IslandX = l_ResponseSub.Substring(l_Index + l_Search.Length,
                        l_ResponseSub.IndexOf(",", l_Index + l_Search.Length, StringComparison.Ordinal) -
                        (l_Index + l_Search.Length));
                    l_Search = "\"y\":";
                    l_Index = l_ResponseSub.IndexOf(l_Search, l_Index, StringComparison.Ordinal);
                    //Get farmers island y coord
                    var l_IslandY = l_ResponseSub.Substring(l_Index + l_Search.Length,
                        l_ResponseSub.IndexOf(",", l_Index + l_Search.Length, StringComparison.Ordinal) -
                        (l_Index + l_Search.Length));
                    l_Search = "\"mood\":";
                    l_Index = l_ResponseSub.IndexOf(l_Search, l_Index, StringComparison.Ordinal);
                    //Get farmers mood
                    var l_Mood = int.Parse(l_ResponseSub.Substring(l_Index + l_Search.Length,
                        l_ResponseSub.IndexOf(",", l_Index + l_Search.Length, StringComparison.Ordinal) -
                        (l_Index + l_Search.Length)));
                    l_Search = "\"relation_status\":";
                    l_Index = l_ResponseSub.IndexOf(l_Search, l_Index, StringComparison.Ordinal);
                    //Get farmers relation status
                    var l_RelationStatus = l_ResponseSub.Substring(l_Index + l_Search.Length,
                        l_ResponseSub.IndexOf(",", l_Index + l_Search.Length, StringComparison.Ordinal) -
                        (l_Index + l_Search.Length)).Equals("1");
                    string l_LootTimer;
                    string l_LootTimerHuman;
                    if (l_RelationStatus)
                    {
                        l_Search = "\"loot\":";
                        l_Index = l_ResponseSub.IndexOf(l_Search, l_Index, StringComparison.Ordinal);
                        //Get farmers loot time
                        l_LootTimer = l_ResponseSub.Substring(l_Index + l_Search.Length,
                            l_ResponseSub.IndexOf(",", l_Index + l_Search.Length, StringComparison.Ordinal) -
                            (l_Index + l_Search.Length));
                        l_Search = "\"lootable_human\":\"";
                        l_Index = l_ResponseSub.IndexOf(l_Search, l_Index, StringComparison.Ordinal);
                        //Get farmers loot time human
                        l_LootTimerHuman = l_ResponseSub.Substring(l_Index + l_Search.Length,
                            l_ResponseSub.IndexOf("\",", l_Index + l_Search.Length, StringComparison.Ordinal) -
                            (l_Index + l_Search.Length));
                        l_LootTimerHuman = Parser.FixSpecialCharacters(l_LootTimerHuman);
                    }
                    else
                    {
                        l_LootTimer = "0";
                        l_LootTimerHuman = "Not available";
                    }

                    //Add or update farmer
                    var l_Towns = Player.Towns.Where(x => x.IslandX == l_IslandX && x.IslandY == l_IslandY);

                    foreach (var l_Town in l_Towns)
                    {
                        if (!l_Town.Farmers.Exists(x => x.ID == l_ID)) //If farmer already in list
                        {
                            l_Town.Farmers.Add(new Farmer(l_ID, l_Name, l_ExpansionState, l_IslandX, l_IslandY, l_Mood,
                                l_RelationStatus, l_LootTimer, l_LootTimerHuman));
                        }
                        else
                        {
                            var l_Farmer = l_Town.Farmers.Single(x => x.ID == l_ID);
                            l_Farmer.ExpansionState = l_ExpansionState;
                            l_Farmer.Mood = l_Mood;
                            l_Farmer.RelationStatus = l_RelationStatus;
                            l_Farmer.LootTimer = l_LootTimer;
                            l_Farmer.LootTimerHuman = l_LootTimerHuman;
                        }
                    }

                    //Search next farmer
                    l_Search = "\"relation_status\":";
                    l_Index = l_ResponseSub.IndexOf(l_Search, l_Index + l_Search.Length, StringComparison.Ordinal);
                }

                //Sort farmers
                foreach (var l_Town in Player.Towns)
                {
                    l_Town.Farmers.Sort((x, y) => CompareOrdinal(x.ID, y.ID));
                }

                //FarmTownPlayerRelations
                l_Search = "{\"class_name\":\"FarmTownPlayerRelations\",\"data\":[";
                l_Index = p_Response.IndexOf(l_Search, 0, StringComparison.Ordinal);
                l_IndexEnd = p_Response.IndexOf("]", l_Index + l_Search.Length, StringComparison.Ordinal);
                l_Search = "\"FarmTownPlayerRelation\"";
                l_Index = p_Response.IndexOf(l_Search, l_Index, StringComparison.Ordinal);
                l_IndexStart = l_Index;
                while (l_Index < l_IndexEnd && l_Index != -1)
                {
                    l_Search = "\"id\":";
                    l_Index = p_Response.IndexOf(l_Search, l_IndexStart, StringComparison.Ordinal);
                    var l_ID = p_Response.Substring(l_Index + l_Search.Length,
                        p_Response.IndexOf(",", l_Index + l_Search.Length, StringComparison.Ordinal) -
                        (l_Index + l_Search.Length));

                    l_Search = "\"farm_town_id\":";
                    l_Index = p_Response.IndexOf(l_Search, l_IndexStart, StringComparison.Ordinal);
                    var l_Farm_TownID = p_Response.Substring(l_Index + l_Search.Length,
                        p_Response.IndexOf(",", l_Index + l_Search.Length, StringComparison.Ordinal) -
                        (l_Index + l_Search.Length));

                    l_Search = "\"loot\":";
                    l_Index = p_Response.IndexOf(l_Search, l_IndexStart, StringComparison.Ordinal);
                    var l_Loot = int.Parse(p_Response.Substring(l_Index + l_Search.Length,
                        p_Response.IndexOf(",", l_Index + l_Search.Length, StringComparison.Ordinal) -
                        (l_Index + l_Search.Length)));

                    l_Search = "\"expansion_stage\":";
                    l_Index = p_Response.IndexOf(l_Search, l_IndexStart, StringComparison.Ordinal);
                    var l_ExpansionState = int.Parse(p_Response.Substring(l_Index + l_Search.Length,
                        p_Response.IndexOf(",", l_Index + l_Search.Length, StringComparison.Ordinal) -
                        (l_Index + l_Search.Length)));

                    var l_Added = false;
                    //Get Farm
                    foreach (var l_Town in Player.Towns)
                    {
                        foreach (var l_Farmer in l_Town.Farmers)
                        {
                            if (l_Farmer.ID == l_Farm_TownID)
                            {
                                l_Added = true;
                                l_Farmer.BattlePointsFarmID = l_ID;
                                l_Farmer.LootedResources = l_Loot;
                                l_Farmer.ExpansionState = l_ExpansionState;
                                break;
                            }
                        }
                    }

                    if (!l_Added)
                    {
                        Player.UnusedFarmTownRelations.Add(new FarmTownRelation(l_ID, l_Farm_TownID, l_Loot, l_ExpansionState));
                    }

                    //next relation
                    l_Search = "\"FarmTownPlayerRelation\"";
                    l_Index = p_Response.IndexOf(l_Search, l_IndexStart + l_Search.Length, StringComparison.Ordinal);
                    l_IndexStart = l_Index;
                }

                //Set Server time for all Towns
                foreach (var town in Player.Towns)
                {
                    town.ServerTime = ServerTime;
                }
            }
            catch (Exception ex)
            {
                Logger.WriteExceptionToLog(ex.Message);
            }
        }

        /// <summary>
        /// Handles the server response for BuildingBuildDatasRequest().
        /// </summary>
        private void BuildingBuildDatasResponse(string p_Response)
        {
            try
            {
                var l_Search = "\"BuildingBuildData\"";
                var l_GlobalIndex = p_Response.IndexOf(l_Search, 0, StringComparison.Ordinal);

                l_Search = "}}}]";
                var l_ResponseSubAll = p_Response.Substring(l_GlobalIndex,
                    p_Response.IndexOf(l_Search, StringComparison.Ordinal) - (l_GlobalIndex - l_Search.Length));
                l_GlobalIndex = 0;
                while (l_GlobalIndex != -1)
                {
                    l_Search = "}}}}";
                    var l_ResponseSubTown = l_ResponseSubAll.Substring(l_GlobalIndex,
                        l_ResponseSubAll.IndexOf(l_Search, l_GlobalIndex, StringComparison.Ordinal) - l_GlobalIndex);
                    l_GlobalIndex = l_ResponseSubAll.IndexOf(l_Search, l_GlobalIndex, StringComparison.Ordinal);

                    l_Search = "\"town_id\":";

                    var l_Index = l_ResponseSubTown.IndexOf(l_Search, StringComparison.Ordinal);
                    var l_TownID = l_ResponseSubTown.Substring(l_Index + l_Search.Length,
                        l_ResponseSubTown.IndexOf(",", l_Index, StringComparison.Ordinal) - (l_Index + l_Search.Length));

                    var l_Town = Player.Towns.Single(x => x.TownID == l_TownID);

                    l_Search = "is_building_order_queue_full\":";
                    l_Index = l_ResponseSubTown.IndexOf(l_Search, StringComparison.Ordinal);

                    l_Town.IsBuildingOrderQueueFull = l_ResponseSubTown.Substring(l_Index + l_Search.Length,
                        l_ResponseSubTown.IndexOf(",", l_Index, StringComparison.Ordinal) +
                        (l_Index + l_Search.Length)).Equals("true");

                    foreach (var building in l_Town.Buildings)
                    {
                        //Get each Building by Name
                        l_Search = "\"" + building.DevName.ToString("G") + "\":{\"can"; //needed because of "missing_dependencies" would cause an error
                        l_Index = l_ResponseSubTown.IndexOf(l_Search, StringComparison.Ordinal);

                        l_Search = "\"can_upgrade\":";
                        l_Index = l_ResponseSubTown.IndexOf(l_Search, l_Index, StringComparison.Ordinal);
                        building.Upgradable = l_ResponseSubTown.Substring(l_Index + l_Search.Length,
                            l_ResponseSubTown.IndexOf(",", l_Index + l_Search.Length, StringComparison.Ordinal) - (l_Index + l_Search.Length))
                            .Equals("true");

                        l_Search = "\"can_tear_down\":";
                        l_Index = l_ResponseSubTown.IndexOf(l_Search, l_Index, StringComparison.Ordinal);
                        building.Teardownable = l_ResponseSubTown.Substring(l_Index + l_Search.Length,
                            l_ResponseSubTown.IndexOf(",", l_Index + l_Search.Length, StringComparison.Ordinal) - (l_Index + l_Search.Length))
                            .Equals("true");

                        l_Search = "\"level\":";
                        l_Index = l_ResponseSubTown.IndexOf(l_Search, l_Index, StringComparison.Ordinal);

                        var l_Level = l_ResponseSubTown.Substring(l_Index + l_Search.Length,
                            l_ResponseSubTown.IndexOf(",", l_Index + l_Search.Length, StringComparison.Ordinal) -
                            (l_Index + l_Search.Length));
                        building.Level = l_Level.Contains("-") ? 0 : int.Parse(l_Level);

                        /*
                        l_Search = "\"next_level\":";
                        l_Index = l_ResponseSubTown.IndexOf(l_Search, l_Index, StringComparison.Ordinal);
                        building.NextLevel = int.Parse(l_ResponseSubTown.Substring(l_Index + l_Search.Length,
                            l_ResponseSubTown.IndexOf(",", l_Index + l_Search.Length, StringComparison.Ordinal) -
                            (l_Index + l_Search.Length)));

                        l_Search = "\"tear_down_level\":";
                        l_Index = l_ResponseSubTown.IndexOf(l_Search, l_Index, StringComparison.Ordinal);
                        building.TearDownLevel = int.Parse(l_ResponseSubTown.Substring(l_Index + l_Search.Length,
                            l_ResponseSubTown.IndexOf(",", l_Index + l_Search.Length, StringComparison.Ordinal) -
                            (l_Index + l_Search.Length)));
                        */
                        l_Search = "\"has_max_level\":";
                        l_Index = l_ResponseSubTown.IndexOf(l_Search, l_Index, StringComparison.Ordinal);
                        building.IsMaxLevel = l_ResponseSubTown.Substring(l_Index + l_Search.Length,
                            l_ResponseSubTown.IndexOf(",", l_Index + l_Search.Length, StringComparison.Ordinal) - (l_Index + l_Search.Length))
                            .Equals("true");
                    }

                    //next Town
                    l_Search = "\"BuildingBuildData\"";
                    l_GlobalIndex = l_ResponseSubAll.IndexOf(l_Search, l_GlobalIndex, StringComparison.Ordinal);
                }
            }
            catch (Exception ex)
            {
                Logger.WriteExceptionToLog(ex.Message);
            }
        }

        /// <summary>
        /// Handles the server response for UpdateTownsRequest().
        /// Load town settings.
        /// </summary>
        private void UpdateTownsResponse(string p_Response)
        {
            try
            {
                Player.Towns.Clear(); //Clear town list

                var l_Search = "\"collections\":{\"Towns\":{\"data\":[";
                var l_Index = p_Response.IndexOf(l_Search, StringComparison.Ordinal);
                var l_IndexEnd = p_Response.IndexOf("]", l_Index + l_Search.Length, StringComparison.Ordinal);

                l_Search = "player_id";
                l_Index = p_Response.IndexOf(l_Search, l_Index, StringComparison.Ordinal);
                while (l_Index < l_IndexEnd && l_Index != -1)
                {
                    var l_ResponseSub = p_Response.Substring(l_Index,
                        p_Response.IndexOf("}}", l_Index, StringComparison.Ordinal) - l_Index);
                    l_Search = "\"name\":\"";
                    var l_IndexSub = l_ResponseSub.IndexOf(l_Search, StringComparison.Ordinal);
                    var l_TownName = l_ResponseSub.Substring(l_IndexSub + l_Search.Length,
                        l_ResponseSub.IndexOf("\"", l_IndexSub + l_Search.Length, StringComparison.Ordinal) -
                        (l_IndexSub + l_Search.Length));
                    l_TownName = Parser.FixSpecialCharacters(l_TownName);
                    l_Search = "\"island_x\":";
                    l_IndexSub = l_ResponseSub.IndexOf(l_Search, StringComparison.Ordinal);
                    var l_IslandX = l_ResponseSub.Substring(l_IndexSub + l_Search.Length,
                        l_ResponseSub.IndexOf(",", l_IndexSub + l_Search.Length, StringComparison.Ordinal) -
                        (l_IndexSub + l_Search.Length));
                    l_Search = "\"island_y\":";
                    l_IndexSub = l_ResponseSub.IndexOf(l_Search, StringComparison.Ordinal);
                    var l_IslandY = l_ResponseSub.Substring(l_IndexSub + l_Search.Length,
                        l_ResponseSub.IndexOf(",", l_IndexSub + l_Search.Length, StringComparison.Ordinal) -
                        (l_IndexSub + l_Search.Length));
                    l_Search = "\"resources_last_update\":";
                    l_IndexSub = l_ResponseSub.IndexOf(l_Search, StringComparison.Ordinal);
                    var l_ResourcesLastUpdate = l_ResponseSub.Substring(l_IndexSub + l_Search.Length,
                        l_ResponseSub.IndexOf(",", l_IndexSub + l_Search.Length, StringComparison.Ordinal) -
                        (l_IndexSub + l_Search.Length));
                    l_Search = "\"resource_rare\":\"";
                    l_IndexSub = l_ResponseSub.IndexOf(l_Search, StringComparison.Ordinal);
                    var l_Rare = l_ResponseSub.Substring(l_IndexSub + l_Search.Length,
                        l_ResponseSub.IndexOf("\"", l_IndexSub + l_Search.Length, StringComparison.Ordinal) -
                        (l_IndexSub + l_Search.Length));
                    Enum.TryParse(l_Rare, true, out Resources l_RareResource);
                    l_Search = "\"resource_plenty\":\"";
                    l_IndexSub = l_ResponseSub.IndexOf(l_Search, StringComparison.Ordinal);
                    var l_Plenty = l_ResponseSub.Substring(l_IndexSub + l_Search.Length,
                        l_ResponseSub.IndexOf("\"", l_IndexSub + l_Search.Length, StringComparison.Ordinal) -
                        (l_IndexSub + l_Search.Length));
                    Enum.TryParse(l_Plenty, true, out Resources l_PlentyResource);
                    l_Search = "\"population_extra\":";
                    l_IndexSub = l_ResponseSub.IndexOf(l_Search, StringComparison.Ordinal);
                    var l_PopulationExtra = l_ResponseSub.Substring(l_IndexSub + l_Search.Length,
                        l_ResponseSub.IndexOf(",", l_IndexSub + l_Search.Length, StringComparison.Ordinal) -
                        (l_IndexSub + l_Search.Length));
                    l_Search = "\"god\":\"";
                    l_IndexSub = l_ResponseSub.IndexOf(l_Search, StringComparison.Ordinal);
                    Gods l_GodEnum;
                    if (l_IndexSub != -1)
                    {
                        var l_God = l_ResponseSub.Substring(l_IndexSub + l_Search.Length,
                            l_ResponseSub.IndexOf("\"", l_IndexSub + l_Search.Length, StringComparison.Ordinal) -
                            (l_IndexSub + l_Search.Length));
                        Enum.TryParse(l_God, true, out l_GodEnum);
                    }
                    else
                    {
                        /*
                        l_Search = "\"god\":";
                        l_IndexSub = l_ResponseSub.IndexOf(l_Search, StringComparison.Ordinal);
                        l_God = l_ResponseSub.Substring(l_IndexSub + l_Search.Length,
                            l_ResponseSub.IndexOf(",", l_IndexSub + l_Search.Length, StringComparison.Ordinal) -
                            (l_IndexSub + l_Search.Length));
                            */
                        l_GodEnum = Gods.none;
                    }
                    l_Search = "\"points\":";
                    l_IndexSub = l_ResponseSub.IndexOf(l_Search, StringComparison.Ordinal);
                    var l_Points = l_ResponseSub.Substring(l_IndexSub + l_Search.Length,
                        l_ResponseSub.IndexOf(",", l_IndexSub + l_Search.Length, StringComparison.Ordinal) -
                        (l_IndexSub + l_Search.Length));
                    l_Search = "\"espionage_storage\":";
                    l_IndexSub = l_ResponseSub.IndexOf(l_Search, StringComparison.Ordinal);
                    var l_EspionageStorage = l_ResponseSub.Substring(l_IndexSub + l_Search.Length,
                        l_ResponseSub.IndexOf(",", l_IndexSub + l_Search.Length, StringComparison.Ordinal) -
                        (l_IndexSub + l_Search.Length));
                    l_Search = "\"id\":";
                    l_IndexSub = l_ResponseSub.IndexOf(l_Search, StringComparison.Ordinal);
                    var l_TownID = l_ResponseSub.Substring(l_IndexSub + l_Search.Length,
                        l_ResponseSub.IndexOf(",", l_IndexSub + l_Search.Length, StringComparison.Ordinal) -
                        (l_IndexSub + l_Search.Length));
                    l_Search = "\"available_population\":";
                    l_IndexSub = l_ResponseSub.IndexOf(l_Search, StringComparison.Ordinal);
                    var l_PopulationAvailable = l_ResponseSub.Substring(l_IndexSub + l_Search.Length,
                        l_ResponseSub.IndexOf(",", l_IndexSub + l_Search.Length, StringComparison.Ordinal) -
                        (l_IndexSub + l_Search.Length));
                    l_Search = "\"has_conqueror\":";
                    l_IndexSub = l_ResponseSub.IndexOf(l_Search, StringComparison.Ordinal);
                    var l_HasConqueror =
                        l_ResponseSub.Substring(l_IndexSub + l_Search.Length,
                            l_ResponseSub.IndexOf(",", l_IndexSub + l_Search.Length, StringComparison.Ordinal) -
                            (l_IndexSub + l_Search.Length)).Equals("true");
                    l_Search = "\"last_wood\":";
                    l_IndexSub = l_ResponseSub.IndexOf(l_Search, StringComparison.Ordinal);
                    var l_Wood = l_ResponseSub.Substring(l_IndexSub + l_Search.Length, l_ResponseSub.IndexOf(",", l_IndexSub + l_Search.Length, StringComparison.Ordinal) - (l_IndexSub + l_Search.Length));
                    l_Search = "\"last_stone\":";
                    l_IndexSub = l_ResponseSub.IndexOf(l_Search, StringComparison.Ordinal);
                    var l_Stone = l_ResponseSub.Substring(l_IndexSub + l_Search.Length, l_ResponseSub.IndexOf(",", l_IndexSub + l_Search.Length, StringComparison.Ordinal) - (l_IndexSub + l_Search.Length));
                    l_Search = "\"last_iron\":";
                    l_IndexSub = l_ResponseSub.IndexOf(l_Search, StringComparison.Ordinal);
                    var l_Iron = l_ResponseSub.Substring(l_IndexSub + l_Search.Length, l_ResponseSub.IndexOf(",", l_IndexSub + l_Search.Length, StringComparison.Ordinal) - (l_IndexSub + l_Search.Length));
                    l_Search = "\"available_trade_capacity\":";
                    l_IndexSub = l_ResponseSub.IndexOf(l_Search, StringComparison.Ordinal);
                    var l_AvailableTradeCapacity = l_ResponseSub.Substring(l_IndexSub + l_Search.Length, l_ResponseSub.IndexOf(",", l_IndexSub + l_Search.Length, StringComparison.Ordinal) - (l_IndexSub + l_Search.Length));
                    l_Search = "\"production\":";
                    l_IndexSub = l_ResponseSub.IndexOf(l_Search, StringComparison.Ordinal);
                    l_Search = "\"wood\":";
                    l_IndexSub = l_ResponseSub.IndexOf(l_Search, l_IndexSub, StringComparison.Ordinal);
                    var l_WoodProduction = l_ResponseSub.Substring(l_IndexSub + l_Search.Length, l_ResponseSub.IndexOf(",", l_IndexSub + l_Search.Length, StringComparison.Ordinal) - (l_IndexSub + l_Search.Length));
                    l_Search = "\"stone\":";
                    l_IndexSub = l_ResponseSub.IndexOf(l_Search, l_IndexSub, StringComparison.Ordinal);
                    var l_StoneProduction = l_ResponseSub.Substring(l_IndexSub + l_Search.Length, l_ResponseSub.IndexOf(",", l_IndexSub + l_Search.Length, StringComparison.Ordinal) - (l_IndexSub + l_Search.Length));
                    l_Search = "\"iron\":";
                    l_IndexSub = l_ResponseSub.IndexOf(l_Search, l_IndexSub, StringComparison.Ordinal);
                    var l_IronProduction = l_ResponseSub.Substring(l_IndexSub + l_Search.Length, l_ResponseSub.IndexOf("}", l_IndexSub + l_Search.Length, StringComparison.Ordinal) - (l_IndexSub + l_Search.Length));
                    l_Search = "\"storage\":";
                    l_IndexSub = l_ResponseSub.IndexOf(l_Search, StringComparison.Ordinal);
                    var l_Storage = l_ResponseSub.Substring(l_IndexSub + l_Search.Length, l_ResponseSub.IndexOf(",", l_IndexSub + l_Search.Length, StringComparison.Ordinal) - (l_IndexSub + l_Search.Length));

                    //Load settings
                    var l_SettingsTown = IOHelper.LoadTownSettingsFromXml(l_TownID);

                    var l_Town = new Town(l_TownID)
                    {
                        TownName = l_TownName,
                        IslandX = l_IslandX,
                        IslandY = l_IslandY,
                        ResourcesLastUpdate = l_ResourcesLastUpdate,
                        ResourceRare = l_RareResource,
                        ResourcePlenty = l_PlentyResource,
                        PopulationExtra = int.Parse(l_PopulationExtra),
                        God = l_GodEnum,
                        Points = int.Parse(l_Points),
                        EspionageStorage = int.Parse(l_EspionageStorage),
                        PopulationAvailable = int.Parse(l_PopulationAvailable),
                        HasConqueror = l_HasConqueror,
                        Wood = int.Parse(l_Wood),
                        Stone = int.Parse(l_Stone),
                        Iron = int.Parse(l_Iron),
                        FreeTradeCapacity = int.Parse(l_AvailableTradeCapacity),
                        WoodProduction = int.Parse(l_WoodProduction),
                        StoneProduction = int.Parse(l_StoneProduction),
                        IronProduction = int.Parse(l_IronProduction),
                        Storage = int.Parse(l_Storage),
                        LootEnabled = l_SettingsTown.LootEnabled,
                        LootIntervalMinutes = l_SettingsTown.LootIntervalMinutes,
                        Farmers = l_SettingsTown.Farmers,
                        TradeEnabled = l_SettingsTown.TradeEnabled,
                        TradeMode = l_SettingsTown.TradeMode,
                        TradeWoodRemaining = l_SettingsTown.TradeWoodRemaining,
                        TradeStoneRemaining = l_SettingsTown.TradeStoneRemaining,
                        TradeIronRemaining = l_SettingsTown.TradeIronRemaining,
                        TradePercentageWarehouse = l_SettingsTown.TradePercentageWarehouse,
                        TradeMinSendAmount = l_SettingsTown.TradeMinSendAmount,
                        TradeMaxDistance = l_SettingsTown.TradeMaxDistance,
                        CulturalFestivalsEnabled = l_SettingsTown.CulturalFestivalsEnabled,
                        BuildingQueueEnabled = l_SettingsTown.BuildingQueueEnabled,
                        BuildingTargetEnabled = l_SettingsTown.BuildingTargetEnabled,
                        DowngradeEnabled = l_SettingsTown.DowngradeEnabled,
                        BotBuildingQueue = l_SettingsTown.BotBuildingQueue,
                        ArmyUnits = l_SettingsTown.ArmyUnits,
                        UnitQueueEnabled = l_SettingsTown.UnitQueueEnabled,
                        CulturalEvents = l_SettingsTown.CulturalEvents,
                        Buildings =  l_SettingsTown.Buildings,
                        Priority = l_SettingsTown.Priority
                    };

                    Player.Towns.Add(l_Town); // Add town

                    //next town
                    l_Search = "player_id";
                    l_Index = p_Response.IndexOf(l_Search, l_Index + l_Search.Length, StringComparison.Ordinal);
                }
                Player.Towns.Sort((x, y) => CompareOrdinal(x.TownName, y.TownName));
            }
            catch (Exception ex)
            {
                Logger.WriteExceptionToLog(ex.Message);
            }
        }

        /// <summary>
        /// Handles the server response for UpdateUnitsRequest().
        /// </summary>
        private void UpdateUnitsResponse(string p_Response)
        {
            try
            {
                var l_Search = "\"collections\":{\"Units\":{\"data\":[";
                var l_Index = p_Response.IndexOf(l_Search, StringComparison.Ordinal);
                l_Search = "\"home_town_id\":";
                l_Index = p_Response.IndexOf(l_Search, l_Index, StringComparison.Ordinal);
                Player.ResetUnits();
                while (l_Index != -1)
                {
                    var l_HomeTownID = p_Response.Substring(l_Index + l_Search.Length,
                        p_Response.IndexOf(",", l_Index + l_Search.Length, StringComparison.Ordinal) -
                        (l_Index + l_Search.Length));
                    l_Search = "\"current_town_id\":";
                    l_Index = p_Response.IndexOf(l_Search, l_Index, StringComparison.Ordinal);
                    var l_CurrentTownID = p_Response.Substring(l_Index + l_Search.Length,
                        p_Response.IndexOf(",", l_Index + l_Search.Length, StringComparison.Ordinal) -
                        (l_Index + l_Search.Length));
                    for (var i = 0; i < 27; i++)
                    {
                        l_Search = ",\"";//,"unit":1,"unit_next".....
                        l_Index = p_Response.IndexOf(l_Search, l_Index, StringComparison.Ordinal);
                        var l_Unit = p_Response.Substring(l_Index + l_Search.Length,
                            p_Response.IndexOf("\"", l_Index + l_Search.Length, StringComparison.Ordinal) -
                            (l_Index + l_Search.Length));
                        l_Search = ":";
                        l_Index = p_Response.IndexOf(l_Search, l_Index, StringComparison.Ordinal);
                        var l_Count = p_Response.Substring(l_Index + l_Search.Length,
                            p_Response.IndexOf(",", l_Index + l_Search.Length, StringComparison.Ordinal) -
                            (l_Index + l_Search.Length));
                        if (l_Count != "0") //Faster
                            Player.SetUnitCount(l_Unit, l_Count, l_HomeTownID, l_CurrentTownID);
                    }
                    //next unit data set
                    l_Search = "\"home_town_id\":";
                    l_Index = p_Response.IndexOf(l_Search, l_Index + l_Search.Length, StringComparison.Ordinal);
                }
            }
            catch (Exception ex)
            {
                Logger.WriteExceptionToLog(ex.Message);
            }
        }

        /// <summary>
        /// Handles the server response for SwitchTownRequest().
        /// </summary>
        private void SwitchTownResponse(string p_Response)
        {
            try
            {
                //Set new currentTownID
                m_CurrentTownID = m_ControllerQueueDataQueue.First()["townid"];
                m_Town = Player.Towns.Single(x => x.TownID == m_ControllerQueueDataQueue.First()["townid"]);

                //Delete request data from queue
                m_ControllerQueueDataQueue.RemoveFirst();


                UpdateTownDataFromNotification(p_Response);

                var l_Search = "\"incoming_attacks_total\\\":";
                var l_Index = p_Response.IndexOf(l_Search, 0, StringComparison.Ordinal);
                var l_Incoming_attacks_total = p_Response.Substring(l_Index + l_Search.Length,
                    p_Response.IndexOf(",", l_Index + l_Search.Length, StringComparison.Ordinal) -
                    (l_Index + l_Search.Length));
                Player.IncomingAttacks = int.Parse(l_Incoming_attacks_total);

                var l_MovArrival_eta = "";
                var l_MovTId = "";
                var l_MovTName = "";
                var l_Add = true;

                //Delete old movement info
                m_Town.Movements.Clear();

                l_Search = "\\\"unit_movements\\\":[{";
                l_Index = p_Response.IndexOf(l_Search, 0, StringComparison.Ordinal);
                if (l_Index != -1)
                {
                    var l_ResponseMov = p_Response.Substring(l_Index + l_Search.Length - 1,
                        p_Response.IndexOf("}]", l_Index, StringComparison.Ordinal) - (l_Index + l_Search.Length - 1));
                    l_Search = "{\\\"type\\\":\\\"";
                    l_Index = l_ResponseMov.IndexOf(l_Search, 0, StringComparison.Ordinal);
                    while (l_Index != -1)
                    {
                        var l_MovType = l_ResponseMov.Substring(l_Index + l_Search.Length,
                            l_ResponseMov.IndexOf("\\\"", l_Index + l_Search.Length, StringComparison.Ordinal) -
                            (l_Index + l_Search.Length));
                        l_Search = "\\\"cancelable\\\":";
                        l_Index = l_ResponseMov.IndexOf(l_Search, l_Index, StringComparison.Ordinal);
                        var l_MovCancelable = l_ResponseMov.Substring(l_Index + l_Search.Length,
                            l_ResponseMov.IndexOf(",", l_Index + l_Search.Length, StringComparison.Ordinal) - (l_Index + l_Search.Length));
                        l_Search = "\\\"started_at\\\":";
                        l_Index = l_ResponseMov.IndexOf(l_Search, l_Index, StringComparison.Ordinal);
                        var l_MovStarted_at = l_ResponseMov.Substring(l_Index + l_Search.Length,
                            l_ResponseMov.IndexOf(",", l_Index + l_Search.Length, StringComparison.Ordinal) - (l_Index + l_Search.Length));
                        l_Search = "\\\"arrival_at\\\":";
                        l_Index = l_ResponseMov.IndexOf(l_Search, l_Index, StringComparison.Ordinal);
                        var l_MovArrival_at = l_ResponseMov.Substring(l_Index + l_Search.Length,
                            l_ResponseMov.IndexOf(",", l_Index + l_Search.Length, StringComparison.Ordinal) - (l_Index + l_Search.Length));
                        l_Search = "\\\"arrived_human\\\":\\\"";
                        l_Index = l_ResponseMov.IndexOf(l_Search, l_Index, StringComparison.Ordinal);
                        var l_MovArrived_human = l_ResponseMov.Substring(l_Index + l_Search.Length,
                            l_ResponseMov.IndexOf("\"", l_Index + l_Search.Length, StringComparison.Ordinal) -
                            (l_Index + l_Search.Length));
                        l_MovArrived_human = Parser.FixSpecialCharacters(l_MovArrived_human);
                        l_Search = "\\\"id\\\":";
                        l_Index = l_ResponseMov.IndexOf(l_Search, l_Index, StringComparison.Ordinal);
                        var l_MovId = l_ResponseMov.Substring(l_Index + l_Search.Length,
                            l_ResponseMov.IndexOf(",", l_Index + l_Search.Length, StringComparison.Ordinal) -
                            (l_Index + l_Search.Length));
                        l_Search = "\\\"incoming\\\":";
                        l_Index = l_ResponseMov.IndexOf(l_Search, l_Index, StringComparison.Ordinal);
                        var l_MovIncoming = l_ResponseMov.Substring(l_Index + l_Search.Length,
                            l_ResponseMov.IndexOf(",", l_Index + l_Search.Length, StringComparison.Ordinal) -
                            (l_Index + l_Search.Length));
                        string l_MovIncoming_attack;//Only when l_MovIncoming is true
                        if (l_MovIncoming.Equals("true"))
                        {
                            l_Search = "\\\"incoming_attack\\\":";
                            l_Index = l_ResponseMov.IndexOf(l_Search, l_Index, StringComparison.Ordinal);
                            l_MovIncoming_attack = l_ResponseMov.Substring(l_Index + l_Search.Length,
                                l_ResponseMov.IndexOf(",", l_Index + l_Search.Length, StringComparison.Ordinal) -
                                (l_Index + l_Search.Length));
                        }
                        else
                        {
                            l_MovIncoming_attack = "";
                        }
                        l_Search = "\\\"command_name\\\":\\\"";
                        l_Index = l_ResponseMov.IndexOf(l_Search, l_Index, StringComparison.Ordinal);
                        var l_MovCommand_name = l_ResponseMov.Substring(l_Index + l_Search.Length,
                            l_ResponseMov.IndexOf("\"", l_Index + l_Search.Length, StringComparison.Ordinal) -
                            (l_Index + l_Search.Length));
                        l_MovCommand_name = Parser.FixSpecialCharacters(l_MovCommand_name);

                        //Movement target town
                        if (l_MovIncoming_attack.Equals("true"))
                        {
                            l_Search = "\\\"town\\\":{";
                            l_Index = l_ResponseMov.IndexOf(l_Search, l_Index, StringComparison.Ordinal);
                            var l_TownData = l_ResponseMov.Substring(l_Index + l_Search.Length,
                                l_ResponseMov.IndexOf("}", l_Index + l_Search.Length, StringComparison.Ordinal) -
                                (l_Index + l_Search.Length));
                            if (l_TownData.Contains("#"))
                            {
                                l_Search = "#";
                                l_Index = l_ResponseMov.IndexOf(l_Search, l_Index, StringComparison.Ordinal);
                                var l_MovTargetTownEncrypted = l_ResponseMov.Substring(l_Index + l_Search.Length,
                                    l_ResponseMov.IndexOf("\\", l_Index + l_Search.Length, StringComparison.Ordinal) - (l_Index + l_Search.Length));
                                var l_MovTargetTownDecrypted = Parser.DecryptLink(l_MovTargetTownEncrypted);
                                if (!l_MovTargetTownDecrypted.Equals("false"))
                                {
                                    l_Search = "\"id\":";
                                    var l_Index2 = l_MovTargetTownDecrypted.IndexOf(l_Search, 0, StringComparison.Ordinal);
                                    l_MovTId = l_MovTargetTownDecrypted.Substring(l_Index2 + l_Search.Length,
                                        l_MovTargetTownDecrypted.IndexOf(",", l_Index2 + l_Search.Length,
                                            StringComparison.Ordinal) - (l_Index2 + l_Search.Length));
                                    l_Search = "\"name\":\"";
                                    l_Index2 = l_MovTargetTownDecrypted.IndexOf(l_Search, l_Index2, StringComparison.Ordinal);
                                    l_MovTName = l_MovTargetTownDecrypted.Substring(l_Index2 + l_Search.Length,
                                        l_MovTargetTownDecrypted.IndexOf("\"", l_Index2 + l_Search.Length,
                                            StringComparison.Ordinal) - (l_Index2 + l_Search.Length));
                                    l_MovTName = Parser.FixSpecialCharacters(l_MovTName);
                                }
                                else
                                {
                                    //Decryptlink failed
                                    l_Add = false;
                                }
                            }
                            else
                            {
                                //Incoming Quest attack
                                l_Add = false;
                            }
                        }

                        //Add
                        if (l_Add)
                        {
                            m_Town.Movements.Add(new Movement(l_MovType, l_MovCancelable.Equals("true"), l_MovStarted_at,
                                l_MovArrival_at, l_MovArrival_eta, l_MovArrived_human, l_MovId,
                                l_MovIncoming.Equals("true"), l_MovIncoming_attack.Equals("true"), l_MovCommand_name, l_MovTId, l_MovTName));
                        }
                        else
                        {
                            l_Add = true;
                        }

                        //Search next
                        l_Search = "{\\\"type\\\":\\\"";
                        l_Index = l_ResponseMov.IndexOf(l_Search, l_Index, StringComparison.Ordinal);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.WriteExceptionToLog(ex.Message);
            }
        }

        /// <summary>
        /// Handles the server response for LocateFarmersRequest().
        /// Assign unused relations to farmers.
        /// </summary>
        private void LocateFarmersResponse(string p_Response)
        {
            try
            {
                //{"id":####,"name":"********","dir":"n","expansion_stage":4,"x":###,"y":###,"ox":###,"oy":###,"offer":"stone","demand":"iron","mood":14,"relation_status":1,"ratio":1.25,"loot":1303078581,"lootable_human":"tomorrow at 12:16 AM ","looted":1303042581}

                //Get island quest data
                m_Town.AvailableQuests = new Regex("island_quest_base_name").Matches(p_Response).Count;

                //Get farmer data
                var l_Search = "\"relation_status\":";
                var l_Index = p_Response.IndexOf(l_Search, 0, StringComparison.Ordinal);

                while (l_Index != -1)
                {
                    while (!p_Response[l_Index].Equals('{'))
                        l_Index--;
                    l_Search = "\"id\":";
                    l_Index = p_Response.IndexOf(l_Search, l_Index, StringComparison.Ordinal);
                    //Get farmers id
                    var l_ID = p_Response.Substring(l_Index + l_Search.Length,
                        p_Response.IndexOf(",", l_Index + l_Search.Length, StringComparison.Ordinal) -
                        (l_Index + l_Search.Length));
                    l_Search = "\"name\":\"";
                    l_Index = p_Response.IndexOf(l_Search, l_Index, StringComparison.Ordinal);
                    //Get farmers name
                    var l_Name = p_Response.Substring(l_Index + l_Search.Length,
                        p_Response.IndexOf("\",", l_Index + l_Search.Length, StringComparison.Ordinal) -
                        (l_Index + l_Search.Length));
                    l_Name = Parser.FixSpecialCharacters(l_Name);
                    l_Search = "\"expansion_stage\":";
                    l_Index = p_Response.IndexOf(l_Search, l_Index, StringComparison.Ordinal);
                    //Get expansion state
                    var l_ExpansionState = int.Parse(p_Response.Substring(l_Index + l_Search.Length,
                        p_Response.IndexOf(",", l_Index + l_Search.Length, StringComparison.Ordinal) -
                        (l_Index + l_Search.Length)));
                    l_Search = "\"x\":";
                    l_Index = p_Response.IndexOf(l_Search, l_Index, StringComparison.Ordinal);
                    //Get farmers island x coord
                    var l_IslandX = p_Response.Substring(l_Index + l_Search.Length,
                        p_Response.IndexOf(",", l_Index + l_Search.Length, StringComparison.Ordinal) -
                        (l_Index + l_Search.Length));
                    l_Search = "\"y\":";
                    l_Index = p_Response.IndexOf(l_Search, l_Index, StringComparison.Ordinal);
                    //Get farmers island y coord
                    var l_IslandY = p_Response.Substring(l_Index + l_Search.Length,
                        p_Response.IndexOf(",", l_Index + l_Search.Length, StringComparison.Ordinal) -
                        (l_Index + l_Search.Length));
                    l_Search = "\"mood\":";
                    l_Index = p_Response.IndexOf(l_Search, l_Index, StringComparison.Ordinal);
                    //Get farmers mood
                    var l_Mood = int.Parse(p_Response.Substring(l_Index + l_Search.Length,
                        p_Response.IndexOf(",", l_Index + l_Search.Length, StringComparison.Ordinal) -
                        (l_Index + l_Search.Length)));
                    l_Search = "\"relation_status\":";
                    l_Index = p_Response.IndexOf(l_Search, l_Index, StringComparison.Ordinal);
                    //Get farmers relation status
                    var l_RelationStatus = p_Response.Substring(l_Index + l_Search.Length,
                        p_Response.IndexOf(",", l_Index + l_Search.Length, StringComparison.Ordinal) -
                        (l_Index + l_Search.Length)).Equals("1");
                    string l_LootTimer;
                    string l_LootTimerHuman;
                    if (l_RelationStatus)
                    {
                        l_Search = "\"loot\":";
                        l_Index = p_Response.IndexOf(l_Search, l_Index, StringComparison.Ordinal);
                        //Get farmers loot time
                        l_LootTimer = p_Response.Substring(l_Index + l_Search.Length,
                            p_Response.IndexOf(",", l_Index + l_Search.Length, StringComparison.Ordinal) -
                            (l_Index + l_Search.Length));
                        l_Search = "\"lootable_human\":\"";
                        l_Index = p_Response.IndexOf(l_Search, l_Index, StringComparison.Ordinal);
                        //Get farmers loot time human
                        l_LootTimerHuman = p_Response.Substring(l_Index + l_Search.Length,
                            p_Response.IndexOf("\",", l_Index + l_Search.Length, StringComparison.Ordinal) -
                            (l_Index + l_Search.Length));
                        l_LootTimerHuman = Parser.FixSpecialCharacters(l_LootTimerHuman);
                    }
                    else
                    {
                        l_LootTimer = "0";
                        l_LootTimerHuman = "Not available";
                    }

                    //Add or update farmer
                    if (m_Town.IslandX.Equals(l_IslandX) && m_Town.IslandY.Equals(l_IslandY))
                    {
                        if (!m_Town.Farmers.Exists(x => x.ID == l_ID)) //If farmer already in list, skip
                        {
                            m_Town.Farmers.Add(new Farmer(l_ID, l_Name, l_ExpansionState, l_IslandX, l_IslandY, l_Mood, l_RelationStatus, l_LootTimer, l_LootTimerHuman));
                        }
                        else
                        {
                            var l_Farmer = m_Town.Farmers.Single(x => x.ID == l_ID);
                            l_Farmer.Name = l_Name;
                            l_Farmer.ExpansionState = l_ExpansionState;
                            l_Farmer.IslandX = l_IslandX;
                            l_Farmer.IslandY = l_IslandY;
                            l_Farmer.Mood = l_Mood;
                            l_Farmer.RelationStatus = l_RelationStatus;
                            l_Farmer.LootTimer = l_LootTimer;
                            l_Farmer.LootTimerHuman = l_LootTimerHuman;
                        }

                        //assign unused farm relations
                        for (var i = 0; i < Player.UnusedFarmTownRelations.Count; i++)
                        {
                            var l_Unused = Player.UnusedFarmTownRelations[i];
                            if (l_Unused.FarmerID == l_ID)
                            {
                                var l_Farmer = m_Town.Farmers.Single(x => x.ID == l_ID);
                                l_Farmer.BattlePointsFarmID = l_Unused.ID;
                                l_Farmer.ExpansionState = l_Unused.ExpansionState;
                                l_Farmer.LootedResources = l_Unused.LootedResources;
                                Player.UnusedFarmTownRelations.RemoveAt(i);
                                break;
                            }
                        }
                    }

                    //Search next farmer
                    l_Search = "\"relation_status\":";
                    l_Index = p_Response.IndexOf(l_Search, l_Index + l_Search.Length, StringComparison.Ordinal);
                }

                //Sort farmers
                m_Town.Farmers.Sort((x, y) => CompareOrdinal(x.ID, y.ID));

                //Decide if something to loot
                foreach (var l_Farmer in m_Town.Farmers)
                {
                    if (l_Farmer.Lootable && !m_Town.StorageFull && l_Farmer.Enabled && m_Town.LootEnabled)
                    {
                        //Add looting request in reverse order
                        m_ControllerQueue.AddFirst(ControllerStates.LootFarmer);

                        m_ControllerQueue.AddFirst(ControllerStates.GetTownSpecificData);

                        m_ControllerQueueDataQueue.AddFirst(new Dictionary<string, string>() { { "farmid", l_Farmer.ID }, { "option", m_Town.LootIntervalOption } }); //how much to loot
                        m_ControllerQueue.AddFirst(ControllerStates.OpenFarmerWindow);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.WriteExceptionToLog(ex.Message);
            }
        }

        /// <summary>
        /// Handle the server response for OpenFarmerWindowRequest().
        /// </summary>
        private void OpenFarmerWindowResponse(string p_Response)
        {
            try
            {
                //Response is useless at the moment
            }
            catch (Exception ex)
            {
                Logger.WriteToLog(ex.Message);
            }
        }

        /// <summary>
        /// Handle the server response for GetTownSpecificDataRequest().
        /// </summary>
        private void GetTownSpecificDataResponse(string p_Response)
        {
            try
            {
                //{"claim_resource_values":[56,125,273,533],"trade_duration":48,"max_trade_capacity":10000,"available_trade_capacity":10000,
                var l_Farmer = m_Town.Farmers.Single(x => x.ID == m_ControllerQueueDataQueue.First()["farmid"]);
                var l_Index = p_Response.IndexOf("[", StringComparison.Ordinal) + 1;
                l_Farmer.ClaimResourceValues =
                    p_Response.Substring(l_Index, p_Response.IndexOf("]", StringComparison.Ordinal) - l_Index)
                        .Split(',');
            }
            catch (Exception ex)
            {
                Logger.WriteExceptionToLog(ex.Message);
            }
        }

        /// <summary>
        /// Handle the server response for LootFarmerRequest().
        /// </summary>
        private void LootFarmerResponse(string p_Response)
        {
            try
            {
                var l_Farmer = m_Town.Farmers.Single(x => x.ID == m_ControllerQueueDataQueue.First()["farmid"]);

                //FarmTownPlayerRelations
                var l_Search = "\\\"FarmTownPlayerRelation\\\"";
                var l_Index = p_Response.IndexOf(l_Search, StringComparison.Ordinal);
                var l_IndexStart = l_Index;

                l_Search = "\\\"lootable_at\\\":";
                l_Index = p_Response.IndexOf(l_Search, l_IndexStart, StringComparison.Ordinal);
                var l_LootableAt = p_Response.Substring(l_Index + l_Search.Length,
                        p_Response.IndexOf(",", l_Index + l_Search.Length, StringComparison.Ordinal) -
                        (l_Index + l_Search.Length));

                l_Search = "\\\"loot\\\":";
                l_Index = p_Response.IndexOf(l_Search, l_IndexStart, StringComparison.Ordinal);
                var l_Loot = int.Parse(p_Response.Substring(l_Index + l_Search.Length,
                        p_Response.IndexOf(",", l_Index + l_Search.Length, StringComparison.Ordinal) -
                        (l_Index + l_Search.Length)));

                l_Farmer.LootedResources = l_Loot;
                l_Farmer.LootTimer = l_LootableAt;

                UpdateTownDataFromNotification(p_Response);

                CallLogEvent(m_Town.TownName + ": Successful demanded resources from " + l_Farmer.Name + ".");
                //Remove data for requests
                m_ControllerQueueDataQueue.RemoveFirst();
            }
            catch (Exception ex)
            {
                Logger.WriteExceptionToLog(ex.Message);
            }
        }

        /// <summary>
        /// Handle the server response for CheckTradeRequest().
        /// No Town data in this response.
        /// </summary>
        private void CheckTradeResponse(string p_Response)
        {
            try
            {
                var l_TargetTown =
                    Player.Towns.Single(x => x.TownID == m_ControllerQueueDataQueue.First()["receivertownid"]);

                var l_Search = "\"max_capacity\":";

                var l_Index = p_Response.IndexOf(l_Search, 0, StringComparison.Ordinal);
                if (l_Index != -1)
                {
                    //Get trade info
                    l_Index = p_Response.IndexOf(l_Search, 0, StringComparison.Ordinal);
                    var l_MaxCapacity = p_Response.Substring(l_Index + l_Search.Length,
                        p_Response.IndexOf(",", l_Index + l_Search.Length, StringComparison.Ordinal) -
                        (l_Index + l_Search.Length));
                    l_Search = "\"available_capacity\":";
                    l_Index = p_Response.IndexOf(l_Search, l_Index, StringComparison.Ordinal);
                    var l_AvailableCapacity = p_Response.Substring(l_Index + l_Search.Length,
                        p_Response.IndexOf(",", l_Index + l_Search.Length, StringComparison.Ordinal) -
                        (l_Index + l_Search.Length));
                    l_Search = "{\"wood\":";
                    l_Index = p_Response.IndexOf(l_Search, l_Index, StringComparison.Ordinal);
                    var l_Wood = p_Response.Substring(l_Index + l_Search.Length,
                        p_Response.IndexOf(",", l_Index + l_Search.Length, StringComparison.Ordinal) -
                        (l_Index + l_Search.Length));
                    l_Search = "\"stone\":";
                    l_Index = p_Response.IndexOf(l_Search, l_Index, StringComparison.Ordinal);
                    var l_Stone = p_Response.Substring(l_Index + l_Search.Length,
                        p_Response.IndexOf(",", l_Index + l_Search.Length, StringComparison.Ordinal) -
                        (l_Index + l_Search.Length));
                    l_Index = p_Response.IndexOf(l_Search, l_Index, StringComparison.Ordinal);
                    var l_Iron = p_Response.Substring(l_Index + l_Search.Length,
                        p_Response.IndexOf(",", l_Index + l_Search.Length, StringComparison.Ordinal) -
                        (l_Index + l_Search.Length));
                    l_Search = "\"storage_volume\":";
                    l_Index = p_Response.IndexOf(l_Search, l_Index, StringComparison.Ordinal);
                    l_Search = "{\"wood\":";
                    l_Index = p_Response.IndexOf(l_Search, l_Index, StringComparison.Ordinal);
                    var l_IncWood = p_Response.Substring(l_Index + l_Search.Length,
                        p_Response.IndexOf(",", l_Index + l_Search.Length, StringComparison.Ordinal) -
                        (l_Index + l_Search.Length));
                    l_Search = "\"stone\":";
                    l_Index = p_Response.IndexOf(l_Search, l_Index, StringComparison.Ordinal);
                    var l_IncStone = p_Response.Substring(l_Index + l_Search.Length,
                        p_Response.IndexOf(",", l_Index + l_Search.Length, StringComparison.Ordinal) -
                        (l_Index + l_Search.Length));
                    l_Search = "\"iron\":";
                    l_Index = p_Response.IndexOf(l_Search, l_Index, StringComparison.Ordinal);
                    var l_IncIron = p_Response.Substring(l_Index + l_Search.Length,
                        p_Response.IndexOf("}", l_Index + l_Search.Length, StringComparison.Ordinal) -
                        (l_Index + l_Search.Length));

                    //Update resources
                    l_TargetTown.Wood = int.Parse(l_Wood);
                    l_TargetTown.Stone = int.Parse(l_Stone);
                    l_TargetTown.Iron = int.Parse(l_Iron);
                    l_TargetTown.WoodInc = int.Parse(l_IncWood);
                    l_TargetTown.StoneInc = int.Parse(l_IncStone);
                    l_TargetTown.IronInc = int.Parse(l_IncIron);
                    l_TargetTown.ResourcesLastUpdate = ServerTime;
                    //Update capacity
                    m_Town.MaxTradeCapacity = int.Parse(l_MaxCapacity);
                    m_Town.FreeTradeCapacity = int.Parse(l_AvailableCapacity);

                    //Add Sending resources to queue
                    m_ControllerQueue.AddFirst(ControllerStates.SendResources);
                }
                else
                {
                    //Trade window not loaded or changed.
                    CallLogEvent(m_Town.TownName + ": Trade window not loaded or changed.");
                }
            }
            catch (Exception ex)
            {
                Logger.WriteExceptionToLog(ex.Message);
            }
        }

        /// <summary>
        /// Handle the server response for SendResourcesRequest().
        /// </summary>
        private void SendResourcesResponse(string p_Response)
        {
            try
            {
                var l_Search = "_srvtime";
                var l_Index = p_Response.IndexOf(l_Search, 0, StringComparison.Ordinal);
                if (l_Index != -1)
                {
                    UpdateTownDataFromNotification(p_Response);
                }
            }
            catch (Exception ex)
            {
                Logger.WriteExceptionToLog(ex.Message);
            }
        }

        /// <summary>
        /// Handle the server response for UpdateCulturalInfoRequest().
        /// No Town Data in this response.
        /// </summary>
        private void UpdateCulturalInfoResponse(string p_Response)
        {
            try
            {
                //Update cultural stats
                //l_Search = "<div id=\"place_culture_level\">"; //2.37
                var l_Search = "<div id=\\\"place_culture_level\\\">";
                var l_Index = p_Response.IndexOf(l_Search, 0, StringComparison.Ordinal);
                var l_CulturalLevelStr = p_Response.Substring(l_Index + l_Search.Length,
                    p_Response.IndexOf("<", l_Index + l_Search.Length, StringComparison.Ordinal) -
                    (l_Index + l_Search.Length));
                l_CulturalLevelStr = Parser.FixSpecialCharacters(l_CulturalLevelStr);

                Player.CultureLevel = int.Parse(Join("", Regex.Split(l_CulturalLevelStr, "[^\\d]")));

                //Player.CulturalLevelStr = l_CulturalLevelStr;
                //l_Search = "<div id=\"place_culture_towns\">"; //2.37
                /*l_Search = "id=\\\"place_culture_towns\\\">";
                l_Index = p_Response.IndexOf(l_Search, l_Index, StringComparison.Ordinal);
                var l_CulturalCitiesStr = p_Response.Substring(l_Index + l_Search.Length,
                    p_Response.IndexOf("<", l_Index + l_Search.Length, StringComparison.Ordinal) -
                    (l_Index + l_Search.Length));
                l_CulturalCitiesStr = Join("", Regex.Split(Parser.FixSpecialCharacters(l_CulturalCitiesStr), "[^\\d]"));

                Player.MaximumTownCount = int.Parse(l_CulturalCitiesStr.Split('/')[1]);*/

                //Player.CulturalCitiesStr = l_CulturalCitiesStr;
                //l_Search = "<div id=\"place_culture_count\">"; //2.37
                l_Search = "id=\\\"place_culture_count\\\">";
                l_Index = p_Response.IndexOf(l_Search, l_Index, StringComparison.Ordinal);
                l_Search = "\\/>";
                l_Index = p_Response.IndexOf(l_Search, l_Index + l_Search.Length, StringComparison.Ordinal);
                var l_CulturalPointsStr = p_Response.Substring(l_Index + l_Search.Length,
                    p_Response.IndexOf("<\\/div>", l_Index + l_Search.Length, StringComparison.Ordinal) -
                    (l_Index + l_Search.Length));
                var l_CulturalPointsCurrent = l_CulturalPointsStr.Split('/')[0];
                var CulturalPointsMax = l_CulturalPointsStr.Split('/')[1];
                l_CulturalPointsCurrent = Join("", Regex.Split(l_CulturalPointsCurrent, "[^\\d]"));
                CulturalPointsMax = Join("", Regex.Split(CulturalPointsMax, "[^\\d]"));
                Player.CulturalPointsCurrent = int.Parse(l_CulturalPointsCurrent);
                Player.CulturalPointsMax = int.Parse(CulturalPointsMax);

                //Update cultural festivals

                var l_IndexParty = p_Response.IndexOf("party.jpg", StringComparison.Ordinal);
                if (l_IndexParty == -1)
                    l_IndexParty = p_Response.IndexOf("birthday.png", StringComparison.Ordinal);
                if (l_IndexParty == -1)
                    l_IndexParty = p_Response.IndexOf("birthday.jpg", StringComparison.Ordinal);
                if (l_IndexParty == -1)
                    l_IndexParty = p_Response.IndexOf("xmas_party_new.jpg", StringComparison.Ordinal);
                if (l_IndexParty == -1)
                    l_IndexParty = p_Response.IndexOf(".jpg", StringComparison.Ordinal);

                var l_IndexGames = p_Response.IndexOf("bread.jpg", StringComparison.Ordinal);
                var l_IndexTriumph = p_Response.IndexOf("triumph.jpg", StringComparison.Ordinal);
                var l_IndexTheater = p_Response.IndexOf("theater.jpg", StringComparison.Ordinal);

                //Get local names
                //l_Search = "<div class=\"game_header bold\">"; //2.37
                l_Search = "class=\\\"game_header bold\\\">";
                l_Index = p_Response.IndexOf(l_Search, 0, StringComparison.Ordinal);
                m_Town.CulturalEvents[0].NameLocal = p_Response.Substring(l_Index + l_Search.Length,
                    p_Response.IndexOf("<", l_Index + l_Search.Length, StringComparison.Ordinal) -
                    (l_Index + l_Search.Length));
                //l_Search = "<div class=\"game_header bold\">"; //2.37
                l_Search = "class=\\\"game_header bold\\\">";
                l_Index = p_Response.IndexOf(l_Search, l_Index + l_Search.Length, StringComparison.Ordinal);
                m_Town.CulturalEvents[1].NameLocal =
                    p_Response.Substring(l_Index + l_Search.Length,
                        p_Response.IndexOf("<", l_Index + l_Search.Length, StringComparison.Ordinal) -
                        (l_Index + l_Search.Length));
                //l_Search = "<div class=\"game_header bold\">"; //2.37
                l_Search = "class=\\\"game_header bold\\\">";
                l_Index = p_Response.IndexOf(l_Search, l_Index + l_Search.Length, StringComparison.Ordinal);
                m_Town.CulturalEvents[2].NameLocal =
                    p_Response.Substring(l_Index + l_Search.Length,
                        p_Response.IndexOf("<", l_Index + l_Search.Length, StringComparison.Ordinal) -
                        (l_Index + l_Search.Length));
                //l_Search = "<div class=\"game_header bold\">"; //2.37
                l_Search = "class=\\\"game_header bold\\\">";
                l_Index = p_Response.IndexOf(l_Search, l_Index + l_Search.Length, StringComparison.Ordinal);
                m_Town.CulturalEvents[3].NameLocal =
                    p_Response.Substring(l_Index + l_Search.Length,
                        p_Response.IndexOf("<", l_Index + l_Search.Length, StringComparison.Ordinal) -
                        (l_Index + l_Search.Length));

                //Check requirements
                var l_SubString = p_Response.Substring(l_IndexParty, l_IndexGames - l_IndexParty);
                //m_Player.Towns[m_CurrentTownIntern].CulturalEvents[0].Ready = l_SubString.Contains("onclick=\"BuildingPlace.startCelebration('"); //2.37
                //l_Town.CulturalEvents[0].Ready = l_SubString.Contains("onclick=\\\"BuildingPlace.startCelebration('");
                m_Town.CulturalEvents[0].Ready = l_SubString.Contains("data-enabled=\\\"1\\\"");
                m_Town.CulturalEvents[0].EnoughResources = !l_SubString.Contains("place_not_enough_resources");
                /*if (m_Player.Towns[m_CurrentTownIntern].CulturalEvents[0].EnoughResources)
                {
                    l_Index = l_IndexParty;
                    //l_Search = "<td class=\"\">"; //2.37
                    l_Search = "<td class=\\\"\\\">";
                    l_Index = p_Response.IndexOf(l_Search, l_Index);
                    l_Wood = p_Response.Substring(l_Index + l_Search.Length, p_Response.IndexOf("<", l_Index + l_Search.Length) - (l_Index + l_Search.Length));
                    //l_Search = "<td class=\"\">"; //2.37
                    l_Search = "<td class=\\\"\\\">";
                    l_Index = p_Response.IndexOf(l_Search, l_Index + l_Search.Length);
                    l_Stone = p_Response.Substring(l_Index + l_Search.Length, p_Response.IndexOf("<", l_Index + l_Search.Length) - (l_Index + l_Search.Length));
                    //l_Search = "<td class=\"\">"; //2.37
                    l_Search = "<td class=\\\"\\\">";
                    l_Index = p_Response.IndexOf(l_Search, l_Index + l_Search.Length);
                    l_Iron = p_Response.Substring(l_Index + l_Search.Length, p_Response.IndexOf("<", l_Index + l_Search.Length) - (l_Index + l_Search.Length));
                    l_Wood = String.Join("", Regex.Split(l_Wood, "[^\\d]"));
                    l_Stone = String.Join("", Regex.Split(l_Stone, "[^\\d]"));
                    l_Iron = String.Join("", Regex.Split(l_Iron, "[^\\d]"));
                    m_Player.Towns[m_CurrentTownIntern].CulturalEvents[0].EnoughResources = int.Parse(l_Wood) <= m_Player.Towns[m_CurrentTownIntern].Wood && int.Parse(l_Stone) <= m_Player.Towns[m_CurrentTownIntern].Stone && int.Parse(l_Iron) <= m_Player.Towns[m_CurrentTownIntern].Iron;
                }*/
                l_SubString = p_Response.Substring(l_IndexGames, l_IndexTriumph - l_IndexGames);
                //m_Player.Towns[m_CurrentTownIntern].CulturalEvents[1].Ready = l_SubString.Contains("onclick=\"BuildingPlace.startCelebration('games'"); //2.37
                //m_Player.Towns[m_CurrentTownIntern].CulturalEvents[1].Ready = l_SubString.Contains("onclick=\\\"BuildingPlace.startCelebration('games'"); //2.66?
                //l_Town.CulturalEvents[1].Ready = l_SubString.Contains("btn_organize_olympic_games");
                m_Town.CulturalEvents[1].Ready = l_SubString.Contains("data-enabled=\\\"1\\\"");
                m_Town.CulturalEvents[1].EnoughResources = Player.Gold >= 50;
                l_SubString = p_Response.Substring(l_IndexTriumph, l_IndexTheater - l_IndexTriumph);
                //m_Player.Towns[m_CurrentTownIntern].CulturalEvents[2].Ready = l_SubString.Contains("onclick=\"BuildingPlace.startCelebration('triumph'"); //2.37
                //l_Town.CulturalEvents[2].Ready = l_SubString.Contains("onclick=\\\"BuildingPlace.startCelebration('triumph'");
                m_Town.CulturalEvents[2].Ready = l_SubString.Contains("data-enabled=\\\"1\\\"");
                m_Town.CulturalEvents[2].EnoughResources = !l_SubString.Contains("place_not_enough_resources");
                l_SubString = p_Response.Substring(l_IndexTheater, p_Response.Length - l_IndexTheater);
                //m_Player.Towns[m_CurrentTownIntern].CulturalEvents[3].Ready = l_SubString.Contains("onclick=\"BuildingPlace.startCelebration('theater'"); //2.37
                //l_Town.CulturalEvents[3].Ready = l_SubString.Contains("onclick=\\\"BuildingPlace.startCelebration('theater'");
                m_Town.CulturalEvents[3].Ready = l_SubString.Contains("data-enabled=\\\"1\\\"");
                m_Town.CulturalEvents[3].EnoughResources = !l_SubString.Contains("place_not_enough_resources");
                /*if (m_Player.Towns[m_CurrentTownIntern].CulturalEvents[3].EnoughResources)
                {
                    l_Index = l_IndexTheater;
                    //l_Search = "<td class=\"\">"; //2.37
                    l_Search = "<td class=\\\"\\\">";
                    l_Index = p_Response.IndexOf(l_Search, l_Index);
                    l_Wood = p_Response.Substring(l_Index + l_Search.Length, p_Response.IndexOf("<", l_Index + l_Search.Length) - (l_Index + l_Search.Length));
                    //l_Search = "<td class=\"\">"; //2.37
                    l_Search = "<td class=\\\"\\\">";
                    l_Index = p_Response.IndexOf(l_Search, l_Index + l_Search.Length);
                    l_Stone = p_Response.Substring(l_Index + l_Search.Length, p_Response.IndexOf("<", l_Index + l_Search.Length) - (l_Index + l_Search.Length));
                    //l_Search = "<td class=\"\">"; //2.37
                    l_Search = "<td class=\\\"\\\">";
                    l_Index = p_Response.IndexOf(l_Search, l_Index + l_Search.Length);
                    l_Iron = p_Response.Substring(l_Index + l_Search.Length, p_Response.IndexOf("<", l_Index + l_Search.Length) - (l_Index + l_Search.Length));
                    l_Wood = String.Join("", Regex.Split(l_Wood, "[^\\d]"));
                    l_Stone = String.Join("", Regex.Split(l_Stone, "[^\\d]"));
                    l_Iron = String.Join("", Regex.Split(l_Iron, "[^\\d]"));
                    m_Player.Towns[m_CurrentTownIntern].CulturalEvents[3].EnoughResources = int.Parse(l_Wood) <= m_Player.Towns[m_CurrentTownIntern].Wood && int.Parse(l_Stone) <= m_Player.Towns[m_CurrentTownIntern].Stone && int.Parse(l_Iron) <= m_Player.Towns[m_CurrentTownIntern].Iron;
                }*/

                
                //return if not enabled
                if (!m_Town.CulturalFestivalsEnabled)
                    return;
                //Decide which cultural events to start
                if (m_Town.CulturalEvents[0].Enabled && m_Town.CulturalEvents[0].Ready && m_Town.CulturalEvents[0].EnoughResources)
                {
                    m_ControllerQueue.AddFirst(ControllerStates.StartCulturalFestival);
                    m_ControllerQueueDataQueue.AddFirst(new Dictionary<string, string>() { { "event", "party" } });
                }
                if (m_Town.CulturalEvents[1].Enabled && m_Town.CulturalEvents[1].Ready && m_Town.CulturalEvents[1].EnoughResources)
                {
                    m_ControllerQueue.AddFirst(ControllerStates.StartCulturalFestival);
                    m_ControllerQueueDataQueue.AddFirst(new Dictionary<string, string>() { { "event", "games" } });
                }
                if (m_Town.CulturalEvents[2].Enabled && m_Town.CulturalEvents[2].Ready && m_Town.CulturalEvents[2].EnoughResources)
                {
                    m_ControllerQueue.AddFirst(ControllerStates.StartCulturalFestival);
                    m_ControllerQueueDataQueue.AddFirst(new Dictionary<string, string>() { { "event", "triumph" } });
                }
                if (m_Town.CulturalEvents[3].Enabled && m_Town.CulturalEvents[3].Ready && m_Town.CulturalEvents[3].EnoughResources)
                {
                    m_ControllerQueue.AddFirst(ControllerStates.StartCulturalFestival);
                    m_ControllerQueueDataQueue.AddFirst(new Dictionary<string, string>() { { "event", "theater" } });
                }
            }
            catch (Exception ex)
            {
                Logger.WriteExceptionToLog(ex.Message);
            }
        }

        /// <summary>
        /// Handle the server response for StartCulturalFestivalRequest().
        /// </summary>
        private void StartCulturalFestivalResponse(string p_Response)
        {
            try
            {
                CallLogEvent(m_Town.TownName + ": Started " + m_ControllerQueueDataQueue.First()["event"] + ".");
                UpdateTownDataFromNotification(p_Response);

                m_ControllerQueueDataQueue.RemoveFirst();
            }
            catch (Exception ex)
            {
                Logger.WriteExceptionToLog(ex.Message);
            }
        }

        /// <summary>
        /// Handle the server response for CheckMainBuildingRequest().
        /// No town data in this response.
        /// </summary>
        private void CheckMainBuildingResponse(string p_Response)
        {
            try
            {
                string l_Search;
                int l_Index;
                //update town data
                foreach (var l_Building in m_Town.Buildings)
                {
                    //\"hide\":{
                    l_Search = "\\\"" + l_Building.DevName.ToString("G") + "\\\":{\\\"name";
                    l_Index = p_Response.IndexOf(l_Search, StringComparison.Ordinal);

                    l_Search = "\\\"level\\\":";
                    l_Index = p_Response.IndexOf(l_Search, l_Index, StringComparison.Ordinal);
                    var l_Level = p_Response.Substring(l_Index + l_Search.Length,
                        p_Response.IndexOf(",", l_Index + l_Search.Length, StringComparison.Ordinal) -
                        (l_Index + l_Search.Length));
                    if (l_Level.Contains("-"))
                        l_Level = "0";
                    l_Building.Level = int.Parse(l_Level);

                    /*
                    l_Search = "\\\"next_level\\\":";
                    l_Index = p_Response.IndexOf(l_Search, l_Index, StringComparison.Ordinal);
                    var l_NextLevel = p_Response.Substring(l_Index + l_Search.Length,
                        p_Response.IndexOf(",", l_Index + l_Search.Length, StringComparison.Ordinal) -
                        (l_Index + l_Search.Length));
                    l_Building.NextLevel = int.Parse(l_NextLevel);
                    */

                    l_Search = "\\\"can_upgrade\\\":";
                    l_Index = p_Response.IndexOf(l_Search, l_Index, StringComparison.Ordinal);
                    var l_CanUpgrade = p_Response.Substring(l_Index + l_Search.Length,
                        p_Response.IndexOf(",", l_Index + l_Search.Length, StringComparison.Ordinal) -
                        (l_Index + l_Search.Length));
                    l_Building.Upgradable = l_CanUpgrade.Equals("true");

                    l_Search = "\\\"can_upgrade_reduced\\\":";
                    l_Index = p_Response.IndexOf(l_Search, l_Index, StringComparison.Ordinal);
                    var l_CanUpgradeReduced = p_Response.Substring(l_Index + l_Search.Length,
                        p_Response.IndexOf(",", l_Index + l_Search.Length, StringComparison.Ordinal) -
                        (l_Index + l_Search.Length));
                    l_Building.UpgradeReduceable = l_CanUpgradeReduced.Equals("true");

                    l_Search = "\\\"max_level\\\":";
                    l_Index = p_Response.IndexOf(l_Search, l_Index, StringComparison.Ordinal);
                    var l_MaxLevel = p_Response.Substring(l_Index + l_Search.Length,
                        p_Response.IndexOf(",", l_Index + l_Search.Length, StringComparison.Ordinal) -
                        (l_Index + l_Search.Length));
                    l_Building.IsMaxLevel = l_MaxLevel.Equals("true");

                    l_Search = "\\\"can_tear_down\\\":";
                    l_Index = p_Response.IndexOf(l_Search, l_Index, StringComparison.Ordinal);
                    var l_CanTearDown = p_Response.Substring(l_Index + l_Search.Length,
                        p_Response.IndexOf(",", l_Index + l_Search.Length, StringComparison.Ordinal) -
                        (l_Index + l_Search.Length));
                    l_Building.Teardownable = l_CanTearDown.Equals("true");
                }

                l_Search = "BuildingMain.full_queue = ";
                l_Index = p_Response.IndexOf(l_Search, StringComparison.Ordinal);
                m_Town.IsBuildingOrderQueueFull =
                    p_Response.Substring(l_Index + l_Search.Length,
                        p_Response.IndexOf(";", l_Index + l_Search.Length, StringComparison.Ordinal) -
                        (l_Index + l_Search.Length)).Equals("true");

                //Remove all finished building queue items.
                m_Town.IngameBuildingQueue.RemoveAll(x => Parser.UnixToHumanTime(x.CompletedAt) < DateTime.Now);

                if (m_Town.IsBuildingOrderQueueFull)
                {
                    CallLogEvent(m_Town.TownName + ": Building queue is full.");
                    return;
                }

                m_ControllerQueue.AddFirst(ControllerStates.CheckBuildingQueue); //Decide what to build
            }
            catch (Exception ex)
            {
                Logger.WriteExceptionToLog(ex.Message);
            }
        }

        /// <summary>
        /// Handle the server response for CheckBuildingQueueRequest().
        /// </summary>
        private void CheckBuildingQueueResponse(string p_Response)
        {
            try
            {
                //Only remove vom botbuildingqueue if not in target mode or advanced queue
                if (!m_Town.BuildingTargetEnabled && m_Town.BotBuildingQueue.Count > 0)
                {
                    m_Town.BotBuildingQueue.RemoveAt(0);
                }
                else if (m_Town.BuildingTargetEnabled && Settings.AdvancedQueue && m_Town.BotBuildingQueue.Count > 0)
                {
                    m_Town.BotBuildingQueue.RemoveAt(0);
                }

                UpdateTownDataFromNotification(p_Response);

                var l_Search = "\\\"building_type\\\":\\\"";
                var l_Index = p_Response.IndexOf(l_Search, StringComparison.Ordinal);

                var l_BuildingName = p_Response.Substring(l_Index + l_Search.Length,
                        p_Response.IndexOf("\\\",", l_Index + l_Search.Length, StringComparison.Ordinal) -
                        (l_Index + l_Search.Length));
                var l_BuildingEnum = (Buildings)Enum.Parse(typeof(Buildings), l_BuildingName);

                l_Search = "\\\"to_be_completed_at\\\":";
                l_Index = p_Response.IndexOf(l_Search, l_Index, StringComparison.Ordinal);
                var l_CompletedAt = p_Response.Substring(l_Index + l_Search.Length,
                        p_Response.IndexOf(",", l_Index + l_Search.Length, StringComparison.Ordinal) -
                        (l_Index + l_Search.Length));

                l_Search = "\\\"created_at\\\":";
                l_Index = p_Response.IndexOf(l_Search, l_Index, StringComparison.Ordinal);
                var l_CreatedAt = p_Response.Substring(l_Index + l_Search.Length,
                        p_Response.IndexOf(",", l_Index + l_Search.Length, StringComparison.Ordinal) -
                        (l_Index + l_Search.Length));

                m_Town.IngameBuildingQueue.Add(new BuildingQueueBuilding(l_BuildingEnum, l_CreatedAt, l_CompletedAt));
            }
            catch (Exception ex)
            {
                Logger.WriteExceptionToLog(ex.Message);
            }
        }

        /// <summary>
        /// Response for CheckBuildingQueueTeardownRequest().
        /// </summary>
        private void CheckBuildingQueueTeardownResponse(string p_Response)
        {
            try
            {
                UpdateTownDataFromNotification(p_Response);

                var l_Search = "\\\"building_type\\\":\\\"";
                var l_Index = p_Response.IndexOf(l_Search, StringComparison.Ordinal);

                var l_BuildingName = p_Response.Substring(l_Index + l_Search.Length,
                    p_Response.IndexOf("\\\",", l_Index + l_Search.Length, StringComparison.Ordinal) -
                    (l_Index + l_Search.Length));
                var l_BuildingEnum = (Buildings)Enum.Parse(typeof(Buildings), l_BuildingName);

                l_Search = "\\\"to_be_completed_at\\\":";
                l_Index = p_Response.IndexOf(l_Search, l_Index, StringComparison.Ordinal);
                var l_CompletedAt = p_Response.Substring(l_Index + l_Search.Length,
                    p_Response.IndexOf(",", l_Index + l_Search.Length, StringComparison.Ordinal) -
                    (l_Index + l_Search.Length));

                l_Search = "\\\"created_at\\\":";
                l_Index = p_Response.IndexOf(l_Search, l_Index, StringComparison.Ordinal);
                var l_CreatedAt = p_Response.Substring(l_Index + l_Search.Length,
                    p_Response.IndexOf(",", l_Index + l_Search.Length, StringComparison.Ordinal) -
                    (l_Index + l_Search.Length));

                m_Town.IngameBuildingQueue.Add(new BuildingQueueBuilding(l_BuildingEnum, l_CreatedAt, l_CompletedAt));
            }
            catch (Exception ex)
            {
                Logger.WriteExceptionToLog(ex.Message);
            }
        }

        /// <summary>
        /// Response for OpenBarracksWindowRequest().
        /// No town data in this reponse.
        /// </summary>
        private void OpenBarracksWindowResponse(string p_Response)
        {
            try
            {
                if (p_Response.Contains("barracks.png")) return;

                var l_Search = "UnitOrder.init(";
                var l_Index = p_Response.IndexOf(l_Search, 0, StringComparison.Ordinal);
                l_Search = "\\\"id\\\":\\\"";
                l_Index = p_Response.IndexOf(l_Search, l_Index, StringComparison.Ordinal);
                while (l_Index != -1)
                {
                    var l_Name = (ArmyUnits) Enum.Parse(typeof(ArmyUnits),
                        p_Response.Substring(l_Index + l_Search.Length,
                            p_Response.IndexOf("\\\"", l_Index + l_Search.Length, StringComparison.Ordinal) -
                            (l_Index + l_Search.Length)));
                    var l_Unit = m_Town.ArmyUnits.Single(x => x.Name == l_Name);
                    l_Search = "\\\"count\\\":";
                    l_Index = p_Response.IndexOf(l_Search, l_Index, StringComparison.Ordinal);
                    var l_Count = p_Response.Substring(l_Index + l_Search.Length,
                        p_Response.IndexOf(",", l_Index + l_Search.Length, StringComparison.Ordinal) -
                        (l_Index + l_Search.Length));
                    l_Unit.CurrentAmount = int.Parse(l_Count);
                    l_Search = "\\\"total\\\":";
                    l_Index = p_Response.IndexOf(l_Search, l_Index, StringComparison.Ordinal);
                    var l_Total = p_Response.Substring(l_Index + l_Search.Length,
                        p_Response.IndexOf(",", l_Index + l_Search.Length, StringComparison.Ordinal) -
                        (l_Index + l_Search.Length));
                    l_Unit.TotalAmount = int.Parse(l_Total);
                    l_Search = "\\\"max_build\\\":";
                    l_Index = p_Response.IndexOf(l_Search, l_Index, StringComparison.Ordinal);
                    var l_MaxBuild = p_Response.Substring(l_Index + l_Search.Length,
                        p_Response.IndexOf(",", l_Index + l_Search.Length, StringComparison.Ordinal) -
                        (l_Index + l_Search.Length));
                    l_Unit.MaxBuild = int.Parse(l_MaxBuild);
                    //Search next
                    l_Search = "\\\"id\\\":\\\"";
                    l_Index = p_Response.IndexOf(l_Search, l_Index, StringComparison.Ordinal);
                }

                //Reset land queue
                foreach (var armyUnit in m_Town.ArmyUnits)
                {
                    if (armyUnit.IsFromBarracks)
                        armyUnit.QueueGame = 0;
                }
                m_Town.LandUnitQueueSize = 0;

                l_Search = "{\\\"unit_id\\\":\\\"";
                l_Index = p_Response.IndexOf(l_Search, 0, StringComparison.Ordinal);
                while (l_Index != -1)
                {
                    var l_Name = (ArmyUnits) Enum.Parse(typeof(ArmyUnits),
                        p_Response.Substring(l_Index + l_Search.Length,
                            p_Response.IndexOf("\\\"", l_Index + l_Search.Length, StringComparison.Ordinal) -
                            (l_Index + l_Search.Length)));
                    var l_Unit = m_Town.ArmyUnits.Single(x => x.Name == l_Name);
                    l_Search = "\\\"units_left\\\":";
                    l_Index = p_Response.IndexOf(l_Search, l_Index, StringComparison.Ordinal);
                    var l_UnitsLeft = p_Response.Substring(l_Index + l_Search.Length,
                        p_Response.IndexOf(",", l_Index + l_Search.Length, StringComparison.Ordinal) - (l_Index + l_Search.Length));

                    l_Unit.QueueGame += int.Parse(l_UnitsLeft);
                    m_Town.LandUnitQueueSize += 1;

                    //Search next
                    l_Search = "{\\\"unit_id\\\":\\\"";
                    l_Index = p_Response.IndexOf(l_Search, l_Index, StringComparison.Ordinal);
                }

                m_ControllerQueue.AddFirst(ControllerStates.CheckLandUnitQueue);
            }
            catch (Exception ex)
            {
                Logger.WriteExceptionToLog(ex.Message);
            }
        }

        /// <summary>
        /// Response for CheckLandUnitQueueRequest().
        /// </summary>
        private void CheckLandUnitQueueResponse(string p_Response)
        {
            try
            {
                CallLogEvent(m_Town.TownName + ": Land unit(s) added to ingame queue.");

                UpdateTownDataFromNotification(p_Response);
                AddNotifications(p_Response);    
            }
            catch (Exception ex)
            {
                Logger.WriteExceptionToLog(ex.Message);
            }
        }

        /// <summary>
        /// Response for OpenDocksWindowResponse().
        /// </summary>
        private void OpenDocksWindowResponse(string p_Response)
        {
            try
            {
                if (p_Response.Contains("docks.png")) return;

                var l_Search = "UnitOrder.init(";
                var l_Index = p_Response.IndexOf(l_Search, StringComparison.Ordinal);
                l_Search = "\\\"id\\\":\\\"";
                l_Index = p_Response.IndexOf(l_Search, l_Index, StringComparison.Ordinal);
                while (l_Index != -1)
                {
                    var l_Name = (ArmyUnits)Enum.Parse(typeof(ArmyUnits),
                        p_Response.Substring(l_Index + l_Search.Length,
                            p_Response.IndexOf("\\\"", l_Index + l_Search.Length, StringComparison.Ordinal) -
                            (l_Index + l_Search.Length)));
                    var l_Unit = m_Town.ArmyUnits.Single(x => x.Name == l_Name);
                    l_Search = "\\\"count\\\":";
                    l_Index = p_Response.IndexOf(l_Search, l_Index, StringComparison.Ordinal);
                    var l_Count = p_Response.Substring(l_Index + l_Search.Length,
                        p_Response.IndexOf(",", l_Index + l_Search.Length, StringComparison.Ordinal) -
                        (l_Index + l_Search.Length));
                    l_Unit.CurrentAmount = int.Parse(l_Count);
                    l_Search = "\\\"total\\\":";
                    l_Index = p_Response.IndexOf(l_Search, l_Index, StringComparison.Ordinal);
                    var l_Total = p_Response.Substring(l_Index + l_Search.Length,
                        p_Response.IndexOf(",", l_Index + l_Search.Length, StringComparison.Ordinal) -
                        (l_Index + l_Search.Length));
                    l_Unit.TotalAmount = int.Parse(l_Total);
                    l_Search = "\\\"max_build\\\":";
                    l_Index = p_Response.IndexOf(l_Search, l_Index, StringComparison.Ordinal);
                    var l_MaxBuild = p_Response.Substring(l_Index + l_Search.Length,
                        p_Response.IndexOf(",", l_Index + l_Search.Length, StringComparison.Ordinal) -
                        (l_Index + l_Search.Length));
                    l_Unit.MaxBuild = int.Parse(l_MaxBuild);
                    //Search next
                    l_Search = "\\\"id\\\":\\\"";
                    l_Index = p_Response.IndexOf(l_Search, l_Index, StringComparison.Ordinal);
                }


                //Reset navy queue
                foreach (var armyUnit in m_Town.ArmyUnits)
                {
                    if (!armyUnit.IsFromBarracks)
                        armyUnit.QueueGame = 0;
                }
                m_Town.NavyUnitQueueSize = 0;

                l_Search = "{\\\"unit_id\\\":\\\"";
                l_Index = p_Response.IndexOf(l_Search, StringComparison.Ordinal);
                while (l_Index != -1)
                {
                    var l_Name = (ArmyUnits)Enum.Parse(typeof(ArmyUnits),
                        p_Response.Substring(l_Index + l_Search.Length,
                            p_Response.IndexOf("\\\"", l_Index + l_Search.Length, StringComparison.Ordinal) -
                            (l_Index + l_Search.Length)));
                    var l_Unit = m_Town.ArmyUnits.Single(x => x.Name == l_Name);

                    l_Search = "\\\"units_left\\\":";
                    l_Index = p_Response.IndexOf(l_Search, l_Index, StringComparison.Ordinal);
                    var l_UnitsLeft = p_Response.Substring(l_Index + l_Search.Length, p_Response.IndexOf(",", l_Index + l_Search.Length, StringComparison.Ordinal) - (l_Index + l_Search.Length));

                    l_Unit.QueueGame += int.Parse(l_UnitsLeft);
                    m_Town.NavyUnitQueueSize += 1;

                    //Search next
                    l_Search = "{\\\"unit_id\\\":\\\"";
                    l_Index = p_Response.IndexOf(l_Search, l_Index, StringComparison.Ordinal);
                }

                m_ControllerQueue.AddFirst(ControllerStates.CheckNavyUnitQueue);
            }
            catch (Exception ex)
            {
                Logger.WriteExceptionToLog(ex.Message);
            }
        }

        /// <summary>
        /// Response for CheckNavyUnitQueueRequest().
        /// </summary>
        private void CheckNavyUnitQueueResponse(string p_Response)
        {
            try
            {
                CallLogEvent(m_Town.TownName + ": Navy unit(s) added to ingame queue.");

                UpdateTownDataFromNotification(p_Response);
                AddNotifications(p_Response);
            }
            catch (Exception ex)
            {
                Logger.WriteExceptionToLog(ex.Message);
            }
        }

        #endregion Response

        #endregion Functions

        #region Events

        #region HttpHandler Grepolis Events

        /// <summary>
        /// Handles all the server responses from the Grepolis server requested as POST.
        /// </summary>
        private void HttpHandlerOnUploadValuesCompleted(object sender, UploadValuesCompletedEventArgs e)
        {
            if (e.Error == null && !e.Cancelled)
            {
                var l_Response = Encoding.Default.GetString(e.Result);
                ResponseManager(l_Response);
            }
            else if (e.Cancelled)
            {
                RequestFailedEvent?.Invoke(this, new CustomArgs());
            }
        }

        /// <summary>
        /// Handles all the server responses from the Grepolis server requested as GET.
        /// </summary>
        private void HttpHandlerOnDownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            if (e.Error == null && !e.Cancelled)
            {
                var l_Response = e.Result;
                ResponseManager(l_Response);
            }
            else if (e.Cancelled)
            {
                RequestFailedEvent?.Invoke(this, new CustomArgs());
            }
        }

        #endregion HttpHandler Grepolis Events

        #region Timer Events

        /// <summary>
        /// Reconnect Timer done. Start Reconnect.
        /// </summary>
        private void ReconnectTimerOnTimerDoneEvent(object sender, CustomArgs ca)
        {
            Randomizer.RandomizeReconnectTimer();
            CallLogEvent("Starting reconnect.");
            StartReconnectEvent?.Invoke(this, new CustomArgs());
        }

        /// <summary>
        /// Refresh timer done. Cycle through all towns.
        /// </summary>
        private void RefreshTimerOnTimerDoneEvent(object sender, CustomArgs ca)
        {
            CallLogEvent("Starting Cycle No. " + (m_CycleCount+1) + "!");
            BotCyclingStartedEvent?.Invoke(this, new CustomArgs());

            foreach (var town in Player.Towns)
            {
                //If logged out or bot stopped
                if (!LoggedIn || !m_BotRunning)
                    return;
                TownCycle(town);
            }

            RandomizeTimers();

            m_ControllerQueue.AddLast(ControllerStates.FinishedCycle);

            m_RequestTimer.Start(); //Start Requests
        }

        /// <summary>
        /// Ticks the Requests to the State Manager.
        /// </summary>
        private void RequestTimerOnElapsed(object sender, EventArgs eventArgs)
        {
            m_RequestTimer.Stop();
            StateManager();
        }

        /// <summary>
        /// Only for refreshing timer label.
        /// </summary>
        private void ReconnectTimerOnElapsed(object sender, ElapsedEventArgs e) => ReconnectTimerUpdatedEvent?.Invoke(
            this, new CustomArgs(m_ReconnectTimer.TimeLeft));

        /// <summary>
        /// Only for refreshing timer label.
        /// </summary>
        private void RefreshTimerOnElapsed(object sender, ElapsedEventArgs e) => RefreshTimerUpdatedEvent?.Invoke(this,
            new CustomArgs(m_RefreshTimer.TimeLeft));

        /// <summary>
        /// Only for refreshing timer label.
        /// </summary>
        private void FarmerTimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs) => FarmerTimerUpdatedEvent
            ?.Invoke(this, new CustomArgs(m_FarmerTimer.TimeLeft));

        /// <summary>
        /// Only for refreshing timer label.
        /// </summary>
        private void TradingTimerOnElapsed(object sender, ElapsedEventArgs e) => TradingTimerUpdatedEvent?.Invoke(this,
            new CustomArgs(m_TradingTimer.TimeLeft));

        /// <summary>
        /// Only for refreshing timer label.
        /// </summary>
        private void BuildTimerOnElapsed(object sender, ElapsedEventArgs e) => BuildTimerUpdatedEvent?.Invoke(this,
            new CustomArgs(m_BuildTimer.TimeLeft));

        /// <summary>
        /// Only for refreshing timer label.
        /// </summary>
        private void UnitTimerOnElapsed(object sender, ElapsedEventArgs e) => UnitTimerUpdatedEvent?.Invoke(this,
            new CustomArgs(m_UnitTimer.TimeLeft));

        #endregion Timer Events

        #region Custom Events

        /// <summary>
        /// Handles the RequestFailed Event.
        /// </summary>
        private void OnRequestFailedEvent(object sender, CustomArgs ca)
        {
            RetryManager();
        }

        /// <summary>
        /// Handles the RequestSuccessful Event.
        /// </summary>
        private void OnRequestSuccessfulEvent(object sender, CustomArgs ca)
        {
            m_ControllerQueue.RemoveFirst(); //Remove the successful action from the queue.
            m_RetryCount = 0; //Reset Retry Count
            if (LoggedIn) //After login randomize each request time.
                m_RequestTimer.Interval = Randomizer.RandomizeRequestTimer();
        }

        #endregion Custom Events

        #region Call Events

        /// <summary>
        /// Call the Log Event.
        /// </summary>
        private void CallLogEvent(string p_Message)
        {
            LogEvent?.Invoke(this, new CustomArgs(p_Message));
        }

        /// <summary>
        /// Call the BotStarted Event.
        /// </summary>
        private void CallBotStartedEvent()
        {
            m_BotRunning = true;
            BotStartedEvent?.Invoke(this, new CustomArgs());
        }

        /// <summary>
        /// Call the BotStopped Event.
        /// </summary>
        private void CallBotStoppedEvent()
        {
            m_BotRunning = false;
            BotStoppedEvent?.Invoke(this, new CustomArgs());
        }

        #endregion Call Events

        #endregion Events
    }
}