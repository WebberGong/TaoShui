using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Awesomium.Core;
using Awesomium.Windows.Forms;
using HtmlAgilityPack;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;

namespace Test
{
    public partial class TestForm : Form
    {
        public TestForm()
        {
            InitializeComponent();
        }

        private void GrabDataByHtml(string html)
        {
            int executeTimes = 10;

            long resultTotalLength = 0L;
            long totalElapsedTime = 0L;
            int executeTotalTimes = 0;
            Stopwatch watch = new Stopwatch();

            for (int i = 0; i < executeTimes; i++)
            {
                watch.Reset();
                watch.Start();
                StringBuilder sbResult = new StringBuilder();
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(html);
                HtmlNode rootnode = doc.DocumentNode;
                foreach (var node in rootnode.Descendants())
                {
                    executeTotalTimes++;
                    sbResult.Append(node.InnerText + "\r\n");
                }
                string result = sbResult.ToString();
                watch.Stop();
                totalElapsedTime += watch.ElapsedMilliseconds;
                resultTotalLength += result.Length;
            }
            Console.WriteLine(@"抓取到的数据总长度:" + resultTotalLength);
            Console.WriteLine(@"总耗时:" + totalElapsedTime);
            Console.WriteLine(@"平均耗时:" + totalElapsedTime / executeTimes);
            Console.WriteLine(@"执行总次数:" + executeTotalTimes);
        }

        private void GrabDataByJs(WebView view)
        {
            int executeTimes = 10;

            long resultTotalLength = 0L;
            long totalElapsedTime = 0L;
            Stopwatch watch = new Stopwatch();

            for (int i = 0; i < executeTimes; i++)
            {
                while (!view.IsDocumentReady)
                {
                    Thread.Sleep(5);
                }
                watch.Reset();
                watch.Start();
                var js = @"
                    (function() {
                        try {
                            var result = '';
                            var executeTotalTimes = 0;
                            var all = $('*');
                            for (var i in all) {
                                executeTotalTimes++;
                                result += all[i].innerText + '\r\n';
                            }
                            return result;
                        } catch (ex) {
                            return ex;
                        }
                    })();";
                var result = view.ExecuteJavascriptWithResult(js);
                watch.Stop();
                totalElapsedTime += watch.ElapsedMilliseconds;
                resultTotalLength += result.ToString().Length;
            }
            Console.WriteLine(@"抓取到的数据总长度:" + resultTotalLength);
            Console.WriteLine(@"总耗时:" + totalElapsedTime);
            Console.WriteLine(@"平均耗时:" + totalElapsedTime / executeTimes);
        }

        private void GrabDataByAwesomium()
        {
            try
            {
                var thread = new Thread(() =>
                {
                    for (int i = 0; i < 10; i++)
                    {
                        using (var webView = WebCore.CreateWebView(100, 100, WebViewType.Offscreen))
                        {
                            webView.LoadingFrameComplete += (sender, e) =>
                            {
                                WebView view = sender as WebView;
                                Console.WriteLine(@"页面地址:" + e.Url);
                                if (view != null)
                                {
                                    GrabDataByHtml(view.HTML);
                                }
                            };

                            //webView.DocumentReady += (sender, e) =>
                            //{
                            //    WebView view = sender as WebView;
                            //    Console.WriteLine(@"页面地址:" + e.Url);
                            //    if (view != null)
                            //    {
                            //        GrabDataByJs(view);
                            //    }
                            //};

                            webView.Source = new Uri("http://www.163.com/");
                            WebCore.Run();

                        }
                    }
                })
                {
                    Priority = ThreadPriority.AboveNormal,
                    IsBackground = true,
                    Name = "Test"
                };
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void btnTest_Click(object sender, EventArgs e)
        {
            GrabDataByAwesomium();
        }
    }
}
