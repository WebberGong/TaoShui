// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WebBrowserWaiter.cs" company="WebBrowserWaiter">
//   Copyright © 2014 WebBrowserWaiter. All rights reserved.
// </copyright>
// <summary>
//   The web browser waiter.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Windows.Forms;

namespace WebBrowserWaiter
{
    /// <summary>
    ///     The web browser waiter.
    /// </summary>
    public class WebBrowserWaiter : IDisposable
    {
        #region Methods

        /// <summary>
        ///     The create wait time span.
        /// </summary>
        /// <param name="wait">
        ///     The wait.
        /// </param>
        /// <returns>
        ///     The <see cref="TimeSpan" />.
        /// </returns>
        private TimeSpan CreateWaitTimeSpan(int wait)
        {
            return TimeSpan.FromMilliseconds(wait);
        }

        #endregion

        #region Internal Class

        /// <summary>
        ///     The headless form.
        /// </summary>
        private sealed class HeadlessForm : Form
        {
            #region Constructors

            public HeadlessForm()
            {
                ServicePointManager.ServerCertificateValidationCallback = ValidateServerCertificate;
            }

            #endregion

            #region Public Properties

            /// <summary>
            ///     Gets or sets a value indicating whether initial visibility.
            /// </summary>
            public bool InitialVisibility { private get; set; }

            #endregion

            #region Private Methods and Operators

            private bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain,
                SslPolicyErrors sslPolicyErrors)
            {
                return true;
            }

            #endregion

            #region Public Methods and Operators

            /// <summary>
            ///     The invoke.
            /// </summary>
            /// <param name="action">
            ///     The action.
            /// </param>
            public void Execute(Action action)
            {
                Invoke(action);
            }

            #endregion

            #region Methods

            /// <summary>
            ///     The set visible core.
            /// </summary>
            /// <param name="value">
            ///     The value.
            /// </param>
            protected override void SetVisibleCore(bool value)
            {
                if (!IsHandleCreated)
                    CreateHandle();

                base.SetVisibleCore(InitialVisibility);
            }

            #endregion
        }

        #endregion

        #region Fields

        /// <summary>
        ///     The default wait.
        /// </summary>
        private static TimeSpan _defaultWait = TimeSpan.FromSeconds(30);

        /// <summary>
        ///     The signal.
        /// </summary>
        private readonly ManualResetEvent _signal;

        /// <summary>
        ///     The browser.
        /// </summary>
        private WebBrowser _browser;

        /// <summary>
        ///     The form.
        /// </summary>
        private HeadlessForm _form;

        /// <summary>
        ///     The last completed.
        /// </summary>
        private DateTime? _lastCompleted;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="WebBrowserWaiter" /> class.
        /// </summary>
        public WebBrowserWaiter()
            // ReSharper disable once RedundantArgumentDefaultValue
            : this(null)
        {
            // NOTE: This constructor must provide at least one named argument to the other
            // constructor to avoid calling this constructor recursively.
        }

