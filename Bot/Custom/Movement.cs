namespace Bot.Custom
{
    public class Movement
    {
        #region Attributes

        /// <summary>
        /// Movement type.
        /// </summary>
        public string Type { get; set; } = "";

        /// <summary>
        /// Movement cancelable or not.
        /// </summary>
        public bool Cancelable { get; set; } = false;

        /// <summary>
        /// Time movement startet.
        /// </summary>
        public string StartedAt { get; set; } = "";

        /// <summary>
        /// Time movement arrives.
        /// </summary>
        public string ArrivalAt { get; set; } = "";

        /// <summary>
        /// Eta Arrival.
        /// </summary>
        public string ArrivalEta { get; set; } = "";

        /// <summary>
        /// Arrival human string.
        /// </summary>
        public string ArrivedHuman { get; set; } = "";

        /// <summary>
        /// Movement ID.
        /// </summary>
        public string ID { get; set; } = "";

        /// <summary>
        /// Is the movement incoming.
        /// </summary>
        public bool Incoming { get; set; } = false;

        /// <summary>
        /// Is the movement an incoming attack.
        /// </summary>
        public bool IncomingAttack { get; set; } = false;

        /// <summary>
        /// Name of the command
        /// </summary>
        public string CommandName { get; set; } = "";

        /// <summary>
        /// Origin town ID.
        /// </summary>
        public string OriginTownID { get; set; } = "";

        /// <summary>
        /// Origin town name.
        /// </summary>
        public string OriginTownName { get; set; } = "";

        #endregion Attributes

        #region Constructor

        public Movement()
        {
        }

        public Movement(string p_Type, bool p_Cancelable, string p_StartedAt, string p_ArrivalAt, string p_Arrival_Eta,
            string p_ArrivedHuman, string p_ID, bool p_Incoming, bool p_IncomingAttack, string p_CommandName, string p_OriginTownID, string p_OriginTownName)
        {
            Type = p_Type;
            Cancelable = p_Cancelable;
            StartedAt = p_StartedAt;
            ArrivalAt = p_ArrivalAt;
            ArrivalEta = p_Arrival_Eta;
            ArrivedHuman = p_ArrivedHuman;
            ID = p_ID;
            Incoming = p_Incoming;
            IncomingAttack = p_IncomingAttack;
            CommandName = p_CommandName;
            OriginTownID = p_OriginTownID;
            OriginTownName = p_OriginTownName;
        }

        #endregion Constructor
    }
}