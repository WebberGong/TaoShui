using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace TaoShui
{
    public class Pinnacle : WebSite
    {
        public Pinnacle(WebBrowser browser, string loginName, string loginPassword, int captchaLength,
            int loginTimeOut = 10)
            : base(browser, loginName, loginPassword, captchaLength, loginTimeOut)
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

        protected override string CaptchaInputPage
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

        public override bool IsCaptchaInputPageLoaded()
        {
            return true;
        }

        public override void RefreshCaptcha()
        {
        }

        public override void CaptchaValidate()
        {
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