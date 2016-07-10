using System.Runtime.InteropServices;
using System.Security.Permissions;

namespace TaoShui
{
    [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
    [ComVisible(true)]
    public class MaxBetMessageHandler
    {
        private readonly WebSite _webSite;
        private int _captchaValidateCount = 3;

        public MaxBetMessageHandler(WebSite webSite)
        {
            _webSite = webSite;
        }

        public void AlertMessage(string msg)
        {
            if (msg == "验证码错误!")
            {
                _captchaValidateCount++;
                if (_captchaValidateCount < 3)
                {
                    _webSite.ValidateCaptcha();
                }
            }
        }
    }
}