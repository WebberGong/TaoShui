using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using CaptchaRecogniser;
using Newtonsoft.Json;
using Utils;

namespace TaoShui
{
    public class MaxBet : WebSite
    {
        public MaxBet(WebBrowser browser, string loginName, string loginPassword, int captchaLength, int loginTimeOut = 10)
            : base(browser, loginName, loginPassword, captchaLength, loginTimeOut)
        {
        }

        protected override Uri BaseUrl
        {
            get { return new Uri("http://www.maxbet.com/Default.aspx"); }
        }

        protected override string LoginPage
        {
            get { return "Default.aspx"; }
        }

        protected override string CaptchaInputPage
        {
            get { return "ProcessLogin.aspx"; }
        }

        protected override string MainPage
        {
            get { return "main.aspx"; }
        }

        protected override Action<bool> EndLogin
        {
            get { return isLoginSuccessful => { LogHelper.LogInfo(GetType(), @"登录是否成功: " + isLoginSuccessful); }; }
        }

        protected override Action<IDictionary<string, IList<string>>> EndGrabData
        {
            get
            {
                return dicData => { LogHelper.LogInfo(GetType(), @"抓取到的数据: " + JsonConvert.SerializeObject(dicData)); };
            }
        }

        protected override Action<EnumLoginStatus> LoginStatusChanged
        {
            get { return loginStatus => { LogHelper.LogInfo(GetType(), @"登录状态: " + loginStatus.ToString()); }; }
        }

        public override void StartLogin()
        {
            if (browser != null && browser.Document != null)
            {
                var id = browser.Document.GetElementById("txtID");
                var password = browser.Document.GetElementById("txtPW");
                var aElements = browser.Document.GetElementsByTagName("a");
                var login =
                    aElements.Cast<HtmlElement>().FirstOrDefault(item => item.GetAttribute("className") == "login_btn");
                if (id != null && password != null && login != null)
                {
                    browser.Document.InvokeScript("changeLan", new object[] { "cs" });
                    id.SetAttribute("value", loginName);
                    password.SetAttribute("value", loginPassword);
                    login.InvokeMember("Click");
                }
            }
        }

        public override bool IsCaptchaInputPageLoaded()
        {
            HtmlElement captchaImage = null;
            if (browser.Document != null)
            {
                captchaImage = browser.Document.GetElementById("validateCode");
            }
            return captchaImage != null;
        }

        private void RefreshCaptcha(HtmlElement captchaRefresh, HtmlElement captchaImage)
        {
            if (captchaRefresh != null && captchaImage != null)
            {
                var src = captchaImage.GetAttribute("src");

                var bitmap = new Bitmap(captchaImage.ClientRectangle.Width - 4,
                        captchaImage.ClientRectangle.Height - 4);
                var rectangle = new Rectangle(0, 0,
                    captchaImage.OffsetRectangle.Width - 4, captchaImage.OffsetRectangle.Height - 4);
                browser.DrawToBitmap(bitmap, rectangle);

                var code = Recogniser.RecognizeFromImage(bitmap, captchaLength, 3,
                    new HashSet<EnumCaptchaType> { EnumCaptchaType.Number });

                captchaRefresh.InvokeMember("Click");
                var timeOut = new TimeSpan(0, 0, 3);
                DateTime startTime = DateTime.Now;
                while (DateTime.Now < startTime.Add(timeOut) && src == captchaImage.GetAttribute("src"))
                {
                    Application.DoEvents();
                    Thread.Sleep(100);
                }

                bitmap = new Bitmap(captchaImage.ClientRectangle.Width - 4,
                    captchaImage.ClientRectangle.Height - 4);
                rectangle = new Rectangle(0, 0,
                    captchaImage.OffsetRectangle.Width - 4, captchaImage.OffsetRectangle.Height - 4);
                browser.DrawToBitmap(bitmap, rectangle);

                code = Recogniser.RecognizeFromImage(bitmap, captchaLength, 3,
                    new HashSet<EnumCaptchaType> { EnumCaptchaType.Number });
            }
        }

        public override void RefreshCaptcha()
        {
            if (browser.Document != null)
            {
                var captchaRefresh = browser.Document.GetElementById("validateCode_href");
                var captchaImage = browser.Document.GetElementById("validateCode");

                RefreshCaptcha(captchaRefresh, captchaImage);
            }
        }

