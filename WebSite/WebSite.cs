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
    public abstract class WebSite
    {
        private readonly int _captchaValidateMaxCount = 3;
        private int _captchaValidateCount;
        private EnumLoginStatus _loginStatus;
        private Timer _loginTimer;
        private DateTime _startTime;

        protected WebBrowser browser;
        protected int captchaLength;
        protected string loginName;
        protected string loginPassword;
        protected int loginTimeOut;

        protected WebSite(string loginName, string loginPassword, int captchaLength, int loginTimeOut)
        {
            this.loginName = loginName;
            this.loginPassword = loginPassword;
            this.captchaLength = captchaLength;
            this.loginTimeOut = loginTimeOut;

            using (var waiter = new WebBrowserWaiter.WebBrowserWaiter(null))
            {

                waiter.Await(
                    wb => wb.Navigate(BaseUrl)
                    );

                var text = waiter.Await(
                        wb => wb.DocumentText
                    );
            }
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

        public EnumLoginStatus LoginStatus
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
                    if (_loginStatus == EnumLoginStatus.LoginSuccessful)
                    {
                        if (EndLogin != null)
                        {
                            EndLogin(true);
                        }
                    }
                    else if (_loginStatus == EnumLoginStatus.LoginFailed)
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
        protected abstract Regex LoginPage { get; }
        protected abstract Regex CaptchaInputPage { get; }
        protected abstract Regex MainPage { get; }
        protected abstract Action<bool> EndLogin { get; }
        protected abstract Action<IDictionary<string, IList<string>>> EndGrabData { get; }
        protected abstract Action<EnumLoginStatus> LoginStatusChanged { get; }
        protected abstract Action<string> PopupMsgHandler { get; }

        private void Initialize()
        {
            var timeOut = new TimeSpan(0, 0, loginTimeOut);
            _startTime = DateTime.Now;
            _loginTimer = new Timer(200);
            _loginTimer.Elapsed += (sender, ev) =>
            {
                Application.DoEvents();

                if (DateTime.Now > _startTime.Add(timeOut))
                {
                    _loginTimer.Enabled = false;
                    _loginTimer.Stop();
                    LoginStatus = EnumLoginStatus.LoginFailed;
                }
                else
                {
                    if (LoginStatus == EnumLoginStatus.LoginSuccessful ||
                        LoginStatus == EnumLoginStatus.LoginFailed)
                    {
                        _loginTimer.Enabled = false;
                        _loginTimer.Stop();
                    }
                }
            };
            _loginTimer.AutoReset = true;
            _loginTimer.Enabled = false;

            _loginStatus = EnumLoginStatus.NotLogin;
            _captchaValidateCount = 0;
        }

        protected abstract void StartLogin();
        protected abstract bool IsCaptchaInputPageLoaded();
        protected abstract void CaptchaValidate();
        protected abstract void RefreshCaptcha();
        protected abstract void StartGrabData();

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
                    browser.DocumentCompleted -= MainPageLoaded;
                    browser.Navigated += WebSiteNavigated;
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
                LoginStatus = EnumLoginStatus.LoginFailed;
            }
        }

        public void DoRefreshCaptcha()
        {
            RefreshCaptcha();
        }

        private void WebSiteNavigated(object sender, WebBrowserNavigatedEventArgs e)
        {
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

            var pageName = e.Url.ToString();
            LogHelper.LogInfo(GetType(), "页面跳转:" + pageName);
        }

        private void LoginPageLoaded(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            var pageName = e.Url.ToString();

            if (LoginPage != null && LoginPage.IsMatch(pageName))
            {
                LoginStatus = EnumLoginStatus.Logging;
                _startTime = DateTime.Now;
                _loginTimer.Enabled = true;
                _loginTimer.Start();

                StartLogin();
            }
        }

        private void CaptchaInputPageLoaded(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            var pageName = e.Url.ToString();

            if (IsCaptchaInputPageLoaded() && CaptchaInputPage != null && CaptchaInputPage.IsMatch(pageName))
            {
                LoginStatus = EnumLoginStatus.CaptchaValidating;
                DoCaptchaValidate();
            }
        }

        private void MainPageLoaded(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            var pageName = e.Url.ToString();

            if (MainPage != null && MainPage.IsMatch(pageName))
            {
                LoginStatus = EnumLoginStatus.LoginSuccessful;
                StartGrabData();
            }
        }
    }
}