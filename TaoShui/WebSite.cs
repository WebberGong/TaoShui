using System;
using System.Collections.Generic;
using System.Windows.Forms;
using mshtml;
using Timer = System.Timers.Timer;

namespace TaoShui
{
    public abstract class WebSite
    {
        private readonly int _captchaValidateMaxCount = 3;
        private readonly Timer _loginTimer;
        private int _captchaValidateCount;
        private EnumLoginStatus _loginStatus;
        private DateTime _startTime;

        protected WebBrowser browser;
        protected int captchaLength;
        protected string loginName;
        protected string loginPassword;
        protected int loginTimeOut;

        protected WebSite(WebBrowser browser, string loginName, string loginPassword, int captchaLength,
            int loginTimeOut)
        {
            this.browser = browser;
            this.loginName = loginName;
            this.loginPassword = loginPassword;
            this.captchaLength = captchaLength;
            this.loginTimeOut = loginTimeOut;

            _loginStatus = EnumLoginStatus.NotLogin;
            _captchaValidateCount = 0;

            if (this.browser == null)
            {
                this.browser = new WebBrowser();
            }

            this.browser.ObjectForScripting = new MaxBetMessageHandler(this);
            this.browser.ScriptErrorsSuppressed = true;
            this.browser.Navigated += WebSiteNavigated;
            this.browser.DocumentCompleted += LoginPageLoaded;
            this.browser.DocumentCompleted += CaptchaInputPageLoaded;
            this.browser.DocumentCompleted += MainPageLoaded;

            var timeOut = new TimeSpan(0, 0, loginTimeOut);
            _startTime = DateTime.Now;
            _loginTimer = new Timer(500);
            _loginTimer.Elapsed += (sender, ev) =>
            {
                if (DateTime.Now > _startTime.Add(timeOut))
                {
                    _loginTimer.Enabled = false;
                    _loginTimer.Stop();
                    LoginStatus = EnumLoginStatus.LoginFailed;
                }
                else
                {
                    if (LoginStatus == EnumLoginStatus.LoginSuccessful)
                    {
                        _loginTimer.Enabled = false;
                        _loginTimer.Stop();
                    }
                }
            };
            _loginTimer.AutoReset = true;
            _loginTimer.Enabled = false;
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
        protected abstract string LoginPage { get; }
        protected abstract string CaptchaInputPage { get; }
        protected abstract string MainPage { get; }
        protected abstract Action<bool> EndLogin { get; }
        protected abstract Action<IDictionary<string, IList<string>>> EndGrabData { get; }
        protected abstract Action<EnumLoginStatus> LoginStatusChanged { get; }

        public void Run()
        {
            browser.Navigate(BaseUrl);
        }

        private void WebSiteNavigated(object sender, WebBrowserNavigatedEventArgs e)
        {
            var webBrowser = sender as WebBrowser;
            if (webBrowser != null && webBrowser.Document != null && webBrowser.Document.Window != null)
            {
                var win = (IHTMLWindow2) webBrowser.Document.Window.DomWindow;
                var js = @"window.alert = function(msg) { window.external.AlertMessage(msg); return true; }; 
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

            if (!string.IsNullOrEmpty(LoginPage) && pageName.Contains(LoginPage))
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

            if (IsCaptchaInputPageLoaded() && !string.IsNullOrEmpty(CaptchaInputPage) &&
                pageName.Contains(CaptchaInputPage))
            {
                if (_captchaValidateCount < _captchaValidateMaxCount)
                {
                    LoginStatus = EnumLoginStatus.CaptchaValidating;

                    _captchaValidateCount++;
                    CaptchaValidate();
                }
            }
        }

        private void MainPageLoaded(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            var pageName = e.Url.ToString();

            if (!string.IsNullOrEmpty(MainPage) && pageName.Contains(MainPage))
            {
                LoginStatus = EnumLoginStatus.LoginSuccessful;

                StartGrabData();
            }
        }

        public abstract void StartLogin();
        public abstract bool IsCaptchaInputPageLoaded();
        public abstract void RefreshCaptcha();
        public abstract void CaptchaValidate();
        public abstract void StartGrabData();
    }
}