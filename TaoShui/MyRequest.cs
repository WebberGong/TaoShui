using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace TaoShui
{
    public class MyRequest
    {
        public static string DoRequest()
        {
            string responseData = string.Empty;
            Encoding responsEncoding = Encoding.UTF8;

            var request = WebRequest.Create("http://www.maxbet.com/ProcessLogin.aspx?") as HttpWebRequest;
            if (request != null)
            {
                request.ProtocolVersion = HttpVersion.Version11;
                request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8;";
                request.KeepAlive = true;
                request.UserAgent =
                    "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 5.1; Trident/4.0; .NET CLR 2.0.50727; .NET CLR 3.0.04506.648; .NET CLR 3.5.21022; InfoPath.2; .NET4.0C; .NET4.0E";
                request.ContentType = "application/x-www-form-urlencoded;";
                request.Headers.Add("Accept-Encoding", "gzip, deflate;");
                request.Headers.Add("Accept-Language", "en-US,en;q=0.8,zh;q=0.6;");
                request.ContentType = "application/x-www-form-urlencoded";
                request.Method = "POST";
                string parameters = "txtID=sfb1337950&txtPW=70b1f5d05cb92a6e6c32dd9236ad3599&txtCode=8196&hidKey=&hidLowerCasePW=&IEVerison=0&hidServerKey=maxbet.com&detecResTime=188&__tk=20b020fa0d1e175d3e84ddf5f30f0fce4bbd526f568aa0&IsSSL=0&PF=Default&RMME=on&__di=eyJuYSI6Ik4vQSIsImRldmljZUNvZGUiOiIxOGQyZTE3MGQyNDI3OTI4YTQ2NzMxNDNmYjY1NTUyOTM5YzNiYWQ5OzJmOGE4ZDE5YWRiYTdjMzVjYjc0OWRjZGE4MTEwYWIzIiwiYXBwVmVyc2lvbiI6IjUuMCAoV2luZG93cyBOVCA2LjM7IFdPVzY0KSBBcHBsZVdlYktpdC81MzcuMzYgKEtIVE1MLCBsaWtlIEdlY2tvKSBDaHJvbWUvNTEuMC4yNzA0LjEwMyBTYWZhcmkvNTM3LjM2IiwidGltZVpvbmUiOiItNDgwIiwidXNlckFnZW50IjoiTW96aWxsYS81LjAgKFdpbmRvd3MgTlQgNi4zOyBXT1c2NCkgQXBwbGVXZWJLaXQvNTM3LjM2IChLSFRNTCwgbGlrZSBHZWNrbykgQ2hyb21lLzUxLjAuMjcwNC4xMDMgU2FmYXJpLzUzNy4zNiIsInNjcmVlbiI6eyJ3aWR0aCI6MTkyMCwiaGVpZ2h0IjoxMDgwLCJjb2xvckRlcHRoIjoyNH0sImRldmljZUlkIjoiZGMyM2VmNDhiZjg1NGMyMGE2YjZjZjQ1ZDIxZWJiYjQiLCJocmVmIjoiaHR0cDovL3d3dy5tYXhiZXQuY29tL0RlZmF1bHQuYXNweCIsImNhcHR1cmVkRGF0ZSI6IjYzNjAzMzYyNTk1NjUzNTI2NCJ9";
                byte[] byteArr = Encoding.ASCII.GetBytes(parameters);
                using (Stream reqStream = request.GetRequestStream())
                {
                    reqStream.Write(byteArr, 0, byteArr.Length);
                    reqStream.Close();
                }
                using (var response = (request.GetResponse()) as HttpWebResponse)
                {
                    if (response != null)
                    {
                        Stream stream = response.GetResponseStream();
                        if (stream != null)
                        {
                            using (StreamReader reader = new StreamReader(stream, responsEncoding))
                            {
                                responseData = reader.ReadToEnd();
                            }
                        }
                    }
                }
            }
            return responseData;
        }

        public static string PostLogin(string postData, string requestUrlString, ref CookieContainer cookie)
        {
            ASCIIEncoding encoding = new ASCIIEncoding();
            byte[] data = encoding.GetBytes(postData);
            HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(requestUrlString);
            myRequest.Method = "POST";
            myRequest.ContentType = "application/x-www-form-urlencoded";
            myRequest.ContentLength = data.Length;
            myRequest.CookieContainer = new CookieContainer();
            Stream newStream = myRequest.GetRequestStream();
            newStream.Write(data, 0, data.Length);
            newStream.Close();
            HttpWebResponse myResponse = (HttpWebResponse)myRequest.GetResponse();
            cookie.Add(myResponse.Cookies);
            StreamReader reader = new StreamReader(myResponse.GetResponseStream(), Encoding.UTF8);
            return reader.ReadToEnd();
        }
    }
}
