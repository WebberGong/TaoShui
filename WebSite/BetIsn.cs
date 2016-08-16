using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using CaptchaRecogniser;
using Utils;

namespace WebSite
{
    public class BetIsn : WebSiteBase
    {
        public BetIsn(string loginName, string loginPassword, int captchaLength, int loginTimeOut, int grabDataInterval)
            : base(loginName, loginPassword, captchaLength, loginTimeOut, grabDataInterval)
        {
            Test.DoTest();
        }

        protected override Uri BaseUrl
        {
            get { return new Uri("http://www.isn99.com/"); }
        }

        protected override Regex ChangeLanguageRegex
        {
            get { return null; }
        }

        protected override Regex LoginPageRegex
        {
            get { return new Regex("membersite\\/login\\.jsp"); }
        }

        protected override Regex CaptchaInputPageRegex
        {
            get { return null; }
        }

        protected override Regex MainPageRegex
        {
            get { return new Regex("zh-CN\\/ui\\/$"); }
        }

        protected override Action<string> ShowJavascriptDialog
        {
            get
            {
                return msg =>
                {
                    switch (msg)
                    {
                        case "输入的账号/密码不正确。请再试一次。":
                            WebSiteStatus = WebSiteStatus.LoginFailed;
                            break;
                        case "登入失败。验证码不正确.":
                            if (captchaValidateCount < CaptchaValidateMaxCount)
                            {
                                RefreshCaptcha();
                                Thread.Sleep(1000);
                                Login();
                            }
                            break;
                    }
                };
            }
        }

        protected override void ChangeLanguage()
        {
        }

        protected override void Login()
        {
            if (IsBrowserOk() && loginAttemptCount < MaxLoginAttemptCount)
            {
                var imgBase64 = JsGetImgBase64String("$('.captcha')[0]", 50, 20);
                var imgBytes = Convert.FromBase64String(imgBase64);
                var stream = new MemoryStream(imgBytes);
                var bitmap = Image.FromStream(stream) as Bitmap;
                string code = string.Empty;
                if (bitmap != null)
                {
                    LogHelper.Instance.LogInfo(GetType(), "获取验证码图片成功");
                    code = Recogniser.Instance.RecognizeFromImage(bitmap, 4, 1,
                        new HashSet<EnumCaptchaType> {EnumCaptchaType.Number});
                    code = Common.GetNumericFromString(code);
                    LogHelper.Instance.LogInfo(GetType(), "验证码识别结果:" + code);
                }
                var js = @"
                    (function() {
                        try {
                            var lang = $('#lang');
                            var id = $('#username');
                            var pw = $('#password');
                            var captchaInput = $('#code');
                            var login = $('#login');
                            if (lang && id && pw && captchaInput && login) {
                                lang.val('zh-CN');
                                id.val('" + loginName + @"');
                                pw.val('" + loginPassword + @"');
                                captchaInput.val('" + code + @"');
                                login.click();
                                return true;
                            }
                            return false;
                        } catch (ex) {
                            return ex.message;
                        }
                    })();";
                var result = browser.ExecuteJavascriptWithResult(js);
                LogHelper.Instance.LogInfo(GetType(), "开始登录:" + result);
                loginAttemptCount++;
            }
        }

        protected override bool IsCaptchaInputPageReady()
        {
            return false;
        }

        protected override void CaptchaValidate()
        {
        }

        protected override void RefreshCaptcha()
        {
            var js = @"
                    (function() {
                        try {
                            var refresh = $('.refresh-captcha')[0];
                            if (refresh) {
                                refresh.click();
                                return true;
                            }
                            return false;
                        } catch (ex) {
                            return ex.message;
                        }
                    })();";
            var result = browser.ExecuteJavascriptWithResult(js);
            LogHelper.Instance.LogInfo(GetType(), "刷新验证码:" + result);
        }

        protected override string[][] GrabData()
        {
            return new string[][] { new[] { "1", "2", "3" }, new[] { "1", "2", "3" } };
        }
    }
}