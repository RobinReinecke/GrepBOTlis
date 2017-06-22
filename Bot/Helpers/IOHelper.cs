using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Bot.Custom;
using Bot.Enums;
using Bot.Properties;

namespace Bot.Helpers
{
    public static class IOHelper
    {
        /// <summary>
        /// Check files and process them if existing.
        /// </summary>
        public static void CheckFiles()
        {
            try
            {
                if (File.Exists(Settings.SettingsFilePath))
                {
                    LoadSettingsFromXml();
                }
                if (!Directory.Exists(Settings.LogDirPath))
                {
                    Directory.CreateDirectory(Settings.LogDirPath);
                }
            }
            catch (Exception ex)
            {
                Logger.WriteExceptionToLog(ex.Message);
            }
        }

        /// <summary>
        /// Load Settings from Settings XML
        /// </summary>
        private static void LoadSettingsFromXml()
        {
            try
            {
                var l_SettingsXml = XElement.Load(Settings.SettingsFilePath);
                var l_Target = l_SettingsXml.Elements("setting");
                var l_Settings = l_Target as XElement[] ?? l_Target.ToArray();

                //Grepolis
                var l_Grepusername = l_Settings.FirstOrDefault(x => x.Attribute("name").Value == "grepusername");
                if (l_Grepusername != null)
                    Settings.GrepolisUsername = l_Grepusername.Value;
                var l_Grepmainserv = l_Settings.FirstOrDefault(x => x.Attribute("name").Value == "grepmainserv");
                if (l_Grepmainserv != null)
                    Settings.GrepolisMainServer = l_Grepmainserv.Value;
                var l_Grepworld = l_Settings.FirstOrDefault(x => x.Attribute("name").Value == "grepworld");
                if (l_Grepworld != null)
                    Settings.GrepolisWorld = l_Grepworld.Value;
                var l_HeroWorld = l_Settings.FirstOrDefault(x => x.Attribute("name").Value == "heroworld");
                if (l_HeroWorld != null)
                    Settings.HeroWorld = l_HeroWorld.Value.Equals("true");
                //Delays
                var l_MinRequestDelay = l_Settings.FirstOrDefault(x => x.Attribute("name").Value == "minreqdelay");
                if (l_MinRequestDelay != null)
                    Settings.MinRequestDelay = int.Parse(l_MinRequestDelay.Value);
                var l_MaxRequestDelay = l_Settings.FirstOrDefault(x => x.Attribute("name").Value == "maxreqdelay");
                if (l_MaxRequestDelay != null)
                    Settings.MaxRequestDelay = int.Parse(l_MaxRequestDelay.Value);
                var l_MinRefreshDelay = l_Settings.FirstOrDefault(x => x.Attribute("name").Value == "minrefdelay");
                if (l_MinRefreshDelay != null)
                    Settings.MinRefreshDelay = int.Parse(l_MinRefreshDelay.Value);
                var l_MaxRefreshDelay = l_Settings.FirstOrDefault(x => x.Attribute("name").Value == "maxrefdelay");
                if (l_MaxRefreshDelay != null)
                    Settings.MaxRefreshDelay = int.Parse(l_MaxRefreshDelay.Value);
                var l_MinFarmerDelay = l_Settings.FirstOrDefault(x => x.Attribute("name").Value == "minfarmdelay");
                if (l_MinFarmerDelay != null)
                    Settings.MinFarmerDelay = int.Parse(l_MinFarmerDelay.Value);
                var l_MaxFarmerDelay = l_Settings.FirstOrDefault(x => x.Attribute("name").Value == "maxfarmdelay");
                if (l_MaxFarmerDelay != null)
                    Settings.MaxFarmerDelay = int.Parse(l_MaxFarmerDelay.Value);
                var l_MinTradingDelay = l_Settings.FirstOrDefault(x => x.Attribute("name").Value == "mintradedelay");
                if (l_MinTradingDelay != null)
                    Settings.MinTradingDelay = int.Parse(l_MinTradingDelay.Value);
                var l_MaxTradingDelay = l_Settings.FirstOrDefault(x => x.Attribute("name").Value == "maxtradedelay");
                if (l_MaxTradingDelay != null)
                    Settings.MaxTradingDelay = int.Parse(l_MaxTradingDelay.Value);
                var l_MinBuildDelay = l_Settings.FirstOrDefault(x => x.Attribute("name").Value == "minbuilddelay");
                if (l_MinBuildDelay != null)
                    Settings.MinBuildDelay = int.Parse(l_MinBuildDelay.Value);
                var l_MaxBuildDelay = l_Settings.FirstOrDefault(x => x.Attribute("name").Value == "maxbuilddelay");
                if (l_MaxBuildDelay != null)
                    Settings.MaxBuildDelay = int.Parse(l_MaxBuildDelay.Value);
                var l_MinUnitDelay = l_Settings.FirstOrDefault(x => x.Attribute("name").Value == "minunitdelay");
                if (l_MinUnitDelay != null)
                    Settings.MinUnitDelay = int.Parse(l_MinUnitDelay.Value);
                var l_MaxUnitDelay = l_Settings.FirstOrDefault(x => x.Attribute("name").Value == "maxunitdelay");
                if (l_MaxUnitDelay != null)
                    Settings.MaxUnitDelay = int.Parse(l_MaxUnitDelay.Value);
                //Masters
                var l_MasterFarming = l_Settings.FirstOrDefault(x => x.Attribute("name").Value == "masterfarming");
                if (l_MasterFarming != null)
                    Settings.MasterFarmingEnabled = l_MasterFarming.Value.Equals("true");
                var l_TradingFarming = l_Settings.FirstOrDefault(x => x.Attribute("name").Value == "mastertrading");
                if (l_TradingFarming != null)
                    Settings.MasterTradingEnabled = l_TradingFarming.Value.Equals("true");
                var l_MasterBuilding = l_Settings.FirstOrDefault(x => x.Attribute("name").Value == "masterbuilding");
                if (l_MasterBuilding != null)
                    Settings.MasterBuildingEnabled = l_MasterBuilding.Value.Equals("true");
                var l_MasterUnit = l_Settings.FirstOrDefault(x => x.Attribute("name").Value == "masterunit");
                if (l_MasterUnit != null)
                    Settings.MasterUnitEnabled = l_MasterUnit.Value.Equals("true");
                //Build
                var l_AdvQueue = l_Settings.FirstOrDefault(x => x.Attribute("name").Value == "advqueue");
                if (l_AdvQueue != null)
                    Settings.AdvancedQueue = l_AdvQueue.Value.Equals("true");
                var l_BuildFarmBelow = l_Settings.FirstOrDefault(x => x.Attribute("name").Value == "buildfarmbelow");
                if (l_BuildFarmBelow != null)
                    Settings.BuildFarmBelow = int.Parse(l_BuildFarmBelow.Value);
                //Reconnect
                var l_MinReconnectDelay = l_Settings.FirstOrDefault(x => x.Attribute("name").Value == "minrecdelay");
                if (l_MinReconnectDelay != null)
                    Settings.MinReconnectDelay = int.Parse(l_MinReconnectDelay.Value);
                var l_MaxReconnectDelay = l_Settings.FirstOrDefault(x => x.Attribute("name").Value == "maxrecdelay");
                if (l_MaxReconnectDelay != null)
                    Settings.MinReconnectDelay = int.Parse(l_MinReconnectDelay.Value);
                //UnitQueue
                var l_QueueLimit = l_Settings.FirstOrDefault(x => x.Attribute("name").Value == "queuelimit");
                if (l_QueueLimit != null)
                    Settings.QueueLimit = int.Parse(l_QueueLimit.Value);
                var l_MinUnitQueuePop = l_Settings.FirstOrDefault(x => x.Attribute("name").Value == "minunitqueuepop");
                if (l_MinUnitQueuePop != null)
                    Settings.MinUnitQueuePop = int.Parse(l_MinUnitQueuePop.Value);
                var l_SkipUnitQueuePop = l_Settings.FirstOrDefault(x => x.Attribute("name").Value == "skipunitqueuepop");
                if (l_SkipUnitQueuePop != null)
                    Settings.SkipUnitQueuePop = int.Parse(l_SkipUnitQueuePop.Value);
                //Others
                var l_AutoStart = l_Settings.FirstOrDefault(x => x.Attribute("name").Value == "autostart");
                if (l_AutoStart != null)
                    Settings.AutoStart = l_AutoStart.Value.Equals("true");
                var l_AdvUserAgent = l_Settings.FirstOrDefault(x => x.Attribute("name").Value == "advuseragent");
                if(l_AdvUserAgent != null)
                    Settings.AdvUserAgent = l_AdvUserAgent.Value;
                var l_GuiLogSize = l_Settings.FirstOrDefault(x => x.Attribute("name").Value == "guilogsize");
                if (l_GuiLogSize != null)
                    Settings.GUILogSize = int.Parse(l_GuiLogSize.Value);
            }
            catch (Exception ex)
            {
                Logger.WriteExceptionToLog(ex.Message);
            }
        }

