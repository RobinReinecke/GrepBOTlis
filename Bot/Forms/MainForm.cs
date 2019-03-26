using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Bot.Custom;
using Bot.Enums;
using Bot.Helpers;
using mshtml;

namespace Bot.Forms
{
    public partial class MainForm : Form
    {
        #region dll Import

        [DllImport("KERNEL32.DLL", EntryPoint = "SetProcessWorkingSetSize", SetLastError = true, CallingConvention = CallingConvention.StdCall)]
        internal static extern bool SetProcessWorkingSetSize(IntPtr pProcess, int dwMinimumWorkingSetSize, int dwMaximumWorkingSetSize);

        [DllImport("KERNEL32.DLL", EntryPoint = "GetCurrentProcess", SetLastError = true, CallingConvention = CallingConvention.StdCall)]
        internal static extern IntPtr GetCurrentProcess();

        [DllImport("urlmon.dll", CharSet = CharSet.Ansi)]
        private static extern int UrlMkSetSessionOption(int dwOption, string pBuffer, int dwBufferLength, int dwReserved);

        private const int URLMON_OPTION_USERAGENT = 0x10000001;

        #endregion dll Import

        #region Attributes

        #region Timer

        private readonly System.Windows.Forms.Timer m_TimerLoginPart2 = new System.Windows.Forms.Timer();

        #endregion Timer

        /// <summary>
        /// Controller instance.
        /// </summary>
        private readonly Controller m_Controller = Controller.Instance;

        /// <summary>
        /// Selected town for all tabs.
        /// </summary>
        private string m_SelectedTownID = "-1";

        /// <summary>
        /// Size of Bot Queue.
        /// </summary>
        private int m_BotQueueSize = 0;

        /// <summary>
        /// Size of Ingame GUI Queue.
        /// </summary>
        private int m_IngameQueueSize = 0;

        /// <summary>
        /// Bot Title
        /// </summary>
        public string AssemblyTitle
        {
            get
            {
                var attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
                if (attributes.Length > 0)
                {
                    var titleAttribute = (AssemblyTitleAttribute)attributes[0];
                    if (titleAttribute.Title != "")
                    {
                        return titleAttribute.Title;
                    }
                }
                return System.IO.Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().CodeBase);
            }
        }

        /// <summary>
        /// Bot Version
        /// </summary>
        public string AssemblyVersion => Assembly.GetExecutingAssembly().GetName().Version.ToString();

        #region Callbacks

        /// <summary>
        /// CallBack for Controller LogEvent.
        /// </summary>
        private delegate void LogCallBack(string message);

        /// <summary>
        /// CallBack for Controller LoginSuccessfulEvent.
        /// </summary>
        private delegate void LoginSuccessfulCallBack();

        /// <summary>
        /// Callback for Controller RefreshTimerUpdatedEvent.
        /// </summary>
        private delegate void RefreshTimerUpdatedCallBack(string time);

        /// <summary>
        /// Callback for Controller FarmerTimerUpdatedEvent.
        /// </summary>
        private delegate void FarmerTimerUpdatedCallBack(string time);

        /// <summary>
        /// Callback for Controller TradingTimerUpdatedEvent.
        /// </summary>
        private delegate void TradingTimerUpdatedCallBack(string time);

        /// <summary>
        /// Callback for Controller StartReconnectEvent.
        /// </summary>
        private delegate void StartReconnectCallBack();

        /// <summary>
        /// Callback for Controller ReconnectTimerUpdatedEvent.
        /// </summary>
        private delegate void ReconnectTimerUpdatedCallBack(string time);

        /// <summary>
        /// Callback for Controller LoggedOutEvent.
        /// </summary>
        private delegate void LoggedOutCallBack();

        /// <summary>
        /// Callback for Controller BotStartedEvent.
        /// </summary>
        private delegate void BotStartedCallBack();

        /// <summary>
        /// Callback for Controller BotStoppedEvent.
        /// </summary>
        private delegate void BotStoppedCallBack();

        /// <summary>
        /// Callback for Controller BotCyclingStartedEvent.
        /// </summary>
        private delegate void BotCyclingStartedCallBack();

        /// <summary>
        /// Callback for Controller BotCyclingStoppedEvent.
        /// </summary>
        private delegate void BotCyclingStoppedCallBack();

        /// <summary>
        /// Callback for Controller BuildTimerUpdatedEvent.
        /// </summary>
        private delegate void BuildTimerUpdatedCallBack(string time);

        /// <summary>
        /// Callback for Controller UnitTimerUpdatedEvent.
        /// </summary>
        private delegate void UnitTimerUpdatedCallBack(string time);

        #endregion Callbacks

        #endregion Attributes

        #region Constructor

        public MainForm()
        {
            InitializeComponent();
            InitForm();
            InitMisc();
            InitFiles();
            InitBrowser();
            InitComplete();
            InitEvents();
            InitTimers();
        }

        #endregion Constructor

        #region Functions

        #region Login


        
        /// <summary>
        /// First part of login process.
        /// </summary>
        private void LoginToGrepolisPart1()
        {
            try
            {
                m_Controller.State = ControllerStates.LoginToGrepolisPart1;
                WriteToLog("Login to Grepolis.");
                CookieHelper.ClearCookie();
                BotWebBrowser.Navigate("https://" + Settings.GrepolisMainServer);
            }
            catch (Exception ex)
            {
                Logger.WriteExceptionToLog(ex.Message);
            }
        }

        /*
        /// <summary>
        /// Second part of login process.
        /// </summary>
        private void LoginToGrepolisPart2()
        {
            try
            {
                m_Controller.State = ControllerStates.LoginToGrepolisPart2;

                var head = BotWebBrowser.Document.GetElementsByTagName("head")[0];
                var scriptEl = BotWebBrowser.Document.CreateElement("script");
                var element = (IHTMLScriptElement)scriptEl.DomElement;
                var l_Password = Settings.GrepolisPassword.Replace("\\", "\\\\"); //For passwords with backslash
                element.text =
                    "function BotLogin() { angular.element(document.getElementById('login')).scope().formdata = {\"login[password]\":\"" +
                    l_Password + "\",\"login[remember_me]\": true,\"login[userid]\":\"" + Settings.GrepolisUsername +
                    "\"}; angular.element(document.getElementById('login')).scope().submitForm({preventDefault: function() {}}); }";
                head.AppendChild(scriptEl);
                BotWebBrowser.Document.InvokeScript("BotLogin");

                
                 * Old Login Process
                HtmlElement l_Name = BotWebBrowser.Document.GetElementById("name");
                l_Name.InnerText = Settings.GrepolisUsername;
                HtmlElement l_Password = BotWebBrowser.Document.GetElementById("password");
                l_Password.InnerText = Settings.GrepolisPassword;

                var l_Button = BotWebBrowser.Document.GetElementById("login_button");
                l_Button.InvokeMember("click");
                
            }
            catch (Exception ex)
            {
                Logger.WriteExceptionToLog(ex.Message);
            }
        }
        */

        /// <summary>
        /// Third part of login process.
        /// </summary>
        private void LoginToGrepolisPart3()
        {
            try
            {
                m_Controller.State = ControllerStates.LoginToGrepolisPart3;

                var l_Url = "https://" + Settings.GrepolisWorld.Substring(0, 2) + "0.grepolis.com/start?action=login_to_game_world";
                var l_Uri = new Uri(l_Url);
                var l_Encoding = System.Text.Encoding.UTF8;
                var l_PostData = "world=" + Settings.GrepolisWorld + "&facebook_session=&facebook_login=&portal_sid=&name=" + Settings.GrepolisUsername + "&password=";
                var l_PostDataBytes = l_Encoding.GetBytes(l_PostData);
                BotWebBrowser.Navigate(l_Uri, "", l_PostDataBytes, "Content-Type: application/x-www-form-urlencoded");
            }
            catch (Exception ex)
            {
                Logger.WriteExceptionToLog(ex.Message);
            }
        }

        #endregion Login

        #region Inits

        /// <summary>
        /// Init the form.
        /// </summary>
        private void InitForm()
        {
            Text = $@"{AssemblyTitle} {AssemblyVersion}";
        }

        /// <summary>
        /// Fix for "The remote server returned an error: (417) Expectation Failed."
        /// </summary>
        private void InitMisc()
        {
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        }

        /// <summary>
        /// Initialize Eventhandlers.
        /// </summary>
        private void InitEvents()
        {
            m_TimerLoginPart2.Tick += TimerLoginPart2OnTick;

            m_Controller.LogEvent += ControllerOnLogEvent;
            m_Controller.LoginSuccessfulEvent += ControllerOnLoginSuccessfulEvent;
            m_Controller.RefreshTimerUpdatedEvent += ControllerOnRefreshTimerUpdatedEvent;
            m_Controller.FarmerTimerUpdatedEvent += ControllerOnFarmerTimerUpdatedEvent;
            m_Controller.TradingTimerUpdatedEvent += ControllerOnTradingTimerUpdatedEvent;
            m_Controller.StartReconnectEvent += ControllerOnStartReconnectEvent;
            m_Controller.ReconnectTimerUpdatedEvent += ControllerOnReconnectTimerUpdatedEvent;
            m_Controller.LoggedOutEvent += ControllerOnLoggedOutEvent;
            m_Controller.BotStartedEvent += ControllerOnBotStartedEvent;
            m_Controller.BotStoppedEvent += ControllerOnBotStoppedEvent;
            m_Controller.BotCyclingStartedEvent += ControllerOnBotCyclingStartedEvent;
            m_Controller.BotCyclingStoppedEvent += ControllerOnBotCyclingStoppedEvent;
            m_Controller.BuildTimerUpdatedEvent += ControllerOnBuildTimerUpdatedEvent;
            m_Controller.UnitTimerUpdatedEvent += ControllerOnUnitTimerUpdatedEvent;
        }

        /// <summary>
        /// Initalize Timers.
        /// </summary>
        private void InitTimers()
        {
            m_TimerLoginPart2.Interval = 1234;
        }

