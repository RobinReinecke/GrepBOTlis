using System;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using Bot.Properties;

namespace Bot.Helpers
{
    public static class Logger
    {
        public static void WriteExceptionToLog(string p_Message, [CallerMemberName]string p_CallerName = "")
        {
            try
            {
                using (var l_Writer = new StreamWriter(Settings.LogFilePath, true))
                    l_Writer.WriteLine(DateTime.Now.ToString(CultureInfo.InvariantCulture) + ": Exception in " + p_CallerName + ": " + p_Message);
            }
            catch (Exception ex)
            {
                WriteExceptionToLog(ex.Message);
            }
        }

        public static void WriteToLog(string p_Message)
        {
            try
            {
                using (var l_Writer = new StreamWriter(Settings.LogFilePath, true))
                    l_Writer.WriteLine(DateTime.Now.ToString(CultureInfo.InvariantCulture) + ": " + p_Message);
            }
            catch (Exception ex)
            {
                WriteExceptionToLog(ex.Message);
            }
        }
    }
}