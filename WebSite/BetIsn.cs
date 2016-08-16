using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using CaptchaRecogniser;
using Entity;
using Newtonsoft.Json;
using Utils;

namespace WebSite
{
    public class BetIsn : WebSiteBase
    {
        public BetIsn(string loginName, string loginPassword, int captchaLength, int loginTimeOut, int grabDataInterval)
            : base(loginName, loginPassword, captchaLength, loginTimeOut, grabDataInterval)
        {
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
            if (IsBrowserOk())
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
                            var zh = $('#language_msa_1');
                            var id = $('#username');
                            var pw = $('#password');
                            var captchaInput = $('#code');
                            var login = $('#login');
                            if (zh && id && pw && captchaInput && login) {
                                zh.click();
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

        protected override FootballData[] GrabData()
        {
            lock (browser)
            {
                var js = @"
                    (function() {
                        try {
                            var tbodies = $('.subcontent:first tbody');
                            var matches = [];
                            var leagueName = '';
                            tbodies.each(function() {
                                var trs = $(this).children('tr');
                                if (trs.length === 1 && $(this).find('tr:eq(0) td').length === 3)
                                {
                                    leagueName = trs.find(':eq(0) td:first').text().trim();
                                }
                                else if (trs.length === 3 && 
                                    $(this).find('tr:eq(0) td').length === 15 &&
                                    $(this).find('tr:eq(1) td').length === 14 &&
                                    $(this).find('tr:eq(2) td').length === 14) {
                                    var match = JSON.parse('" + FootballData.GetEmptyObjectJsonString() + @"');
                                    match.LeagueName = leagueName;
                                    var index = -1;
                                    $(this).find('tr:eq(0) td').each(function () {
                                        index++;
                                        if (index === 0) {
                                            var a1 = $(this).children('span.black-bold');
                                            var a2 = $(this).children('span.red-bold');
                                            var a3 = $(this).children('span.soccer_rb');
                                            if (a1.length === 1 && a2.length === 1) {
                                                match.Score = a1.text().trim();
                                                match.Time = a2.text().trim();
                                            } else if (a1.length === 1 && a3.length === 1) {
                                                match.Time = a1.text().trim();
                                            }
                                        }
                                        else if (index === 1) {
                                            var b1 = $(this).find('span span.team-name-wrap:first');
                                            if (b1.length === 1) {
                                                match.HostTeam = b1.text().trim();
                                            }
                                        }
                                        else if (index === 2) {
                                            var c1 = $(this).find('span a:first');
                                            if (c1.length === 1) {
                                                match.WholeOneByTwoHostOdds = c1.text().trim();
                                            }
                                        }
                                        else if (index === 3) {
                                            var d1 = $(this).children('span:first');
                                            if (d1.length === 1) {
                                                match.WholeConcedePoints = d1.text().trim();
                                            }
                                        }
                                        else if (index === 4) {
                                            var e1 = $(this).find('span a:first');
                                            if (e1.length === 1) {
                                                match.WholeConcedeHostOdds = e1.text().trim();
                                            }
                                        }
                                        else if (index === 5) {
                                            var f1 = $(this).children('span:first');
                                            if (f1.length === 1) {
                                                match.WholeSizeBallPoints = f1.text().trim();
                                            }
                                        }
                                        else if (index === 7) {
                                            var g1 = $(this).find('span a:first');
                                            if (g1.length === 1) {
                                                match.WholeSizeBallHostOdds = g1.text().trim();
                                            }
                                        }
                                        else if (index === 8) {
                                            var h1 = $(this).find('span a:first');
                                            if (h1.length === 1) {
                                                match.HalfOneByTwoHostOdds = h1.text().trim();
                                            }
                                        }
                                        else if (index === 9) {
                                            var i1 = $(this).children('span:first');
                                            if (i1.length === 1) {
                                                match.HalfConcedePoints = i1.text().trim();
                                            }
                                        }
                                        else if (index === 10) {
                                            var j1 = $(this).find('span a:first');
                                            if (j1.length === 1) {
                                                match.HalfConcedeHostOdds = j1.text().trim();
                                            }
                                        }
                                        else if (index === 11) {
                                            var k1 = $(this).children('span:first');
                                            if (k1.length === 1) {
                                                match.HalfSizeBallPoints = k1.text().trim();
                                            }
                                        }
                                        else if (index === 13) {
                                            var l1 = $(this).find('span a:first');
                                            if (l1.length === 1) {
                                                match.HalfSizeBallHostOdds = l1.text().trim();
                                            }
                                        }
                                    });
                                    index = 0;
                                    $(this).find('tr:eq(1) td').each(function () {
                                        index++;
                                        if (index === 0) {
                                            var b1 = $(this).find('span span.team-name-wrap:first');
                                            if (b1.length === 1) {
                                                match.GuestTeam = b1.text().trim();
                                            }
                                        }
                                        else if (index === 1) {
                                            var c1 = $(this).find('span a:first');
                                            if (c1.length === 1) {
                                                match.WholeOneByTwoGuestOdds = c1.text().trim();
                                            }
                                        }
                                        else if (index === 3) {
                                            var e1 = $(this).find('span a:first');
                                            if (e1.length === 1) {
                                                match.WholeConcedeGuestOdds = e1.text().trim();
                                            }
                                        }
                                        else if (index === 6) {
                                            var g1 = $(this).find('span a:first');
                                            if (g1.length === 1) {
                                                match.WholeSizeBallGuestOdds = g1.text().trim();
                                            }
                                        }
                                        else if (index === 7) {
                                            var h1 = $(this).find('span a:first');
                                            if (h1.length === 1) {
                                                match.HalfOneByTwoGuestOdds = h1.text().trim();
                                            }
                                        }
                                        else if (index === 9) {
                                            var j1 = $(this).find('span a:first');
                                            if (j1.length === 1) {
                                                match.HalfConcedeGuestOdds = j1.text().trim();
                                            }
                                        }
                                        else if (index === 12) {
                                            var l1 = $(this).find('span a:first');
                                            if (l1.length === 1) {
                                                match.HalfSizeBallGuestOdds = l1.text().trim();
                                            }
                                        }
                                    });
                                    index = 0;
                                    $(this).find('tr:eq(2) td').each(function () {
                                        index++;
                                        if (index === 0) {
                                            var b1 = $(this).children('span:first');
                                            if (b1.length === 1) {
                                                match.DrawnGame = b1.text().trim();
                                            }
                                        }
                                        else if (index === 1) {
                                            var c1 = $(this).find('span a:first');
                                            if (c1.length === 1) {
                                                match.WholeOneByTwoDrawnGameOdds = c1.text().trim();
                                            }
                                        }
                                        else if (index === 7) {
                                            var h1 = $(this).find('span a:first');
                                            if (h1.length === 1) {
                                                match.HalfOneByTwoDrawnGameOdds = h1.text().trim();
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
                var grabbedData = JsonConvert.DeserializeObject(result, typeof(FootballData[])) as FootballData[];
                return grabbedData;
            }
        }
    }
}