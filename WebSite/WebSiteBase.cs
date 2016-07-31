using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using mshtml;
using Newtonsoft.Json;
using Utils;
using WebBrowserWaiter;
using Timer = System.Timers.Timer;

namespace WebSite
{
    public abstract class WebSiteBase
    {
        public delegate void GrabDataSuccessHandler(IDictionary<string, IDictionary<string, IList<string>>> grabbedData);
        public delegate void WebSiteStatusChangedHandler(WebSiteStatus webSiteStatus);

        private readonly int _captchaValidateMaxCount = 3;
        private int _captchaValidateCount;
        private Timer _loginTimer;
        private DateTime _startLoginTime;
        private WebSiteStatus _webSiteStatus;

        protected WebBrowser browser;
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
        protected abstract Regex LoginPageRegex { get; }
        protected abstract Regex CaptchaInputPageRegex { get; }
        protected abstract Regex MainPageRegex { get; }
        protected abstract Action<string> PopupMsg { get; }
        protected abstract Action<string> SendData { get; }

        protected abstract void Login();
        protected abstract bool IsCaptchaInputPageLoaded();
        protected abstract void CaptchaValidate();
        protected abstract void RefreshCaptcha();
        protected abstract IDictionary<string, IDictionary<string, IList<string>>> GrabData(WebBrowser browser);
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
                    Stop();
                }
                else
                {
                    if (WebSiteStatus == WebSiteStatus.LoginSuccessful ||
                        WebSiteStatus == WebSiteStatus.LoginFailed)
                    {
                        _loginTimer.Enabled = false;
                        _loginTimer.Stop();

                        if (WebSiteStatus == WebSiteStatus.LoginFailed)
                        {
                            Stop();
                        }
                    }
                }
            };
            _loginTimer.AutoReset = true;
            _loginTimer.Enabled = false;

            _webSiteStatus = WebSiteStatus.NotLogin;
            _captchaValidateCount = 0;
        }

        protected bool IsBrowserOk()
        {
            return browser != null && !browser.IsDisposed && browser.Document != null;
        }

        public void Run()
        {
            Initialize();

            var thread = new Thread(() =>
            {
                using (var waiter = new WebBrowserWaiter.WebBrowserWaiter(new MessageHandler(PopupMsg, SendData), true, true))
                {
                    browser = waiter.Browser;

                    browser.Navigating -= WebSiteNavigating;
                    browser.Navigated -= WebSiteNavigated;
                    browser.DocumentCompleted -= WebSiteDocumentCompleted;
                    browser.DocumentCompleted -= LoginPageLoaded;
                    browser.DocumentCompleted -= CaptchaInputPageLoaded;
                    browser.DocumentCompleted -= MainPageLoaded;
                    browser.Navigating += WebSiteNavigating;
                    browser.Navigated += WebSiteNavigated;
                    browser.DocumentCompleted += WebSiteDocumentCompleted;
                    browser.DocumentCompleted += LoginPageLoaded;
                    browser.DocumentCompleted += CaptchaInputPageLoaded;
                    browser.DocumentCompleted += MainPageLoaded;

                    waiter.Await(
                            wb => wb.Navigate(BaseUrl)
                        );

                    while (WebSiteStatus == WebSiteStatus.LoginSuccessful)
                    {
                        Stopwatch watch = new Stopwatch();
                        watch.Start();
                        var data = waiter.Await(
                            wb => GrabData(wb)
                            );
                        watch.Stop();
                        string elapsedTimeMsg = "抓取数据耗时:" + watch.ElapsedMilliseconds;
                        LogHelper.LogInfo(GetType(), elapsedTimeMsg);
                        Console.WriteLine(elapsedTimeMsg);
                        OnGrabDataSuccess(data);
                        Thread.Sleep(grabDataInterval * 1000);
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

        public void DoCaptchaValidate()
        {
            if (_captchaValidateCount < _captchaValidateMaxCount)
            {
                CaptchaValidate();
                _captchaValidateCount++;
            }
            else
            {
                WebSiteStatus = WebSiteStatus.LoginFailed;
            }
        }

        public void DoRefreshCaptcha()
        {
            RefreshCaptcha();
        }

        private void WebSiteNavigating(object sender, WebBrowserNavigatingEventArgs e)
        {
            var url = e.Url.ToString();
            LogHelper.LogInfo(GetType(), "正在跳转页面:" + url);
        }

        private void WebSiteNavigated(object sender, WebBrowserNavigatedEventArgs e)
        {
            var url = e.Url.ToString();
            LogHelper.LogInfo(GetType(), "页面跳转成功:" + url);

            var webBrowser = sender as WebBrowser;
            if (webBrowser != null && webBrowser.Document != null && webBrowser.Document.Window != null)
            {
                var win = (IHTMLWindow2)webBrowser.Document.Window.DomWindow;
                const string js =
                    @"window.alert = function(msg) { window.external.PopupMsg(msg); return true; }; 
                    window.onerror = function() { return true; };
                    window.confirm = function() { return true; }; 
                    window.open = function() { return true; }; 
                    window.showModalDialog = function() { return true; };";
                win.execScript(js, "javascript");
            }
        }

        private void WebSiteDocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            var url = e.Url.ToString();
            LogHelper.LogInfo(GetType(), "页面加载成功:" + url);
        }

        private void LoginPageLoaded(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            var url = e.Url.ToString();

            if (LoginPageRegex != null && LoginPageRegex.IsMatch(url))
            {
                WebSiteStatus = WebSiteStatus.Logging;
                _startLoginTime = DateTime.Now;
                _loginTimer.Enabled = true;
                _loginTimer.Start();

                Login();
            }
        }

        private void CaptchaInputPageLoaded(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            var url = e.Url.ToString();

            if (IsCaptchaInputPageLoaded() && CaptchaInputPageRegex != null && CaptchaInputPageRegex.IsMatch(url))
            {
                WebSiteStatus = WebSiteStatus.CaptchaValidating;
                DoCaptchaValidate();
            }
        }

        private void MainPageLoaded(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            var url = e.Url.ToString();

            if (MainPageRegex != null && MainPageRegex.IsMatch(url))
            {
                WebSiteStatus = WebSiteStatus.LoginSuccessful;
            }
        }

        private void Stop()
        {
            browser.Stop();
            browser.Dispose();
            browser = null;
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
            int matchCount = 0;
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