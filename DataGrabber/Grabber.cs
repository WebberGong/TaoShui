using System;
using System.Collections.Generic;
using System.Text;
using SeasideResearch.LibCurlNet;

namespace DataGrabber
{
    public class Grabber
    {
        private StringBuilder _grabDataHtml;
        private static Grabber _instance;
        private static readonly object locker = new object();

        private Grabber()
        {
            Curl.GlobalInit((int)CURLinitFlag.CURL_GLOBAL_ALL);
        }

        public static Grabber Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (locker)
                    {
                        if (_instance == null)
                        {
                            _instance = new Grabber();
                        }
                    }
                }
                return _instance;
            }
        }

        private int OnGrabDataWriteData(byte[] buf, int size, int nmemb, object extraData)
        {
            var html = Encoding.UTF8.GetString(buf);
            _grabDataHtml.Append(html);
            return size*nmemb;
        }

        public IDictionary<string, string> Run(IDictionary<string, string> grabDataUrlDictionary, string cookie, int grabDataTimeOut)
        {
            IDictionary<string, string> grabbedData = new Dictionary<string, string>();
            foreach (var item in grabDataUrlDictionary)
            {
                _grabDataHtml = new StringBuilder();
                var easy = new Easy();
                Easy.WriteFunction grabData = OnGrabDataWriteData;
                easy.SetOpt(CURLoption.CURLOPT_URL, item.Value);
                easy.SetOpt(CURLoption.CURLOPT_WRITEFUNCTION, grabData);
                easy.SetOpt(CURLoption.CURLOPT_USERAGENT,
                    "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/51.0.2704.103 Safari/537.36");
                easy.SetOpt(CURLoption.CURLOPT_HEADER, 0);
                easy.SetOpt(CURLoption.CURLOPT_VERBOSE, 1);
                easy.SetOpt(CURLoption.CURLOPT_HTTPGET, 1);
                easy.SetOpt(CURLoption.CURLOPT_COOKIE, cookie);
                var code = easy.Perform();
                if (code == CURLcode.CURLE_OK)
                {
                    grabbedData.Add(item.Key, _grabDataHtml.ToString());
                }
            }
            return grabbedData;
        }

        public virtual void Dispose()
        {
            Curl.GlobalCleanup();
            GC.Collect();
        }
    }
}