        /// <summary>
        /// Initialize Files.
        /// Load settings.
        /// </summary>
        private void InitFiles()
        {
            try
            {
                IOHelper.CheckFiles();

                //Grepolis
                BotTextBox_Username.Text = Settings.GrepolisUsername;
                BotTextBox_MainServer.Text = Settings.GrepolisMainServer;
                BotTextBox_World.Text = Settings.GrepolisWorld;
                BotCheckBox_HeroWorld.Checked = Settings.HeroWorld;  

                //Delays
                BotNumericUpDown_MaxDelayRequests.Text = Settings.MaxRequestDelay.ToString();
                BotNumericUpDown_MinDelayRequests.Text = Settings.MinRequestDelay.ToString();
                BotNumericUpDown_MaxTimerRefresh.Text = Settings.MaxRefreshDelay.ToString();
                BotNumericUpDown_MinTimerRefresh.Text = Settings.MinRefreshDelay.ToString();
                BotNumericUpDown_MinTimerFarmer.Text = Settings.MinFarmerDelay.ToString();
                BotNumericUpDown_MaxTimerFarmer.Text = Settings.MaxFarmerDelay.ToString();
                BotNumericUpDown_MinTimerTrading.Text = Settings.MinTradingDelay.ToString();
                BotNumericUpDown_MaxTimerTrading.Text = Settings.MaxTradingDelay.ToString();
                BotNumericUpDown_MinTimerBuild.Text = Settings.MinBuildDelay.ToString();
                BotNumericUpDown_MaxTimerBuild.Text = Settings.MaxBuildDelay.ToString();
                BotNumericUpDown_MinTimerUnit.Text = Settings.MinUnitDelay.ToString();
                BotNumericUpDown_MaxTimerUnit.Text = Settings.MaxUnitDelay.ToString();

                //Masters
                BotCheckBox_MasterFarmingEnabled.Checked = Settings.MasterFarmingEnabled;
                BotCheckBox_MasterTradingEnabled.Checked = Settings.MasterTradingEnabled;
                BotCheckBox_MasterBuildingEnabled.Checked = Settings.MasterBuildingEnabled;
                BotCheckBox_MasterUnitEnabled.Checked = Settings.MasterUnitEnabled;

                //Build
                BotCheckBox_SettingsAdvQueue.Checked = Settings.AdvancedQueue;
                BotNumericUpDown_FarmBelow.Text = Settings.BuildFarmBelow.ToString();

                //Reconnect
                BotNumericUpDown_MinDelayReconnect.Text = Settings.MinReconnectDelay.ToString();
                BotNumericUpDown_MaxDelayReconnect.Text = Settings.MaxReconnectDelay.ToString();
                BotNumericUpDown_MaxRetryCount.Text = Settings.MaxRetryCount.ToString();

                //UnitQueue
                BotNumericUpDown_QueueLimit.Text = Settings.QueueLimit.ToString();
                BotNumericUpDown_MinUnitQueuePop.Text = Settings.MinUnitQueuePop.ToString();
                BotNumericUpDown_SkipUnitQueuePop.Text = Settings.SkipUnitQueuePop.ToString();

                //Others
                BotCheckBox_AutoStart.Checked = Settings.AutoStart;
                BotTextBox_AdvUserAgent.Text = Settings.AdvUserAgent;
                BotNumericUpDown_GuiLogSize.Text = Settings.GUILogSize.ToString();

            }
            catch (Exception ex)
            {
                Logger.WriteExceptionToLog(ex.Message);
            }
        }

        /// <summary>
        /// Initialize WebBrowser
        /// </summary>
        private void InitBrowser()
        {
            try
            {
                //Set newest User Agent
                UrlMkSetSessionOption(URLMON_OPTION_USERAGENT, Settings.AdvUserAgent, Settings.AdvUserAgent.Length, 0);

                var l_Script =
                    @"<script type='text/javascript'>function GetUserAgent(){document.write(navigator.userAgent)}</script>";
                BotWebBrowser.Document.Write(l_Script);
                BotWebBrowser.Document.InvokeScript("GetUserAgent");

                Settings.AdvUserAgent = BotWebBrowser.DocumentText.Substring(l_Script.Length); //UserAgent written after script
                BotWebBrowser.ScriptErrorsSuppressed = true; //Prevent pop-ups
            }
            catch (Exception ex)
            {
                Logger.WriteExceptionToLog(ex.Message);
            }
        }

        /// <summary>
        /// Finish the Init.
        /// </summary>
        private void InitComplete()
        {
            try
            {
                WriteToLog("Started " + AssemblyTitle + " V." + AssemblyVersion);
            }
            catch (Exception ex)
            {
                Logger.WriteExceptionToLog(ex.Message);
            }
        }

        #endregion Inits

        #region GUI Update

        /// <summary>
        /// Update the Farming GUI.
        /// </summary>
        private void UpdateFarmingGUI()
        {
            try
            {
                //Get Town
                var l_Town = m_Controller.Player.Towns.Single(x => x.TownID == m_SelectedTownID);
                //Reset if no farmers available
                if (l_Town.Farmers.Count < 1)
                {
                    //Village Names
                    BotLabel_VillageName1.Text = @"No name";
                    BotLabel_VillageName2.Text = @"No name";
                    BotLabel_VillageName3.Text = @"No name";
                    BotLabel_VillageName4.Text = @"No name";
                    BotLabel_VillageName5.Text = @"No name";
                    BotLabel_VillageName6.Text = @"No name";
                    //Village limit
                    BotLabel_VillageLimit1.Text = @"0 / 0";
                    BotLabel_VillageLimit2.Text = @"0 / 0";
                    BotLabel_VillageLimit3.Text = @"0 / 0";
                    BotLabel_VillageLimit4.Text = @"0 / 0";
                    BotLabel_VillageLimit5.Text = @"0 / 0";
                    BotLabel_VillageLimit6.Text = @"0 / 0";
                    //Village Timer
                    BotLabel_TimerVillage1.Text = @"Not available";
                    BotLabel_TimerVillage2.Text = @"Not available";
                    BotLabel_TimerVillage3.Text = @"Not available";
                    BotLabel_TimerVillage4.Text = @"Not available";
                    BotLabel_TimerVillage5.Text = @"Not available";
                    BotLabel_TimerVillage6.Text = @"Not available";
                    //Village enabled
                    BotCheckBox_Village1.Checked = false;
                    BotCheckBox_Village2.Checked = false;
                    BotCheckBox_Village3.Checked = false;
                    BotCheckBox_Village4.Checked = false;
                    BotCheckBox_Village5.Checked = false;
                    BotCheckBox_Village6.Checked = false;
                    BotCheckBox_Village1.Enabled = false;
                    BotCheckBox_Village2.Enabled = false;
                    BotCheckBox_Village3.Enabled = false;
                    BotCheckBox_Village4.Enabled = false;
                    BotCheckBox_Village5.Enabled = false;
                    BotCheckBox_Village6.Enabled = false;

                    //Enabled check box
                    BotCheckBox_FarmersLootEnabled.Enabled = false;

                    //Loot interval
                    BotDomainUpDown_LootInterval.Enabled = false;

                    //Disable save button
                    BotButton_SaveFarmers.Enabled = false;
                }
                else
                {
                    //Village Names
                    BotLabel_VillageName1.Text = l_Town.Farmers[0].Name;
                    BotLabel_VillageName2.Text = l_Town.Farmers[1].Name;
                    BotLabel_VillageName3.Text = l_Town.Farmers[2].Name;
                    BotLabel_VillageName4.Text = l_Town.Farmers[3].Name;
                    BotLabel_VillageName5.Text = l_Town.Farmers[4].Name;
                    BotLabel_VillageName6.Text = l_Town.Farmers[5].Name;
                    //Village limit
                    BotLabel_VillageLimit1.Text = l_Town.Farmers[0].LootedResources + @" / " + l_Town.Farmers[0].Limit;
                    BotLabel_VillageLimit2.Text = l_Town.Farmers[1].LootedResources + @" / " + l_Town.Farmers[1].Limit;
                    BotLabel_VillageLimit3.Text = l_Town.Farmers[2].LootedResources + @" / " + l_Town.Farmers[2].Limit;
                    BotLabel_VillageLimit4.Text = l_Town.Farmers[3].LootedResources + @" / " + l_Town.Farmers[3].Limit;
                    BotLabel_VillageLimit5.Text = l_Town.Farmers[4].LootedResources + @" / " + l_Town.Farmers[4].Limit;
                    BotLabel_VillageLimit6.Text = l_Town.Farmers[5].LootedResources + @" / " + l_Town.Farmers[5].Limit;
                    //Village Timer
                    BotLabel_TimerVillage1.Text = Parser.UnixToHumanTime(l_Town.Farmers[0].LootTimer).ToString("G");
                    BotLabel_TimerVillage2.Text = Parser.UnixToHumanTime(l_Town.Farmers[1].LootTimer).ToString("G");
                    BotLabel_TimerVillage3.Text = Parser.UnixToHumanTime(l_Town.Farmers[2].LootTimer).ToString("G");
                    BotLabel_TimerVillage4.Text = Parser.UnixToHumanTime(l_Town.Farmers[3].LootTimer).ToString("G");
                    BotLabel_TimerVillage5.Text = Parser.UnixToHumanTime(l_Town.Farmers[4].LootTimer).ToString("G");
                    BotLabel_TimerVillage6.Text = Parser.UnixToHumanTime(l_Town.Farmers[5].LootTimer).ToString("G");
                    //Village enabled
                    BotCheckBox_Village1.Checked = l_Town.Farmers[0].Enabled;
                    BotCheckBox_Village2.Checked = l_Town.Farmers[1].Enabled;
                    BotCheckBox_Village3.Checked = l_Town.Farmers[2].Enabled;
                    BotCheckBox_Village4.Checked = l_Town.Farmers[3].Enabled;
                    BotCheckBox_Village5.Checked = l_Town.Farmers[4].Enabled;
                    BotCheckBox_Village6.Checked = l_Town.Farmers[5].Enabled;
                    BotCheckBox_Village1.Enabled = true;
                    BotCheckBox_Village2.Enabled = true;
                    BotCheckBox_Village3.Enabled = true;
                    BotCheckBox_Village4.Enabled = true;
                    BotCheckBox_Village5.Enabled = true;
                    BotCheckBox_Village6.Enabled = true;

                    //enable check box
                    BotCheckBox_FarmersLootEnabled.Enabled = true;

                    //Loot interval
                    BotDomainUpDown_LootInterval.Enabled = true;

                    //save button
                    BotButton_SaveFarmers.Enabled = true;
                }
            }
            catch (Exception ex)
            {
                Logger.WriteExceptionToLog(ex.Message);
            }
        }

