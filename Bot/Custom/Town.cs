using System;
using System.Collections.Generic;
using Bot.Enums;

namespace Bot.Custom
{
    public class Town
    {
        #region Attributes

        /// <summary>
        /// List of Army Units in Town.
        /// </summary>
        public List<ArmyUnit> ArmyUnits { get; set; } = new List<ArmyUnit>();

        /// <summary>
        /// Class with List of all Technologys..
        /// </summary>
        public Research Research { get; set; } = new Research();

        /// <summary>
        /// StringList of casted Powers on a town.
        /// </summary>
        public List<string> CastedPowers { get; set; } = new List<string>();

        /// <summary>
        /// List of Farmers.
        /// </summary>
        public List<Farmer> Farmers { get; set; } = new List<Farmer>();

        /// <summary>
        /// Town ID.
        /// </summary>
        public string TownID { get; set; } = "";

        /// <summary>
        /// Building Queue.
        /// </summary>
        public List<BuildingQueueBuilding> IngameBuildingQueue { get; set; } = new List<BuildingQueueBuilding>();

        /// <summary>
        /// Bot building queue.
        /// </summary>
        public List<Buildings> BotBuildingQueue { get; set; } = new List<Buildings>();

        /// <summary>
        /// List of all Movements of a town.
        /// </summary>
        public List<Movement> Movements { get; set; } = new List<Movement>();

        /// <summary>
        /// Unit queue enabled or not.
        /// </summary>
        public bool UnitQueueEnabled { get; set; } = false;

        /// <summary>
        /// Size of Land Unit Queue.
        /// </summary>
        public int LandUnitQueueSize { get; set; } = 0;

        /// <summary>
        /// Size of Navy Unit Queue.
        /// </summary>
        public int NavyUnitQueueSize { get; set; } = 0;

        /// <summary>
        /// Same Server time like in Controller.
        /// </summary>
        public string ServerTime { get; set; } = "";

        /// <summary>
        /// List of all Building of a Town.
        /// </summary>
        public List<Building> Buildings { get; set; } = new List<Building>();

        /// <summary>
        /// Is Building Queue full or not.
        /// </summary>
        public bool IsBuildingOrderQueueFull { get; set; } = false;

        /// <summary>
        /// Should the bot build buildings.
        /// </summary>
        public bool BuildingQueueEnabled { get; set; } = false;

        /// <summary>
        /// Is building target level mode enabled or use bot queue?
        /// </summary>
        public bool BuildingTargetEnabled { get; set; } = false;

        /// <summary>
        /// Should the bot grade buildings down?
        /// </summary>
        public bool DowngradeEnabled { get; set; } = false;

        /// <summary>
        /// Town Name.
        /// </summary>
        public string TownName { get; set; } = "";

        /// <summary>
        /// Island X Coordinate.
        /// </summary>
        public string IslandX { get; set; } = "000";

        /// <summary>
        /// Chunk X.
        /// </summary>
        public string ChunkX => ((int)(int.Parse(IslandX) / 20)).ToString();

        /// <summary>
        /// Island Y Coordinate.
        /// </summary>
        public string IslandY { get; set; } = "000";

        /// <summary>
        /// Chunk Y.
        /// </summary>
        public string ChunkY => ((int)(int.Parse(IslandY) / 20)).ToString();

        /// <summary>
        /// Last time Resources recieved from server.
        /// </summary>
        public string ResourcesLastUpdate { get; set; } = "";

        /// <summary>
        /// Rare Resource of Island.
        /// </summary>
        public Resources ResourceRare { get; set; }

        /// <summary>
        /// Plenty Resource of Island.
        /// </summary>
        public Resources ResourcePlenty { get; set; }

        /// <summary>
        /// God of town.
        /// </summary>
        public Gods God { get; set; } = Gods.none;

        /// <summary>
        /// Points of a town.
        /// </summary>
        public int Points { get; set; } = 0;

        /// <summary>
        /// Iron in Espionage Storage.
        /// </summary>
        public int EspionageStorage { get; set; } = 0;

        /// <summary>
        /// Extra Population.
        /// </summary>
        public int PopulationExtra { get; set; } = 0;

        /// <summary>
        /// Free Population.
        /// </summary>
        public int PopulationAvailable { get; set; } = 0;

        /// <summary>
        /// Has the town a conqueror.
        /// </summary>
        public bool HasConqueror { get; set; } = false;

        /// <summary>
        /// Wood in storage.
        /// </summary>
        public int Wood { get; set; } = 0;

