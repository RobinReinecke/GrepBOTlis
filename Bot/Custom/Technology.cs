using Bot.Enums;

namespace Bot.Custom
{
    public class Technology
    {
        /// <summary>
        /// ID of Technology
        /// </summary>
        public Technologies ID { get; set; }

        /// <summary>
        /// Is researched.
        /// </summary>
        public bool Researched { get; set; } = false;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="p_Technology">Witch Technology the object represents.</param>
        public Technology(Enums.Technologies p_Technology)
        {
            ID = p_Technology;
        }
    }
}