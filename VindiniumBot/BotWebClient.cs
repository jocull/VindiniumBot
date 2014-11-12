using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace VindiniumBot
{
    /// <summary>
    /// A web request object with expanded timeout settings (30 minutes)
    /// </summary>
    internal class BotWebClient : WebClient
    {
        protected override WebRequest GetWebRequest(Uri address)
        {
            WebRequest w = base.GetWebRequest(address);
            w.Timeout = (int)TimeSpan.FromMinutes(30).TotalMilliseconds;
            return w;
        }
    }
}