        /// <summary>
        /// Stone in storage.
        /// </summary>
        public int Stone { get; set; } = 0;

        /// <summary>
        /// Iron in storage.
        /// </summary>
        public int Iron { get; set; } = 0;

        /// <summary>
        /// Maximum of each resource fits into storage.
        /// </summary>
        public int Storage { get; set; } = 0;

        /// <summary>
        /// Wood Production.
        /// </summary>
        public int WoodProduction { get; set; } = 0;

        /// <summary>
        /// Stone Production.
        /// </summary>
        public int StoneProduction { get; set; } = 0;

        /// <summary>
        /// Iron Production.
        /// </summary>
        public int IronProduction { get; set; } = 0;

        /// <summary>
        /// Count of available Quests for this town.
        /// </summary>
        public int AvailableQuests { get; set; } = 0;

        /// <summary>
        /// Is space in storage or not.
        /// </summary>
        public bool StorageFull => (Storage == Wood && Wood == Stone && Stone == Iron && Storage != 0);

        /// <summary>
        /// Loot Interval of town for farmers.
        /// </summary>
        public int LootIntervalMinutes { get; set; } = 5;

        /// <summary>
        /// Loot interval for requests.
        /// </summary>
        public string LootIntervalOption
        {
            get
            {
                switch (LootIntervalMinutes)
                {
                    case 5:
                        return "1";

                    case 10:
                        return Research.Technologies[7].Researched ? "2" : "1";

                    case 20:
                        return Research.Technologies[7].Researched ? "3" : "2";

                    case 40:
                        return Research.Technologies[7].Researched ? "4" : "2";

                    case 90:
                        return Research.Technologies[7].Researched ? "5" : "3";

                    case 180:
                        return Research.Technologies[7].Researched ? "6" : "3";

                    case 240:
                        return Research.Technologies[7].Researched ? "7" : "4";

                    case 480:
                        return Research.Technologies[7].Researched ? "8" : "4";

                    default:
                        return "1";
                }
            }
        }

        /// <summary>
        /// Loot enabled or not.
        /// </summary>
        public bool LootEnabled { get; set; } = false;

        /// <summary>
        /// Used for trading. True if town dont need more resources.
        /// </summary>
        public bool HasEnoughResources { get; set; } = false;

        /// <summary>
        /// Trade Mode.
        /// </summary>
        public TradingModes TradeMode { get; set; } = TradingModes.Receive;

        /// <summary>
        /// Trade enabled or not.
        /// </summary>
        public bool TradeEnabled { get; set; } = false;

        /// <summary>
        /// Maximal trade distance between towns.
        /// </summary>
        public int TradeMaxDistance { get; set; } = 100;

        /// <summary>
        /// max Trade Capacity.
        /// </summary>
        public int MaxTradeCapacity { get; set; } = 0;

        /// <summary>
        /// Free Trade Capacity.
        /// </summary>
        public int FreeTradeCapacity { get; set; } = 0;

        /// <summary>
        /// Incoming wood by trade
        /// </summary>
        public int WoodInc { get; set; } = 0;

        /// <summary>
        /// Incoming stone by trade.
        /// </summary>
        public int StoneInc { get; set; } = 0;

        /// <summary>
        /// Incoming iron by trade.
        /// </summary>
        public int IronInc { get; set; } = 0;

        /// <summary>
        /// For sender. How many wood you want to keep for yourself.
        /// </summary>
        public int TradeWoodRemaining { get; set; } = 15000;

        /// <summary>
        /// For sender. How many stone you want to keep for yourself.
        /// </summary>
        public int TradeStoneRemaining { get; set; } = 18000;

        /// <summary>
        /// For sender. How many iron you want to keep for yourself.
        /// </summary>
        public int TradeIronRemaining { get; set; } = 15000;

        /// <summary>
        /// For sender. What the minimum amout of resources is you want to send.
        /// </summary>
        public int TradeMinSendAmount { get; set; } = 500;

        /// <summary>
        /// For receiver. How far you want to fill the warehouse in percent.
        /// </summary>
        public int TradePercentageWarehouse { get; set; } = 95;

        /// <summary>
        /// List of cultural events for the town.
        /// </summary>
        public List<CulturalEvent> CulturalEvents { get; set; } = new List<CulturalEvent>();

        /// <summary>
        /// Should the bot start cultural festivals in this city
        /// </summary>
        public bool CulturalFestivalsEnabled { get; set; } = false;

