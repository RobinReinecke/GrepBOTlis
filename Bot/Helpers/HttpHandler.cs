using System;
using System.Net;

namespace Bot.Helpers
{
    public class HttpHandler : WebClient
    {
        protected override WebRequest GetWebRequest(Uri address)
        {
            var request = base.GetWebRequest(address);
            if (request is HttpWebRequest)
            {
                (request as HttpWebRequest).CookieContainer = CookieContainer;
                //The underlying connection was closed: A connection that was expected to be kept alive was closed by the server.
                (request as HttpWebRequest).KeepAlive = false;
                (request as HttpWebRequest).UserAgent = Settings.AdvUserAgent;
            }

            //The underlying connection was closed: The connection was closed unexpectedly.
            //request.ConnectionGroupName = Guid.NewGuid().ToString();
            return request;
        }

        public CookieContainer CookieContainer { get; set; } = new CookieContainer();

        public void ClearCookies()
        {
            CookieContainer = new CookieContainer();
        }
    }
}