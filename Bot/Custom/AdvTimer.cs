using System;
using System.Timers;

namespace Bot.Custom
{
    public class AdvTimer
    {
        #region Attributes

        /// <summary>
        /// Start Time of Timer.
        /// </summary>
        private DateTime m_StartTime = new DateTime();

        /// <summary>
        /// End Time of Timer
        /// </summary>
        private DateTime m_EndTime = new DateTime();

        /// <summary>
        /// Refresh interval of internal Timer.
        /// </summary>
        private double m_RefreshInterval = 1000;//Internal timer

        /// <summary>
        /// Time left when timer paused.
        /// </summary>
        private TimeSpan m_PausedTimeLeft = new TimeSpan(0, 0, 0);

        /// <summary>
        /// Time left till Timer elapsed.
        /// </summary>
        public string TimeLeft
        {
            get
            {
                var l_TimeLeft = m_EndTime.Subtract(DateTime.Now).ToString();
                l_TimeLeft = l_TimeLeft.Substring(0, 8);
                return l_TimeLeft;
            }
        }

        /// <summary>
        /// Timer duration in Minutes.
        /// </summary>
        public double Duration { get; set; } = 1;

        /// <summary>
        /// Internal timer for refresh only.
        /// </summary>
        public Timer InternalTimer { get; set; } = new Timer();

        /// <summary>
        /// Is timer done.
        /// </summary>
        public bool Done
        {
            get
            {
                var l_Ready = false;
                var l_TimeLeft = TimeLeft;
                if (l_TimeLeft.Equals("00:00:00") || l_TimeLeft.Contains("-"))
                    l_Ready = true;
                return l_Ready;
            }
        }

        /// <summary>
        /// Timer ready to start again.
        /// </summary>
        public bool Ready { get; set; } = true;

        /// <summary>
        /// Event handler for TimerDoneEvent.
        /// </summary>
        public delegate void TimerDoneEventHandler(object sender, CustomArgs ca);

        /// <summary>
        /// Event fires when timer done.
        /// </summary>
        public event TimerDoneEventHandler TimerDoneEvent;

        #endregion Attributes

        #region Constructor

        public AdvTimer()
        {
            m_StartTime = DateTime.Now;
            m_EndTime = DateTime.Now;
            initTimer();
            initEvents();
            SetTimer();
        }

        #endregion Constructor

        #region Methods

        #region Inits

        /// <summary>
        /// Init event handlers.
        /// </summary>
        private void initEvents()
        {
            InternalTimer.Elapsed += InternalTimerOnElapsed;
        }

        /// <summary>
        /// Init internal timer.
        /// </summary>
        private void initTimer()
        {
            InternalTimer.Stop();
            InternalTimer.AutoReset = true;
            InternalTimer.Interval = m_RefreshInterval;
        }

        #endregion Inits

        /// <summary>
        /// Set Timer Start and Endtime.
        /// </summary>
        private void SetTimer()
        {
            m_StartTime = DateTime.Now;
            m_EndTime = m_StartTime.AddMinutes(Duration);
        }

        /// <summary>
        /// Start Timer with given duration in minutes.
        /// </summary>
        public void Start()
        {
            SetTimer();
            InternalTimer.Start();
            Ready = false; //after processing the timer this is set to true
        }

        /// <summary>
        /// Pause the timer
        /// </summary>
        public void Pause()
        {
            InternalTimer.Stop();
            m_PausedTimeLeft = m_EndTime.Subtract(DateTime.Now);
        }

        /// <summary>
        /// Stop Timer.
        /// </summary>
        public void Stop()
        {
            InternalTimer.Stop();
        }

        /// <summary>
        /// Resume timer.
        /// </summary>
        public void Resume()
        {
            m_StartTime = DateTime.Now;
            m_EndTime = m_StartTime.Add(m_PausedTimeLeft);
            InternalTimer.Start();
        }

        #endregion Methods

        #region Events

        /// <summary>
        /// Stop internal timer if Time elapsed.
        /// </summary>
        private void InternalTimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            if (Done)
            {
                InternalTimer.Stop();
                TimerDoneEvent?.Invoke(this, new CustomArgs());
            }
        }

        #endregion Events
    }
}