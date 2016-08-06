using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Entity;
using Utils;
using WebSite;

namespace WebSiteProcess
{
    internal class Program
    {
        private static int _loginFailedCount;
        private static readonly int maxLoginAttemptCount = 3;
        private static readonly TimeSpan loginInterval = new TimeSpan(0, 5, 0);

        private static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                //args = new[] { "WebSite.MaxBet", "pyh667h00a111", "A123456a111", "4", "60", "1" };
                args = new[] { "WebSite.MaxBet", "sfb1337952", "Aaaa2234", "4", "60", "1" };
            }
            if (args.Length != 6)
            {
                LogHelper.LogInfo(typeof(Program), "运行出错，传递到该进程的参数不完整！");
                return;
            }
            var webSiteType = args[0];
            var loginName = args[1];
            var loginPassword = args[2];
            var captchaLength = int.Parse(args[3]);
            var loginTimeOut = int.Parse(args[4]);
            var grabDataInterval = int.Parse(args[5]);

            Thread webSiteThread = new Thread(() =>
            {
                var site = WebSiteFactory.CreateWebSite(webSiteType, loginName, loginPassword,
                    captchaLength, loginTimeOut, grabDataInterval);

                site.WebSiteStatusChanged += (status) =>
                {
                    switch (site.WebSiteStatus)
                    {
                        case WebSiteStatus.LoginSuccessful:
                            _loginFailedCount = 0;

                            var watch = new Stopwatch();
                            watch.Start();
                            var data = site.GrabData();
                            watch.Stop();

                            var elapsedTimeMsg = "抓取数据耗时:" + watch.ElapsedMilliseconds;
                            LogHelper.LogInfo(typeof(MaxBet), elapsedTimeMsg);

                            var matchCount = 0;
                            foreach (var item in data)
                            {
                                matchCount += item.Value.Count;
                            }
                            LogHelper.LogInfo(typeof(MaxBet),
                                string.Format("抓取数据成功, 联赛数: {0}, 比赛数: {1}", data.Count, matchCount));

                            var webSiteData = new WebSiteData
                            {
                                WebSiteStatus = site.WebSiteStatus.ToString(),
                                GrabbedData = data
                            };
                            SharedMemoryManager.Instance.Write("isDataReady", "false");
                            SharedMemoryManager.Instance.Write(webSiteType, webSiteData);
                            SharedMemoryManager.Instance.Write("isDataReady", "true");
                            break;
                        case WebSiteStatus.LoginFailed:
                            _loginFailedCount++;
                            site.Stop();
                            if (_loginFailedCount < maxLoginAttemptCount)
                            {
                                if (_loginFailedCount > 0)
                                {
                                    Thread.Sleep((int)loginInterval.TotalSeconds);
                                }
                                site.Run();
                            }
                            break;
                        default:
                            break;
                    }
                };

                site.Run();
            })
            {
                Name = "GrabDataThread",
                IsBackground = false,
                Priority = ThreadPriority.AboveNormal
            };
            webSiteThread.Start();
        }
    }
}