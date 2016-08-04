using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Entity;
using Newtonsoft.Json;
using Utils;
using WebSite;

namespace WebSiteProcess
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            if (args.Length < 0)
            {
                Console.WriteLine("运行出错，没有传递参数到该进程！");
                return;
            }
            var parameters = args[0];
            string[] arr = parameters.Split('|');
            if (arr.Length != 6)
            {
                Console.WriteLine("运行出错，传递到该进程的参数不全！");
                return;
            }
            string webSiteType = arr[0];
            string loginName = arr[1];
            string loginPassword = arr[2];
            int captchaLength = int.Parse(arr[3]);
            int loginTimeOut = int.Parse(arr[4]);
            int grabDataInterval = int.Parse(arr[5]);

            WebSiteBase webSite = WebSiteFactory.CreateWebSite(webSiteType, loginName, loginPassword, captchaLength,
                loginTimeOut, grabDataInterval);
            webSite.WebSiteStatusChanged += status =>
            {
                switch (status)
                {
                    case WebSiteStatus.LoginFailed:
                        break;
                    case WebSiteStatus.LoginSuccessful:
                        break;
                    default:
                        break;
                }
            };
            webSite.Run();

            while (webSite.WebSiteStatus != WebSiteStatus.LoginSuccessful &&
                   webSite.WebSiteStatus != WebSiteStatus.LoginFailed)
            {
                Thread.Sleep(50);
            }

            while (webSite.WebSiteStatus == WebSiteStatus.LoginSuccessful)
            {
                var watch = new Stopwatch();
                watch.Start();
                var data = webSite.GrabData();
                watch.Stop();

                var elapsedTimeMsg = "抓取数据耗时:" + watch.ElapsedMilliseconds;
                LogHelper.LogInfo(typeof(MaxBet), elapsedTimeMsg);
                Console.WriteLine(elapsedTimeMsg);

                var matchCount = 0;
                foreach (var item in data)
                {
                    matchCount += item.Value.Count;
                }
                LogHelper.LogInfo(typeof(MaxBet), string.Format("抓取数据成功, 联赛数: {0}, 比赛数: {1}", data.Count, matchCount));

                var buffer = new SharedMemory.BufferReadWrite(webSiteType, 4096);
                WebSiteData webSiteData = new WebSiteData()
                {
                    WebSiteStatus = webSite.WebSiteStatus,
                    GrabbedData = data
                };
                byte[] byteArr = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(webSiteData));
                buffer.Write(byteArr);

                Thread.Sleep(webSite.GrabDataInterval * 1000);
            }
        }
    }
}