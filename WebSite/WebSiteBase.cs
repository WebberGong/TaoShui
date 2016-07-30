using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using mshtml;
using Utils;
using WebBrowserWaiter;
using Timer = System.Timers.Timer;

namespace WebSite
{
    public abstract class WebSiteBase
    {
        private readonly int _captchaValidateMaxCount = 3;
        private int _captchaValidateCount;
        private WebSiteStatus _loginStatus;
        private Timer _loginTimer;
        private DateTime _startLoginTime;

        protected WebBrowser browser;
        protected int captchaLength;
        protected string cookie;
        protected string grabDataBaseUrl;
        protected int grabDataTimeOut;
        protected string loginName;
        protected string loginPassword;
        protected int loginTimeOut;

        protected WebSiteBase(string loginName, string loginPassword, int captchaLength, int loginTimeOut,
            int grabDataTimeOut)
        {
            this.loginName = loginName;
            this.loginPassword = loginPassword;
            this.captchaLength = captchaLength;
            this.loginTimeOut = loginTimeOut;
            this.grabDataTimeOut = grabDataTimeOut;
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

        public int GrabDataTimeOut
        {
            get { return grabDataTimeOut; }
        }

        public WebSiteStatus LoginStatus
        {
            get { return _loginStatus; }
            set
            {
                if (_loginStatus != value)
                {
                    _loginStatus = value;
                    if (LoginStatusChanged != null)
                    {
                        LoginStatusChanged(_loginStatus);
                    }
                }
            }
        }

        protected abstract Uri BaseUrl { get; }
        protected abstract Regex LoginPageRegex { get; }
        protected abstract Regex CaptchaInputPageRegex { get; }
        protected abstract Regex MainPageRegex { get; }
        protected abstract IDictionary<string, string> GrabDataUrlDictionary { get; }
        protected abstract Action<WebSiteStatus> LoginStatusChanged { get; }
        protected abstract Action<string> PopupMsg { get; }
        protected abstract Action<string> SendData { get; }

        protected abstract void Login();
        protected abstract bool IsCaptchaInputPageLoaded();
        protected abstract void CaptchaValidate();
        protected abstract void RefreshCaptcha();
        protected abstract void GrabData();

        private void Initialize()
        {
            var tsLoginTimeOut = new TimeSpan(0, 0, loginTimeOut);
            _startLoginTime = DateTime.Now;
            _loginTimer = new Timer(200);
            _loginTimer.Elapsed += (sender, ev) =>
            {
                Application.DoEvents();

                if (DateTime.Now > _startLoginTime.Add(tsLoginTimeOut))
                {
                    _loginTimer.Enabled = false;
                    _loginTimer.Stop();
                    LoginStatus = WebSiteStatus.LoginFailed;
                    Stop();
                }
                else
                {
                    if (LoginStatus == WebSiteStatus.LoginSuccessful ||
                        LoginStatus == WebSiteStatus.LoginFailed)
                    {
                        _loginTimer.Enabled = false;
                        _loginTimer.Stop();

                        if (LoginStatus == WebSiteStatus.LoginFailed)
                        {
                            Stop();
                        }
                    }
                }
            };
            _loginTimer.AutoReset = true;
            _loginTimer.Enabled = false;

            _loginStatus = WebSiteStatus.NotLogin;
            _captchaValidateCount = 0;

            cookie = string.Empty;
            grabDataBaseUrl = string.Empty;
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
                using (var waiter = new WebBrowserWaiter.WebBrowserWaiter(new MessageHandler(PopupMsg, SendData), true))
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
                }
            })
            {
                Priority = ThreadPriority.Normal,
                IsBackground = true,
                Name = "WebBrowserThread"
            };
            thread.SetApartmentState(ApartmentState.STA);
            thread.Priority = ThreadPriority.Highest;
            thread.IsBackground = true;
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
                LoginStatus = WebSiteStatus.LoginFailed;
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
                LoginStatus = WebSiteStatus.Logging;
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
                LoginStatus = WebSiteStatus.CaptchaValidating;
                DoCaptchaValidate();
            }
        }

        private void MainPageLoaded(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            var url = e.Url.ToString();

            if (MainPageRegex != null && MainPageRegex.IsMatch(url))
            {
                int index = MainPageRegex.Match(url).Index;
                grabDataBaseUrl = url.Substring(0, index);

                var webBrowser = sender as WebBrowser;
                if (webBrowser != null && webBrowser.Document != null)
                {
                    cookie = webBrowser.Document.Cookie;
                    LogHelper.LogInfo(GetType(), cookie);
                }

                LoginStatus = WebSiteStatus.LoginSuccessful;

                GrabData();
            }
        }

        private void Stop()
        {
            browser.Stop();
            browser.Dispose();
            browser = null;
        }
    }
}