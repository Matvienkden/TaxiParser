using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace TaxiParser.Core.Extensions
{
    public static class WebResponseExtensions
    {
        public static string GetStrFromResponseStream(this WebResponse response)
            => new StreamReader(response.GetResponseStream()).ReadToEnd();
    }
}