        public override void CaptchaValidate()
        {
            if (browser != null && browser.Document != null)
            {
                var captchaImage = browser.Document.GetElementById("validateCode");
                var captchaRefresh = browser.Document.GetElementById("validateCode_href");
                var captchaInput = browser.Document.GetElementById("txtCode");
                var aElements = browser.Document.GetElementsByTagName("a");
                var submit = aElements.Cast<HtmlElement>().FirstOrDefault(item => item.InnerHtml == "递交");
                var divElements = browser.Document.GetElementsByTagName("div");
                var captchaDiv = divElements.Cast<HtmlElement>().FirstOrDefault(item => item.GetAttribute("className") == "validationCode");

                if (browser.Document.Window != null && browser.Document.Window.Parent != null &&
                    captchaImage != null && captchaRefresh != null && captchaInput != null && submit != null && captchaDiv != null)
                {
                    captchaDiv.Style = "position: static; top: 0; left: 0; margin: 0;";
                    captchaImage.Style = "position: absolute; z-index: 99999999; top: -2px; left: -2px";
                    var bitmap = new Bitmap(captchaImage.ClientRectangle.Width - 4,
                        captchaImage.ClientRectangle.Height - 4);
                    var rectangle = new Rectangle(0, 0,
                        captchaImage.OffsetRectangle.Width - 4, captchaImage.OffsetRectangle.Height - 4);
                    browser.DrawToBitmap(bitmap, rectangle);

                    var code = Recogniser.RecognizeFromImage(bitmap, captchaLength, 3,
                        new HashSet<EnumCaptchaType> { EnumCaptchaType.Number });
                    code = Common.GetNumericFromString(code);
                    if (code.Length != captchaLength)
                    {
                        Console.WriteLine(code);
                        RefreshCaptcha(captchaRefresh, captchaImage);
                        return;
                    }
                    Console.WriteLine(code);
                    captchaInput.SetAttribute("value", code);
                    submit.InvokeMember("Click");
                }
            }
        }

        public override void StartGrabData()
        {
            if (browser != null && browser.Document != null && browser.Document.Window != null &&
                browser.Document.Window.Frames != null && browser.Document.Window.Frames.Count > 0)
            {
                var htmlWindow = browser.Document.Window.Frames["mainFrame"];
                if (htmlWindow != null && htmlWindow.Document != null)
                {
                    var mainTables =
                        htmlWindow.Document.GetElementsByTagName("table")
                            .Cast<HtmlElement>()
                            .Where(x => x.GetAttribute("className") == "oddsTable");
                    IDictionary<string, IList<string>> dicData = new Dictionary<string, IList<string>>();
                    foreach (var mainTable in mainTables)
                    {
                        if (mainTable.Document != null)
                        {
                            var mainTableRows = mainTable.Document.GetElementsByTagName("tr");
                            var leagueName = string.Empty;
                            foreach (HtmlElement item in mainTableRows)
                            {
                                var spanElements = item.GetElementsByTagName("span");
                                var addToMyFavorite = spanElements.Cast<HtmlElement>()
                                    .FirstOrDefault(x => x.GetAttribute("Title") == "加入我的最爱");
                                if (addToMyFavorite != null && addToMyFavorite.Parent != null)
                                {
                                    leagueName = addToMyFavorite.Parent.InnerText;
                                    if (!string.IsNullOrEmpty(leagueName))
                                    {
                                        if (!dicData.ContainsKey(leagueName))
                                        {
                                            dicData.Add(leagueName, new List<string>());
                                        }
                                        else
                                        {
                                            LogHelper.LogWarn(GetType(), @"联赛名称重复！");
                                        }
                                        continue;
                                    }
                                }
                                if (!string.IsNullOrEmpty(leagueName) &&
                                    item.GetAttribute("className").Contains("displayOn"))
                                {
                                    if (!dicData.ContainsKey(leagueName))
                                    {
                                        LogHelper.LogWarn(GetType(), @"未找到联赛名称！");
                                    }
                                    else
                                    {
                                        var dataElementCollection =
                                            item.GetElementsByTagName("a")
                                                .Cast<HtmlElement>()
                                                .Where(x => x.Name == "cvmy");
                                        var elementCollection = dataElementCollection as HtmlElement[] ??
                                                                dataElementCollection.ToArray();
                                        if (elementCollection.Count() % 8 == 1)
                                        {
                                            LogHelper.LogWarn(GetType(), @"比赛数据不完整！");
                                            continue;
                                        }
                                        foreach (var dataElement in elementCollection)
                                        {
                                            dicData[leagueName].Add(dataElement.InnerHtml);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    EndGrabData(dicData);
                }
            }
        }
    }
}