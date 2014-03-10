using System;
using System.IO;
using System.Net;
using FotM.Utilities;
using log4net;
using Newtonsoft.Json;

namespace FotM.ArmoryScanner
{
    public class RawJsonPuller
    {
        private static readonly ILog Logger = LoggingExtensions.GetLogger<RawJsonPuller>();
        private readonly Uri _baseAddress;

        public RawJsonPuller(string baseAddress): this(new Uri(baseAddress))
        {
        }

        public RawJsonPuller(Uri baseAddress)
        {
            _baseAddress = baseAddress;
            Logger.InfoFormat("Base address set to {0}", baseAddress);
        }

        public T DownloadJson<T>(string relativeAddress)
        {
            var address = new Uri(_baseAddress, relativeAddress);

            Logger.InfoFormat("Querying {0}...", address);

            WebRequest downloadRequest = WebRequest.CreateHttp(address);

            using (var response = downloadRequest.GetResponse())
            {
                var stream = response.GetResponseStream();

                if (stream == null) 
                    throw new Exception("Response stream is null.");

                using (var reader = new StreamReader(stream))
                {
                    Logger.InfoFormat("Received response from {0}. Deserializing JSON.", address);

                    string responseText = reader.ReadToEnd();
                    Logger.Debug(responseText);
                    return JsonConvert.DeserializeObject<T>(responseText);
                }
            }
        }
    }
}