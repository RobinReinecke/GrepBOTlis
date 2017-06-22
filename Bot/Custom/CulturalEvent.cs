using Bot.Enums;

namespace Bot.Custom
{
    public class CulturalEvent
    {
        #region Attributes

        /// <summary>
        /// Name of event.
        /// </summary>
        public CulturalEvents Name { get; set; }

        /// <summary>
        /// Translated name received from server.
        /// </summary>
        public string NameLocal { get; set; } = "";

        /// <summary>
        /// Checks if already started and if building requirements are OK.
        /// </summary>
        public bool Ready { get; set; } = false;

        /// <summary>
        /// Enough resources to start the event.
        /// </summary>
        public bool EnoughResources { get; set; } = false;

        /// <summary>
        /// Should the bot start this event.
        /// </summary>
        public bool Enabled { get; set; } = false;

        #endregion Attributes

        #region Constructor

        public CulturalEvent(CulturalEvents p_Name)
        {
            Name = p_Name;
        }

        #endregion Constructor
    }
}