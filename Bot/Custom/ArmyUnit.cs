using Bot.Enums;

namespace Bot.Custom
{
    public class ArmyUnit
    {
        private string m_LocalName = "";

        #region Attributes

        public ArmyUnits Name { get; set; }

        public string LocalName
        {
            get
            {
                //Forces LocalName to return dev name of unit when translation is not available.
                return m_LocalName.Length > 0 ? m_LocalName : Name.ToString("G");
            }
            set { m_LocalName = value; }
        }

        /// <summary>
        /// Free population Unit needs.
        /// </summary>
        public int Population { get; set; } = 0;

        /// <summary>
        /// Capacity of Boats.
        /// </summary>
        public int Capacity { get; set; } = 0;

        /// <summary>
        /// For mysticle Units.
        /// </summary>
        public Gods God { get; set; } = Gods.none;

        /// <summary>
        /// Is this Unit trained in Barracks.
        /// </summary>
        public bool IsFromBarracks { get; set; } = true;

        /// <summary>
        /// Current amount in town.
        /// </summary>
        public int CurrentAmount { get; set; } = 0;

        /// <summary>
        /// Total amount.
        /// </summary>
        public int TotalAmount { get; set; } = 0;

        /// <summary>
        /// Target training amount.
        /// </summary>
        public int QueueBot { get; set; } = 0;

        /// <summary>
        /// Current amount that is beeing trained ingame.
        /// </summary>
        public int QueueGame { get; set; } = 0;

        /// <summary>
        /// How many you can build with the current resources.
        /// </summary>
        public int MaxBuild { get; set; } = 0;

        /// <summary>
        /// Is Researched?
        /// </summary>
        public bool IsResearched { get; set; } = false;

        /// <summary>
        /// Wood Costs.
        /// </summary>
        public int Wood { get; set; } = 0;

        /// <summary>
        /// Stone Costs.
        /// </summary>
        public int Stone { get; set; } = 0;

        /// <summary>
        /// Iron Costs.
        /// </summary>
        public int Iron { get; set; } = 0;

        /// <summary>
        /// Favor Costs.
        /// </summary>
        public int Favor { get; set; } = 0;

        /// <summary>
        /// Temple Level required.
        /// </summary>
        public int TempleLvlReq { get; set; } = 0;

        /// <summary>
        /// Barracks Level required.
        /// </summary>
        public int BarracksLvlReq { get; set; } = 0;

        /// <summary>
        /// Docks Level required.
        /// </summary>
        public int DocksLvlReq { get; set; } = 0;

        #endregion Attributes

        #region Constructor

        public ArmyUnit()
        {
        }

        public ArmyUnit(ArmyUnits p_Name, int p_Population, int p_Capacity, Gods p_God, bool p_IsFromBarracks,
            bool p_IsResearched, int p_Wood, int p_Stone, int p_Iron, int p_Favor, int p_TempleLvlReq, int p_BarracksLvlReq, int p_DocksLvlReq)
        {
            Name = p_Name;
            Population = p_Population;
            Capacity = p_Capacity;
            God = p_God;
            IsFromBarracks = p_IsFromBarracks;
            IsResearched = p_IsResearched;
            Wood = p_Wood;
            Stone = p_Stone;
            Iron = p_Iron;
            Favor = p_Favor;
            TempleLvlReq = p_TempleLvlReq;
            BarracksLvlReq = p_BarracksLvlReq;
            DocksLvlReq = p_DocksLvlReq;
        }

        #endregion Constructor
    }
}