using System;
using System.Windows.Forms;

namespace Bot.Custom
{
    public class CustomArgs : EventArgs
    {
        public CustomArgs()
        {
        }

        public string Message { get; } = "";

        public CustomArgs(string p_Message)
        {
            Message = p_Message;
        }
    }
}