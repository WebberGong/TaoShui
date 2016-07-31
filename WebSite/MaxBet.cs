using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using CaptchaRecogniser;
using mshtml;
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

        protected override Action<string> PopupMsg
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

        protected override Action<string> SendData
        {
            get { return data => { LogHelper.LogInfo(GetType(), data); }; }
        }

        protected override void Login()
        {
            if (IsBrowserOk() && browser.Document != null)
            {
                var loginForm = browser.Document.Forms["frmLogin"];
                if (loginForm != null && loginForm.Document != null)
                {
                    var id = loginForm.Document.GetElementById("txtID");
                    var password = loginForm.Document.GetElementById("txtPW");
                    var aElements = loginForm.GetElementsByTagName("div");
                    var login =
                        aElements.Cast<HtmlElement>().FirstOrDefault(item => item.GetAttribute("title") == "LOGIN");
                    if (id != null && password != null && login != null)
                    {
                        browser.Document.InvokeScript("changeLan", new object[] { "cs" });
                        id.SetAttribute("value", loginName);
                        password.SetAttribute("value", loginPassword);
                        login.InvokeMember("Click");
                    }
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
            if (IsBrowserOk() && browser.Document != null)
            {
                var captchaImage = browser.Document.GetElementById("validateCode");
                var captchaRefresh = browser.Document.GetElementById("validateCode_href");
                var captchaInput = browser.Document.GetElementById("txtCode");
                var aElements = browser.Document.GetElementsByTagName("a");
                var submit = aElements.Cast<HtmlElement>().FirstOrDefault(item => item.InnerHtml == "递交");
                var divElements = browser.Document.GetElementsByTagName("div");
                var captchaDiv =
                    divElements.Cast<HtmlElement>()
                        .FirstOrDefault(item => item.GetAttribute("className").Contains("validationCode"));

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

        protected override IDictionary<string, IDictionary<string, IList<string>>> GrabData(WebBrowser wb)
        {
            IDictionary<string, IDictionary<string, IList<string>>> grabbedData = new Dictionary<string, IDictionary<string, IList<string>>>();
            if (wb != null && wb.Document != null && wb.Document.Window != null &&
                wb.Document.Window.Frames != null && wb.Document.Window.Frames.Count > 0)
            {
                var mainFrame = wb.Document.Window.Frames["mainFrame"];
                if (mainFrame != null && mainFrame.Document != null)
                {
                    var tableContainerL = mainFrame.Document.GetElementById("oTableContainer_L");
                    if (tableContainerL != null)
                    {
                        var tablesL = tableContainerL.GetElementsByTagName("TABLE");
                        foreach (HtmlElement table in tablesL)
                        {
                            var trs = table.GetElementsByTagName("TR");
                            var leagueName = string.Empty;
                            foreach (HtmlElement tr in trs)
                            {
                                if (tr.GetElementsByTagName("TD").Count == 3 &&
                                    tr.Children[1].GetAttribute("className").Contains("tabtitle") &&
                                    tr.Children[1].GetElementsByTagName("SPAN")
                                        .Cast<HtmlElement>()
                                        .FirstOrDefault(x => x.GetAttribute("Title") == "加入我的最爱") != null)
                                {
                                    var span = tr.Children[1].GetElementsByTagName("SPAN")
                                        .Cast<HtmlElement>()
                                        .FirstOrDefault(x => x.GetAttribute("Title") == "加入我的最爱");
                                    if (span != null && span.Parent != null)
                                    {
                                        leagueName = span.Parent.InnerText;
                                    }
                                    if (!string.IsNullOrEmpty(leagueName))
                                    {
                                        if (!grabbedData.ContainsKey(leagueName))
                                        {
                                            grabbedData.Add(leagueName, new Dictionary<string, IList<string>>());
                                        }
                                        else
                                        {
                                            LogHelper.LogWarn(GetType(), "联赛名称重复！\r\n" + leagueName);
                                        }
                                    }
                                }
                                else
                                {
                                    if (!string.IsNullOrEmpty(leagueName) &&
                                        tr.GetAttribute("className").Contains("displayOn"))
                                    {
                                        if (!grabbedData.ContainsKey(leagueName))
                                        {
                                            LogHelper.LogWarn(GetType(), "未找到联赛名称！");
                                        }
                                        else
                                        {
                                            StringBuilder matchName = new StringBuilder();
                                            IList<string> matchData = new List<string>();
                                            var tds = tr.GetElementsByTagName("TD");
                                            foreach (HtmlElement td in tds)
                                            {
                                                if (td.Children.Count == 2 &&
                                                    td.Children[0].TagName == "B" &&
                                                    td.Children[1].TagName == "DIV" &&
                                                    td.Children[1].GetElementsByTagName("SPAN")
                                                        .Cast<HtmlElement>()
                                                        .FirstOrDefault(
                                                            x => x.GetAttribute("className").Contains("displayOn")) !=
                                                    null)
                                                {
                                                    var b1 = td.Children[0];
                                                    var div = td.Children[1];
                                                    var span = div.GetElementsByTagName("SPAN")
                                                        .Cast<HtmlElement>()
                                                        .FirstOrDefault(
                                                            x => x.GetAttribute("className").Contains("displayOn"));
                                                    if (span != null)
                                                    {
                                                        var b2 = span.GetElementsByTagName("B")
                                                            .Cast<HtmlElement>()
                                                            .FirstOrDefault();
                                                        if (b2 != null)
                                                        {
                                                            string str1 = Common.InnerTextTrim(b1.InnerText);
                                                            string str2 = Common.InnerTextTrim(b2.InnerText);
                                                            matchData.Add(str1);
                                                            matchData.Add(str2);
                                                            matchName.Append(str1);
                                                            matchName.Append(" | " + str2);
                                                        }
                                                    }
                                                }
                                                else if (td.Children.Count == 3 &&
                                                    td.Children[0].TagName == "DIV" &&
                                                    td.Children[1].TagName == "DIV" &&
                                                    td.Children[2].TagName == "DIV" &&
                                                    td.Children[0].GetElementsByTagName("SPAN").Count > 0 &&
                                                    td.Children[1].GetElementsByTagName("SPAN").Count > 0)
                                                {
                                                    string str1 = Common.InnerTextTrim(td.Children[0].Children[0].InnerText);
                                                    string str2 = Common.InnerTextTrim(td.Children[1].Children[0].InnerText);
                                                    string str3 = Common.InnerTextTrim(td.Children[2].InnerText);
                                                    matchData.Add(str1);
                                                    matchData.Add(str2);
                                                    matchData.Add(str3);
                                                    matchName.Append(" | " + str1);
                                                    matchName.Append(" - " + str2);
                                                    matchName.Append(" | " + str3);
                                                }
                                                else if (td.Children.Count == 2 &&
                                                    td.Children[0].TagName == "DIV" &&
                                                    td.Children[1].TagName == "DIV" &&
                                                    td.Children[0].GetElementsByTagName("SPAN").Count > 0 &&
                                                    td.Children[1].GetElementsByTagName("SPAN").Count > 0)
                                                {
                                                    string str1 = Common.InnerTextTrim(td.Children[0].Children[0].InnerText);
                                                    string str2 = Common.InnerTextTrim(td.Children[1].Children[0].InnerText);
                                                    matchData.Add(str1);
                                                    matchData.Add(str2);
                                                    matchName.Append(" | " + str1);
                                                    matchName.Append(" - " + str2);
                                                }
                                                else if (td.Children.Count == 2 &&
                                                         td.GetElementsByTagName("DIV")
                                                             .Cast<HtmlElement>()
                                                             .FirstOrDefault(
                                                                 x => x.GetAttribute("className").Contains("line_divL")) !=
                                                         null &&
                                                         td.GetElementsByTagName("DIV")
                                                             .Cast<HtmlElement>()
                                                             .FirstOrDefault(
                                                                 x => x.GetAttribute("className").Contains("line_divR")) !=
                                                         null)
                                                {
                                                    var div1 = td.GetElementsByTagName("DIV")
                                                        .Cast<HtmlElement>()
                                                        .FirstOrDefault(
                                                            x => x.GetAttribute("className").Contains("line_divL"));
                                                    var div2 = td.GetElementsByTagName("DIV")
                                                        .Cast<HtmlElement>()
                                                        .FirstOrDefault(
                                                            x => x.GetAttribute("className").Contains("line_divR"));
                                                    if (div1 != null && div2 != null)
                                                    {
                                                        var aCollection = div2.GetElementsByTagName("A")
                                                            .Cast<HtmlElement>()
                                                            .Where(x => x.GetAttribute("Name") == "cvmy").ToList();
                                                        if (aCollection.Count == 2)
                                                        {
                                                            matchData.Add(Common.InnerTextTrim(div1.InnerText));
                                                            matchData.Add(Common.InnerTextTrim(aCollection[0].InnerText));
                                                            matchData.Add(Common.InnerTextTrim(aCollection[1].InnerText));
                                                        }
                                                    }
                                                }
                                                else if (td.Children.Count == 1 &&
                                                         td.GetElementsByTagName("DIV")
                                                             .Cast<HtmlElement>()
                                                             .FirstOrDefault(
                                                                 x => x.GetAttribute("className").Contains("line_divL line_divR")) !=
                                                         null)
                                                {
                                                    var div1 = td.GetElementsByTagName("DIV")
                                                        .Cast<HtmlElement>()
                                                        .FirstOrDefault(
                                                            x =>
                                                                x.GetAttribute("className")
                                                                    .Contains("line_divL line_divR"));
                                                    if (div1 != null && div1.GetElementsByTagName("A").Count == 3)
                                                    {
                                                        var aCollection = div1.GetElementsByTagName("A");
                                                        matchData.Add(Common.InnerTextTrim(aCollection[0].InnerText));
                                                        matchData.Add(Common.InnerTextTrim(aCollection[1].InnerText));
                                                        matchData.Add(Common.InnerTextTrim(aCollection[2].InnerText));
                                                    }
                                                }
                                            }
                                            if (grabbedData[leagueName].ContainsKey(matchName.ToString()))
                                            {
                                                LogHelper.LogWarn(GetType(), "比赛名称重复！\r\n" + matchName);
                                            }
                                            else
                                            {
                                                grabbedData[leagueName].Add(matchName.ToString(), matchData);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return grabbedData;
        }
    }
}