        /// <summary>
        /// Save program settings to xml file.
        /// </summary>
        public static void SaveSettingsToXml()
        {
            try
            {
                //create XML
                var l_SettingsElement = new XElement("Settings",
                                                        //Grepolis
                                                        new XElement("setting", new XAttribute("name", "grepusername"), Settings.GrepolisUsername),
                                                        new XElement("setting", new XAttribute("name", "grepmainserv"), Settings.GrepolisMainServer),
                                                        new XElement("setting", new XAttribute("name", "grepworld"), Settings.GrepolisWorld),
                                                        new XElement("setting", new XAttribute("name", "heroworld"), Settings.HeroWorld),
                                                        //Delays
                                                        new XElement("setting", new XAttribute("name", "minreqdelay"), Settings.MinRequestDelay),
                                                        new XElement("setting", new XAttribute("name", "maxreqdelay"), Settings.MaxRequestDelay),
                                                        new XElement("setting", new XAttribute("name", "minrefdelay"), Settings.MinRefreshDelay),
                                                        new XElement("setting", new XAttribute("name", "maxrefdelay"), Settings.MaxRefreshDelay),
                                                        new XElement("setting", new XAttribute("name", "minfarmdelay"), Settings.MinFarmerDelay),
                                                        new XElement("setting", new XAttribute("name", "maxfarmdelay"), Settings.MaxFarmerDelay),
                                                        new XElement("setting", new XAttribute("name", "mintradedelay"), Settings.MinTradingDelay),
                                                        new XElement("setting", new XAttribute("name", "maxtradedelay"), Settings.MaxTradingDelay),
                                                        new XElement("setting", new XAttribute("name", "minbuilddelay"), Settings.MinBuildDelay),
                                                        new XElement("setting", new XAttribute("name", "maxbuilddelay"), Settings.MaxBuildDelay),
                                                        new XElement("setting", new XAttribute("name", "minunitdelay"), Settings.MinUnitDelay),
                                                        new XElement("setting", new XAttribute("name", "maxunitdelay"), Settings.MaxUnitDelay),
                                                        //Masters
                                                        new XElement("setting", new XAttribute("name", "masterfarming"), Settings.MasterFarmingEnabled),
                                                        new XElement("setting", new XAttribute("name", "mastertrading"), Settings.MasterTradingEnabled),
                                                        new XElement("setting", new XAttribute("name", "masterbuilding"), Settings.MasterBuildingEnabled),
                                                        new XElement("setting", new XAttribute("name", "masterunit"), Settings.MasterUnitEnabled),
                                                        //Build
                                                        new XElement("setting", new XAttribute("name", "advqueue"), Settings.AdvancedQueue),
                                                        new XElement("setting", new XAttribute("name", "buildfarmbelow"), Settings.BuildFarmBelow),
                                                        //Reconnect
                                                        new XElement("setting", new XAttribute("name", "minrecdelay"), Settings.MinReconnectDelay),
                                                        new XElement("setting", new XAttribute("name", "maxrecdelay"), Settings.MaxReconnectDelay),
                                                        new XElement("setting", new XAttribute("name", "maxretrycount"), Settings.MaxRetryCount),
                                                        //UnitQueue
                                                        new XElement("setting", new XAttribute("name", "queuelimit"), Settings.QueueLimit),
                                                        new XElement("setting", new XAttribute("name", "minunitqueuepop"), Settings.MinUnitQueuePop),
                                                        new XElement("setting", new XAttribute("name", "skipunitqueuepop"), Settings.SkipUnitQueuePop),
                                                        //Others
                                                        new XElement("setting", new XAttribute("name", "autostart"), Settings.AutoStart),
                                                        new XElement("setting", new XAttribute("name", "advuseragent"), Settings.AdvUserAgent),
                                                        new XElement("setting", new XAttribute("name", "guilogsize"), Settings.GUILogSize)
                                                        );
                l_SettingsElement.Save(Settings.SettingsFilePath);
            }
            catch (Exception ex)
            {
                Logger.WriteExceptionToLog(ex.Message);
            }
        }