        /// <summary>
        /// Priority of this town (how often the bot should check it).
        /// </summary>
        public int Priority { get; set; } = 1;

        #endregion Attributes

        #region Constructor

        public Town()
        {
        }

        public Town(string p_ID)
        {
            TownID = p_ID;
            AddArmyUnits();
            AddBuildings();
            AddCulturalEvents();
        }

        #endregion Constructor

        #region Functions

        /// <summary>
        /// Fill the Building List with all Buildings.
        /// </summary>
        private void AddBuildings()
        {
            Buildings.Add(new Building(Enums.Buildings.main, 25, 25, 1, 1.5));
            Buildings.Add(new Building(Enums.Buildings.hide, 10, 10, 3, 0.5));
            Buildings.Add(new Building(Enums.Buildings.storage, 30, 35, 0, 1));
            Buildings.Add(new Building(Enums.Buildings.farm, 40, 45, 0, 0));
            Buildings.Add(new Building(Enums.Buildings.lumber, 40, 40, 1, 1.25));
            Buildings.Add(new Building(Enums.Buildings.stoner, 40, 40, 1, 1.25));
            Buildings.Add(new Building(Enums.Buildings.ironer, 40, 40, 1, 1.25));
            Buildings.Add(new Building(Enums.Buildings.market, 30, 40, 2, 1.1));
            Buildings.Add(new Building(Enums.Buildings.docks, 30, 30, 4, 1));//8
            Buildings.Add(new Building(Enums.Buildings.barracks, 30, 30, 1, 1.3));//9
            Buildings.Add(new Building(Enums.Buildings.wall, 25, 25, 2, 1.16));
            Buildings.Add(new Building(Enums.Buildings.academy, 30, 36, 3, 1));
            Buildings.Add(new Building(Enums.Buildings.temple, 25, 30, 5, 1));//12
            Buildings.Add(new Building(Enums.Buildings.theater, 1, 1, 60, 1));
            Buildings.Add(new Building(Enums.Buildings.thermal, 1, 1, 60, 1));
            Buildings.Add(new Building(Enums.Buildings.library, 1, 1, 60, 1));
            Buildings.Add(new Building(Enums.Buildings.lighthouse, 1, 1, 60, 1));
            Buildings.Add(new Building(Enums.Buildings.tower, 1, 1, 60, 1));
            Buildings.Add(new Building(Enums.Buildings.statue, 1, 1, 60, 1));
            Buildings.Add(new Building(Enums.Buildings.oracle, 1, 1, 60, 1));
            Buildings.Add(new Building(Enums.Buildings.trade_office, 1, 1, 60, 1));
        }