        public WebBrowserWaiter(Action<string> popupMsgHandler)
            // ReSharper disable once RedundantArgumentDefaultValue
            : this(popupMsgHandler, false)
        {
            // NOTE: This constructor must provide at least one named argument to the other
            // constructor to avoid calling this constructor recursively.
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="WebBrowserWaiter" /> class.
        /// </summary>
        /// <param name="popupMsgHandler">
        ///     The popup message handler
        /// </param>
        /// <param name="visibility">
        ///     The visibility.
        /// </param>
        /// <param name="position">
        ///     The position.
        /// </param>
        /// <param name="width">
        ///     The width.
        /// </param>
        /// <param name="height">
        ///     The height.
        /// </param>
        /// <param name="top">
        ///     The top.
        /// </param>
        /// <param name="left">
        ///     The left.
        /// </param>
        public WebBrowserWaiter(Action<string> popupMsgHandler = null, bool visibility = false,
            FormStartPosition position = FormStartPosition.CenterScreen, int width = -1, int height = -1, int top = 0,
            int left = 0)
        {
            _signal = new ManualResetEvent(false);

            var thread = new Thread(() =>
            {
                _browser = new WebBrowser
                {
                    Width = width < 0 ? Screen.PrimaryScreen.WorkingArea.Width*3/4 : width,
                    Height = height < 0 ? Screen.PrimaryScreen.WorkingArea.Height*3/4 : height,
                    ObjectForScripting = new MessageHandler(popupMsgHandler),
                    ScriptErrorsSuppressed = true
                };

                _browser.Navigating += (p, q) => _lastCompleted = null;
                _browser.DocumentCompleted += (p, q) => _lastCompleted = DateTime.UtcNow;

                _form = new HeadlessForm
                {
                    Width = _browser.Width,
                    Height = _browser.Height,
                    StartPosition = position,
                    Top = top,
                    Left = left
                };

                _form.Controls.Add(_browser);
                _form.HandleCreated += (p, q) => _signal.Set();
                _form.InitialVisibility = visibility;
                _form.Visible = visibility;

                Application.Run(_form);
            }) {Name = "WebBrowserWaiterThread"};

            thread.SetApartmentState(ApartmentState.STA);
            thread.IsBackground = true;
            thread.Start();

            _signal.WaitOne();
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets or sets the default wait.
        /// </summary>
        public virtual TimeSpan DefaultWait
        {
            get { return _defaultWait; }
            set { _defaultWait = value; }
        }

        public WebBrowser Browser
        {
            get { return _browser; }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     The await.
        /// </summary>
        /// <param name="order">
        ///     The order.
        /// </param>
        public virtual void Await(Action<WebBrowser> order)
        {
            Await(
                DefaultWait,
                order
                );
        }

        /// <summary>
        ///     The await.
        /// </summary>
        /// <param name="wait">
        ///     The wait.
        /// </param>
        /// <param name="order">
        ///     The order.
        /// </param>
        public virtual void Await(int wait, Action<WebBrowser> order)
        {
            Await(
                CreateWaitTimeSpan(wait),
                order
                );
        }

        /// <summary>
        ///     The await.
        /// </summary>
        /// <param name="wait">
        ///     The wait.
        /// </param>
        /// <param name="order">
        ///     The order.
        /// </param>
        public virtual void Await(TimeSpan wait, Action<WebBrowser> order)
        {
            Await(
                wait,
                new[] {order}
                );
        }

        /// <summary>
        ///     The await.
        /// </summary>
        /// <param name="orders">
        ///     The orders.
        /// </param>
        public virtual void Await(params Action<WebBrowser>[] orders)
        {
            Await(
                DefaultWait,
                orders
                );
        }

        /// <summary>
        ///     The await.
        /// </summary>
        /// <param name="wait">
        ///     The wait.
        /// </param>
        /// <param name="orders">
        ///     The orders.
        /// </param>
        public virtual void Await(int wait, params Action<WebBrowser>[] orders)
        {
            Await(
                CreateWaitTimeSpan(wait),
                orders
                );
        }

        /// <summary>
        ///     The await.
        /// </summary>
        /// <param name="waits">
        ///     The waits.
        /// </param>
        /// <param name="orders">
        ///     The orders.
        /// </param>
        public virtual void Await(int[] waits, params Action<WebBrowser>[] orders)
        {
            Await(
                waits.Select(CreateWaitTimeSpan).ToArray(),
                orders
                );
        }

        /// <summary>
        ///     The await.
        /// </summary>
        /// <param name="wait">
        ///     The wait.
        /// </param>
        /// <param name="orders">
        ///     The orders.
        /// </param>
        public virtual void Await(TimeSpan wait, params Action<WebBrowser>[] orders)
        {
            Await(
                orders.Select(p => wait).ToArray(),
                orders
                );
        }

        /// <summary>
        ///     The await.
        /// </summary>
        /// <param name="waits">
        ///     The waits.
        /// </param>
        /// <param name="orders">
        ///     The orders.
        /// </param>
        /// <exception cref="ArgumentException">
        ///     Throws ArgumentException if waits and orders differ in length.
        /// </exception>
        public virtual void Await(TimeSpan[] waits, params Action<WebBrowser>[] orders)
        {
            Await(
                waits,
                orders.Select(
                    (p, q) => (Func<WebBrowser, object>) (r =>
                    {
                        p(r);
                        return null;
                    })
                    ).ToArray()
                );
        }

        /// <summary>
        ///     The await.
        /// </summary>
        /// <param name="order">
        ///     The order.
        /// </param>
        /// <typeparam name="T">The return type.</typeparam>
        /// <returns>
        ///     The <see cref="T" />.
        /// </returns>
        public virtual T Await<T>(Func<WebBrowser, T> order)
        {
            return Await(
                DefaultWait,
                order
                );
        }

        /// <summary>
        ///     The await.
        /// </summary>
        /// <param name="wait">
        ///     The wait.
        /// </param>
        /// <param name="order">
        ///     The order.
        /// </param>
        /// <typeparam name="T">
        ///     The return type.
        /// </typeparam>
        /// <returns>
        ///     The <see cref="T" />.
        /// </returns>
        public virtual T Await<T>(int wait, Func<WebBrowser, T> order)
        {
            return Await(
                CreateWaitTimeSpan(wait),
                order
                );
        }

        /// <summary>
        ///     The await.
        /// </summary>
        /// <param name="wait">
        ///     The wait.
        /// </param>
        /// <param name="order">
        ///     The order.
        /// </param>
        /// <typeparam name="T">
        ///     The return type.
        /// </typeparam>
        /// <returns>
        ///     The <see cref="T" />.
        /// </returns>
        public virtual T Await<T>(TimeSpan wait, Func<WebBrowser, T> order)
        {
            return Await(
                wait,
                new[] {order}
                ).First();
        }

        /// <summary>
        ///     The await.
        /// </summary>
        /// <param name="orders">
        ///     The orders.
        /// </param>
        /// <typeparam name="T">
        ///     The return type.
        /// </typeparam>
        /// <returns>
        ///     The an array of the return type.
        /// </returns>
        public virtual T[] Await<T>(params Func<WebBrowser, T>[] orders)
        {
            return Await(
                DefaultWait,
                orders
                );
        }

        /// <summary>
        ///     The await.
        /// </summary>
        /// <param name="wait">
        ///     The wait.
        /// </param>
        /// <param name="orders">
        ///     The orders.
        /// </param>
        /// <typeparam name="T">
        ///     The return type.
        /// </typeparam>
        /// <returns>
        ///     The an array of the return type.
        /// </returns>
        public virtual T[] Await<T>(int wait, params Func<WebBrowser, T>[] orders)
        {
            return Await(
                CreateWaitTimeSpan(wait),
                orders
                );
        }

        /// <summary>
        ///     The await.
        /// </summary>
        /// <param name="waits">
        ///     The waits.
        /// </param>
        /// <param name="orders">
        ///     The orders.
        /// </param>
        /// <typeparam name="T">
        ///     The return type.
        /// </typeparam>
        /// <returns>
        ///     The an array of the return type.
        /// </returns>
        public virtual T[] Await<T>(int[] waits, params Func<WebBrowser, T>[] orders)
        {
            return Await(
                waits.Select(CreateWaitTimeSpan).ToArray(),
                orders
                );
        }

        /// <summary>
        ///     The await.
        /// </summary>
        /// <param name="wait">
        ///     The wait.
        /// </param>
        /// <param name="orders">
        ///     The orders.
        /// </param>
        /// <typeparam name="T">
        ///     The return type.
        /// </typeparam>
        /// <returns>
        ///     The an array of the return type.
        /// </returns>
        public virtual T[] Await<T>(TimeSpan wait, params Func<WebBrowser, T>[] orders)
        {
            return Await(
                orders.Select(p => wait).ToArray(),
                orders
                );
        }

        /// <summary>
        ///     The await.
        /// </summary>
        /// <param name="waits">
        ///     The waits.
        /// </param>
        /// <param name="orders">
        ///     The orders.
        /// </param>
        /// <typeparam name="T">
        ///     The return type.
        /// </typeparam>
        /// <returns>
        ///     The an array of the return type.
        /// </returns>
        public virtual T[] Await<T>(TimeSpan[] waits, params Func<WebBrowser, T>[] orders)
        {
            if (waits.Length != orders.Length)
                throw new ArgumentException("The waits and orders arguments must have the same length.");

            var results = new T[orders.Length];

            for (var i = 0; i < orders.Length; i++)
            {
                var x = i;
                _form.Execute(
                    () => results[x] = orders[x](_browser)
                    );

                while (true)
                {
                    if (!_lastCompleted.HasValue)
                    {
                        Thread.Sleep(50);
                        continue;
                    }

                    var diff = _lastCompleted.Value.Add(waits[i]) - DateTime.UtcNow;

                    if (diff.Ticks < 0)
                    {
                        break;
                    }

                    Thread.Sleep(diff);
                }
            }

            return results;
        }

        /// <summary>
        ///     The dispose.
        /// </summary>
        public virtual void Dispose()
        {
            _form.Execute(
                () =>
                {
                    _form.Close();
                    _browser.Stop();
                    _browser.Dispose();
                    _browser = null;
                    GC.Collect();
                });
        }

        #endregion
    }
}