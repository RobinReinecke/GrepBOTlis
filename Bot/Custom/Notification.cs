namespace Bot.Custom
{
    public class Notification
    {
        #region Attributes

        /// <summary>
        /// Server Time.
        /// </summary>
        public string ServerTime { get; set; } = "";

        /// <summary>
        /// Notify ID.
        /// </summary>
        public string Notify_ID { get; set; } = "";

        /// <summary>
        /// Time recieved.
        /// </summary>
        public string Time { get; set; } = "";

        /// <summary>
        /// Type of Notification.
        /// </summary>
        public string Type { get; set; } = "";

        /// <summary>
        /// Notification Subject.
        /// </summary>
        public string Subject { get; set; } = "";

        #endregion Attributes

        #region Constructor

        public Notification()
        {
        }

        public Notification(string p_ServerTime, string p_Notify_id, string p_Time, string p_Type, string p_Subject)
        {
            ServerTime = p_ServerTime;
            Notify_ID = p_Notify_id;
            Time = p_Time;
            Type = p_Type;
            Subject = p_Subject;
        }

        #endregion Constructor
    }
}