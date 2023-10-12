using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCardProtocolAPI.Command
{
    public class HttpHelper
    {
        static HttpClient _httpClient = new HttpClient();

        public static byte[] GetByteArray(string url)
        {
            byte[] bytes = null;
            try
            {
                bytes = _httpClient.GetByteArrayAsync(url).ConfigureAwait(false).GetAwaiter().GetResult();
            }
            catch
            {}
            return bytes;
        }
    }
}
