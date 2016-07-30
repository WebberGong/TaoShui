using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using CaptchaRecogniser;
using DataGrabber;
using mshtml;
using Utils;

namespace WebSite
{
    public class MaxBet : WebSiteBase
    {
        public MaxBet(string loginName, string loginPassword, int captchaLength,
            int loginTimeOut = 10, int grabDataTimeOut = 10)
            : base(loginName, loginPassword, captchaLength, loginTimeOut, grabDataTimeOut)
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

        protected override IDictionary<string, string> GrabDataUrlDictionary
        {
            get
            {
                IDictionary<string, string> grabDataUrlDic = new Dictionary<string, string>();
                grabDataUrlDic.Add("1", "miniOdds_data.aspx?OddsType=&_=" + DateTime.Now.Ticks);
                grabDataUrlDic.Add("2", "LiveStreamingData.aspx?OddsType=&_=" + DateTime.Now.Ticks);
                grabDataUrlDic.Add("3", "UnderOver_data.aspx?Market=l&Sport=1&DT=&RT=W&CT=&Game=0&OrderBy=0&OddsType=4&MainLeague=0&k1169006800=v1169006972&key=QV1BW15AWV9AR05AWE9fXwVaCQUaREcRSV4bOl44IjocWh4dQBhFDBUZCQAXRlJdCUoWABtbCg4G%0AGBFsQBBB&_=" + DateTime.Now.Ticks);
                grabDataUrlDic.Add("4", "UnderOver_data.aspx?Market=t&Sport=1&DT=&RT=W&CT=&Game=0&OrderBy=0&OddsType=4&MainLeague=0&DispRang=0&k1169006800=v1169006972&key=XFleXlFWWUZYXFtLQVtVCAEXXARIARVdWAFsBWY9bR9FAR1GFkcCAxkSDRFaEQ4KTR0JEkVWQU5e%0AWlw%3DKPKE&_=" + DateTime.Now.Ticks);
                return grabDataUrlDic;
            }
        }

        protected override Action<WebSiteStatus> LoginStatusChanged
        {
            get { return loginStatus => { LogHelper.LogInfo(GetType(), "登录状态: " + loginStatus.ToString()); }; }
        }

        protected override Action<string> PopupMsgHandler
        {
            get
            {
                return msg =>
                {
                    switch (msg)
                    {
                        case "帐号/密码错误":
                            LoginStatus = WebSiteStatus.LoginFailed;
                            break;
                        case "验证码错误":
                            DoRefreshCaptcha();
                            break;
                        default:
                            LogHelper.LogInfo(GetType(), msg);
                            break;
                    }
                };
            }
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
                        browser.Document.InvokeScript("changeLan", new object[] {"cs"});
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
                        .FirstOrDefault(item => item.GetAttribute("className") == "validationCode");

                if (browser.Document.Window != null && browser.Document.Window.Parent != null &&
                    captchaImage != null && captchaRefresh != null && captchaInput != null && submit != null &&
                    captchaDiv != null)
                {
                    var doc = (HTMLDocument) browser.Document.DomDocument;
                    var body = (HTMLBody) doc.body;
                    var range = (IHTMLControlRange) body.createControlRange();
                    var img = (IHTMLControlElement) captchaImage.DomElement;
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
                            new HashSet<EnumCaptchaType> {EnumCaptchaType.Number});
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

        protected override void GrabData()
        {
            if (browser != null && browser.Document != null && browser.Document.Window != null &&
                browser.Document.Window.Frames != null && browser.Document.Window.Frames.Count > 0)
            {
                var htmlWindow = browser.Document.Window.Frames["mainFrame"];
                if (htmlWindow != null)
                {
                    if (htmlWindow.Frames != null)
                    {
                        var frame = htmlWindow.Frames["DataFrame_L"];
                    }
                }
                var win = (IHTMLWindow2)browser.Document.Window.DomWindow;
                const string js =
                    @"this.top.grabData = function() {
                        try {
                            var data = JSON.stringify({
                                D1: JSON.stringify(this.top.frames['mainFrame'].frames['DataFrame_L'].Nl),
                                D2: JSON.stringify(this.top.frames['mainFrame'].frames['DataFrame_D'].Nt),
                                D3: this.top.frames['mainFrame'].frames['DataFrame_L'].document.head.innerHTML,
                                D4: this.top.frames['mainFrame'].frames['DataFrame_D'].document.head.innerHTML
                            });
                            window.external.PopupMsgHandler(data);
                            alert('data');
                        }
                        catch(ex){
                            console.log(ex);
                        }
                    }
                    setInterval(this.top.grabData, 6000);
                    this.top.grabData();";
                win.execScript(js, "javascript");
            }

            //if (GrabDataUrlDictionary != null && GrabDataUrlDictionary.Count > 0 &&
            //    !string.IsNullOrEmpty(grabDataBaseUrl) && !string.IsNullOrEmpty(cookie))
            //{
            //    IDictionary<string, string> grabDataUrlDic = new Dictionary<string, string>();
            //    foreach (var item in GrabDataUrlDictionary)
            //    {
            //        grabDataUrlDic.Add(item.Key, grabDataBaseUrl + item.Value);
            //    }
            //    string html = string.Empty;
            //    for (int i = 0; i < 20; i++)
            //    {
            //        if (!string.IsNullOrEmpty(html) && html != browser.DocumentText)
            //        {
            //            html = browser.DocumentText;
            //        }
            //        Thread.Sleep(10000);
            //    }
            //    var grabedData = Grabber.Instance.Run(grabDataUrlDic, cookie, grabDataTimeOut);
            //    Console.WriteLine(grabedData);
            //}
        }
    }
}