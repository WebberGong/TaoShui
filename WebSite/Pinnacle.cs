using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Awesomium.Core;
using Awesomium.Windows.Forms;
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

        ~Pinnacle()
        {
        }

        protected override Uri BaseUrl
        {
            get { return new Uri("https://www.pinnacle.com/zh-cn/"); }
        }

        protected override Regex ChangeLanguageRegex
        {
            get { return null; }
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

        protected override Action<WebView, string> ShowJavascriptDialog
        {
            get { return (browser, msg) => { LogHelper.LogWarn(GetType(), msg); }; }
        }

        protected override void ChangeLanguage(WebView browser)
        {
        }

        protected override void Login(WebView browser)
        {
        }

        protected override bool IsCaptchaInputPageReady(WebView browser)
        {
            return true;
        }

        protected override void CaptchaValidate(WebView browser)
        {
        }

        protected override void RefreshCaptcha(WebView browser)
        {
        }

        protected override IDictionary<string, IDictionary<string, IList<string>>> GrabData(WebView wb)
        {
            return new Dictionary<string, IDictionary<string, IList<string>>>();
        }
    }
}