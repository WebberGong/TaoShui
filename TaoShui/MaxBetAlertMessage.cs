using System.Runtime.InteropServices;
using System.Security.Permissions;

namespace TaoShui
{
    [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
    [ComVisible(true)]
    public class MaxBetMessageHandler
    {
        private readonly WebSite _webSite;

        public MaxBetMessageHandler(WebSite webSite)
        {
            _webSite = webSite;
        }

        public void AlertMessage(string msg)
        {
            if (msg == "验证码错误!")
            {
                _webSite.ProcessLogin();
            }
        }
    }
}