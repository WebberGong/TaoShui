using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using Awesomium.Windows.Forms;
using CaptchaRecogniser;
using Utils;

namespace WebSite
{
    public class MaxBet : WebSiteBase
    {
        public MaxBet(string loginName, string loginPassword, int captchaLength,
            int loginTimeOut = 10, int grabDataInterval = 10)
            : base(loginName, loginPassword, captchaLength, loginTimeOut, grabDataInterval)
        {
        }

        protected override Uri BaseUrl
        {
            get { return new Uri("http://www.maxbet.com/Default.aspx"); }
        }

        protected override Regex ChangeLanguageRegex
        {
            get { return new Regex("ChangeLanguage\\.aspx"); }
        }

        protected override Regex LoginPageRegex
        {
            get { return new Regex("Default\\.aspx"); }
        }

        protected override Regex CaptchaInputPageRegex
        {
            get { return new Regex("ProcessLogin\\.aspx$"); }
        }

        protected override Regex MainPageRegex
        {
            get { return new Regex("main\\.aspx"); }
        }

        protected override Action<WebControl, string> ShowJavascriptDialog
        {
            get
            {
                return (browser, msg) =>
                {
                    switch (msg)
                    {
                        case "帐号/密码错误":
                            WebSiteStatus = WebSiteStatus.LoginFailed;
                            break;
                        case "验证码错误":
                            DoRefreshCaptcha(browser);
                            break;
                    }
                };
            }
        }

        protected override void ChangeLanguage(WebControl browser)
        {
            if (IsBrowserOk(browser))
            {
                var changeLanguageJs = @"
                    (function() {
                        try {
                            changeLan('cs');
                            return true;
                        } catch (ex) {
                            return ex;
                        }
                    })();";
                var changeLanguageResult = browser.ExecuteJavascriptWithResult(changeLanguageJs);
                LogHelper.LogInfo(GetType(), "设置语言:" + changeLanguageResult);
            }
        }

        protected override void Login(WebControl browser)
        {
            if (IsBrowserOk(browser))
            {
                var loginJs = @"
                    (function() {
                        try {
                            var id = $('#txtID');
                            var pw = $('#txtPW');
                            var login = $('div[title=" + "\"LOGIN\"" + @"]');
                            if (id && pw && login) {
                                id.val('" + loginName + @"');
                                pw.val('" + loginPassword + @"');
                                login.click();
                                return true;
                            }
                            return false;
                        } catch (ex) {
                            return ex;
                        }
                    })();";
                var loginResult = browser.ExecuteJavascriptWithResult(loginJs);
                LogHelper.LogInfo(GetType(), "开始登录:" + loginResult);
            }
        }

        protected override bool IsCaptchaInputPageReady(WebControl browser)
        {
            if (IsBrowserOk(browser))
            {
                var isCaptchaReadyJs = @"
                    (function() {
                        try {
                            var img = $('#validateCode');
                            if (img) {
                                return true;
                            } else {
                                return false;
                            }
                        } catch (ex) {
                            return ex;
                        }
                    })();";
                var isCaptchaReadyResult = browser.ExecuteJavascriptWithResult(isCaptchaReadyJs);
                LogHelper.LogInfo(GetType(), "当前页是否为验证码输入页:" + isCaptchaReadyResult.ToString());
                return isCaptchaReadyResult.ToString() == True;
            }
            return false;
        }

        protected override void CaptchaValidate(WebControl browser)
        {
            if (IsBrowserOk(browser))
            {
                var getPositionJs = @"
                    (function() {
                        try {
                            var img = $('#validateCode');
                            if (img) {
                                return img.offset().left + '|' + img.offset().top;
                            } else {
                                return null;
                            }
                        } catch (ex) {
                            return ex;
                        }
                    })();";
                var getPositionResult = browser.ExecuteJavascriptWithResult(getPositionJs);
                LogHelper.LogInfo(GetType(), "获取验证码坐标:" + getPositionResult);

                if (getPositionResult != null && getPositionResult.ToString() != Undefined &&
                    getPositionResult.ToString().Contains("|"))
                {
                    var arr = getPositionResult.ToString().Split('|');
                    if (arr.Length == 2)
                    {
                        var x = int.Parse(arr[0]);
                        var y = int.Parse(arr[1]);
                        browser.CopyImageAt(x, y);
                        Thread.Sleep(50);
                        Bitmap bitmap = null;
                        if (Clipboard.ContainsImage())
                        {
                            bitmap = Clipboard.GetImage() as Bitmap;
                        }
                        if (bitmap != null)
                        {
                            LogHelper.LogInfo(GetType(), "获取验证码图片成功");
                            var code = Recogniser.Instance.RecognizeFromImage(bitmap, 4, 3,
                                new HashSet<EnumCaptchaType> {EnumCaptchaType.Number});
                            code = Common.GetNumericFromString(code);
                            LogHelper.LogInfo(GetType(), "验证码识别结果:" + code);

                            var submitJs = @"
                                (function() {
                                    try {
                                        var captchaInput = $('#txtCode');
                                        var submit = $('a:contains(" + "\"递交\"" + @")');
                                        if (captchaInput && submit) {
                                            captchaInput.val('" + code + @"');
                                            submit.click();
                                            return true;
                                        }
                                        return false;
                                    } catch (ex) {
                                        return ex;
                                    }
                                })();";
                            var submitResult = browser.ExecuteJavascriptWithResult(submitJs);
                            LogHelper.LogInfo(GetType(), "提交验证码:" + submitResult);
                        }
                        else
                        {
                            LogHelper.LogInfo(GetType(), "获取验证码图片失败");
                        }
                    }
                }
            }
        }

        protected override void RefreshCaptcha(WebControl browser)
        {
            browser.Source = BaseUrl;
        }

        protected override IDictionary<string, IDictionary<string, IList<string>>> GrabData(WebControl wb)
        {
            IDictionary<string, IDictionary<string, IList<string>>> grabbedData =
                new Dictionary<string, IDictionary<string, IList<string>>>();
            return grabbedData;
        }
    }
}