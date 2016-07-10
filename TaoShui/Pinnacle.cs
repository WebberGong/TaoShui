using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CaptchaRecogniser;
using Newtonsoft.Json;

namespace TaoShui
{
    public class Pinnacle : WebSite
    {
        public Pinnacle(WebBrowser browser, string loginName, string loginPassword, int loginTimeOut = 10)
            : base(browser, loginName, loginPassword, loginTimeOut)
        {
        }

        protected override Uri BaseUrl
        {
            get { return new Uri("https://www.pinnacle.com/zh-cn/"); }
        }

        protected override string LoginPage
        {
            get { return "https://www.pinnacle.com/zh-cn/"; }
        }

        protected override string ProcessLoginPage
        {
            get { return ""; }
        }

        protected override string MainPage
        {
            get { return "#tab=Menu&sport="; }
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

        public override void ProcessLogin()
        {
            if (browser != null && browser.Document != null)
            {
                var captchaCodeImage = browser.Document.GetElementById("validateCode");
                var captchaCodeInput = browser.Document.GetElementById("txtCode");
                var aElements = browser.Document.GetElementsByTagName("a");
                var submit = aElements.Cast<HtmlElement>().FirstOrDefault(item => item.InnerHtml == "递交");
                var divElements = browser.Document.GetElementsByTagName("div");
                var captchaCodeDiv =
                    divElements.Cast<HtmlElement>()
                        .FirstOrDefault(item => item.GetAttribute("className") == "validationCode");

                if (browser.Document.Window != null && browser.Document.Window.Parent != null &&
                    captchaCodeImage != null && captchaCodeInput != null && submit != null && captchaCodeDiv != null)
                {
                    captchaCodeDiv.Style = "position: static; top: 0; left: 0; margin: 0;";
                    captchaCodeImage.Style = "position: absolute; z-index: 99999999; top: -2px; left: -2px";
                    var bitmap = new Bitmap(captchaCodeImage.ClientRectangle.Width - 4,
                        captchaCodeImage.ClientRectangle.Height - 4);
                    var rectangle = new Rectangle(0, 0,
                        captchaCodeImage.OffsetRectangle.Width - 4, captchaCodeImage.OffsetRectangle.Height - 4);
                    browser.DrawToBitmap(bitmap, rectangle);

                    var code = Recogniser.RecognizeFromImage(bitmap, 4, 3,
                        new HashSet<EnumCaptchaType> {EnumCaptchaType.Number});
                    captchaCodeInput.SetAttribute("value", code);
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
                                        if (elementCollection.Count()%8 == 1)
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