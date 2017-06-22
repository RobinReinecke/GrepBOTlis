using Bot.Enums;

namespace Bot.Custom
{
    public class Building
    {
        #region Attributes

        private string m_LocalName = "";

        /// <summary>
        /// Translated Name.
        /// </summary>
        public string LocalName
        {
            get
            {
                if (m_LocalName.Length > 0)
                    return m_LocalName;
                else
                    return DevName.ToString("G");
            }
        }

        /// <summary>
        /// Name from Requests.
        /// </summary>
        public Buildings DevName { get; set; }

        /// <summary>
        /// Building Level.
        /// </summary>
        public int Level { get; set; } = 0;

        /// <summary>
        /// Next Building Level.
        /// </summary>
        //public int NextLevel { get; set; } = 0;

        /// <summary>
        /// Level Down Level.
        /// </summary>
        //public int TearDownLevel { get; set; } = 0;

        /// <summary>
        /// Maximal Level.
        /// </summary>
        public int MaxLevel { get; set; } = 0;

        /// <summary>
        /// Is Max Level.
        /// </summary>
        public bool IsMaxLevel { get; set; } = false;

        /// <summary>
        /// Level targeted by Bot.
        /// </summary>
        public int TargetLevel { get; set; } = 0;

        /// <summary>
        /// Is the Building upgradable.
        /// </summary>
        public bool Upgradable { get; set; } = false;

        /// <summary>
        /// Is the Building teardownable.
        /// </summary>
        public bool Teardownable { get; set; } = false;

        /// <summary>
        /// Is the upgrade reduceable?
        /// </summary>
        public bool UpgradeReduceable { get; set; } = false;

        /// <summary>
        /// Max level of Hero Worlds.
        /// </summary>
        public int MaxLevelHero { get; set; } = 0;

        /// <summary>
        /// Returns the "true" max level. Depending if Hero World or not.
        /// </summary>
        public int MaxLevelCombined => Settings.HeroWorld ? MaxLevelHero : MaxLevel;

        /// <summary>
        /// Population needed base.
        /// </summary>
        public double PopBase { get; set; } = 0;

        /// <summary>
        /// Factor the population rises with each level.
        /// </summary>
        public double PopFactor { get; set; } = 0;

        #endregion Attributes

        #region Constructor

        public Building()
        {
        }

        public Building(Buildings p_DevName, int p_MaxLevel, int p_MaxLevelHero, double p_PopBase, double p_PopFactor)
        {
            DevName = p_DevName;
            MaxLevel = p_MaxLevel;
            MaxLevelHero = p_MaxLevelHero;
            PopBase = p_PopBase;
            PopFactor = p_PopFactor;
        }

        #endregion Constructor
    }
}