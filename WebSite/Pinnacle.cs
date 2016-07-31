using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Utils;

namespace WebSite
{
    public class Pinnacle : WebSiteBase
    {
        public Pinnacle(string loginName, string loginPassword, int captchaLength,
            int loginTimeOut = 10, int grabDataInterval = 5)
            : base(loginName, loginPassword, captchaLength, loginTimeOut, grabDataInterval)
        {
        }

        protected override Uri BaseUrl
        {
            get { return new Uri("https://www.pinnacle.com/zh-cn/"); }
        }

        protected override Regex LoginPageRegex
        {
            get { return new Regex("https://www\\.pinnacle\\.com/zh-cn/"); }
        }

        protected override Regex CaptchaInputPageRegex
        {
            get { return null; }
        }

        protected override Regex MainPageRegex
        {
            get { return new Regex("#tab=Menu&sport="); }
        }

        protected override Action<string> PopupMsg
        {
            get
            {
                return msg =>
                {
                    switch (msg)
                    {
                        case "帐号/密码错误":
                            WebSiteStatus = WebSiteStatus.LoginFailed;
                            break;
                        case "验证码错误":
                            DoRefreshCaptcha();
                            break;
                    }
                };
            }
        }

        protected override Action<string> SendData
        {
            get { return data => { LogHelper.LogInfo(GetType(), data); }; }
        }

        protected override void Login()
        {
            if (IsBrowserOk() && browser.Document != null)
            {
                var startLoginBtn = browser.Document.GetElementById("loginButton");
                if (startLoginBtn != null)
                {
                    startLoginBtn.InvokeMember("Click");
                }
                var loginForm = browser.Document.Forms["loginForm"];
                if (loginForm != null && loginForm.Document != null)
                {
                    var inputs = loginForm.Document.GetElementsByTagName("input");
                    var id =
                        inputs.Cast<HtmlElement>()
                            .FirstOrDefault(item => item.GetAttribute("className") == "customerId");
                    var password =
                        inputs.Cast<HtmlElement>().FirstOrDefault(item => item.GetAttribute("className") == "password");
                    var login =
                        inputs.Cast<HtmlElement>()
                            .FirstOrDefault(item => item.GetAttribute("className") == "loginSubmit");
                    if (id != null && password != null && login != null)
                    {
                        id.SetAttribute("value", loginName);
                        password.SetAttribute("value", loginPassword);
                        login.InvokeMember("Click");
                    }
                }
            }
        }

        protected override bool IsCaptchaInputPageLoaded()
        {
            return true;
        }

        protected override void CaptchaValidate()
        {
        }

        protected override void RefreshCaptcha()
        {
        }

        protected override IDictionary<string, IDictionary<string, IList<string>>> GrabData(WebBrowser wb)
        {
            return new Dictionary<string, IDictionary<string, IList<string>>>();
        }
    }
}