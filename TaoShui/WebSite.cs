﻿using System;
using System.Collections.Generic;
using System.Windows.Forms;
using mshtml;
using Timer = System.Timers.Timer;

namespace TaoShui
{
    public abstract class WebSite
    {
        private Timer _loginTimer;
        private EnumLoginStatus _loginStatus;
        private DateTime _startTime;
        private int _captchaValidateCount;
        private readonly int _captchaValidateMaxCount = 3;
        protected WebBrowser browser;
        protected string loginName;
        protected string loginPassword;
        protected int captchaLength;
        protected int loginTimeOut;

        protected WebSite(string loginName, string loginPassword, int captchaLength, int loginTimeOut)
        {
            this.loginName = loginName;
            this.loginPassword = loginPassword;
            this.captchaLength = captchaLength;
            this.loginTimeOut = loginTimeOut;

            Init();
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

        public void Run()
        {
            browser.Navigate(BaseUrl);
        }

        private void Init()
        {
            _loginStatus = EnumLoginStatus.NotLogin;
            _captchaValidateCount = 0;

            browser = new WebBrowser
            {
                ObjectForScripting = new MaxBetMessageHandler(this),
                ScriptErrorsSuppressed = true
            };

            browser.Navigated += WebSiteNavigated;
            browser.DocumentCompleted += LoginPageLoaded;
            browser.DocumentCompleted += CaptchaInputPageLoaded;
            browser.DocumentCompleted += MainPageLoaded;

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
                LoginStatus = EnumLoginStatus.Logging;
                _startTime = DateTime.Now;
                _loginTimer.Enabled = true;
                _loginTimer.Start();
            }
            else if (!string.IsNullOrEmpty(CaptchaInputPage) && pageName.Contains(CaptchaInputPage))
            {
                LoginStatus = EnumLoginStatus.CaptchaValidating;
            }
            else if (!string.IsNullOrEmpty(MainPage) && pageName.Contains(MainPage))
            {
                LoginStatus = EnumLoginStatus.LoginSuccessful;
            }
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

            if (IsCaptchaInputPageLoaded() && !string.IsNullOrEmpty(CaptchaInputPage) && pageName.Contains(CaptchaInputPage))
            {
                if (_captchaValidateCount < _captchaValidateMaxCount)
                {
                    LoginStatus = EnumLoginStatus.CaptchaValidating;

                    CaptchaValidate();
                    _captchaValidateCount++;
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

        protected abstract Uri BaseUrl { get; }
        protected abstract string LoginPage { get; }
        protected abstract string CaptchaInputPage { get; }
        protected abstract string MainPage { get; }
        protected abstract Action<bool> EndLogin { get; }
        protected abstract Action<IDictionary<string, IList<string>>> EndGrabData { get; }
        protected abstract Action<EnumLoginStatus> LoginStatusChanged { get; }
        public abstract void StartLogin();
        public abstract bool IsCaptchaInputPageLoaded();
        public abstract void RefreshCaptcha();
        public abstract void CaptchaValidate();
        public abstract void StartGrabData();
    }
}