using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using CaptchaRecogniser;
using mshtml;
using Newtonsoft.Json;
using Utils;

namespace WebSite
{
    public class MaxBet : WebSite
    {
        public MaxBet(WebBrowser browser, string loginName, string loginPassword, int captchaLength,
            int loginTimeOut = 10)
            : base(browser, loginName, loginPassword, captchaLength, loginTimeOut)
        {
        }

        protected override Uri BaseUrl
        {
            get { return new Uri("http://www.maxbet.com/Default.aspx"); }
        }

        protected override Regex LoginPage
        {
            get { return new Regex("Default.aspx"); }
        }

        protected override Regex CaptchaInputPage
        {
            get { return new Regex("ProcessLogin.aspx$"); }
        }

        protected override Regex MainPage
        {
            get { return new Regex("main.aspx"); }
        }

        protected override Action<bool> EndLogin
        {
            get { return isLoginSuccessful => { LogHelper.LogInfo(GetType(), "登录是否成功: " + isLoginSuccessful); }; }
        }

        protected override Action<IDictionary<string, IList<string>>> EndGrabData
        {
            get
            {
                return dicData => { LogHelper.LogInfo(GetType(), "抓取到的数据: " + JsonConvert.SerializeObject(dicData)); };
            }
        }

        protected override Action<EnumLoginStatus> LoginStatusChanged
        {
            get { return loginStatus => { LogHelper.LogInfo(GetType(), "登录状态: " + loginStatus.ToString()); }; }
        }

        protected override void StartLogin()
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

        protected override bool IsCaptchaInputPageLoaded()
        {
            HtmlElement captchaImage = null;
            if (browser.Document != null)
            {
                captchaImage = browser.Document.GetElementById("validateCode");
            }
            return captchaImage != null;
        }

        protected override void CaptchaValidate()
        {
            if (browser != null && browser.Document != null)
            {
                var captchaImage = browser.Document.GetElementById("validateCode");
                var captchaRefresh = browser.Document.GetElementById("validateCode_href");
                var captchaInput = browser.Document.GetElementById("txtCode");
                var aElements = browser.Document.GetElementsByTagName("a");
                var submit = aElements.Cast<HtmlElement>().FirstOrDefault(item => item.InnerHtml == "递交");
                var divElements = browser.Document.GetElementsByTagName("div");
                var captchaDiv =
                    divElements.Cast<HtmlElement>()
                        .FirstOrDefault(item => item.GetAttribute("className") == "validationCode");

                if (browser.Document.Window != null && browser.Document.Window.Parent != null &&
                    captchaImage != null && captchaRefresh != null && captchaInput != null && submit != null &&
                    captchaDiv != null)
                {
                    var doc = (HTMLDocument)browser.Document.DomDocument;
                    var body = (HTMLBody)doc.body;
                    var range = (IHTMLControlRange)body.createControlRange();
                    var img = (IHTMLControlElement)captchaImage.DomElement;
                    range.add(img);
                    range.execCommand("Copy");
                    range.remove(0);
                    Bitmap bitmap = null;
                    if (Clipboard.ContainsImage())
                    {
                        bitmap = Clipboard.GetImage() as Bitmap;
                    }

                    if (bitmap != null)
                    {
                        var code = Recogniser.Instance.RecognizeFromImage(bitmap, captchaLength, 3,
                            new HashSet<EnumCaptchaType> { EnumCaptchaType.Number });
                        code = Common.GetNumericFromString(code);
                        LogHelper.LogInfo(GetType(), "验证码识别结果:" + code);
                        captchaInput.SetAttribute("value", code);
                        submit.InvokeMember("Click");
                    }
                }
            }
        }

        protected override void RefreshCaptcha()
        {
            if (browser.Document != null)
            {
                browser.Navigate(BaseUrl);
            }
        }

        protected override void StartGrabData()
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
                                            LogHelper.LogWarn(GetType(), "联赛名称重复！");
                                        }
                                        continue;
                                    }
                                }
                                if (!string.IsNullOrEmpty(leagueName) &&
                                    item.GetAttribute("className").Contains("displayOn"))
                                {
                                    if (!dicData.ContainsKey(leagueName))
                                    {
                                        LogHelper.LogWarn(GetType(), "未找到联赛名称！");
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
                                            LogHelper.LogWarn(GetType(), "比赛数据不完整！");
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