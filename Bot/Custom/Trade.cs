namespace Bot.Custom
{
    public class Trade
    {
        #region Attributes

        /// <summary>
        /// Origin of Trade.
        /// </summary>
        public string Origin_Town_ID { get; set; } = "";

        /// <summary>
        /// Destination of Trade.
        /// </summary>
        public string Destination_Town_ID { get; set; } = "";

        /// <summary>
        /// ID of Trade.
        /// </summary>
        public string ID { get; set; } = "";

        /// <summary>
        /// Wood traded.
        /// </summary>
        public int Wood { get; set; } = 0;

        /// <summary>
        /// Stone traded.
        /// </summary>
        public int Stone { get; set; } = 0;

        /// <summary>
        /// Orin traded.
        /// </summary>
        public int Iron { get; set; } = 0;

        /// <summary>
        /// In exchange.
        /// </summary>
        public string In_Exchange { get; set; } = "";

        #endregion Attributes

        #region Constructor

        public Trade()
        {
        }

        public Trade(string p_ID)
        {
            ID = p_ID;
        }

        #endregion Constructor
    }
}