        /// <summary>
        /// Save all town settings to xml.
        /// ALL SETTINGS TO LOAD HAVE TO BE ADDED IN UpdateTownsResponse()!
        /// </summary>
        public static void SaveTownSettingsToXml(List<Town> p_TownList)
        {
            try
            {
                //create XML
                var l_TownsElement = new XElement("towns");

                foreach (var l_Town in p_TownList)
                {
                    var l_TownElement = new XElement("town", 
                                                        new XAttribute("id", l_Town.TownID),
                                                        new XAttribute("priority", l_Town.Priority));

                    var l_FarmElement = new XElement("farming",
                        new XAttribute("lootinterval", l_Town.LootIntervalMinutes),
                        new XAttribute("lootenabled", l_Town.LootEnabled));

                    //Farmers
                    if (l_Town.Farmers.Count > 0)
                    {
                        foreach (var l_Farmer in l_Town.Farmers)
                        {
                            l_FarmElement.Add(new XElement("farm", new XAttribute("id", l_Farmer.ID),
                                                                new XAttribute("enabled", l_Farmer.Enabled)));
                        }
                    }
                    l_TownElement.Add(l_FarmElement);

                    //Trading
                    l_TownElement.Add(new XElement("trading", new XAttribute("enabled", l_Town.TradeEnabled),
                                                            new XElement("mode", l_Town.TradeMode.ToString("G")),
                                                            new XElement("woodremain", l_Town.TradeWoodRemaining),
                                                            new XElement("stoneremain", l_Town.TradeStoneRemaining),
                                                            new XElement("ironremain", l_Town.TradeIronRemaining),
                                                            new XElement("percware", l_Town.TradePercentageWarehouse),
                                                            new XElement("minsend", l_Town.TradeMinSendAmount),
                                                            new XElement("maxdist", l_Town.TradeMaxDistance)
                                                            ));

                    //Culture
                    l_TownElement.Add(new XElement("culture", new XAttribute("enabled", l_Town.CulturalFestivalsEnabled),
                                                    new XElement("party", l_Town.CulturalEvents[0].Enabled),
                                                    new XElement("games", l_Town.CulturalEvents[1].Enabled),
                                                    new XElement("triumph", l_Town.CulturalEvents[2].Enabled),
                                                    new XElement("theater", l_Town.CulturalEvents[3].Enabled)));

                    //Buildings
                    var l_BuildingElement = new XElement("build",
                                                    new XAttribute("buildenabled", l_Town.BuildingQueueEnabled),
                                                    new XAttribute("buildtarget", l_Town.BuildingTargetEnabled),
                                                    new XAttribute("downgradeenabled", l_Town.DowngradeEnabled));
                    foreach (var l_Building in l_Town.Buildings)
                    {
                        l_BuildingElement.Add(new XElement("building",
                                                    new XAttribute("name", l_Building.DevName.ToString("G")),
                                                    new XElement("target", l_Building.TargetLevel)));
                    }
                    var l_BotQueueElement = new XElement("botqueue");
                    foreach (var l_Building in l_Town.BotBuildingQueue)
                    {
                        l_BotQueueElement.Add(new XElement("building",
                                                        new XAttribute("name", l_Building.ToString("G"))));
                    }

                    l_BuildingElement.Add(l_BotQueueElement);
                    l_TownElement.Add(l_BuildingElement);

                    //Unit queue
                    var l_UnitQueueElement = new XElement("unitqueue",
                                                    new XAttribute("enabled", l_Town.UnitQueueEnabled));
                    foreach (var l_ArmyUnit in l_Town.ArmyUnits)
                    {
                        l_UnitQueueElement.Add(new XElement("unit",
                                                        new XAttribute("name", l_ArmyUnit.Name.ToString("G")),
                                                        new XElement("target", l_ArmyUnit.QueueBot)));
                    }
                    l_TownElement.Add(l_UnitQueueElement);
                    

                    //Add finished town element
                    l_TownsElement.Add(l_TownElement);
                }

                l_TownsElement.Save(Settings.TownsFilePath);
            }
            catch (Exception ex)
            {
                Logger.WriteExceptionToLog(ex.Message);
            }
        }

