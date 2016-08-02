using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading;
using Awesomium.Core;
using Awesomium.Windows.Forms;
using CaptchaRecogniser;
using Newtonsoft.Json;
using Utils;
using Timer = System.Timers.Timer;

namespace WebSite
{
    public abstract class WebSiteBase
    {
        public delegate void GrabDataSuccessHandler(IDictionary<string, IDictionary<string, IList<string>>> grabbedData);

        public delegate void WebSiteStatusChangedHandler(WebSiteStatus webSiteStatus);

        protected const string Undefined = "undefined";
        protected const string True = "true";
        protected const string False = "false";

        private readonly int _captchaValidateMaxCount = 3;
        private int _captchaValidateCount;
        private Timer _loginTimer;
        private DateTime _startLoginTime;
        private WebSiteStatus _webSiteStatus;

        protected int captchaLength;
        protected int grabDataInterval;
        protected string loginName;
        protected string loginPassword;
        protected int loginTimeOut;

        protected WebSiteBase(string loginName, string loginPassword, int captchaLength, int loginTimeOut,
            int grabDataInterval)
        {
            this.loginName = loginName;
            this.loginPassword = loginPassword;
            this.captchaLength = captchaLength;
            this.loginTimeOut = loginTimeOut;
            this.grabDataInterval = grabDataInterval;
        }

        public string LoginName
        {
            get { return loginName; }
        }

        public string LoginPassword
        {
            get { return loginPassword; }
        }

        public int CaptchaLength
        {
            get { return captchaLength; }
        }

        public int LoginTimeOut
        {
            get { return loginTimeOut; }
        }

        public int GrabDataInterval
        {
            get { return grabDataInterval; }
        }

        public WebSiteStatus WebSiteStatus
        {
            get { return _webSiteStatus; }
            set
            {
                if (_webSiteStatus != value)
                {
                    _webSiteStatus = value;
                    OnWebSiteStatusChanged(_webSiteStatus);
                }
            }
        }

        protected abstract Uri BaseUrl { get; }
        protected abstract Regex ChangeLanguageRegex { get; }
        protected abstract Regex LoginPageRegex { get; }
        protected abstract Regex CaptchaInputPageRegex { get; }
        protected abstract Regex MainPageRegex { get; }
        protected abstract Action<WebControl, string> ShowJavascriptDialog { get; }

        protected abstract void ChangeLanguage(WebControl browser);
        protected abstract void Login(WebControl browser);
        protected abstract bool IsCaptchaInputPageReady(WebControl browser);
        protected abstract void CaptchaValidate(WebControl browser);
        protected abstract void RefreshCaptcha(WebControl browser);
        protected abstract IDictionary<string, IDictionary<string, IList<string>>> GrabData(WebControl browser);
        public event WebSiteStatusChangedHandler WebSiteStatusChanged;
        public event GrabDataSuccessHandler GrabDataSuccess;

        private void Initialize()
        {
            var tsLoginTimeOut = new TimeSpan(0, 0, loginTimeOut);
            _startLoginTime = DateTime.Now;
            _loginTimer = new Timer(200);
            _loginTimer.Elapsed += (sender, ev) =>
            {
                if (DateTime.Now > _startLoginTime.Add(tsLoginTimeOut) &&
                    WebSiteStatus != WebSiteStatus.LoginSuccessful)
                {
                    _loginTimer.Enabled = false;
                    _loginTimer.Stop();
                    WebSiteStatus = WebSiteStatus.LoginFailed;
                }
                else
                {
                    if (WebSiteStatus == WebSiteStatus.LoginSuccessful ||
                        WebSiteStatus == WebSiteStatus.LoginFailed)
                    {
                        _loginTimer.Enabled = false;
                        _loginTimer.Stop();
                    }
                }
            };
            _loginTimer.AutoReset = true;
            _loginTimer.Enabled = false;

            _webSiteStatus = WebSiteStatus.NotLogin;
            _captchaValidateCount = 0;
        }

        protected bool IsBrowserOk(WebControl browser)
        {
            return browser != null && browser.IsLive && !browser.IsCrashed && !browser.IsDisposed &&
                   browser.IsDocumentReady;
        }

