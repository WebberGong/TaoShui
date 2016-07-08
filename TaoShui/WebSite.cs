using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Timer = System.Timers.Timer;

namespace TaoShui
{
    public abstract class WebSite
    {
        private readonly string _loginPage;
        private readonly Timer _loginTimer;
        private readonly string _mainPage;
        private readonly string _processLoginPage;
        private EnumLoginStatus _loginStatus = EnumLoginStatus.NotLogin;
        protected Uri baseUrl;
        protected WebBrowser browser;
        protected Uri captchaCodeImgUrl;
        protected string loginName;
        protected string loginPassword;
        protected int loginTimeOut;

        protected WebSite(WebBrowser browser, string loginName, string loginPassword, Uri baseUrl,
            Uri captchaCodeImgUrl,
            string loginPage, string processLoginPage, string mainPage, int loginTimeOut)
        {
            this.browser = browser ?? new WebBrowser();
            this.loginName = loginName;
            this.loginPassword = loginPassword;
            this.baseUrl = baseUrl;
            this.captchaCodeImgUrl = captchaCodeImgUrl;
            this.loginTimeOut = loginTimeOut;

            this.browser.ScriptErrorsSuppressed = false;
            this.browser.Navigated += WebSiteNavigated;
            this.browser.DocumentCompleted += LoginPageLoaded;
            this.browser.DocumentCompleted += ProcessLoginPageLoaded;
            this.browser.DocumentCompleted += MainPageLoaded;

            _loginPage = loginPage;
            _processLoginPage = processLoginPage;
            _mainPage = mainPage;

            var timeOut = new TimeSpan(0, 0, loginTimeOut);
            var startTime = DateTime.Now;
            _loginTimer = new Timer(200);
            _loginTimer.Elapsed += (sr, ev) =>
            {
                if (DateTime.Now > startTime.Add(timeOut))
                {
                    _loginTimer.Stop();
                    LoginStatus = EnumLoginStatus.LoginFailed;
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

        public Uri BaseUrl
        {
            get { return baseUrl; }
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

        protected abstract Action<bool> EndLogin { get; }
        protected abstract Action<IDictionary<string, IList<string>>> EndGrabData { get; }
        protected abstract Action<EnumLoginStatus> LoginStatusChanged { get; }

        public void Run()
        {
            browser.Navigate(baseUrl);
        }

        private void WebSiteNavigated(object sender, WebBrowserNavigatedEventArgs e)
        {
            var pageName = e.Url.ToString();

            if (pageName.Contains(_loginPage))
            {
                LoginStatus = EnumLoginStatus.NotLogin;
            }
            else if (pageName.Contains(_processLoginPage))
            {
                LoginStatus = EnumLoginStatus.Logging;
                _loginTimer.Enabled = true;
                _loginTimer.Start();
            }
            else if (pageName.Contains(_mainPage))
            {
                LoginStatus = EnumLoginStatus.LoginSuccessful;
            }
        }

        private void LoginPageLoaded(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            var pageName = e.Url.ToString();

            if (pageName.Contains(_loginPage))
            {
                StartLogin();
            }
        }

        private void ProcessLoginPageLoaded(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            var pageName = e.Url.ToString();

            if (pageName.Contains(_processLoginPage))
            {
                ProcessLogin();
            }
        }

        private void MainPageLoaded(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            var pageName = e.Url.ToString();

            if (pageName.Contains(_mainPage))
            {
                StartGrabData();
            }
        }

        protected abstract void StartLogin();
        protected abstract void ProcessLogin();
        protected abstract void StartGrabData();
    }
}