        /// <summary>
        /// Update the Trading GUI.
        /// </summary>
        private void UpdateTradingGUI()
        {
            try
            {
                //Get Town
                var l_Town = m_Controller.Player.Towns.Single(x => x.TownID == m_SelectedTownID);

                BotCheckBox_TradingEnabled.Enabled = true;
                BotComboBox_TradeMode.Enabled = true;
                BotNumericUpDown_TradeRemainingWood.Enabled = true;
                BotNumericUpDown_TradeRemainingStone.Enabled = true;
                BotNumericUpDown_TradeRemainingIron.Enabled = true;
                BotNumericUpDown_TradePercentageWarehouse.Enabled = true;
                BotNumericUpDown_TradeMinSendAmount.Enabled = true;
                BotNumericUpDown_TradeMaxDistance.Enabled = true;

                BotButton_SaveTrade.Enabled = true;

                BotCheckBox_TradingEnabled.Checked = l_Town.TradeEnabled;
                switch (l_Town.TradeMode)
                {
                    case TradingModes.Receive:
                        BotComboBox_TradeMode.SelectedIndex = 1;
                        break;

                    case TradingModes.Spycave:
                        BotComboBox_TradeMode.SelectedIndex = 2;
                        break;

                    case TradingModes.Send:
                        BotComboBox_TradeMode.SelectedIndex = 0;
                        break;
                }
                BotNumericUpDown_TradeRemainingWood.Text = l_Town.TradeWoodRemaining.ToString();
                BotNumericUpDown_TradeRemainingStone.Text = l_Town.TradeStoneRemaining.ToString();
                BotNumericUpDown_TradeRemainingIron.Text = l_Town.TradeIronRemaining.ToString();
                BotNumericUpDown_TradePercentageWarehouse.Text = l_Town.TradePercentageWarehouse.ToString();
                BotNumericUpDown_TradeMinSendAmount.Text = l_Town.TradeMinSendAmount.ToString();
                BotNumericUpDown_TradeMaxDistance.Text = l_Town.TradeMaxDistance.ToString();
            }
            catch (Exception ex)
            {
                Logger.WriteExceptionToLog(ex.Message);
            }
        }

        /// <summary>
        /// Update the Culture GUI.
        /// </summary>
        private void UpdateCultureGUI()
        {
            try
            {
                //Get Town
                var l_Town = m_Controller.Player.Towns.Single(x => x.TownID == m_SelectedTownID);

                BotCheckBox_PartyEnabled.Enabled = true;
                BotCheckBox_GamesEnabled.Enabled = true;
                BotCheckBox_TriumphEnabled.Enabled = true;
                BotCheckBox_TheaterEnabled.Enabled = true;

                BotCheckBox_CultureEnabled.Enabled = true;
                BotButton_SaveCulture.Enabled = true;

                BotCheckBox_PartyEnabled.Checked = l_Town.CulturalEvents[0].Enabled;
                BotCheckBox_GamesEnabled.Checked = l_Town.CulturalEvents[1].Enabled;
                BotCheckBox_TriumphEnabled.Checked = l_Town.CulturalEvents[2].Enabled;
                BotCheckBox_TheaterEnabled.Checked = l_Town.CulturalEvents[3].Enabled;

                BotCheckBox_CultureEnabled.Checked = l_Town.CulturalFestivalsEnabled;

                BotLabel_CulturalLevel.Text = @"Culture Points: " + m_Controller.Player.CulturalPointsCurrent + @" / " +
                                              m_Controller.Player.CulturalPointsMax + @" (Level " +
                                              m_Controller.Player.CultureLevel + @")";
                BotLabel_CulturalCities.Text = @"Cities: " + m_Controller.Player.Towns.Count + @" / " +
                                               m_Controller.Player.CultureLevel;

                BotProgressBar_CP.Maximum = m_Controller.Player.CulturalPointsMax;
                BotProgressBar_CP.Value = m_Controller.Player.CulturalPointsCurrent;
            }
            catch (Exception ex)
            {
                Logger.WriteExceptionToLog(ex.Message);
            }
        }

        /// <summary>
        /// Update the Queue GUI.
        /// </summary>
        private void UpdateQueueGUI()
        {
            try
            {
                //Get Town
                var l_Town = m_Controller.Player.Towns.Single(x => x.TownID == m_SelectedTownID);

                //Set hero world level
                if (Settings.HeroWorld)
                {
                    BotNumericUpDown_Storage.Maximum = 35;
                    BotNumericUpDown_Farm.Maximum = 45;
                    BotNumericUpDown_Academy.Maximum = 36;
                    BotNumericUpDown_Temple.Maximum = 30;
                }
                else
                {
                    BotNumericUpDown_Storage.Maximum = 30;
                    BotNumericUpDown_Farm.Maximum = 40;
                    BotNumericUpDown_Academy.Maximum = 30;
                    BotNumericUpDown_Temple.Maximum = 25;
                }

                BotCheckBox_BuildingQueueEnabled.Checked = l_Town.BuildingQueueEnabled;
                BotCheckBox_BuildingDowngrade.Checked = l_Town.DowngradeEnabled;
                BotCheckBox_BuildingQueueTarget.Checked = l_Town.BuildingTargetEnabled;
                BotCheckBox_UnitQueueEnabled.Checked = l_Town.UnitQueueEnabled;
                BotNumericUpDown_TownPriority.Text = l_Town.Priority.ToString();
                BotCheckBox_BuildingQueueEnabled.Enabled = true;
                BotCheckBox_BuildingDowngrade.Enabled = true;
                BotCheckBox_BuildingQueueTarget.Enabled = true;
                BotCheckBox_UnitQueueEnabled.Enabled = true;
                BotNumericUpDown_TownPriority.Enabled = true;
                BotButton_SaveQueue.Enabled = true;

                for (var i = 0; i < l_Town.Buildings.Count; i++)
                {
                    //Set building level
                    if (i < BotFlowLayoutPanel_Buildings.Controls.Count / 2)
                        ((Label)BotFlowLayoutPanel_Buildings.Controls[i * 2]).Text = l_Town.Buildings[i].Level.ToString();
                    //Set target level
                    if (i < BotFlowLayoutPanel_Target.Controls.Count)
                        ((NumericUpDown)BotFlowLayoutPanel_Target.Controls[i]).Value = l_Town.Buildings[i].TargetLevel;
                    //Set level color
                    ((Label)BotFlowLayoutPanel_Buildings.Controls[i * 2]).ForeColor =
                        ((NumericUpDown)BotFlowLayoutPanel_Target.Controls[i]).Value.ToString(
                            CultureInfo.InvariantCulture) == ((Label)BotFlowLayoutPanel_Buildings.Controls[i * 2]).Text
                            ? Color.GreenYellow
                            : Color.White;
                }

                //clear queues
                foreach (Label l_Label in BotFlowLayoutPanel_QueueBot.Controls)
                {
                    l_Label.Image = Properties.Resources.emptyx20;
                    l_Label.Tag = "empty";
                }
                m_BotQueueSize = 0;
                foreach (Label l_Label in BotFlowLayoutPanel_QueueGame.Controls)
                {
                    l_Label.Image = Properties.Resources.emptyx20;
                    l_Label.Tag = "empty";
                }
                m_IngameQueueSize = 0;

                //Fill bot queue
                foreach (var l_Building in l_Town.BotBuildingQueue)
                {
                    AddBuildingToBotQueue(l_Building);
                }
                //Fill ingame queue
                foreach (var l_Building in l_Town.IngameBuildingQueue)
                {
                    AddBuildingToIngameQueue(l_Building.DevName);
                }


                //Fill army units
                for (var i = 0; i < l_Town.ArmyUnits.Count; i++)
                {
                    ((Label) BotFlowLayoutPanel_UnitsImg.Controls[i]).Text = l_Town.ArmyUnits[i].QueueGame.ToString();
                    ((Label) BotFlowLayoutPanel_UnitsTotal.Controls[i]).Text =
                        l_Town.ArmyUnits[i].TotalAmount.ToString();
                    ((NumericUpDown) BotFlowLayoutPanel_UnitsTarget.Controls[i]).Text = l_Town.ArmyUnits[i].QueueBot.ToString();

                    ((Label) BotFlowLayoutPanel_UnitsTotal.Controls[i]).ForeColor =
                        ((NumericUpDown) BotFlowLayoutPanel_UnitsTarget.Controls[i]).Value.ToString(CultureInfo
                            .InvariantCulture) == ((Label) BotFlowLayoutPanel_UnitsTotal.Controls[i]).Text
                            ? Color.Red
                            : Color.Black;
                }
            }
            catch (Exception ex)
            {
                Logger.WriteExceptionToLog(ex.Message);
            }
        }

        #endregion GUI Update

        #endregion Functions

        #region Events

        #region GUI Events - Overview tab

        /// <summary>
        /// Start login.
        /// </summary>
        private void BotBtn_Login_Click(object sender, EventArgs e)
        {
            BotBtn_Login.Enabled = false;
            LoginToGrepolisPart1();
        }

        /// <summary>
        /// Start bot.
        /// </summary>
        private void BotBtn_Start_Click(object sender, EventArgs e)
        {
            try
            {
                switch (BotBtn_Start.Text)
                {
                    case @"Start":
                        BotBtn_Start.Text = @"Pause";
                        m_Controller.StartBot();
                        break;

                    case @"Pause":
                        BotBtn_Start.Text = @"Resume";
                        m_Controller.PauseBot();
                        break;

                    case @"Resume":
                        BotBtn_Start.Text = @"Pause";
                        m_Controller.ResumeBot();
                        break;
                }
            }
            catch (Exception ex)
            {
                Logger.WriteExceptionToLog(ex.Message);
            }
        }

        #endregion GUI Events - Overview tab

        #region GUI Events - Browser tab

