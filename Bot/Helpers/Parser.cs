using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Bot.Helpers
{
    public static class Parser
    {
        #region Functions

        /// <summary>
        /// Fix special characters of responses.
        /// </summary>
        public static string FixSpecialCharacters(string p_String)
        {
            var l_Regex = new Regex(@"\\[uU]([0-9A-F]{4})", RegexOptions.IgnoreCase);
            var l_String = p_String;

            //Fix special characters
            l_String = l_Regex.Replace(l_String, l_Match => ((char)int.Parse(l_Match.Groups[1].Value, NumberStyles.HexNumber)).ToString());
            l_String = l_String.Replace("\\", "");

            return l_String;
        }

        /// <summary>
        /// Convert Unix to Human time.
        /// </summary>
        public static DateTime UnixToHumanTime(string p_Time)
        {
            var l_Offset = double.Parse(Settings.LocaleTimeOffset);

            while (p_Time.Length > 10)
            {
                p_Time = p_Time.Remove(p_Time.Length - 1);
            }
            var l_Seconds = double.Parse(p_Time) + (l_Offset * 1.0);
            return new DateTime(1970, 1, 1, 0, 0, 0).AddSeconds(l_Seconds);
            //return l_Time.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Decryption of a link.
        /// </summary>
        public static string DecryptLink(string p_Link)
        {
            try
            {
                const string l_Key_str = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/=";
                var l_Output = "";
                var i = 0;
                var l_Len = p_Link.Length;
                while (i < l_Len)
                {
                    var l_Enc1 = l_Key_str.IndexOf(p_Link[i++]);
                    var l_Enc2 = l_Key_str.IndexOf(p_Link[i++]);
                    var l_Enc3 = l_Key_str.IndexOf(p_Link[i++]);
                    var l_Enc4 = l_Key_str.IndexOf(p_Link[i++]);
                    var l_Chr1 = (l_Enc1 << 2) | (l_Enc2 >> 4);
                    var l_Chr2 = ((l_Enc2 & 15) << 4) | (l_Enc3 >> 2);
                    var l_Chr3 = ((l_Enc3 & 3) << 6) | l_Enc4;
                    l_Output += Convert.ToChar(l_Chr1);
                    if (l_Enc3 != 64)
                    {
                        l_Output = l_Output + Convert.ToChar(l_Chr2);
                    }
                    if (l_Enc4 != 64)
                    {
                        l_Output = l_Output + Convert.ToChar(l_Chr3);
                    }
                }
                return l_Output;
            }
            catch (Exception ex)
            {
                Logger.WriteExceptionToLog(ex.Message + " " + p_Link);
                return "false";
            }
        }

        #endregion Functions
    }
}