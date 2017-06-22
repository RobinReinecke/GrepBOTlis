using System;
using System.Collections.Generic;
using System.Linq;

namespace Bot.Custom
{
    public class Player
    {
        #region Attributes

        /// <summary>
        /// Player ID recieved from server.
        /// </summary>
        public string PlayerID { get; set; }

        /// <summary>
        /// Town ID selected after Login.
        /// </summary>
        public string DefaultTownID { get; set; }

        /// <summary>
        /// Commander Premium Status.
        /// </summary>
        public string CommanderActive { get; set; }

        /// <summary>
        /// Curator Premium Status.
        /// </summary>
        public string CuratorActive { get; set; }

        /// <summary>
        /// Captain Premium Status.
        /// </summary>
        public string CaptainActive { get; set; }

        /// <summary>
        /// Priest Premium Status.
        /// </summary>
        public string PriestActive { get; set; }

        /// <summary>
        /// Trader Premium Status.
        /// </summary>
        public string TraderActive { get; set; }

        /// <summary>
        /// Player Gold.
        /// </summary>
        public int Gold { get; set; }

        /// <summary>
        /// Favor Zeus.
        /// </summary>
        public int FavorZeus { get; set; }

        /// <summary>
        /// Favor Production Zeus.
        /// </summary>
        public string FavorZeusProduction { get; set; }

        /// <summary>
        /// Favor Poseidon.
        /// </summary>
        public int FavorPoseidon { get; set; }

        /// <summary>
        /// Favor Production Zeus.
        /// </summary>
        public string FavorPoseidonProduction { get; set; }

        /// <summary>
        /// Favor Hera.
        /// </summary>
        public int FavorHera { get; set; }

        /// <summary>
        /// Favor Production Hera.
        /// </summary>
        public string FavorHeraProduction { get; set; }

        /// <summary>
        /// Favor Athena.
        /// </summary>
        public int FavorAthena { get; set; }

        /// <summary>
        /// Favor Production Athena.
        /// </summary>
        public string FavorAthenaProduction { get; set; }

        /// <summary>
        /// Favor Hades.
        /// </summary>
        public int FavorHades { get; set; }

        /// <summary>
        /// Favor Production Hades.
        /// </summary>
        public string FavorHadesProduction { get; set; }

        /// <summary>
        /// Favor Artemis.
        /// </summary>
        public int FavorArtemis { get; set; }

        /// <summary>
        /// Favor Production Artemis.
        /// </summary>
        public string FavorArtemisProduction { get; set; }

        /// <summary>
        /// Cultural Cities String.
        /// </summary>
        //public string CulturalCitiesStr { get; set; }

        /// <summary>
        /// Cultural Points String.
        /// </summary>
        //public string CulturalPointsStr { get; set; }

        /// <summary>
        /// Cultural Points.
        /// </summary>
        public int CulturalPointsCurrent { get; set; } = 0;

        /// <summary>
        /// Cultural Points needed for next Level.
        /// </summary>
        public int CulturalPointsMax { get; set; } = 0;

        /// <summary>
        /// Culutral Level String from game.
        /// </summary>
        //public string CulturalLevelStr { get; set; }

        /// <summary>
        /// Cultural level.
        /// </summary>
        public int CultureLevel { get; set; } = 0;

        /// <summary>
        /// Kill Points.
        /// </summary>
        public int KillPoints => AttackKillPoints + DefendKillPoints;

        /// <summary>
        /// Attacker Kill Points.
        /// </summary>
        public int AttackKillPoints { get; set; }

        /// <summary>
        /// Defender Kill Points.
        /// </summary>
        public int DefendKillPoints { get; set; }

        /// <summary>
        /// used Kill Points.
        /// </summary>
        public int UsedKillPoints { get; set; }

        /// <summary>
        /// Count of incoming Attacks.
        /// </summary>
        public int IncomingAttacks { get; set; }

        /// <summary>
        /// List of all Towns.
        /// </summary>
        public List<Town> Towns { get; set; } = new List<Town>();

        /// <summary>
        /// List of all Trades.
        /// </summary>
        public List<Trade> Trades { get; set; } = new List<Trade>();

        /// <summary>
        /// List of Notifications.
        /// </summary>
        public List<Notification> Notifications { get; set; } = new List<Notification>();

        /// <summary>
        /// List of unused FarmTownRelations
        /// </summary>
        public List<FarmTownRelation> UnusedFarmTownRelations { get; set; } = new List<FarmTownRelation>();

        #endregion Attributes

        #region Constructor

        public Player()
        {
        }

        #endregion Constructor

        #region Functions

        /// <summary>
        /// Add a Notification to the List.
        /// </summary>
        public void AddNotification(string p_ServerTime, string p_Notify_id, string p_Time, string p_Type, string p_Subject)
        {
            if (Notifications.All(notification => notification.Notify_ID != p_Notify_id))
                Notifications.Add(new Notification(p_ServerTime, p_Notify_id, p_Time, p_Type, p_Subject));
            else
                return;
        }