        /// <summary>
        /// Fires when Browser finishs loading.
        /// </summary>
        private void BotWebBrowser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            try
            {
                //Fix for .NET browser memory leak
                var l_Process = GetCurrentProcess();
                SetProcessWorkingSetSize(l_Process, -1, -1);

                //Skip rest if logged in
                if (m_Controller.LoggedIn)
                    return;

                //If session exists browser starts in Part 2.
                if (BotWebBrowser.Url.OriginalString.Contains("0") && m_Controller.State == ControllerStates.LoginToGrepolisPart1)
                    m_Controller.State = ControllerStates.LoginToGrepolisPart2;
                
                if (m_Controller.State == ControllerStates.LoginToGrepolisPart2)
                {
                    m_TimerLoginPart2.Start();
                }
                else if (m_Controller.State == ControllerStates.LoginToGrepolisPart3)
                {
                    var l_Response = BotWebBrowser.DocumentText;
                    if (l_Response.Contains("csrfToken\":\""))
                    {
                        m_Controller.InitCookies(BotWebBrowser);

                        m_Controller.LoggedIn = true;
                        m_Controller.LoginVerified = true;

                        //Get csrfToken
                        var l_Search = "csrfToken\":\"";
                        var l_Index = l_Response.IndexOf(l_Search, 0, StringComparison.Ordinal);
                        m_Controller.H = l_Response.Substring(l_Index + l_Search.Length, l_Response.IndexOf("\"", l_Index + l_Search.Length, StringComparison.Ordinal) - (l_Index + l_Search.Length));
                        //Game speed
                        l_Search = "game_speed\":";
                        l_Index = l_Response.IndexOf(l_Search, 0, StringComparison.Ordinal);
                        Settings.GameSpeed = l_Response.Substring(l_Index + l_Search.Length, l_Response.IndexOf(",", l_Index + l_Search.Length, StringComparison.Ordinal) - (l_Index + l_Search.Length));
                        //Get locale time offset
                        l_Search = "locale_gmt_offset\":";
                        l_Index = l_Response.IndexOf(l_Search, l_Index, StringComparison.Ordinal);
                        Settings.LocaleTimeOffset = l_Response.Substring(l_Index + l_Search.Length, l_Response.IndexOf(",", l_Index + l_Search.Length, StringComparison.Ordinal) - (l_Index + l_Search.Length));
                        //Get player id
                        l_Search = "player_id\":";
                        l_Index = l_Response.IndexOf(l_Search, l_Index, StringComparison.Ordinal);
                        m_Controller.Player.PlayerID = l_Response.Substring(l_Index + l_Search.Length, l_Response.IndexOf(",", l_Index + l_Search.Length, StringComparison.Ordinal) - (l_Index + l_Search.Length));
                        //Get server time offset
                        l_Search = "server_gmt_offset\":";
                        l_Index = l_Response.IndexOf(l_Search, l_Index, StringComparison.Ordinal);
                        Settings.ServerTimeOffset = l_Response.Substring(l_Index + l_Search.Length, l_Response.IndexOf(",", l_Index + l_Search.Length, StringComparison.Ordinal) - (l_Index + l_Search.Length));
                        //Get server time
                        l_Search = "server_time\":";
                        l_Index = l_Response.IndexOf(l_Search, l_Index, StringComparison.Ordinal);
                        m_Controller.ServerTime = l_Response.Substring(l_Index + l_Search.Length, l_Response.IndexOf(",", l_Index + l_Search.Length, StringComparison.Ordinal) - (l_Index + l_Search.Length));
                        //Get town id
                        l_Search = "townId\":";
                        l_Index = l_Response.IndexOf(l_Search, l_Index, StringComparison.Ordinal);
                        m_Controller.Player.DefaultTownID = l_Response.Substring(l_Index + l_Search.Length, l_Response.IndexOf(",", l_Index + l_Search.Length, StringComparison.Ordinal) - (l_Index + l_Search.Length));
                        //Premium status
                        l_Search = "\"premium_features\":{";
                        l_Index = l_Response.IndexOf(l_Search, 0, StringComparison.Ordinal);
                        if (l_Index != -1)
                        {
                            var l_PremiumStatus = l_Response.Substring(l_Index, l_Response.IndexOf("}", l_Index, StringComparison.Ordinal) - l_Index);
                            l_Search = "\"commander\":";
                            l_Index = l_PremiumStatus.IndexOf(l_Search, 0, StringComparison.Ordinal);
                            var l_PremiumTime = l_PremiumStatus.Substring(l_Index + l_Search.Length, l_PremiumStatus.IndexOf(",", l_Index + l_Search.Length, StringComparison.Ordinal) - (l_Index + l_Search.Length));
                            //m_Controller.Player.CommanderActive = l_PremiumTime.Equals("null") ? "0" : l_PremiumTime;
                            m_Controller.Player.CommanderActive = l_PremiumTime.Equals("null") ? "0" : l_PremiumTime;

                            l_Search = "\"curator\":";
                            l_Index = l_PremiumStatus.IndexOf(l_Search, 0, StringComparison.Ordinal);
                            l_PremiumTime = l_PremiumStatus.Substring(l_Index + l_Search.Length, l_PremiumStatus.IndexOf(",", l_Index + l_Search.Length, StringComparison.Ordinal) - (l_Index + l_Search.Length));
                            m_Controller.Player.CuratorActive = l_PremiumTime.Equals("null") ? "0" : l_PremiumTime;

                            l_Search = "\"captain\":";
                            l_Index = l_PremiumStatus.IndexOf(l_Search, 0, StringComparison.Ordinal);
                            l_PremiumTime = l_PremiumStatus.Substring(l_Index + l_Search.Length, l_PremiumStatus.IndexOf(",", l_Index + l_Search.Length, StringComparison.Ordinal) - (l_Index + l_Search.Length));
                            m_Controller.Player.CaptainActive = l_PremiumTime.Equals("null") ? "0" : l_PremiumTime;

                            l_Search = "\"priest\":";
                            l_Index = l_PremiumStatus.IndexOf(l_Search, 0, StringComparison.Ordinal);
                            l_PremiumTime = l_PremiumStatus.Substring(l_Index + l_Search.Length, l_PremiumStatus.IndexOf(",", l_Index + l_Search.Length, StringComparison.Ordinal) - (l_Index + l_Search.Length));
                            m_Controller.Player.PriestActive = l_PremiumTime.Equals("null") ? "0" : l_PremiumTime;

                            l_Search = "\"trader\":";
                            l_Index = l_PremiumStatus.IndexOf(l_Search, 0, StringComparison.Ordinal);
                            l_PremiumTime = l_PremiumStatus.Substring(l_Index + l_Search.Length);
                            m_Controller.Player.TraderActive = l_PremiumTime.Equals("null") ? "0" : l_PremiumTime;
                        }

                        //Finish part one login sequence, need to gather data now.
                        m_Controller.LoginToGrepolis();
                    }
                }
                /*
                else
                {
                    m_Controller.InitCookies(BotWebBrowser);
                }
                */
            }
            catch (Exception ex)
            {
                Logger.WriteExceptionToLog(ex.Message);
            }
        }

        #endregion GUI Events - Browser tab

        #region GUI Events - Settings tab

        /// <summary>
        /// Save the settings to Settings static class.
        /// </summary>
        private void BotBtn_SaveSettings_Click(object sender, EventArgs e)
        {
            try
            {
                //Grepolis
                Settings.GrepolisUsername = BotTextBox_Username.Text;
                Settings.GrepolisMainServer = BotTextBox_MainServer.Text;
                Settings.GrepolisWorld = BotTextBox_World.Text;
                Settings.HeroWorld = BotCheckBox_HeroWorld.Checked;
                
                //Delays
                ValidateDelays(BotNumericUpDown_MinDelayRequests, BotNumericUpDown_MaxDelayRequests);
                Settings.MinRequestDelay = int.Parse(BotNumericUpDown_MinDelayRequests.Text);
                Settings.MaxRequestDelay = int.Parse(BotNumericUpDown_MaxDelayRequests.Text);
                ValidateDelays(BotNumericUpDown_MinTimerRefresh, BotNumericUpDown_MaxTimerRefresh);
                Settings.MinRefreshDelay = int.Parse(BotNumericUpDown_MinTimerRefresh.Text);
                Settings.MaxRefreshDelay = int.Parse(BotNumericUpDown_MaxTimerRefresh.Text);
                ValidateDelays(BotNumericUpDown_MinTimerFarmer, BotNumericUpDown_MaxTimerFarmer);
                Settings.MinFarmerDelay = int.Parse(BotNumericUpDown_MinTimerFarmer.Text);
                Settings.MaxFarmerDelay = int.Parse(BotNumericUpDown_MaxTimerFarmer.Text);
                ValidateDelays(BotNumericUpDown_MinTimerTrading, BotNumericUpDown_MaxTimerTrading);
                Settings.MinTradingDelay = int.Parse(BotNumericUpDown_MinTimerTrading.Text);
                Settings.MaxTradingDelay = int.Parse(BotNumericUpDown_MaxTimerTrading.Text);
                ValidateDelays(BotNumericUpDown_MinTimerBuild, BotNumericUpDown_MaxTimerBuild);
                Settings.MinBuildDelay = int.Parse(BotNumericUpDown_MinTimerBuild.Text);
                Settings.MaxBuildDelay = int.Parse(BotNumericUpDown_MaxTimerBuild.Text);
                ValidateDelays(BotNumericUpDown_MinTimerUnit, BotNumericUpDown_MaxTimerUnit);
                Settings.MinUnitDelay = int.Parse(BotNumericUpDown_MinTimerUnit.Text);
                Settings.MaxUnitDelay = int.Parse(BotNumericUpDown_MaxTimerUnit.Text);

                //Masters
                Settings.MasterFarmingEnabled = BotCheckBox_MasterFarmingEnabled.Checked;
                Settings.MasterTradingEnabled = BotCheckBox_MasterTradingEnabled.Checked;
                Settings.MasterBuildingEnabled = BotCheckBox_MasterBuildingEnabled.Checked;
                Settings.MasterUnitEnabled = BotCheckBox_MasterUnitEnabled.Checked;

                //Build
                Settings.AdvancedQueue = BotCheckBox_SettingsAdvQueue.Checked;
                Settings.BuildFarmBelow = int.Parse(BotNumericUpDown_FarmBelow.Text);

                //Reconnect
                ValidateDelays(BotNumericUpDown_MinDelayReconnect, BotNumericUpDown_MaxDelayReconnect);
                Settings.MinReconnectDelay = int.Parse(BotNumericUpDown_MinDelayReconnect.Text);
                Settings.MaxReconnectDelay = int.Parse(BotNumericUpDown_MaxDelayReconnect.Text);
                Settings.MaxRetryCount = int.Parse(BotNumericUpDown_MaxRetryCount.Text);

                //UnitQueue
                Settings.QueueLimit = int.Parse(BotNumericUpDown_QueueLimit.Text);
                Settings.MinUnitQueuePop = int.Parse(BotNumericUpDown_MinUnitQueuePop.Text);
                Settings.SkipUnitQueuePop = int.Parse(BotNumericUpDown_SkipUnitQueuePop.Text);

                //Others
                Settings.AutoStart = BotCheckBox_AutoStart.Checked;
                Settings.AdvUserAgent = BotTextBox_AdvUserAgent.Text;
                Settings.GUILogSize = int.Parse(BotNumericUpDown_GuiLogSize.Text);

                IOHelper.SaveSettingsToXml();
            }
            catch (Exception ex)
            {
                Logger.WriteExceptionToLog(ex.Message);
            }
        }

        /// <summary>
        /// Validate the delays.
        /// </summary>
        private void ValidateDelays(Control p_Min, Control p_Max)
        {
            try
            {
                if (int.Parse(p_Min.Text) > int.Parse(p_Max.Text))
                    p_Min.Text = p_Max.Text;
            }
            catch (Exception ex)
            {
                Logger.WriteExceptionToLog(ex.Message);
            }
        }

        #endregion GUI Events - Settings tab

        #region GUI Events - Farming tab

        /// <summary>
        /// Town selected.
        /// </summary>
        private void BotComboBox_TownListFarmers_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (BotComboBox_TownListFarmers.Items.Count < 1)
                    return;

                m_SelectedTownID = ((Town)BotComboBox_TownListFarmers.SelectedItem).TownID;

                UpdateFarmingGUI();
            }
            catch (Exception ex)
            {
                Logger.WriteExceptionToLog(ex.Message);
            }
        }

        /// <summary>
        /// Switch to prev town.
        /// </summary>
        private void BotButton_PrevTownFarmers_Click(object sender, EventArgs e)
        {
            try
            {
                if (BotComboBox_TownListFarmers.Items.Count < 1)
                    return;
                if (BotComboBox_TownListFarmers.SelectedIndex == 0)
                {
                    BotComboBox_TownListFarmers.SelectedIndex = BotComboBox_TownListFarmers.Items.Count - 1;
                }
                else
                {
                    BotComboBox_TownListFarmers.SelectedIndex = BotComboBox_TownListFarmers.SelectedIndex - 1;
                }
            }
            catch (Exception ex)
            {
                Logger.WriteExceptionToLog(ex.Message);
            }
        }

        /// <summary>
        /// Switch to next town.
        /// </summary>
        private void BotButton_NextTownFarmers_Click(object sender, EventArgs e)
        {
            try
            {
                if (BotComboBox_TownListFarmers.Items.Count < 1)
                    return;
                if (BotComboBox_TownListFarmers.SelectedIndex + 1 == BotComboBox_TownListFarmers.Items.Count)
                {
                    BotComboBox_TownListFarmers.SelectedIndex = 0;
                }
                else
                {
                    BotComboBox_TownListFarmers.SelectedIndex = BotComboBox_TownListFarmers.SelectedIndex + 1;
                }
            }
            catch (Exception ex)
            {
                Logger.WriteExceptionToLog(ex.Message);
            }
        }

        /// <summary>
        /// Save farmer setting.
        /// </summary>
        private void BotButton_SaveFarmers_Click(object sender, EventArgs e)
        {
            try
            {
                //Get Town
                var l_Town = m_Controller.Player.Towns.Single(x => x.TownID == m_SelectedTownID);

                //Loot interval
                l_Town.LootIntervalMinutes = int.Parse(BotDomainUpDown_LootInterval.Text);

                //Loot enabled
                l_Town.LootEnabled = BotCheckBox_FarmersLootEnabled.Checked;

                //Farmer enabled.
                l_Town.Farmers[0].Enabled = BotCheckBox_Village1.Checked;
                l_Town.Farmers[1].Enabled = BotCheckBox_Village2.Checked;
                l_Town.Farmers[2].Enabled = BotCheckBox_Village3.Checked;
                l_Town.Farmers[3].Enabled = BotCheckBox_Village4.Checked;
                l_Town.Farmers[4].Enabled = BotCheckBox_Village5.Checked;
                l_Town.Farmers[5].Enabled = BotCheckBox_Village6.Checked;

                IOHelper.SaveTownSettingsToXml(m_Controller.Player.Towns);
            }
            catch (Exception ex)
            {
                Logger.WriteExceptionToLog(ex.Message);
            }
        }

        #endregion GUI Events - Farming tab

        #region GUI Events - Trading tab

        /// <summary>
        /// Town selected.
        /// </summary>
        private void BotComboBox_TownListTrading_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (BotComboBox_TownListTrading.Items.Count < 1)
                    return;

                m_SelectedTownID = ((Town)BotComboBox_TownListTrading.SelectedItem).TownID;

                UpdateTradingGUI();
            }
            catch (Exception ex)
            {
                Logger.WriteExceptionToLog(ex.Message);
            }
        }

        /// <summary>
        /// Switch to prev town.
        /// </summary>
        private void BotButton_PrevTownTrading_Click(object sender, EventArgs e)
        {
            try
            {
                if (BotComboBox_TownListTrading.Items.Count < 1)
                    return;
                if (BotComboBox_TownListTrading.SelectedIndex == 0)
                {
                    BotComboBox_TownListTrading.SelectedIndex = BotComboBox_TownListTrading.Items.Count - 1;
                }
                else
                {
                    BotComboBox_TownListTrading.SelectedIndex = BotComboBox_TownListTrading.SelectedIndex - 1;
                }
            }
            catch (Exception ex)
            {
                Logger.WriteExceptionToLog(ex.Message);
            }
        }

        /// <summary>
        /// Switch to next town.
        /// </summary>
        private void BotButton_NextTownTrading_Click(object sender, EventArgs e)
        {
            try
            {
                if (BotComboBox_TownListTrading.Items.Count < 1)
                    return;
                if (BotComboBox_TownListTrading.SelectedIndex + 1 == BotComboBox_TownListTrading.Items.Count)
                {
                    BotComboBox_TownListTrading.SelectedIndex = 0;
                }
                else
                {
                    BotComboBox_TownListTrading.SelectedIndex = BotComboBox_TownListTrading.SelectedIndex + 1;
                }
            }
            catch (Exception ex)
            {
                Logger.WriteExceptionToLog(ex.Message);
            }
        }

        /// <summary>
        /// Save trade settings.
        /// </summary>
        private void BotButton_SaveTrade_Click(object sender, EventArgs e)
        {
            try
            {
                //Get Town
                var l_Town = m_Controller.Player.Towns.Single(x => x.TownID == m_SelectedTownID);

                l_Town.TradeEnabled = BotCheckBox_TradingEnabled.Checked;
                l_Town.TradeMode = (TradingModes)Enum.Parse(typeof(TradingModes), BotComboBox_TradeMode.Text);
                l_Town.TradeWoodRemaining = int.Parse(BotNumericUpDown_TradeRemainingWood.Text);
                l_Town.TradeStoneRemaining = int.Parse(BotNumericUpDown_TradeRemainingStone.Text);
                l_Town.TradeIronRemaining = int.Parse(BotNumericUpDown_TradeRemainingIron.Text);
                l_Town.TradePercentageWarehouse = int.Parse(BotNumericUpDown_TradePercentageWarehouse.Text);
                l_Town.TradeMinSendAmount = int.Parse(BotNumericUpDown_TradeMinSendAmount.Text);
                l_Town.TradeMaxDistance = int.Parse(BotNumericUpDown_TradeMaxDistance.Text);

                IOHelper.SaveTownSettingsToXml(m_Controller.Player.Towns);
            }
            catch (Exception ex)
            {
                Logger.WriteExceptionToLog(ex.Message);
            }
        }

        #endregion GUI Events - Trading tab

        #region GUI Events - Culture tab

        /// <summary>
        /// Town selected.
        /// </summary>
        private void BotComboBox_TownListCulture_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (BotComboBox_TownListCulture.Items.Count < 1)
                    return;

                m_SelectedTownID = ((Town)BotComboBox_TownListCulture.SelectedItem).TownID;

                UpdateCultureGUI();
            }
            catch (Exception ex)
            {
                Logger.WriteExceptionToLog(ex.Message);
            }
        }

        /// <summary>
        /// Switch to prev town.
        /// </summary>
        private void BotButton_PrevTownCulture_Click(object sender, EventArgs e)
        {
            try
            {
                if (BotComboBox_TownListCulture.Items.Count < 1)
                    return;
                if (BotComboBox_TownListCulture.SelectedIndex == 0)
                {
                    BotComboBox_TownListCulture.SelectedIndex = BotComboBox_TownListCulture.Items.Count - 1;
                }
                else
                {
                    BotComboBox_TownListCulture.SelectedIndex = BotComboBox_TownListCulture.SelectedIndex - 1;
                }
            }
            catch (Exception ex)
            {
                Logger.WriteExceptionToLog(ex.Message);
            }
        }

        /// <summary>
        /// Switch to next town.
        /// </summary>
        private void BotButton_NextTownCulture_Click(object sender, EventArgs e)
        {
            try
            {
                if (BotComboBox_TownListCulture.Items.Count < 1)
                    return;
                if (BotComboBox_TownListCulture.SelectedIndex + 1 == BotComboBox_TownListCulture.Items.Count)
                {
                    BotComboBox_TownListCulture.SelectedIndex = 0;
                }
                else
                {
                    BotComboBox_TownListCulture.SelectedIndex = BotComboBox_TownListCulture.SelectedIndex + 1;
                }
            }
            catch (Exception ex)
            {
                Logger.WriteExceptionToLog(ex.Message);
            }
        }

        /// <summary>
        /// Save Culture Settings
        /// </summary>
        private void BotButton_SaveCulture_Click(object sender, EventArgs e)
        {
            //Get Town
            var l_Town = m_Controller.Player.Towns.Single(x => x.TownID == m_SelectedTownID);

            l_Town.CulturalFestivalsEnabled = BotCheckBox_CultureEnabled.Checked;

            l_Town.CulturalEvents[0].Enabled = BotCheckBox_PartyEnabled.Checked;
            l_Town.CulturalEvents[1].Enabled = BotCheckBox_GamesEnabled.Checked;
            l_Town.CulturalEvents[2].Enabled = BotCheckBox_TriumphEnabled.Checked;
            l_Town.CulturalEvents[3].Enabled = BotCheckBox_TheaterEnabled.Checked;

            IOHelper.SaveTownSettingsToXml(m_Controller.Player.Towns);
        }

        #endregion GUI Events - Culture tab

        #region GUI Events - Queue tab

        /// <summary>
        /// Town selected.
        /// </summary>
        private void BotComboBox_TownListQueue_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (BotComboBox_TownListQueue.Items.Count < 1)
                    return;

                m_SelectedTownID = ((Town)BotComboBox_TownListQueue.SelectedItem).TownID;

                UpdateQueueGUI();
            }
            catch (Exception ex)
            {
                Logger.WriteExceptionToLog(ex.Message);
            }
        }

        /// <summary>
        /// Switch to prev town.
        /// </summary>
        private void BotButton_PrevTownQueue_Click(object sender, EventArgs e)
        {
            try
            {
                if (BotComboBox_TownListQueue.Items.Count < 1)
                    return;
                if (BotComboBox_TownListQueue.SelectedIndex == 0)
                {
                    BotComboBox_TownListQueue.SelectedIndex = BotComboBox_TownListQueue.Items.Count - 1;
                }
                else
                {
                    BotComboBox_TownListQueue.SelectedIndex = BotComboBox_TownListQueue.SelectedIndex - 1;
                }
            }
            catch (Exception ex)
            {
                Logger.WriteExceptionToLog(ex.Message);
            }
        }

        /// <summary>
        /// Switch to next town.
        /// </summary>
        private void BotButton_NextTownQueue_Click(object sender, EventArgs e)
        {
            try
            {
                if (BotComboBox_TownListQueue.Items.Count < 1)
                    return;
                if (BotComboBox_TownListQueue.SelectedIndex + 1 == BotComboBox_TownListQueue.Items.Count)
                {
                    BotComboBox_TownListQueue.SelectedIndex = 0;
                }
                else
                {
                    BotComboBox_TownListQueue.SelectedIndex = BotComboBox_TownListQueue.SelectedIndex + 1;
                }
            }
            catch (Exception ex)
            {
                Logger.WriteExceptionToLog(ex.Message);
            }
        }

        /// <summary>
        /// Save settings.
        /// </summary>
        private void BotButton_SaveQueue_Click(object sender, EventArgs e)
        {
            try
            {
                var l_Town = m_Controller.Player.Towns.Single(x => x.TownID == m_SelectedTownID);
                l_Town.BotBuildingQueue = new List<Buildings>(); //Reset Building queue

                l_Town.BuildingQueueEnabled = BotCheckBox_BuildingQueueEnabled.Checked;
                l_Town.DowngradeEnabled = BotCheckBox_BuildingDowngrade.Checked;
                l_Town.BuildingTargetEnabled = BotCheckBox_BuildingQueueTarget.Checked;
                l_Town.UnitQueueEnabled = BotCheckBox_UnitQueueEnabled.Checked;
                l_Town.Priority = int.Parse(BotNumericUpDown_TownPriority.Text);

                //fill bot building queue
                for (var i = 0; i < m_BotQueueSize; i++)
                {
                    var l_BuildingEnum =
                        (Buildings)
                            Enum.Parse(typeof(Buildings),
                                ((Label)BotFlowLayoutPanel_QueueBot.Controls[i]).Tag.ToString());
                    l_Town.BotBuildingQueue.Add(l_BuildingEnum);
                }

                //Set target levels
                for (var i = 0; i < l_Town.Buildings.Count; i++)
                {
                    l_Town.Buildings[i].TargetLevel =
                        int.Parse(((NumericUpDown)BotFlowLayoutPanel_Target.Controls[i]).Value.ToString(CultureInfo.InvariantCulture));
                }

                //Save army units
                for (var i = 0; i < l_Town.ArmyUnits.Count; i++)
                {
                    l_Town.ArmyUnits[i].QueueBot =
                        int.Parse(((NumericUpDown) BotFlowLayoutPanel_UnitsTarget.Controls[i]).Text);
                }
                
                IOHelper.SaveTownSettingsToXml(m_Controller.Player.Towns);
            }
            catch (Exception ex)
            {
                Logger.WriteExceptionToLog(ex.Message);
            }
        }

        /// <summary>
        /// Double click event for all queuebot labels.
        /// </summary>
        private void BotLabel_QueueBot_DoubleClick(object sender, EventArgs e)
        {
            RemoveBuildingFromQueueAtIndex(((Label)sender).TabIndex);
        }

        /// <summary>
        /// Single click on add button for adding to bot queue.
        /// </summary>
        private void BotLabel_BuildingAdd_Click(object sender, EventArgs e)
        {
            try
            {
                var l_Building = (Buildings)Enum.Parse(typeof(Buildings), ((Label)sender).Tag.ToString());
                AddBuildingToBotQueue(l_Building);
            }
            catch (Exception ex)
            {
                Logger.WriteExceptionToLog(ex.Message);
            }
        }

        /// <summary>
        /// Add the given building to the bot queue.
        /// </summary>
        private void AddBuildingToBotQueue(Buildings p_Building)
        {
            try
            {
                if (m_BotQueueSize == 42) //Queue full
                    return;
                var l_Label = ((Label)BotFlowLayoutPanel_QueueBot.Controls[m_BotQueueSize]);
                switch (p_Building)
                {
                    case Buildings.main:
                        l_Label.Image = Properties.Resources.mainx20;
                        break;

                    case Buildings.hide:
                        l_Label.Image = Properties.Resources.hidex20;
                        break;

                    case Buildings.storage:
                        l_Label.Image = Properties.Resources.storagex20;
                        break;

                    case Buildings.farm:
                        l_Label.Image = Properties.Resources.farmx20;
                        break;

                    case Buildings.lumber:
                        l_Label.Image = Properties.Resources.lumberx20;
                        break;

                    case Buildings.stoner:
                        l_Label.Image = Properties.Resources.stonerx20;
                        break;

                    case Buildings.ironer:
                        l_Label.Image = Properties.Resources.ironerx20;
                        break;

                    case Buildings.market:
                        l_Label.Image = Properties.Resources.marketx20;
                        break;

                    case Buildings.docks:
                        l_Label.Image = Properties.Resources.docksx20;
                        break;

                    case Buildings.barracks:
                        l_Label.Image = Properties.Resources.barracksx20;
                        break;

                    case Buildings.wall:
                        l_Label.Image = Properties.Resources.wallx20;
                        break;

                    case Buildings.academy:
                        l_Label.Image = Properties.Resources.academyx20;
                        break;

                    case Buildings.temple:
                        l_Label.Image = Properties.Resources.templex20;
                        break;

                    case Buildings.theater:
                        l_Label.Image = Properties.Resources.theaterx20;
                        break;

                    case Buildings.thermal:
                        l_Label.Image = Properties.Resources.theaterx20;
                        break;

                    case Buildings.library:
                        l_Label.Image = Properties.Resources.libraryx20;
                        break;

                    case Buildings.lighthouse:
                        l_Label.Image = Properties.Resources.lighthousex20;
                        break;

                    case Buildings.tower:
                        l_Label.Image = Properties.Resources.towerx20;
                        break;

                    case Buildings.statue:
                        l_Label.Image = Properties.Resources.statuex20;
                        break;

                    case Buildings.oracle:
                        l_Label.Image = Properties.Resources.oraclex20;
                        break;

                    case Buildings.trade_office:
                        l_Label.Image = Properties.Resources.trade_officex20;
                        break;
                }
                l_Label.Tag = p_Building.ToString("G");
                m_BotQueueSize++;
            }
            catch (Exception ex)
            {
                Logger.WriteExceptionToLog(ex.Message);
            }
        }

        /// <summary>
        /// Add the given building to the ingame queue.
        /// </summary>
        private void AddBuildingToIngameQueue(Buildings p_Building)
        {
            try
            {
                var l_Label = ((Label)BotFlowLayoutPanel_QueueGame.Controls[m_IngameQueueSize]);
                switch (p_Building)
                {
                    case Buildings.main:
                        l_Label.Image = Properties.Resources.mainx20;
                        break;

                    case Buildings.hide:
                        l_Label.Image = Properties.Resources.hidex20;
                        break;

                    case Buildings.storage:
                        l_Label.Image = Properties.Resources.storagex20;
                        break;

                    case Buildings.farm:
                        l_Label.Image = Properties.Resources.farmx20;
                        break;

                    case Buildings.lumber:
                        l_Label.Image = Properties.Resources.lumberx20;
                        break;

                    case Buildings.stoner:
                        l_Label.Image = Properties.Resources.stonerx20;
                        break;

                    case Buildings.ironer:
                        l_Label.Image = Properties.Resources.ironerx20;
                        break;

                    case Buildings.market:
                        l_Label.Image = Properties.Resources.marketx20;
                        break;

                    case Buildings.docks:
                        l_Label.Image = Properties.Resources.docksx20;
                        break;

                    case Buildings.barracks:
                        l_Label.Image = Properties.Resources.barracksx20;
                        break;

                    case Buildings.wall:
                        l_Label.Image = Properties.Resources.wallx20;
                        break;

                    case Buildings.academy:
                        l_Label.Image = Properties.Resources.academyx20;
                        break;

                    case Buildings.temple:
                        l_Label.Image = Properties.Resources.templex20;
                        break;

                    case Buildings.theater:
                        l_Label.Image = Properties.Resources.theaterx20;
                        break;

                    case Buildings.thermal:
                        l_Label.Image = Properties.Resources.thermalx20;
                        break;

                    case Buildings.library:
                        l_Label.Image = Properties.Resources.libraryx20;
                        break;

                    case Buildings.lighthouse:
                        l_Label.Image = Properties.Resources.lighthousex20;
                        break;

                    case Buildings.tower:
                        l_Label.Image = Properties.Resources.towerx20;
                        break;

                    case Buildings.statue:
                        l_Label.Image = Properties.Resources.statuex20;
                        break;

                    case Buildings.oracle:
                        l_Label.Image = Properties.Resources.oraclex20;
                        break;

                    case Buildings.trade_office:
                        l_Label.Image = Properties.Resources.trade_officex20;
                        break;
                }
                l_Label.Tag = p_Building.ToString("G");
                m_IngameQueueSize++;
            }
            catch (Exception ex)
            {
                Logger.WriteExceptionToLog(ex.Message);
            }
        }

        /// <summary>
        /// Remove building from queue at a specific index.
        /// </summary>
        private void RemoveBuildingFromQueueAtIndex(int p_Index)
        {
            try
            {
                //Queue is empty at this spot
                if ((string)((Label)BotFlowLayoutPanel_QueueBot.Controls[p_Index - 1]).Tag == "empty")
                    return;
                //Shift images one left
                for (var i = p_Index - 1; i < BotFlowLayoutPanel_QueueBot.Controls.Count - 1; i++)
                {
                    ((Label)BotFlowLayoutPanel_QueueBot.Controls[i]).Image =
                        ((Label)BotFlowLayoutPanel_QueueBot.Controls[i + 1]).Image;
                    ((Label)BotFlowLayoutPanel_QueueBot.Controls[i]).Tag =
                        ((Label)BotFlowLayoutPanel_QueueBot.Controls[i + 1]).Tag;
                }

                ((Label)BotFlowLayoutPanel_QueueBot.Controls[BotFlowLayoutPanel_QueueBot.Controls.Count - 1]).Image =
                    Properties.Resources.emptyx20;
                ((Label)BotFlowLayoutPanel_QueueBot.Controls[BotFlowLayoutPanel_QueueBot.Controls.Count - 1]).Tag =
                    "empty";

                m_BotQueueSize--;
            }
            catch (Exception ex)
            {
                Logger.WriteExceptionToLog(ex.Message);
            }
        }

        #endregion GUI Events - Queue tab

        #region Timer Events

        /// <summary>
        /// TimerLoginPart2 Tick Event.
        /// </summary>
        private void TimerLoginPart2OnTick(object sender, EventArgs eventArgs)
        {
            m_TimerLoginPart2.Stop();
            LoginToGrepolisPart3();
        }

        #endregion Timer Events

        #region Controller Events

        /// <summary>
        /// Handle the Controller StartReconnectEvent.
        /// </summary>
        private void ControllerOnStartReconnectEvent(object sender, CustomArgs ca)
            => new Thread(StartReconnectCrossThread).Start();

        /// <summary>
        /// Handle the Controller LogEvent.
        /// </summary>
        private void ControllerOnLogEvent(object sender, CustomArgs ca) => new Thread(LogCrossThread).Start(ca.Message);

        /// <summary>
        /// Handle the Controller LoginSuccessfulEvent
        /// </summary>
        private void ControllerOnLoginSuccessfulEvent(object sender, CustomArgs ca) => new Thread(LoginSuccessfulCrossThread).Start();

        /// <summary>
        /// Handle the Controller RefreshTimerUpdatedEvent
        /// </summary>
        private void ControllerOnRefreshTimerUpdatedEvent(object sender, CustomArgs ca) => new Thread(RefreshTimerUpdatedCrossThread).Start(ca.Message);

        /// <summary>
        /// Handle the Controller FarmerTimerUpdatedEvent.
        /// </summary>
        private void ControllerOnFarmerTimerUpdatedEvent(object sender, CustomArgs ca) => new Thread(FarmerTimerUpdatedCrossThread).Start(ca.Message);

        /// <summary>
        /// Handle the Controller TradingTimerUpdatedEvent.
        /// </summary>
        private void ControllerOnTradingTimerUpdatedEvent(object sender, CustomArgs ca) => new Thread(TradingTimerUpdatedCrossThread).Start(ca.Message);

        /// <summary>
        /// Handle the Controller ReconnectTimerUpdatedEvent.
        /// </summary>
        private void ControllerOnReconnectTimerUpdatedEvent(object sender, CustomArgs ca) => new Thread(ReconnectTimerUpdatedCrossThread).Start(ca.Message);

        /// <summary>
        /// Handle the Controller LoggedOutEvent.
        /// </summary>
        private void ControllerOnLoggedOutEvent(object sender, CustomArgs ca) => new Thread(LoggedOutCrossThread).Start();

        /// <summary>
        /// Handle the Controller BotStartedEvent.
        /// </summary>
        private void ControllerOnBotStartedEvent(object sender, CustomArgs ca) => new Thread(BotStartedCrossThread).Start();

        /// <summary>
        /// Handle the Controller BotStoppedEvent.
        /// </summary>
        private void ControllerOnBotStoppedEvent(object sender, CustomArgs ca) => new Thread(BotStoppedCrossThread).Start();

        /// <summary>
        /// Handle the Controller BotCyclingStartedEvent.
        /// </summary>
        private void ControllerOnBotCyclingStartedEvent(object sender, CustomArgs ca) => new Thread(BotCyclingStartedCrossThread).Start();

        /// <summary>
        /// Handle the Controller BotCyclingStoppedEvent.
        /// </summary>
        private void ControllerOnBotCyclingStoppedEvent(object sender, CustomArgs ca) => new Thread(BotCyclingStoppedCrossThread).Start();

        /// <summary>
        /// Handle the Controller BuildTimerUpdatedEvent.
        /// </summary>
        private void ControllerOnBuildTimerUpdatedEvent(object sender, CustomArgs ca)
            => new Thread(BuildTimerUpdatedCrossThread).Start(ca.Message);

        /// <summary>
        /// Handle the Controller UnitTimerUpdatedEvent.
        /// </summary>
        private void ControllerOnUnitTimerUpdatedEvent(object sender, CustomArgs ca)
            => new Thread(UnitTimerUpdatedCrossThread).Start(ca.Message);

        #endregion Controller Events

        #endregion Events

        #region Cross Threads

        /// <summary>
        /// New Thread handling the Controller StartReconnect Event.
        /// </summary>
        private void StartReconnectCrossThread()
        {
            try
            {
                if (BotWebBrowser.InvokeRequired)
                    Invoke(new StartReconnectCallBack(LoginToGrepolisPart1));
                else
                    LoginToGrepolisPart1();
            }
            catch (Exception ex)
            {
                Logger.WriteExceptionToLog(ex.Message);
            }
        }

        /// <summary>
        /// New Thread handling the Controller Log Event.
        /// </summary>
        private void LogCrossThread(object p_Message)
        {
            try
            {
                if (BotTextBox_Log.InvokeRequired)
                    Invoke(new LogCallBack(WriteToLog), (string)p_Message);
                else
                    WriteToLog((string)p_Message);
            }
            catch (Exception ex)
            {
                Logger.WriteExceptionToLog(ex.Message);
            }
        }

        /// <summary>
        /// Called to write Message to Log Tab.
        /// </summary>
        private void WriteToLog(string p_Message)
        {
            try
            {
                Logger.WriteToLog(p_Message);

                BotTextBox_Log.Text = DateTime.Now.ToString(CultureInfo.InvariantCulture) + @": " + p_Message + Environment.NewLine + BotTextBox_Log.Text;

                if (BotTextBox_Log.Lines.Length > Settings.GUILogSize)
                {
                    var l_Length = 0;
                    for (var i = Settings.GUILogSize; i < BotTextBox_Log.Lines.Length; i++)
                    {
                        l_Length += BotTextBox_Log.Lines[i].Length;
                    }
                    BotTextBox_Log.Text = BotTextBox_Log.Text.Remove(BotTextBox_Log.Text.Length - l_Length);
                    BotTextBox_Log.Lines[BotTextBox_Log.Lines.Length - 1].Remove(0);
                }
            }
            catch (Exception ex)
            {
                Logger.WriteExceptionToLog(ex.Message);
            }
        }

        /// <summary>
        /// New thread handling the Controller LoginSuccessful Event.
        /// </summary>
        private void LoginSuccessfulCrossThread()
        {
            try
            {
                if (BotBtn_Start.InvokeRequired)
                    Invoke(new LoginSuccessfulCallBack(LoginSuccessful));
                else
                    LoginSuccessful();
            }
            catch (Exception ex)
            {
                Logger.WriteExceptionToLog(ex.Message);
            }
        }

        /// <summary>
        /// Enable the Start button.
        /// Enable town lists on tabs.
        /// TODO: Check for invoke
        /// </summary>
        private void LoginSuccessful()
        {
            try
            {
                //Overview page
                //Enable Start Button
                BotBtn_Start.Enabled = true;
                BotLabel_LoginStatus.ForeColor = Color.Green;
                BotLabel_LoginStatus.Text = @"Logged in";

                /*
                 * if (labelCulturalLevel.InvokeRequired)
                    labelCulturalLevel.Invoke(new MethodInvoker(delegate { labelCulturalLevel.Text = m_Controller.Player.CulturalLevelStr; }));
                   else
                    labelCulturalLevel.Text = m_Controller.Player.CulturalLevelStr;
                 *
                 */

                //Because the combobox towns are bind to the same datasource, all change if one index changes.
                //http://stackoverflow.com/questions/11261468/why-changing-selecteditem-in-one-combo-changes-all-other-combos

                //Farming Tab
                BotComboBox_TownListFarmers.DataSource = m_Controller.Player.Towns;
                BotComboBox_TownListFarmers.DisplayMember = "TownName";
                BotButton_PrevTownFarmers.Enabled = true;
                BotButton_NextTownFarmers.Enabled = true;

                //Trading Tab
                BotComboBox_TownListTrading.DataSource = m_Controller.Player.Towns;
                BotComboBox_TownListTrading.DisplayMember = "TownName";
                BotButton_PrevTownTrading.Enabled = true;
                BotButton_NextTownTrading.Enabled = true;

                //Culture Tab
                BotComboBox_TownListCulture.DataSource = m_Controller.Player.Towns;
                BotComboBox_TownListCulture.DisplayMember = "TownName";
                BotButton_PrevTownCulture.Enabled = true;
                BotButton_NextTownCulture.Enabled = true;

                //Queue Tab
                BotComboBox_TownListQueue.DataSource = m_Controller.Player.Towns;
                BotComboBox_TownListQueue.DisplayMember = "TownName";
                BotButton_PrevTownQueue.Enabled = true;
                BotButton_NextTownQueue.Enabled = true;
            }
            catch (Exception ex)
            {
                Logger.WriteExceptionToLog(ex.Message);
            }
        }

        /// <summary>
        /// New thread handling the Controller RefreshTimerUpdatedEvent.
        /// </summary>
        private void RefreshTimerUpdatedCrossThread(object p_TimeLeft)
        {
            try
            {
                if (BotLabel_RefreshTimer.InvokeRequired)
                    Invoke(new RefreshTimerUpdatedCallBack(UpdateRefreshTimer), (string)p_TimeLeft);
                else
                    UpdateRefreshTimer((string)p_TimeLeft);
            }
            catch (Exception ex)
            {
                Logger.WriteExceptionToLog(ex.Message);
            }
        }

        /// <summary>
        /// Update the RefreshTimer label.
        /// </summary>
        private void UpdateRefreshTimer(string p_TimeLeft)
        {
            try
            {
                BotLabel_RefreshTimer.Text = p_TimeLeft.Contains("-") ? @"00:00:00" : p_TimeLeft;
            }
            catch (Exception ex)
            {
                Logger.WriteExceptionToLog(ex.Message);
            }
        }

        /// <summary>
        /// New thread handling the Controller FarmerTimerUpdatedEvent.
        /// </summary>
        private void FarmerTimerUpdatedCrossThread(object p_TimeLeft)
        {
            try
            {
                if (BotLabel_FarmerTimer.InvokeRequired)
                    Invoke(new FarmerTimerUpdatedCallBack(UpdateFarmerTimer), (string)p_TimeLeft);
                else
                    UpdateFarmerTimer((string)p_TimeLeft);
            }
            catch (Exception ex)
            {
                Logger.WriteExceptionToLog(ex.Message);
            }
        }

        /// <summary>
        /// Update the FarmerTimer label.
        /// </summary>
        private void UpdateFarmerTimer(string p_TimeLeft)
        {
            try
            {
                BotLabel_FarmerTimer.Text = p_TimeLeft.Contains("-") ? @"00:00:00" : p_TimeLeft;
            }
            catch (Exception ex)
            {
                Logger.WriteExceptionToLog(ex.Message);
            }
        }

        /// <summary>
        /// New thread handling the Controller TradingTimerUpdatedEvent.
        /// </summary>
        private void TradingTimerUpdatedCrossThread(object p_TimeLeft)
        {
            try
            {
                if (BotLabel_TradingTimer.InvokeRequired)
                    Invoke(new TradingTimerUpdatedCallBack(UpdateTradingTimer), (string)p_TimeLeft);
                else
                    UpdateTradingTimer((string)p_TimeLeft);
            }
            catch (Exception ex)
            {
                Logger.WriteExceptionToLog(ex.Message);
            }
        }

        /// <summary>
        /// Update the TradingTimer label.
        /// </summary>
        private void UpdateTradingTimer(string p_TimeLeft)
        {
            try
            {
                BotLabel_TradingTimer.Text = p_TimeLeft.Contains("-") ? @"00:00:00" : p_TimeLeft;
            }
            catch (Exception ex)
            {
                Logger.WriteExceptionToLog(ex.Message);
            }
        }

        /// <summary>
        /// New thread handling the Controller ReconnectTimerUpdatedEvent.
        /// </summary>
        private void ReconnectTimerUpdatedCrossThread(object p_TimeLeft)
        {
            try
            {
                if (BotLabel_ReconnectTimer.InvokeRequired)
                    Invoke(new ReconnectTimerUpdatedCallBack(UpdateReconnectTimer), (string)p_TimeLeft);
                else
                    UpdateReconnectTimer((string)p_TimeLeft);
            }
            catch (Exception ex)
            {
                Logger.WriteExceptionToLog(ex.Message);
            }
        }

        /// <summary>
        /// Update the ReconnectTimer label.
        /// </summary>
        private void UpdateReconnectTimer(string p_TimeLeft)
        {
            try
            {
                BotLabel_ReconnectTimer.Text = p_TimeLeft.Contains("-") ? @"00:00:00" : p_TimeLeft;
            }
            catch (Exception ex)
            {
                Logger.WriteExceptionToLog(ex.Message);
            }
        }

        /// <summary>
        /// New thread handling the Controller LoggedOutEvent.
        /// </summary>
        private void LoggedOutCrossThread()
        {
            try
            {
                if (BotLabel_LoginStatus.InvokeRequired)
                    Invoke(new LoggedOutCallBack(LoggedOut));
                else
                    LoggedOut();
            }
            catch (Exception ex)
            {
                Logger.WriteExceptionToLog(ex.Message);
            }
        }

        /// <summary>
        /// Set GUI to Logged Out.
        /// </summary>
        private void LoggedOut()
        {
            try
            {
                BotLabel_LoginStatus.ForeColor = Color.Red;
                BotLabel_LoginStatus.Text = @"Logged out";
            }
            catch (Exception ex)
            {
                Logger.WriteExceptionToLog(ex.Message);
            }
        }

        /// <summary>
        /// New thread handling the Controller BotStartedEvent.
        /// </summary>
        private void BotStartedCrossThread()
        {
            try
            {
                if (BotLabel_BotStatus.InvokeRequired)
                    Invoke(new BotStartedCallBack(BotStarted));
                else
                    BotStarted();
            }
            catch (Exception ex)
            {
                Logger.WriteExceptionToLog(ex.Message);
            }
        }

        /// <summary>
        /// Set GUI to Bot started.
        /// </summary>
        private void BotStarted()
        {
            try
            {
                BotLabel_BotStatus.ForeColor = Color.Green;
                BotLabel_BotStatus.Text = @"Bot running";
            }
            catch (Exception ex)
            {
                Logger.WriteExceptionToLog(ex.Message);
            }
        }

        /// <summary>
        /// New thread handling the Controller BotStoppedEvent.
        /// </summary>
        private void BotStoppedCrossThread()
        {
            try
            {
                if (BotLabel_BotStatus.InvokeRequired)
                    Invoke(new BotStoppedCallBack(BotStopped));
                else
                    BotStopped();
            }
            catch (Exception ex)
            {
                Logger.WriteExceptionToLog(ex.Message);
            }
        }

        /// <summary>
        /// Set GUI to Bot stopped.
        /// </summary>
        private void BotStopped()
        {
            try
            {
                BotLabel_BotStatus.ForeColor = Color.Red;
                BotLabel_BotStatus.Text = @"Not running";
            }
            catch (Exception ex)
            {
                Logger.WriteExceptionToLog(ex.Message);
            }
        }

        /// <summary>
        /// New thread handling the Controller BotCyclingStartedEvent.
        /// </summary>
        private void BotCyclingStartedCrossThread()
        {
            try
            {
                if (BotLabel_CycleStatus.InvokeRequired)
                    Invoke(new BotCyclingStartedCallBack(BotCyclingStarted));
                else
                    BotCyclingStarted();
            }
            catch (Exception ex)
            {
                Logger.WriteExceptionToLog(ex.Message);
            }
        }

        /// <summary>
        /// Set GUI to Cycling started.
        /// </summary>
        private void BotCyclingStarted()
        {
            try
            {
                BotLabel_CycleStatus.ForeColor = Color.Green;
                BotLabel_CycleStatus.Text = @"Cycling";
            }
            catch (Exception ex)
            {
                Logger.WriteExceptionToLog(ex.Message);
            }
        }

        /// <summary>
        /// New thread handling the Controller BotCyclingStoppedEvent.
        /// </summary>
        private void BotCyclingStoppedCrossThread()
        {
            try
            {
                if (BotLabel_CycleStatus.InvokeRequired)
                    Invoke(new BotCyclingStoppedCallBack(BotCyclingStopped));
                else
                    BotCyclingStopped();
            }
            catch (Exception ex)
            {
                Logger.WriteExceptionToLog(ex.Message);
            }
        }

        /// <summary>
        /// Set GUI to Cycling stopped.
        /// </summary>
        private void BotCyclingStopped()
        {
            try
            {
                BotLabel_CycleStatus.ForeColor = Color.Red;
                BotLabel_CycleStatus.Text = @"Not cycling";
            }
            catch (Exception ex)
            {
                Logger.WriteExceptionToLog(ex.Message);
            }
        }

        /// <summary>
        /// New thread handling the Controller BuildTimerUpdatedEvent.
        /// </summary>
        private void BuildTimerUpdatedCrossThread(object p_TimeLeft)
        {
            try
            {
                if (BotLabel_BuildTimer.InvokeRequired)
                    Invoke(new BuildTimerUpdatedCallBack(UpdateBuildTimer), (string)p_TimeLeft);
                else
                    UpdateBuildTimer((string)p_TimeLeft);
            }
            catch (Exception ex)
            {
                Logger.WriteExceptionToLog(ex.Message);
            }
        }

        /// <summary>
        /// Update the BuildTimer label.
        /// </summary>
        private void UpdateBuildTimer(string p_TimeLeft)
        {
            try
            {
                BotLabel_BuildTimer.Text = p_TimeLeft.Contains("-") ? @"00:00:00" : p_TimeLeft;
            }
            catch (Exception ex)
            {
                Logger.WriteExceptionToLog(ex.Message);
            }
        }

        /// <summary>
        /// New thread handling the Controller UnitTimerUpdatedEvent.
        /// </summary>
        private void UnitTimerUpdatedCrossThread(object p_TimeLeft)
        {
            try
            {
                if (BotLabel_UnitTimer.InvokeRequired)
                    Invoke(new UnitTimerUpdatedCallBack(UpdateUnitTimer), (string)p_TimeLeft);
                else
                    UpdateUnitTimer((string)p_TimeLeft);
            }
            catch (Exception ex)
            {
                Logger.WriteExceptionToLog(ex.Message);
            }
        }

        /// <summary>
        /// Update the UnitTimer label.
        /// </summary>
        private void UpdateUnitTimer(string p_TimeLeft)
        {
            try
            {
                BotLabel_UnitTimer.Text = p_TimeLeft.Contains("-") ? @"00:00:00" : p_TimeLeft;
            }
            catch (Exception ex)
            {
                Logger.WriteExceptionToLog(ex.Message);
            }
        }

        #endregion Cross Threads

    }
}
