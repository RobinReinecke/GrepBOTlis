namespace Bot.Custom
{
    public class FarmTownRelation
    {
        /// <summary>
        /// Relation ID.
        /// </summary>
        public string ID { get; set; } = "";

        /// <summary>
        /// ID of farmer.
        /// </summary>
        public string FarmerID { get; set; } = "";

        /// <summary>
        /// Already looted resources.
        /// </summary>
        public int LootedResources { get; set; } = 0;

        /// <summary>
        /// ExpansionState of farmer
        /// </summary>
        public int ExpansionState { get; set; } = 0;

        public FarmTownRelation(string p_ID, string p_FarmerID, int p_LootedResources, int p_ExpansionState)
        {
            ID = p_ID;
            FarmerID = p_FarmerID;
            LootedResources = p_LootedResources;
            ExpansionState = p_ExpansionState;
        }
    }
}