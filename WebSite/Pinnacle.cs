using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
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

        protected override Action<string> ShowJavascriptDialog
        {
            get { return msg => { LogHelper.LogWarn(GetType(), msg); }; }
        }

        protected override void ChangeLanguage()
        {
        }

        protected override void Login()
        {
        }

        protected override bool IsCaptchaInputPageReady()
        {
            return true;
        }

        protected override void CaptchaValidate()
        {
        }

        protected override void RefreshCaptcha()
        {
        }

        public override IDictionary<string, IDictionary<string, IList<string>>> GrabData()
        {
            return new Dictionary<string, IDictionary<string, IList<string>>>();
        }
    }
}