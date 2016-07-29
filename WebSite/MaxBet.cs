using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
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
            int loginTimeOut = 10, int getGrabDataUrlTimeOut = 10)
            : base(loginName, loginPassword, captchaLength, loginTimeOut, getGrabDataUrlTimeOut)
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

        protected override IDictionary<string, Regex> GrabDataUrlRegexDictionary
        {
            get
            {
                IDictionary<string, Regex> grabDataUrlRegexeDic = new Dictionary<string, Regex>();
                grabDataUrlRegexeDic.Add("1", new Regex("miniOdds_data\\.aspx"));
                grabDataUrlRegexeDic.Add("2", new Regex("LiveStreamingData\\.aspx"));
                grabDataUrlRegexeDic.Add("3", new Regex("UnderOver_data\\.aspx\\?Market=l"));
                grabDataUrlRegexeDic.Add("4", new Regex("UnderOver_data.aspx?Market=t"));
                return grabDataUrlRegexeDic;
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
                    if (msg == "验证码错误!")
                    {
                        DoRefreshCaptcha();
                    }
                };
            }
        }

        protected override void Login()
        {
            if (IsBrowserOk() && browser.Document != null)
            {
                var id = browser.Document.GetElementById("txtID");
                var password = browser.Document.GetElementById("txtPW");
                var aElements = browser.Document.GetElementsByTagName("a");
                var login =
                    aElements.Cast<HtmlElement>().FirstOrDefault(item => item.GetAttribute("className") == "login_btn");
                if (id != null && password != null && login != null)
                {
                    browser.Document.InvokeScript("changeLan", new object[] {"cs"});
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
            var grabber = new Grabber();
            if (grabDataUrlDictionary != null && grabDataUrlDictionary.Count > 0 && !string.IsNullOrEmpty(cookie))
            {
                var grabedData = grabber.Run(grabDataUrlDictionary, cookie);
                Console.WriteLine(grabedData);
            }
        }
    }
}