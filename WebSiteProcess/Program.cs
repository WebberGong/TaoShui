using Utils;
using WebSite;

namespace WebSiteProcess
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                //args = new[] { "WebSite.MaxBet", "sfb1337952", "Aaaa2235", "4", "60", "1" };
                args = new[] { "WebSite.BetIsn", "zb999111", "sss123456", "4", "60", "1" };
            }
            if (args.Length != 6)
            {
                LogHelper.Instance.LogInfo(typeof(Program), "运行出错,传递到该进程的参数不完整!");
                return;
            }
            var webSiteType = args[0];
            var loginName = args[1];
            var loginPassword = args[2];
            var captchaLength = int.Parse(args[3]);
            var loginTimeOut = int.Parse(args[4]);
            var grabDataInterval = int.Parse(args[5]);

            var site = WebSiteFactory.CreateWebSite(webSiteType, loginName, loginPassword,
                captchaLength, loginTimeOut, grabDataInterval);
            site.Run();
        }
    }
}