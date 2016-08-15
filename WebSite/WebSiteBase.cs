using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using Awesomium.Core;
using Utils;
using WcfService;
using Timer = System.Timers.Timer;

namespace WebSite
{
    public abstract class WebSiteBase
    {
        protected const string Undefined = "undefined";
        protected const string True = "true";
        protected const string False = "false";

        private readonly string _jquery;
        private readonly int _captchaValidateMaxCount = 3;
        private readonly TimeSpan _loginInterval = new TimeSpan(0, 5, 0);
        private readonly int _maxLoginAttemptCount = 3;
        private int _captchaValidateCount;
        private int _loginFailedCount;
        private Stopwatch _loginStopWatch;
        private Timer _loginTimer;
        private WebSiteStatus _webSiteStatus;

        protected WebView browser;
        protected int captchaLength;
        protected GrabDataClient grabDataClient;
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

            grabDataClient = new GrabDataClient();

            lock (this)
            {
                _jquery = File.ReadAllText(@"jquery-3.1.0.min.js");
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
                    OnWebSiteStatusChanged(this, _webSiteStatus);
                }
            }
        }

        protected abstract Uri BaseUrl { get; }
        protected abstract Regex ChangeLanguageRegex { get; }
        protected abstract Regex LoginPageRegex { get; }
        protected abstract Regex CaptchaInputPageRegex { get; }
        protected abstract Regex MainPageRegex { get; }
        protected abstract Action<string> ShowJavascriptDialog { get; }

        protected abstract void ChangeLanguage();
        protected abstract void Login();
        protected abstract bool IsCaptchaInputPageReady();
        protected abstract void CaptchaValidate();
        protected abstract void RefreshCaptcha();
        protected abstract string[][] GrabData();
        public event Action<WebSiteBase, WebSiteStatus> WebSiteStatusChanged;

        private void Initialize()
        {
            WebCore.Initialize(new WebConfig {LogLevel = LogLevel.Verbose, UserScript = _jquery});
            browser = WebCore.CreateWebView(Screen.PrimaryScreen.WorkingArea.Width,
                Screen.PrimaryScreen.WorkingArea.Height, WebViewType.Offscreen);

            browser.LoadingFrame -= WebSiteLoading;
            browser.LoadingFrameComplete -= WebSiteLoadingComplete;
            browser.LoadingFrameComplete -= ChangeLanguagePageLoadingComplete;
            browser.LoadingFrameComplete -= LoginPageLoadingComplete;
            browser.LoadingFrameComplete -= CaptchaInputPageLoadingComplete;
            browser.LoadingFrameComplete -= MainPageLoadingComplete;
            browser.JavascriptMessage -= JavascriptMessageHandler;
            browser.ShowJavascriptDialog -= ShowJavascriptDialogHandler;
            browser.LoadingFrame += WebSiteLoading;
            browser.LoadingFrameComplete += WebSiteLoadingComplete;
            browser.LoadingFrameComplete += ChangeLanguagePageLoadingComplete;
            browser.LoadingFrameComplete += LoginPageLoadingComplete;
            browser.LoadingFrameComplete += CaptchaInputPageLoadingComplete;
            browser.LoadingFrameComplete += MainPageLoadingComplete;
            browser.JavascriptMessage += JavascriptMessageHandler;
            browser.ShowJavascriptDialog += ShowJavascriptDialogHandler;

            var tsLoginTimeOut = new TimeSpan(0, 0, loginTimeOut);
            _loginStopWatch = new Stopwatch();
            _loginTimer = new Timer(200);
            _loginTimer.Elapsed += (sender, ev) =>
            {
                if (_loginStopWatch.Elapsed > tsLoginTimeOut &&
                    WebSiteStatus != WebSiteStatus.LoginSuccessful)
                {
                    _loginStopWatch.Reset();
                    _loginTimer.Enabled = false;
                    _loginTimer.Stop();
                    WebSiteStatus = WebSiteStatus.LoginFailed;
                }
                else
                {
                    if (WebSiteStatus == WebSiteStatus.LoginSuccessful ||
                        WebSiteStatus == WebSiteStatus.LoginFailed)
                    {
                        _loginStopWatch.Reset();
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

        protected bool IsBrowserOk()
        {
            return browser != null && browser.IsLive && !browser.IsCrashed && !browser.IsDisposed &&
                   browser.IsDocumentReady;
        }

        public void Run()
        {
            Initialize();
            browser.Source = BaseUrl;
            WebCore.Run();
        }

        public void Stop()
        {
            WebCore.Shutdown();
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

        protected string JsGetImgBase64String(string getElementQuery, bool leaveOnlyBase64Data = true)
        {
            var js = @"
                (function() {
                    try {
                        var img = " + getElementQuery + @";
                        var cnv = document.createElement('CANVAS');
                        var ctx = cnv.getContext('2d');
                        ctx.drawImage(img, 0, 0);
                        return cnv.toDataURL();
                    } catch (ex) {
                        return ex.message;
                    }
                })();";
            string data = browser.ExecuteJavascriptWithResult(js);
            if (data == Undefined)
            {
                return string.Empty;
            }
            if (leaveOnlyBase64Data && data.Contains(","))
            {
                data = data.Substring(data.IndexOf(",", StringComparison.Ordinal) + 1);
            }

            return data;
        }

        private void JavascriptMessageHandler(object sender, JavascriptMessageEventArgs e)
        {
            var msg = e.Message;
            LogHelper.Instance.LogWarn(GetType(), msg);
        }

        private void ShowJavascriptDialogHandler(object sender, JavascriptDialogEventArgs e)
        {
            var msg = e.Message;
            LogHelper.Instance.LogWarn(GetType(), msg);
            ShowJavascriptDialog(msg);
            e.Handled = true;
        }

        private void WebSiteLoading(object sender, LoadingFrameEventArgs e)
        {
            var url = e.Url.ToString();
            LogHelper.Instance.LogInfo(GetType(), "页面正在加载:" + url);
        }

        private void WebSiteLoadingComplete(object sender, FrameEventArgs e)
        {
            if (e.IsMainFrame)
            {
                var url = e.Url.ToString();
                LogHelper.Instance.LogInfo(GetType(), "页面加载成功:" + url);
            }
        }

        private void ChangeLanguagePageLoadingComplete(object sender, FrameEventArgs e)
        {
            if (e.IsMainFrame)
            {
                var url = e.Url.ToString();

                if (ChangeLanguageRegex != null && ChangeLanguageRegex.IsMatch(url))
                {
                    ChangeLanguage();
                }
            }
        }

        private void LoginPageLoadingComplete(object sender, FrameEventArgs e)
        {
            if (e.IsMainFrame)
            {
                var url = e.Url.ToString();

                if (LoginPageRegex != null && LoginPageRegex.IsMatch(url))
                {
                    WebSiteStatus = WebSiteStatus.Logging;
                    _loginStopWatch.Reset();
                    _loginStopWatch.Start();
                    _loginTimer.Enabled = true;
                    _loginTimer.Start();

                    Login();
                }
            }
        }

        private void CaptchaInputPageLoadingComplete(object sender, FrameEventArgs e)
        {
            if (e.IsMainFrame)
            {
                var url = e.Url.ToString();

                if (IsCaptchaInputPageReady() && CaptchaInputPageRegex != null &&
                    CaptchaInputPageRegex.IsMatch(url))
                {
                    WebSiteStatus = WebSiteStatus.CaptchaValidating;
                    DoCaptchaValidate();
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
                    _loginStopWatch.Reset();
                    _loginTimer.Enabled = false;
                    _loginTimer.Stop();

                    browser.BeginInvoke(new Action(DoGrabData));
                }
            }
        }

        private void DoGrabData()
        {
            while (true)
            {
                if (IsBrowserOk() && _webSiteStatus == WebSiteStatus.LoginSuccessful)
                {
                    _loginFailedCount = 0;

                    var watch = new Stopwatch();
                    watch.Start();
                    var grabbedData = GrabData();
                    watch.Stop();

                    grabDataClient.SendData(new GrabbedData
                    {
                        Data = grabbedData,
                        Type = GetType().ToString(),
                        GrabbedTime = DateTime.Now
                    });

                    if (grabbedData != null)
                    {
                        var elapsedTimeMsg = "抓取数据耗时:" + watch.ElapsedMilliseconds;
                        LogHelper.Instance.LogInfo(GetType(), elapsedTimeMsg);
                        LogHelper.Instance.LogInfo(GetType(), string.Format("抓取到的数据条数:{0}", grabbedData.Length));
                    }
                    Thread.Sleep(grabDataInterval*1000);
                }
                else
                {
                    _loginFailedCount++;
                    Stop();
                    if (_loginFailedCount < _maxLoginAttemptCount)
                    {
                        Thread.Sleep((int) _loginInterval.TotalSeconds);
                        Run();
                    }
                }
            }
            // ReSharper disable once FunctionNeverReturns
        }

        private void OnWebSiteStatusChanged(WebSiteBase sender, WebSiteStatus status)
        {
            LogHelper.Instance.LogInfo(GetType(), "网站状态: " + status);

            var handler = WebSiteStatusChanged;
            if (handler != null)
            {
                handler(sender, status);
            }
        }
    }
}