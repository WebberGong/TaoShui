using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Newtonsoft.Json;
using Utils;

namespace WebSite
{
    public class Pinnacle : WebSiteBase
    {
        public Pinnacle(string loginName, string loginPassword, int captchaLength,
            int loginTimeOut = 10, int getGrabDataUrlTimeOut = 5)
            : base(loginName, loginPassword, captchaLength, loginTimeOut, getGrabDataUrlTimeOut)
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

        protected override IDictionary<string, Regex> GrabDataUrlRegexDictionary
        {
            get
            {
                IDictionary<string, Regex> grabDataUrlRegexeDic = new Dictionary<string, Regex>();
                return grabDataUrlRegexeDic;
            }
        }

        protected override Action<bool> EndLogin
        {
            get { return isLoginSuccessful => { LogHelper.LogInfo(GetType(), "登录是否成功: " + isLoginSuccessful); }; }
        }

        protected override Action<IDictionary<string, IList<string>>> EndGrabData
        {
            get
            {
                return dicData => { LogHelper.LogInfo(GetType(), "抓取到的数据: " + JsonConvert.SerializeObject(dicData)); };
            }
        }

        protected override Action<WebSiteState> LoginStatusChanged
        {
            get { return loginStatus => { LogHelper.LogInfo(GetType(), "登录状态: " + loginStatus.ToString()); }; }
        }

        protected override Action<string> PopupMsgHandler
        {
            get
            {
                return msg =>
                {
                    if (msg == "验证码错误!")
                    {
                        DoRefreshCaptcha();
                    }
                };
            }
        }

        protected override void StartLogin()
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

        protected override void StartGrabData()
        {
        }
    }
}