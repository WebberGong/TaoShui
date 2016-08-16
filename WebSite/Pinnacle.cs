using System;
using System.Text.RegularExpressions;
using Entity;
using Utils;

namespace WebSite
{
    public class Pinnacle : WebSiteBase
    {
        public Pinnacle(string loginName, string loginPassword, int captchaLength,
            int loginTimeOut, int grabDataInterval)
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
            get { return msg => { LogHelper.Instance.LogWarn(GetType(), msg); }; }
        }

        protected override void ChangeLanguage()
        {
        }

        protected override void Login()
        {
            if (IsBrowserOk())
            {
                var js = @"
                    (function() {
                        try {
                            var openLogin = $('#loginButton');
                            if (openLogin) {
                                openLogin.click();
                                var id = $('input.customerId:first');
                                var pw = $('input.password:first');
                                var login = $('input.loginSubmit:first');
                                if (id && pw && login) {
                                    id.val('" + loginName + @"');
                                    pw.val('" + loginPassword + @"');
                                    login.click();
                                    return true;
                                }
                            }
                            return false;
                        } catch (ex) {
                            return ex.message;
                        }
                    })();";
                var result = browser.ExecuteJavascriptWithResult(js);
                LogHelper.Instance.LogInfo(GetType(), "开始登录:" + result);
            }
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

        protected override FootballData[] GrabData()
        {
            return null;
        }
    }
}