using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using mshtml;
using Utils;
using Timer = System.Timers.Timer;

namespace WebSite
{
    public abstract class WebSiteBase
    {
        private readonly int _captchaValidateMaxCount = 3;
        private int _captchaValidateCount;
        private WebSiteState _loginStatus;
        private Timer _loginTimer;
        private DateTime _startLoginTime;
        private Timer _getGrabDataUrlTimer;
        private DateTime _startGetGrabDataUrlTime;

        protected WebBrowser browser;
        protected int captchaLength;
        protected string cookie;
        protected IDictionary<string, string> grabDataUrlDictionary;
        protected string loginName;
        protected string loginPassword;
        protected int loginTimeOut;
        protected int getGrabDataUrlTimeOut;

        protected WebSiteBase(string loginName, string loginPassword, int captchaLength, int loginTimeOut, int getGrabDataUrlTimeOut)
        {
            this.loginName = loginName;
            this.loginPassword = loginPassword;
            this.captchaLength = captchaLength;
            this.loginTimeOut = loginTimeOut;
            this.getGrabDataUrlTimeOut = getGrabDataUrlTimeOut;
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

        public int GetGrabDataUrlTimeOut
        {
            get { return getGrabDataUrlTimeOut; }
        }

        public WebSiteState LoginStatus
        {
            get { return _loginStatus; }
            private set
            {
                if (_loginStatus != value)
                {
                    _loginStatus = value;
                    if (LoginStatusChanged != null)
                    {
                        LoginStatusChanged(_loginStatus);
                    }
                    if (_loginStatus == WebSiteState.LoginSuccessful)
                    {
                        if (EndLogin != null)
                        {
                            EndLogin(true);
                        }
                    }
                    else if (_loginStatus == WebSiteState.LoginFailed)
                    {
                        if (EndLogin != null)
                        {
                            EndLogin(false);
                        }
                    }
                }
            }
        }

        protected abstract Uri BaseUrl { get; }
        protected abstract Regex LoginPageRegex { get; }
        protected abstract Regex CaptchaInputPageRegex { get; }
        protected abstract Regex MainPageRegex { get; }
        protected abstract IDictionary<string, Regex> GrabDataUrlRegexDictionary { get; }
        protected abstract Action<bool> EndLogin { get; }
        protected abstract Action<IDictionary<string, IList<string>>> EndGrabData { get; }
        protected abstract Action<WebSiteState> LoginStatusChanged { get; }
        protected abstract Action<string> PopupMsgHandler { get; }

        protected abstract void StartLogin();
        protected abstract bool IsCaptchaInputPageLoaded();
        protected abstract void CaptchaValidate();
        protected abstract void RefreshCaptcha();
        protected abstract void StartGrabData();

        private void Initialize()
        {
            TimeSpan tsLoginTimeOut = new TimeSpan(0, 0, loginTimeOut);
            _startLoginTime = DateTime.Now;
            _loginTimer = new Timer(200);
            _loginTimer.Elapsed += (sender, ev) =>
            {
                Application.DoEvents();

                if (DateTime.Now > _startLoginTime.Add(tsLoginTimeOut))
                {
                    _loginTimer.Enabled = false;
                    _loginTimer.Stop();
                    LoginStatus = WebSiteState.LoginFailed;
                }
                else
                {
                    if (LoginStatus == WebSiteState.LoginSuccessful ||
                        LoginStatus == WebSiteState.LoginFailed)
                    {
                        _loginTimer.Enabled = false;
                        _loginTimer.Stop();
                    }
                }
            };
            _loginTimer.AutoReset = true;
            _loginTimer.Enabled = false;

            TimeSpan tsGetGrabDataUrlTimeOut = new TimeSpan(0, 0, getGrabDataUrlTimeOut);
            _startGetGrabDataUrlTime = DateTime.Now;
            _getGrabDataUrlTimer = new Timer(200);
            _getGrabDataUrlTimer.Elapsed += (sender, ev) =>
            {
                Application.DoEvents();

                if (DateTime.Now > _startGetGrabDataUrlTime.Add(tsGetGrabDataUrlTimeOut))
                {
                    _getGrabDataUrlTimer.Enabled = false;
                    _getGrabDataUrlTimer.Stop();
                    LoginStatus = WebSiteState.End;

                    StartGrabData();
                }
                else
                {
                    if (LoginStatus == WebSiteState.End)
                    {
                        _getGrabDataUrlTimer.Enabled = false;
                        _getGrabDataUrlTimer.Stop();

                        StartGrabData();
                    }
                }
            };
            _getGrabDataUrlTimer.AutoReset = true;
            _getGrabDataUrlTimer.Enabled = false;

            _loginStatus = WebSiteState.Start;
            _captchaValidateCount = 0;

            grabDataUrlDictionary = new Dictionary<string, string>();
            cookie = string.Empty;
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
                using (var waiter = new WebBrowserWaiter.WebBrowserWaiter(PopupMsgHandler))
                {
                    browser = waiter.Browser;

                    browser.Navigated -= WebSiteNavigated;
                    browser.DocumentCompleted -= LoginPageLoaded;
                    browser.DocumentCompleted -= CaptchaInputPageLoaded;
                    browser.Navigating -= MainPageNavigating;
                    browser.Navigated += WebSiteNavigated;
                    browser.DocumentCompleted += LoginPageLoaded;
                    browser.DocumentCompleted += CaptchaInputPageLoaded;
                    browser.Navigating += MainPageNavigating;

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
            thread.SetApartmentState(ApartmentState.MTA);
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
                LoginStatus = WebSiteState.LoginFailed;
            }
        }

        public void DoRefreshCaptcha()
        {
            RefreshCaptcha();
        }

        private void WebSiteNavigated(object sender, WebBrowserNavigatedEventArgs e)
        {
            var url = e.Url.ToString();
            var webBrowser = sender as WebBrowser;
            if (webBrowser != null && webBrowser.Document != null && webBrowser.Document.Window != null)
            {
                var win = (IHTMLWindow2) webBrowser.Document.Window.DomWindow;
                const string js =
                    @"window.alert = function(msg) { window.external.PopupMsgHandler(msg); return true; }; 
                    window.onerror = function() { return true; };
                    window.confirm = function() { return true; }; 
                    window.open = function() { return true; }; 
                    window.showModalDialog = function() { return true; };";
                win.execScript(js, "javascript");
            }

            foreach (var item in GrabDataUrlRegexDictionary)
            {
                if (item.Value.IsMatch(url))
                {
                    if (!grabDataUrlDictionary.ContainsKey(item.Key))
                    {
                        grabDataUrlDictionary.Add(item.Key, url);
                        if (grabDataUrlDictionary.Count == GrabDataUrlRegexDictionary.Count)
                        {
                            LoginStatus = WebSiteState.End;
                        }
                    }
                }
            }

            LogHelper.LogInfo(GetType(), "页面跳转:" + url);
        }

        private void LoginPageLoaded(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            var url = e.Url.ToString();

            if (LoginPageRegex != null && LoginPageRegex.IsMatch(url))
            {
                LoginStatus = WebSiteState.Logging;
                _startLoginTime = DateTime.Now;
                _loginTimer.Enabled = true;
                _loginTimer.Start();

                StartLogin();
            }
        }

        private void CaptchaInputPageLoaded(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            var url = e.Url.ToString();

            if (IsCaptchaInputPageLoaded() && CaptchaInputPageRegex != null && CaptchaInputPageRegex.IsMatch(url))
            {
                LoginStatus = WebSiteState.CaptchaValidating;
                DoCaptchaValidate();
            }
        }

        private void MainPageNavigating(object sender, WebBrowserNavigatingEventArgs e)
        {
            var url = e.Url.ToString();

            if (MainPageRegex != null && MainPageRegex.IsMatch(url))
            {
                var webBrowser = sender as WebBrowser;
                if (webBrowser != null && webBrowser.Document != null)
                {
                    cookie = webBrowser.Document.Cookie;
                }

                LoginStatus = WebSiteState.LoginSuccessful;
                _startGetGrabDataUrlTime = DateTime.Now;
                _getGrabDataUrlTimer.Enabled = true;
                _getGrabDataUrlTimer.Start();
            }
        }
    }
}