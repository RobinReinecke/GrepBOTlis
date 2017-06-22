using System;

namespace Bot.Helpers
{
    public static class Randomizer
    {
        private static readonly Random m_Random = new Random();

        public static double RandomizeDouble(int p_Interval)
        {
            var l_Random = new Random();
            //l_Quotient = l_Random.Next(-10, 10);
            double l_Quotient = l_Random.Next(0, 20);
            l_Quotient = 100 + l_Quotient;//Value between 90% and 110%;
            l_Quotient = (l_Quotient / 100.0);//Value between 0.9 and 1.1
            return p_Interval * l_Quotient;
        }

        /// <summary>
        /// Randomize the RequestTimer Intervall.
        /// </summary>
        public static int RandomizeRequestTimer()
        {
            return m_Random.Next(Settings.MinRequestDelay, Settings.MaxRequestDelay);
        }

        /// <summary>
        /// Randomize the RefreshTimer Intervall.
        /// </summary>
        public static double RandomizeRefreshTimer()
        {
            return m_Random.Next(Settings.MinRefreshDelay, Settings.MaxRefreshDelay) * ((m_Random.Next(0, 20) + 100) / 100.0);
        }

        /// <summary>
        /// Randomize the FarmerTimer Intervall.
        /// </summary>
        public static double RandomizeFarmerTimer()
        {
            return m_Random.Next(Settings.MinFarmerDelay, Settings.MaxFarmerDelay) * ((m_Random.Next(0, 20) + 100) / 100.0);
        }

        /// <summary>
        /// Randomize the TradingTimer Intervall.
        /// </summary>
        public static double RandomizeTradingTimer()
        {
            return m_Random.Next(Settings.MinTradingDelay, Settings.MaxTradingDelay) * ((m_Random.Next(0, 20) + 100) / 100.0);
        }

        /// <summary>
        /// Randomize the ReconnectTimer Intervall.
        /// </summary>
        /// <returns></returns>
        public static double RandomizeReconnectTimer()
        {
            return m_Random.Next(Settings.MinReconnectDelay, Settings.MaxReconnectDelay) * ((m_Random.Next(0, 20) + 100) / 100.0);
        }

        /// <summary>
        /// Randomize the BuildTimer Intervall.
        /// </summary>
        /// <returns></returns>
        public static double RandomizeBuildTimer()
        {
            return m_Random.Next(Settings.MinBuildDelay, Settings.MaxBuildDelay) * ((m_Random.Next(0, 20) + 100) / 100.0);
        }

        /// <summary>
        /// Randomize the UnitTimer Intervall.
        /// </summary>
        /// <returns></returns>
        public static double RandomizeUnitTimer()
        {
            return m_Random.Next(Settings.MinUnitDelay, Settings.MaxUnitDelay) * ((m_Random.Next(0, 20) + 100) / 100.0);
        }
    }
}