using System.Collections.Generic;
using System.Text;
using SeasideResearch.LibCurlNet;

namespace DataGrabber
{
    public class Grabber
    {
        private StringBuilder _grabDataHtml;

        private int OnGrabDataWriteData(byte[] buf, int size, int nmemb, object extraData)
        {
            var html = Encoding.UTF8.GetString(buf);
            _grabDataHtml.Append(html);
            return size*nmemb;
        }

        public string Run(IDictionary<string, string> grabDataUrlDictionary, string cookie)
        {
            _grabDataHtml = new StringBuilder();
            foreach (var item in grabDataUrlDictionary)
            {
                var easy = new Easy();
                Easy.WriteFunction grabData = OnGrabDataWriteData;
                easy.SetOpt(CURLoption.CURLOPT_URL, item.Value);
                easy.SetOpt(CURLoption.CURLOPT_WRITEFUNCTION, grabData);
                easy.SetOpt(CURLoption.CURLOPT_USERAGENT,
                    "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/51.0.2704.103 Safari/537.36");
                easy.SetOpt(CURLoption.CURLOPT_HEADER, 0);
                easy.SetOpt(CURLoption.CURLOPT_VERBOSE, 1);
                easy.SetOpt(CURLoption.CURLOPT_HTTPGET, 1);
                easy.SetOpt(CURLoption.CURLOPT_COOKIESESSION, 1);
                easy.SetOpt(CURLoption.CURLOPT_COOKIE, cookie);
                var code = easy.Perform();
            }
            return _grabDataHtml.ToString();
        }
    }
}