        /// <summary>
        /// Load Settings for one town from xml.
        /// Called in UpdateTownsResponse.
        /// Important: Edit UpdateTownsResponse
        /// </summary>
        public static Town LoadTownSettingsFromXml(string p_ID)
        {
            var l_Town = new Town(p_ID);
            try
            {
                if (!File.Exists(Settings.TownsFilePath))
                    return l_Town;

                var l_TownsXml = XElement.Load(Settings.TownsFilePath);

                if (!l_TownsXml.Elements("town").Any(x => x.Attribute("id").Value == p_ID))
                    return l_Town;

                var l_TownXml = l_TownsXml.Elements("town").Single(x => x.Attribute("id").Value == p_ID);

                var l_Priority = l_TownXml.Attribute("priority");
                if (l_Priority != null)
                    l_Town.Priority = int.Parse(l_Priority.Value);

                //Farming
                var l_FarmingXml = l_TownXml.Element("farming");

                if (l_FarmingXml != null)
                {
                    l_Town.LootIntervalMinutes = int.Parse(l_FarmingXml.Attribute("lootinterval").Value);
                    l_Town.LootEnabled = l_FarmingXml.Attribute("lootenabled").Value.Equals("true");

                    //Single farms
                    var l_FarmXml = l_FarmingXml.Elements("farm");

                    foreach (var l_Farm in l_FarmXml)
                    {
                        var l_Farmer = new Farmer(l_Farm.Attribute("id").Value)
                        {
                            Enabled = l_Farm.Attribute("enabled").Value.Equals("true")
                        };

                        l_Town.Farmers.Add(l_Farmer);
                    }
                }

                //Trading
                var l_TradingXml = l_TownXml.Element("trading");
                if (l_TradingXml != null)
                {
                    l_Town.TradeEnabled = l_TradingXml.Attribute("enabled").Value.Equals("true");
                    l_Town.TradeMode = (TradingModes)Enum.Parse(typeof(TradingModes), l_TradingXml.Element("mode").Value);
                    l_Town.TradeWoodRemaining = int.Parse(l_TradingXml.Element("woodremain").Value);
                    l_Town.TradeStoneRemaining = int.Parse(l_TradingXml.Element("stoneremain").Value);
                    l_Town.TradeIronRemaining = int.Parse(l_TradingXml.Element("ironremain").Value);
                    l_Town.TradePercentageWarehouse = int.Parse(l_TradingXml.Element("percware").Value);
                    l_Town.TradeMinSendAmount = int.Parse(l_TradingXml.Element("minsend").Value);
                    l_Town.TradeMaxDistance = int.Parse(l_TradingXml.Element("maxdist").Value);
                }

                //Culture
                var l_CultureXml = l_TownXml.Element("culture");
                if (l_CultureXml != null)
                {
                    l_Town.CulturalFestivalsEnabled = l_CultureXml.Attribute("enabled").Value.Equals("true");
                    l_Town.CulturalEvents[0].Enabled = l_CultureXml.Element("party").Value.Equals("true");
                    l_Town.CulturalEvents[1].Enabled = l_CultureXml.Element("games").Value.Equals("true");
                    l_Town.CulturalEvents[2].Enabled = l_CultureXml.Element("triumph").Value.Equals("true");
                    l_Town.CulturalEvents[3].Enabled = l_CultureXml.Element("theater").Value.Equals("true");
                }

                //Buildings
                var l_BuildXml = l_TownXml.Element("build");
                if (l_BuildXml != null)
                {
                    l_Town.BuildingQueueEnabled = l_BuildXml.Attribute("buildenabled").Value.Equals("true");
                    l_Town.BuildingTargetEnabled = l_BuildXml.Attribute("buildtarget").Value.Equals("true");
                    l_Town.DowngradeEnabled = l_BuildXml.Attribute("downgradeenabled").Value.Equals("true");

                    //Single buildings
                    var l_BuildingXml = l_BuildXml.Elements("building");

                    foreach (var l_BuildingElement in l_BuildingXml)
                    {
                        var l_Building =
                            l_Town.Buildings.Single(
                                x =>
                                    x.DevName ==
                                    (Buildings)Enum.Parse(typeof(Buildings), l_BuildingElement.Attribute("name").Value));

                        l_Building.TargetLevel = int.Parse(l_BuildingElement.Element("target").Value);
                    }
                    //Bot building queue
                    var l_BotQueueElements = l_BuildXml.Element("botqueue").Elements("building");
                    foreach (var l_BotQueueElement in l_BotQueueElements)
                    {
                        l_Town.BotBuildingQueue.Add((Buildings)Enum.Parse(typeof(Buildings), l_BotQueueElement.Attribute("name").Value));
                    }
                }

                //Unit queue
                var l_UnitQueueXml = l_TownXml.Element("unitqueue");
                if (l_UnitQueueXml != null)
                {
                    l_Town.UnitQueueEnabled = l_UnitQueueXml.Attribute("enabled").Value.Equals("true");

                    //single units
                    var l_UnitXml = l_UnitQueueXml.Elements("unit");
                    foreach (var l_UnitElement in l_UnitXml)
                    {
                        var l_Unit =
                            l_Town.ArmyUnits.Single(
                                x => x.Name == (ArmyUnits) Enum.Parse(typeof(ArmyUnits),
                                         l_UnitElement.Attribute("name").Value));
                        l_Unit.QueueBot = int.Parse(l_UnitElement.Element("target").Value);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.WriteExceptionToLog(ex.Message);
            }

            return l_Town;
        }
    }
}