        /// <summary>
        /// Fill the ArmyUnits List with possible Units.
        /// </summary>
        private void AddArmyUnits()
        {
            ArmyUnits.Add(new ArmyUnit(Enums.ArmyUnits.sword, 1, 0, Gods.none, true, true, 95, 0, 85, 0, 0, 1, 0));
            ArmyUnits.Add(new ArmyUnit(Enums.ArmyUnits.slinger, 1, 0, Gods.none, true, false, 55, 100, 40, 0, 0, 1, 0));
            ArmyUnits.Add(new ArmyUnit(Enums.ArmyUnits.archer, 1, 0, Gods.none, true, false, 120, 0, 75, 0, 0, 1, 0));
            ArmyUnits.Add(new ArmyUnit(Enums.ArmyUnits.hoplite, 1, 0, Gods.none, true, false, 0, 75, 150, 0, 0, 1, 0));
            ArmyUnits.Add(new ArmyUnit(Enums.ArmyUnits.rider, 3, 0, Gods.none, true, false, 240, 120, 360, 0, 0, 10, 0));
            ArmyUnits.Add(new ArmyUnit(Enums.ArmyUnits.chariot, 4, 0, Gods.none, true, false, 200, 440, 320, 0, 0, 15, 0));
            ArmyUnits.Add(new ArmyUnit(Enums.ArmyUnits.catapult, 15, 0, Gods.none, true, false, 1200, 1200, 1200, 0, 0, 1, 0));
            ArmyUnits.Add(new ArmyUnit(Enums.ArmyUnits.minotaur, 30, 0, Gods.zeus, true, true, 1400, 600, 3100, 202, 10, 1, 0));
            ArmyUnits.Add(new ArmyUnit(Enums.ArmyUnits.manticore, 45, 0, Gods.zeus, true, true, 4400, 3000, 3400, 405, 15, 1, 0));
            ArmyUnits.Add(new ArmyUnit(Enums.ArmyUnits.centaur, 12, 0, Gods.athena, true, true, 1740, 300, 700, 100, 4, 1, 0));
            ArmyUnits.Add(new ArmyUnit(Enums.ArmyUnits.pegasus, 20, 0, Gods.athena, true, true, 2800, 360, 80, 180, 12, 1, 0));
            ArmyUnits.Add(new ArmyUnit(Enums.ArmyUnits.harpy, 14, 0, Gods.hera, true, true, 1600, 400, 1360, 130, 5, 1, 0));
            ArmyUnits.Add(new ArmyUnit(Enums.ArmyUnits.medusa, 18, 0, Gods.hera, true, true, 1500, 3800, 2200, 210, 10, 1, 0));
            ArmyUnits.Add(new ArmyUnit(Enums.ArmyUnits.zyklop, 40, 0, Gods.poseidon, true, true, 2000, 4200, 3360, 360, 12, 1, 0));
            ArmyUnits.Add(new ArmyUnit(Enums.ArmyUnits.cerberus, 30, 0, Gods.hades, true, true, 1250, 1500, 3000, 320, 10, 1, 0));
            ArmyUnits.Add(new ArmyUnit(Enums.ArmyUnits.fury, 55, 0, Gods.hades, true, true, 2500, 5000, 5000, 480, 16, 1, 0));
            ArmyUnits.Add(new ArmyUnit(Enums.ArmyUnits.griffin, 38, 0, Gods.artemis, true, true, 3800, 1900, 4800, 250, 15, 1, 0));
            ArmyUnits.Add(new ArmyUnit(Enums.ArmyUnits.calydonian_boar, 20, 0, Gods.artemis, true, true, 2800, 1400, 1600, 110, 10, 1, 0));
            ArmyUnits.Add(new ArmyUnit(Enums.ArmyUnits.godsent, 3, 0, Gods.none, true, true, 0, 0, 0, 15, 1, 1, 0));
            ArmyUnits.Add(new ArmyUnit(Enums.ArmyUnits.big_transporter, 7, 26, Gods.none, false, true, 500, 500, 400, 0, 0, 0, 1));//19
            ArmyUnits.Add(new ArmyUnit(Enums.ArmyUnits.bireme, 8, 0, Gods.none, false, false, 800, 700, 180, 0, 0, 0, 1));
            ArmyUnits.Add(new ArmyUnit(Enums.ArmyUnits.attack_ship, 10, 0, Gods.none, false, false, 1300, 300, 800, 0, 0, 0, 1));
            ArmyUnits.Add(new ArmyUnit(Enums.ArmyUnits.demolition_ship, 8, 0, Gods.none, false, false, 500, 750, 150, 0, 0, 0, 1));
            ArmyUnits.Add(new ArmyUnit(Enums.ArmyUnits.small_transporter, 5, 10, Gods.none, false, false, 800, 0, 400, 0, 0, 0, 1));
            ArmyUnits.Add(new ArmyUnit(Enums.ArmyUnits.trireme, 16, 0, Gods.none, false, false, 2000, 1300, 900, 0, 0, 0, 1));
            ArmyUnits.Add(new ArmyUnit(Enums.ArmyUnits.colonize_ship, 170, 0, Gods.none, false, false, 10000, 10000, 10000, 0, 0, 0, 20));
            ArmyUnits.Add(new ArmyUnit(Enums.ArmyUnits.sea_monster, 50, 0, Gods.poseidon, false, true, 5400, 2800, 3800, 400, 22, 0, 1));
        }

        /// <summary>
        /// Full the List of Cultural Events.
        /// </summary>
        private void AddCulturalEvents()
        {
            CulturalEvents.Add(new CulturalEvent(Enums.CulturalEvents.party));
            CulturalEvents.Add(new CulturalEvent(Enums.CulturalEvents.games));
            CulturalEvents.Add(new CulturalEvent(Enums.CulturalEvents.triumph));
            CulturalEvents.Add(new CulturalEvent(Enums.CulturalEvents.theater));
        }

        /// <summary>
        /// Get distance to passed Island
        /// </summary>
        public double GetDistance(string l_IslandX, string l_IslandY)
        {
            var l_Distance = Math.Sqrt(
                    Math.Pow(Math.Abs(int.Parse(l_IslandX) - int.Parse(IslandX)), 2) +
                    Math.Pow(Math.Abs(int.Parse(l_IslandY) - int.Parse(IslandY)), 2));
            return l_Distance;
        }

        #endregion Functions
    }
}