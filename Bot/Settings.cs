using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace Bot
{
    public static class Settings
    {
        #region Attributs

        #region Bot Intern

        /// <summary>
        /// Path to log file.
        /// </summary>
        public static string LogFilePath => LogDirPath + "/log" + DateTime.Now.ToString("yyyyMMdd") +  ".txt";

        /// <summary>
        /// Path of log dir.
        /// </summary>
        public static string LogDirPath => "log";

        /// <summary>
        /// Path of Settings file.
        /// </summary>
        public static string SettingsFilePath => "Settings.xml";

        /// <summary>
        /// Path of towns file.
        /// </summary>
        public static string TownsFilePath => "Towns.xml";

        /// <summary>
        /// Grepolis world server url.
        /// </summary>
        public static string GrepolisWorldServer => GrepolisWorld + ".grepolis.com";

        /// <summary>
        /// Local Time Offset recieved from server.
        /// </summary>
        public static string LocaleTimeOffset { get; set; }

        /// <summary>
        /// Server Time Offset recieved from server.
        /// </summary>
        public static string ServerTimeOffset { get; set; }

        /// <summary>
        /// Game Speed
        /// </summary>
        public static string GameSpeed { get; set; } = "0";

        #endregion

        #region Settings

        #region Grepolis

        /// <summary>
        /// Grepolis Login Username.
        /// </summary>
        public static string GrepolisUsername { get; set; } = "username";

        /// <summary>
        /// Grepolis Main Server.
        /// </summary>
        public static string GrepolisMainServer { get; set; } = "en.grepolis.com";

        /// <summary>
        /// Grepolis World.
        /// </summary>
        public static string GrepolisWorld { get; set; } = "en1";

        /// <summary>
        /// Hero World or not.
        /// </summary>
        public static bool HeroWorld { get; set; }

        #endregion

        #region Delays

        /// <summary>
        /// Minimum Delay between Requests in Milliseconds.
        /// </summary>
        public static int MinRequestDelay { get; set; } = 1200;

        /// <summary>
        /// Maximum Delay between Requests in Milliseconds.
        /// </summary>
        public static int MaxRequestDelay { get; set; } = 2000;

        /// <summary>
        /// Minimum delay between bot refreshs in Minutes.
        /// </summary>
        public static int MinRefreshDelay { get; set; } = 1;

        /// <summary>
        /// Maximum delay between bot refreshs in Minutes.
        /// </summary>
        public static int MaxRefreshDelay { get; set; } = 5;

        /// <summary>
        /// Min delay between farming in Minutes.
        /// </summary>
        public static int MinFarmerDelay { get; set; } = 5;

        /// <summary>
        /// Max delay between farming in Minutes.
        /// </summary>
        public static int MaxFarmerDelay { get; set; } = 7;

        /// <summary>
        /// Min delay between trading in minutes.
        /// </summary>
        public static int MinTradingDelay { get; set; } = 10;

        /// <summary>
        /// Max delay between trading in minutes.
        /// </summary>
        public static int MaxTradingDelay { get; set; } = 15;

        /// <summary>
        /// Minimum delay between build in minutes.
        /// </summary>
        public static int MinBuildDelay { get; set; } = 10;

        /// <summary>
        /// Max delay between build in minutes.
        /// </summary>
        public static int MaxBuildDelay { get; set; } = 15;

        /// <summary>
        /// Min delay between unit building in minutes.
        /// </summary>
        public static int MinUnitDelay { get; set; } = 10;

        /// <summary>
        /// Max delay between unit building in minutes.
        /// </summary>
        public static int MaxUnitDelay { get; set; } = 15;

        #endregion

        #region Masters

        /// <summary>
        /// Should the bot farm.
        /// </summary>
        public static bool MasterFarmingEnabled { get; set; } = false;

        /// <summary>
        /// Should the bot trade.
        /// </summary>
        public static bool MasterTradingEnabled { get; set; } = false;

        /// <summary>
        /// Should the bot build building.
        /// </summary>
        public static bool MasterBuildingEnabled { get; set; } = false;

        /// <summary>
        /// Should the bot build units.
        /// </summary>
        public static bool MasterUnitEnabled { get; set; } = false;

        #endregion

        #region Build

        /// <summary>
        /// Whether or not to use AdvancedQueue (target and bot queue at same time).
        /// </summary>
        public static bool AdvancedQueue { get; set; } = false;

        /// <summary>
        /// Minimum population when to build farm.
        /// </summary>
        public static int BuildFarmBelow { get; set; } = 20;

        #endregion

        #region Reconnect

        /// <summary>
        /// Minimum delay before reconnect in Minutes.
        /// </summary>
        public static int MinReconnectDelay { get; set; } = 1;

        /// <summary>
        /// Maximum delay before reconnect in Minutes.
        /// </summary>
        public static int MaxReconnectDelay { get; set; } = 2;

        /// <summary>
        /// Maximum count of Retrys for reconnect.
        /// </summary>
        public static int MaxRetryCount { get; set; } = 3;

        #endregion

        #region Unit Queue

        /// <summary>
        /// Limit of queues.
        /// </summary>
        public static int QueueLimit { get; set; } = 7;

        /// <summary>
        /// Minimum population of units to put in queue.
        /// </summary>
        public static int MinUnitQueuePop { get; set; } = 10;

        /// <summary>
        /// Skip unit queue if pop is under this.
        /// </summary>
        public static int SkipUnitQueuePop { get; set; } = 8;

        #endregion

        #region Others

        /// <summary>
        /// Auto start bot.
        /// </summary>
        public static bool AutoStart { get; set; } = false;

        /// <summary>
        /// WebBrowser User Agent.
        /// </summary>
        public static string AdvUserAgent { get; set; } =
            "Mozilla/5.0 (Windows NT 6.3; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/56.0.2924.87 Safari/537.36";

        /// <summary>
        /// Maximum lines of Log.
        /// </summary>
        public static int GUILogSize { get; set; } = 500;

        #endregion

        #endregion

        #endregion Attributs
    }
}