        /// <summary>
        /// Returns the latest Notification ID (Nlreq).
        /// </summary>
        /// <returns></returns>
        public string GetLatestNotificationID()
        {
            var l_NotificationID = "0";
            if (Notifications.Count > 0)
                l_NotificationID = Notifications.Last().Notify_ID;
            return l_NotificationID;
            //return "0";
        }

        /// <summary>
        /// Resets the Queues of all Units.
        /// </summary>
        public void ResetUnitQueue()
        {
            foreach (var town in Towns)
            {
                foreach (var armyUnit in town.ArmyUnits)
                {
                    armyUnit.QueueGame = 0;
                }
                town.LandUnitQueueSize = 0;
                town.NavyUnitQueueSize = 0;
            }
        }

        /// <summary>
        /// Resets all Trades.
        /// </summary>
        public void ResetTrades()
        {
            Trades.Clear();
        }

        /// <summary>
        /// Resets all Casted Powers on all Towns.
        /// </summary>
        public void ResetCastedPowers()
        {
            foreach (var town in Towns)
            {
                town.CastedPowers.Clear();
            }
        }

        /// <summary>
        /// Resets all Building Queues on all Towns.
        /// </summary>
        public void ResetBuildingQueue()
        {
            foreach (var town in Towns)
            {
                town.IngameBuildingQueue.Clear();
            }
        }

        /// <summary>
        /// Clear Units all all Towns.
        /// </summary>
        public void ResetUnits()
        {
            foreach (var town in Towns)
            {
                foreach (var unit in town.ArmyUnits)
                {
                    unit.TotalAmount = 0;
                    unit.CurrentAmount = 0;
                }
            }
        }

        /// <summary>
        /// Queues from server are in false order.
        /// </summary>
        public void OrderAllIngameQueues()
        {
            foreach (var town in Towns)
            {
                town.IngameBuildingQueue = town.IngameBuildingQueue.OrderBy(x => x.CreatedAt).ToList();
            }
        }

        /// <summary>
        /// Set Count of Unit in specific town.
        /// </summary>
        /// <param name="p_Unit">Unit name</param>
        /// <param name="p_Count">Unit count</param>
        /// <param name="p_HomeTown">HomeTown ID</param>
        /// <param name="p_CurrentTown">CurrentTown ID</param>
        public void SetUnitCount(string p_Unit, string p_Count, string p_HomeTown, string p_CurrentTown)
        {
            if (p_HomeTown.Equals(p_CurrentTown))
            {
                var l_UnitIndex = Towns.Single(x => x.TownID == p_HomeTown).ArmyUnits.FindIndex(a => a.Name.ToString("G") == p_Unit);
                if (l_UnitIndex != -1)//All unknown or ignored units give -1, e.g. militia
                {
                    Towns.Single(x => x.TownID == p_HomeTown).ArmyUnits[l_UnitIndex].CurrentAmount = int.Parse(p_Count);
                    Towns.Single(x => x.TownID == p_HomeTown).ArmyUnits[l_UnitIndex].TotalAmount += int.Parse(p_Count);
                }
            }
            else
            {
                foreach (var town in Towns)
                {
                    if (town.TownID.Equals(p_HomeTown))
                    {
                        var l_UnitIndex = Towns.Single(x => x.TownID == p_HomeTown).ArmyUnits.FindIndex(a => a.Name.ToString("g") == p_Unit);
                        if (l_UnitIndex != -1)//All unknown or ignored units give -1, e.g. militia
                        {
                            Towns.Single(x => x.TownID == p_HomeTown).ArmyUnits[l_UnitIndex].TotalAmount += int.Parse(p_Count);
                        }
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Get towns sorted by distance to passed ID town.
        /// </summary>
        public List<string> GetTownsSortedByDistance(string p_ID)
        {
            var l_Town = Towns.Single(x => x.TownID == p_ID);

            var l_SortedTowns = Towns.Select(town => town.TownID).ToList();

            for (var i = 0; i < Towns.Count; i++)
            {
                var min = i;
                var minDistance = Math.Sqrt(
                    Math.Pow(Math.Abs((int.Parse(l_Town.IslandX) - int.Parse(Towns.Single(x => x.TownID == l_SortedTowns[i]).IslandX))), 2) +
                    Math.Pow(Math.Abs((int.Parse(l_Town.IslandY) - int.Parse(Towns.Single(x => x.TownID == l_SortedTowns[i]).IslandY))), 2));

                int j;
                for (j = i + 1; j < Towns.Count; j++)
                {
                    var curDistance = Math.Sqrt(
                        Math.Pow(Math.Abs((int.Parse(l_Town.IslandX) - int.Parse(Towns.Single(x => x.TownID == l_SortedTowns[j]).IslandX))), 2) +
                        Math.Pow(Math.Abs((int.Parse(l_Town.IslandY) - int.Parse(Towns.Single(x => x.TownID == l_SortedTowns[j]).IslandY))), 2));

                    if (curDistance < minDistance)
                    {
                        min = j;
                        minDistance = curDistance;
                    }
                }
                var l_TownIndex = l_SortedTowns[min];
                l_SortedTowns[min] = l_SortedTowns[i];
                l_SortedTowns[i] = l_TownIndex;
            }
            return l_SortedTowns;
        }

        #endregion Functions
    }
}