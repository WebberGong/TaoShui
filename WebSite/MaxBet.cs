using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using CaptchaRecogniser;
using Newtonsoft.Json;
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

        protected override string[][] GrabData()
        {
            lock (browser)
            {
                var js = @"
                    (function() {
                        try {
                            var trs = $(this.top.frames['mainFrame'].document).find('#tabbox tbody tr');
                            var matches = [];
                            var leagueName = '';
                            trs.each(function() {
                                var tds = $(this).children('td');
                                if (tds.length === 3)
                                {
                                    leagueName = $(this).text().trim();
                                }
                                else if (tds.length === 10) {
                                    var match = [];
                                    match.push(leagueName);
                                    var index = -1;
                                    tds.each(function () {
                                        index++;
                                        if (index === 0) {
                                            var a1 = $(this).children('b');
                                            var a2 = $(this).find('div span b:last');
                                            if (a1.length === 1 && a2.length === 1) {
                                                match.push(a1.text().trim());
                                                match.push(a2.text().trim());
                                            } else {
                                                match.push($(this).text().trim());
                                                match.push('');
                                            }
                                        }
                                        else if (index === 1) {
                                            var b1 = $(this).find('div span');
                                            if (b1.length === 2) {
                                                match.push(b1[0].innerHTML.trim());
                                                match.push(b1[1].innerHTML.trim());
                                                var b2 = $(this).children('div:last');
                                                if (b2 && b2.children('span').length === 0) {
                                                    match.push(b2.text().trim());
                                                }
                                                else {
                                                    match.push('');
                                                }
                                            }
                                        }
                                        else if (index === 3 || index === 4 || index === 6 || index === 7) {
                                            var c1 = $(this).find('div a[name=cvmy]');
                                            if (c1.length === 2) {
                                                var c2 = $(this).children('div:first');
                                                if (c2 && c2.children('a').length === 0) {
                                                    match.push(c2.text().trim().replace(/\s+/, '|'));
                                                    match.push(c1[0].innerHTML.trim());
                                                    match.push(c1[1].innerHTML.trim());
                                                }
                                            }
                                        }
                                        else if (index === 5 || index === 8) {
                                            var d1 = $(this).find('div a');
                                            if (d1.length === 3) {
                                                match.push(d1[0].innerHTML.trim());
                                                match.push(d1[1].innerHTML.trim());
                                                match.push(d1[2].innerHTML.trim());
                                            }
                                        }
                                    });
                                    matches.push(match);
                                }
                            });
                            return JSON.stringify(matches);
                        } catch (ex) {
                            return ex.message;
                        }
                    })();";
                var result = browser.ExecuteJavascriptWithResult(js);
                var grabbedData = JsonConvert.DeserializeObject(result, typeof(string[][])) as string[][];
                return grabbedData;
            }
        }
    }
}