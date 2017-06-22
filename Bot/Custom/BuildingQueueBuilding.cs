using Bot.Enums;

namespace Bot.Custom
{
    public class BuildingQueueBuilding
    {
        /// <summary>
        /// Name from Requests.
        /// </summary>
        public Buildings DevName { get; set; }

        /// <summary>
        /// Time the Building queued.
        /// </summary>
        public string CreatedAt { get; set; } = "0";

        /// <summary>
        /// Building completed at.
        /// </summary>
        public string CompletedAt { get; set; } = "0";

        public BuildingQueueBuilding(Buildings p_DevName, string p_CreatedAt, string p_CompletedAt)
        {
            DevName = p_DevName;
            CreatedAt = p_CreatedAt;
            CreatedAt = p_CompletedAt;
        }
    }
}