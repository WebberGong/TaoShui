using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
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
            get { return new Uri("http://www.maxbet.com/Default.aspx?hidSelLang=cs"); }
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

        protected override Action<string> ShowJavascriptDialog
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

        ~MaxBet()
        {
            if (Recogniser.Instance != null)
            {
                Recogniser.Instance.Dispose();
            }
        }

        protected override void ChangeLanguage()
        {
            if (IsBrowserOk())
            {
                var js = @"
                    (function() {
                        try {
                            changeLan('cs');
                            return true;
                        } catch (ex) {
                            return ex.message;
                        }
                    })();";
                var result = browser.ExecuteJavascriptWithResult(js);
                LogHelper.LogInfo(GetType(), "设置语言:" + result);
            }
        }

        protected override void Login()
        {
            if (IsBrowserOk())
            {
                var js = @"
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
                            return ex.message;
                        }
                    })();";
                var result = browser.ExecuteJavascriptWithResult(js);
                LogHelper.LogInfo(GetType(), "开始登录:" + result);
            }
        }

        protected override bool IsCaptchaInputPageReady()
        {
            if (IsBrowserOk())
            {
                var js = @"
                    (function() {
                        try {
                            var img = document.getElementById('validateCode');
                            if (img) {
                                return true;
                            } else {
                                return false;
                            }
                        } catch (ex) {
                            return ex.message;
                        }
                    })();";
                var result = browser.ExecuteJavascriptWithResult(js);
                LogHelper.LogInfo(GetType(), "当前页是否为验证码输入页:" + result.ToString());
                return result.ToString() == True;
            }
            return false;
        }

        protected override void CaptchaValidate()
        {
            if (IsBrowserOk())
            {
                var imgBase64 = JsGetImgBase64String("document.getElementById('validateCode')");
                var imgBytes = Convert.FromBase64String(imgBase64);
                var stream = new MemoryStream(imgBytes);
                var bitmap = Image.FromStream(stream) as Bitmap;
                if (bitmap != null)
                {
                    LogHelper.LogInfo(GetType(), "获取验证码图片成功");
                    var code = Recogniser.Instance.RecognizeFromImage(bitmap, 4, 3,
                        new HashSet<EnumCaptchaType> {EnumCaptchaType.Number});
                    code = Common.GetNumericFromString(code);
                    LogHelper.LogInfo(GetType(), "验证码识别结果:" + code);

                    var js = @"
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
                                return ex.message;
                            }
                        })();";
                    var result = browser.ExecuteJavascriptWithResult(js);
                    LogHelper.LogInfo(GetType(), "提交验证码:" + result);
                }
                else
                {
                    LogHelper.LogInfo(GetType(), "获取验证码图片失败");
                }
            }
        }

        protected override void RefreshCaptcha()
        {
            browser.Source = BaseUrl;
        }

        protected override IDictionary<string, IDictionary<string, string[][]>> GrabData()
        {
            lock (browser)
            {
                IDictionary<string, IDictionary<string, string[][]>> grabbedData =
                    new Dictionary<string, IDictionary<string, string[][]>>();

                var js = @"
                        (function() {
                           try {
                               var trs = $(this.top.frames['mainFrame'].document).find('#oTableContainer_L tbody tr.displayOn');
                               var result = [];
                               trs.each(function() {
                                   var tds = $(this).find('td');
                                   result.push(tds.text());
                               });
                               return result;
                           } catch (ex) {
                               return ex.message;
                           }
                        })();";
                var result = browser.ExecuteJavascriptWithResult(js);
                return grabbedData;
            }
        }
    }
}