        public void Run()
        {
            Initialize();

            var thread = new Thread(() =>
            {
                using (var waiter = new WebControlWaiter.WebControlWaiter(true))
                {
                    waiter.Browser.LoadingFrame -= WebSiteLoading;
                    waiter.Browser.LoadingFrameComplete -= WebSiteLoadingComplete;
                    waiter.Browser.LoadingFrameComplete -= ChangeLanguagePageLoadingComplete;
                    waiter.Browser.LoadingFrameComplete -= LoginPageLoadingComplete;
                    waiter.Browser.LoadingFrameComplete -= CaptchaInputPageLoadingComplete;
                    waiter.Browser.LoadingFrameComplete -= MainPageLoadingComplete;
                    waiter.Browser.JavascriptMessage -= JavascriptMessageHandler;
                    waiter.Browser.ShowJavascriptDialog -= ShowJavascriptDialogHandler;
                    waiter.Browser.LoadingFrame += WebSiteLoading;
                    waiter.Browser.LoadingFrameComplete += WebSiteLoadingComplete;
                    waiter.Browser.LoadingFrameComplete += ChangeLanguagePageLoadingComplete;
                    waiter.Browser.LoadingFrameComplete += LoginPageLoadingComplete;
                    waiter.Browser.LoadingFrameComplete += CaptchaInputPageLoadingComplete;
                    waiter.Browser.LoadingFrameComplete += MainPageLoadingComplete;
                    waiter.Browser.JavascriptMessage += JavascriptMessageHandler;
                    waiter.Browser.ShowJavascriptDialog += ShowJavascriptDialogHandler;

                    waiter.Await(
                        wb => wb.Source = BaseUrl
                        );

                    while (WebSiteStatus != WebSiteStatus.LoginSuccessful &&
                           WebSiteStatus != WebSiteStatus.LoginFailed)
                    {
                        Thread.Sleep(50);
                    }

                    while (WebSiteStatus == WebSiteStatus.LoginSuccessful)
                    {
                        var watch = new Stopwatch();
                        watch.Start();
                        var data = waiter.Await(
                            wb => GrabData(wb)
                            );
                        watch.Stop();
                        var elapsedTimeMsg = "抓取数据耗时:" + watch.ElapsedMilliseconds;
                        LogHelper.LogInfo(GetType(), elapsedTimeMsg);
                        Console.WriteLine(elapsedTimeMsg);
                        OnGrabDataSuccess(data);
                        Thread.Sleep(grabDataInterval*1000);
                    }
                }
            })
            {
                Priority = ThreadPriority.AboveNormal,
                IsBackground = true,
                Name = "WebBrowserThread"
            };
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }

        public void DoCaptchaValidate(WebControl browser)
        {
            if (_captchaValidateCount < _captchaValidateMaxCount)
            {
                CaptchaValidate(browser);
                _captchaValidateCount++;
            }
            else
            {
                WebSiteStatus = WebSiteStatus.LoginFailed;
            }
        }

        public void DoRefreshCaptcha(WebControl browser)
        {
            RefreshCaptcha(browser);
        }

        private void JavascriptMessageHandler(object sender, JavascriptMessageEventArgs e)
        {
            var msg = e.Message;
            LogHelper.LogWarn(GetType(), msg);
        }

        private void ShowJavascriptDialogHandler(object sender, JavascriptDialogEventArgs e)
        {
            var browser = sender as WebControl;
            var msg = e.Message;
            LogHelper.LogWarn(GetType(), msg);
            ShowJavascriptDialog(browser, msg);
            e.Handled = true;
        }

        private void WebSiteLoading(object sender, LoadingFrameEventArgs e)
        {
            var url = e.Url.ToString();
            LogHelper.LogInfo(GetType(), "页面正在加载:" + url);
        }

        private void WebSiteLoadingComplete(object sender, FrameEventArgs e)
        {
            if (e.IsMainFrame)
            {
                var url = e.Url.ToString();
                LogHelper.LogInfo(GetType(), "页面加载成功:" + url);
            }
        }

        private void ChangeLanguagePageLoadingComplete(object sender, FrameEventArgs e)
        {
            if (e.IsMainFrame)
            {
                var browser = sender as WebControl;
                var url = e.Url.ToString();

                if (ChangeLanguageRegex != null && ChangeLanguageRegex.IsMatch(url))
                {
                    ChangeLanguage(browser);
                }
            }
        }

        private void LoginPageLoadingComplete(object sender, FrameEventArgs e)
        {
            if (e.IsMainFrame)
            {
                var browser = sender as WebControl;
                var url = e.Url.ToString();

                if (LoginPageRegex != null && LoginPageRegex.IsMatch(url))
                {
                    WebSiteStatus = WebSiteStatus.Logging;
                    _startLoginTime = DateTime.Now;
                    _loginTimer.Enabled = true;
                    _loginTimer.Start();

                    Login(browser);
                }
            }
        }

        private void CaptchaInputPageLoadingComplete(object sender, FrameEventArgs e)
        {
            if (e.IsMainFrame)
            {
                var browser = sender as WebControl;
                var url = e.Url.ToString();

                if (IsCaptchaInputPageReady(browser) && CaptchaInputPageRegex != null &&
                    CaptchaInputPageRegex.IsMatch(url))
                {
                    WebSiteStatus = WebSiteStatus.CaptchaValidating;
                    DoCaptchaValidate(browser);
                }
            }
        }

        private void MainPageLoadingComplete(object sender, FrameEventArgs e)
        {
            if (e.IsMainFrame)
            {
                var url = e.Url.ToString();

                if (MainPageRegex != null && MainPageRegex.IsMatch(url))
                {
                    WebSiteStatus = WebSiteStatus.LoginSuccessful;
                }
            }
        }

        private void OnWebSiteStatusChanged(WebSiteStatus webSiteStatus)
        {
            LogHelper.LogInfo(GetType(), "网站状态: " + webSiteStatus);

            var handler = WebSiteStatusChanged;
            if (handler != null)
            {
                handler(webSiteStatus);
            }
        }

        private void OnGrabDataSuccess(IDictionary<string, IDictionary<string, IList<string>>> grabbedData)
        {
            var matchCount = 0;
            foreach (var item in grabbedData)
            {
                matchCount += item.Value.Count;
            }
            LogHelper.LogInfo(GetType(), string.Format("抓取数据成功, 联赛数: {0}, 比赛数: {1}", grabbedData.Count, matchCount));
            Console.WriteLine(JsonConvert.SerializeObject(grabbedData));

            var handler = GrabDataSuccess;
            if (handler != null)
            {
                handler(grabbedData);
            }
        }
    }
}