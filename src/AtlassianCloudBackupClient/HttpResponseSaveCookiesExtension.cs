using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace AtlassianCloudBackupClient
{
    public static class HttpResponseSaveCookiesExtension
    {
        /// <remarks>https://stackoverflow.com/questions/14681144/httpclient-not-storing-cookies-in-cookiecontainer</remarks>
        public static CookieContainer ReadCookies(this HttpResponseMessage response)
        {
            var pageUri = response.RequestMessage.RequestUri;

            var cookieContainer = new CookieContainer();
            IEnumerable<string> cookies;
            if (response.Headers.TryGetValues("set-cookie", out cookies))
            {
                foreach (var c in cookies)
                {
                    cookieContainer.SetCookies(pageUri, c);
                }
            }

            return cookieContainer;
        }
    }
}