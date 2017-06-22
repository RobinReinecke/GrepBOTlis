using System;
using Bot.Helpers;

namespace Bot.Custom
{
    public class Farmer
    {
        #region Attributes

        /// <summary>
        /// Unique farmer ID.
        /// </summary>
        public string ID { get; set; }

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
        /// Name of farmer.
        /// </summary>
        public string Name { get; set; } = "";

        /// <summary>
        /// Expansion Stage of farmer.
        /// </summary>
        public int ExpansionState { get; set; } = -1;

        /// <summary>
        /// Mood.
        /// </summary>
        public int Mood { get; set; } = -1;

        /// <summary>
        /// Relation Status.
        /// </summary>
        public bool RelationStatus { get; set; } = false;

        /// <summary>
        /// When you are able to loot again.
        /// </summary>
        public string LootTimer { get; set; } = "0";

        /// <summary>
        /// Shows in understandable words when you're able to loot again
        /// </summary>
        public string LootTimerHuman { get; set; } = "";

        /// <summary>
        /// ID from relation.
        /// </summary>
        public string BattlePointsFarmID { get; set; } = "";

        /// <summary>
        /// Already lotted Resources.
        /// </summary>
        public int LootedResources { get; set; } = 0;

        /// <summary>
        /// Claimable Resource values.
        /// </summary>
        public string[] ClaimResourceValues { get; set; }

        /// <summary>
        /// Is the Farmer lootable or not.
        /// </summary>
        public bool Lootable => RelationStatus && Parser.UnixToHumanTime(LootTimer) < DateTime.Now;

        /// <summary>
        /// Should the bot loot the farm.
        /// </summary>
        public bool Enabled { get; set; } = false;

        /// <summary>
        /// Limit to farm.
        /// </summary>
        public int Limit
        {
            get
            {
                var l_Base = 0;
                switch (ExpansionState)
                {
                    case 1:
                        l_Base = 2500;
                        break;

                    case 2:
                        l_Base = 3000;
                        break;

                    case 3:
                        l_Base = 3500;
                        break;

                    case 4:
                        l_Base = 4000;
                        break;

                    case 5:
                        l_Base = 4500;
                        break;

                    case 6:
                        l_Base = 5000;
                        break;
                }
                return (int)(l_Base * double.Parse(Settings.GameSpeed));
            }
        }

        #endregion Attributes

        #region Constructor

        public Farmer(string p_ID)
        {
            ID = p_ID;
        }

        public Farmer(string p_ID, string p_Name, int p_ExpansionState, string p_IslandX, string p_IslandY, int p_Mood, bool p_RelationStatus, string p_LootTimer, string p_LootTimerHuman)
        {
            ID = p_ID;
            Name = p_Name;
            ExpansionState = p_ExpansionState;
            IslandX = p_IslandX;
            IslandY = p_IslandY;
            Mood = p_Mood;
            RelationStatus = p_RelationStatus;
            LootTimer = p_LootTimer;
            LootTimerHuman = p_LootTimerHuman;
        }

        #endregion Constructor
    }
}