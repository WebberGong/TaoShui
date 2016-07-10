using System;
using System.Collections.Generic;
using System.Windows.Forms;
using mshtml;
using Timer = System.Timers.Timer;

namespace TaoShui
{
    public abstract class WebSite
    {
        private readonly Timer _loginTimer;
        private EnumLoginStatus _loginStatus = EnumLoginStatus.NotLogin;
        protected WebBrowser browser;
        protected string loginName;
        protected string loginPassword;
        protected int loginTimeOut;

        protected WebSite(WebBrowser browser, string loginName, string loginPassword, int loginTimeOut)
        {
            this.browser = browser ?? new WebBrowser();
            this.loginName = loginName;
            this.loginPassword = loginPassword;
            this.loginTimeOut = loginTimeOut;

            this.browser.ObjectForScripting = new MaxBetMessageHandler(this);
            this.browser.ScriptErrorsSuppressed = true;
            this.browser.Navigated += WebSiteNavigated;
            this.browser.DocumentCompleted += LoginPageLoaded;
            this.browser.DocumentCompleted += ProcessLoginPageLoaded;
            this.browser.DocumentCompleted += MainPageLoaded;

            var timeOut = new TimeSpan(0, 0, loginTimeOut);
            var startTime = DateTime.Now;
            _loginTimer = new Timer(200);
            _loginTimer.Elapsed += (sr, ev) =>
            {
                if (DateTime.Now > startTime.Add(timeOut))
                {
                    _loginTimer.Stop();
                    LoginStatus = EnumLoginStatus.LoginFailed;
                    if (browser != null)
                    {
                        browser.Stop();
                    }
                }
                else
                {
                    if (LoginStatus == EnumLoginStatus.LoginSuccessful)
                    {
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

        public void Run()
        {
            LoginStatus = EnumLoginStatus.NotLogin;
            browser.Navigate(BaseUrl);
        }

        private void WebSiteNavigated(object sender, WebBrowserNavigatedEventArgs e)
        {
            var webBrowser = sender as WebBrowser;
            if (webBrowser != null && webBrowser.Document != null && webBrowser.Document.Window != null)
            {
                var win = (IHTMLWindow2)webBrowser.Document.Window.DomWindow;
                var js = @"window.alert = function(msg) { window.external.AlertMessage(msg); return true; }; 
                    window.onerror = function() { return true; }; 
                    window.confirm = function() { return true; }; 
                    window.open = function() { return true; }; 
                    window.showModalDialog = function() { return true; };";
                win.execScript(js, "javascript");
            }

            var pageName = e.Url.ToString();

            if (!string.IsNullOrEmpty(LoginPage) && pageName.Contains(LoginPage))
            {
                if (string.IsNullOrEmpty(ProcessLoginPage) && LoginStatus == EnumLoginStatus.NotLogin)
                {
                    LoginStatus = EnumLoginStatus.Logging;
                    _loginTimer.Enabled = true;
                    _loginTimer.Start();
                }
            }
            else if (!string.IsNullOrEmpty(ProcessLoginPage) && LoginStatus == EnumLoginStatus.NotLogin &&
                     pageName.Contains(ProcessLoginPage))
            {
                LoginStatus = EnumLoginStatus.Logging;
                _loginTimer.Enabled = true;
                _loginTimer.Start();
            }
            else if (!string.IsNullOrEmpty(MainPage) && LoginStatus == EnumLoginStatus.Logging &&
                     pageName.Contains(MainPage))
            {
                LoginStatus = EnumLoginStatus.LoginSuccessful;
            }
        }

        private void LoginPageLoaded(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            var pageName = e.Url.ToString();

            if (!string.IsNullOrEmpty(LoginPage) && pageName.Contains(LoginPage))
            {
                StartLogin();
            }
        }

        private void ProcessLoginPageLoaded(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            var pageName = e.Url.ToString();

            if (!string.IsNullOrEmpty(ProcessLoginPage) && pageName.Contains(ProcessLoginPage))
            {
                ProcessLogin();
            }
        }

        private void MainPageLoaded(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            var pageName = e.Url.ToString();

            if (!string.IsNullOrEmpty(MainPage) && pageName.Contains(MainPage))
            {
                StartGrabData();
            }
        }

        protected abstract Uri BaseUrl { get; }
        protected abstract string LoginPage { get; }
        protected abstract string ProcessLoginPage { get; }
        protected abstract string MainPage { get; }
        protected abstract Action<bool> EndLogin { get; }
        protected abstract Action<IDictionary<string, IList<string>>> EndGrabData { get; }
        protected abstract Action<EnumLoginStatus> LoginStatusChanged { get; }
        public abstract void StartLogin();
        public abstract void ProcessLogin();
        public abstract void StartGrabData();
    }
}