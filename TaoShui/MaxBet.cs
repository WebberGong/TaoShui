using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CaptchaRecogniser;
using Newtonsoft.Json;

namespace TaoShui
{
    public class MaxBet : WebSite
    {
        public MaxBet(WebBrowser browser, string loginName, string loginPassword, Uri baseUrl, Uri captchaCodeImgUrl,
            string loginPage, string processLoginPage, string mainPage, int loginTimeOut = 10)
            : base(browser, loginName, loginPassword, baseUrl, captchaCodeImgUrl, loginPage,
                processLoginPage, mainPage, loginTimeOut)
        {
        }

        protected override Action<bool> EndLogin
        {
            get { return isLoginSuccessful => { Console.WriteLine(@"Login Successful: " + isLoginSuccessful); }; }
        }

        protected override Action<IDictionary<string, IList<string>>> EndGrabData
        {
            get
            {
                return dicData => { Console.WriteLine("Grabed Data: \r\n" + JsonConvert.SerializeObject(dicData)); };
            }
        }

        protected override Action<EnumLoginStatus> LoginStatusChanged
        {
            get { return loginStatus => { Console.WriteLine(@"Login Status: " + loginStatus.ToString()); }; }
        }

        protected override void StartLogin()
        {
            if (browser != null && browser.Document != null)
            {
                var id = browser.Document.GetElementById("txtID");
                var password = browser.Document.GetElementById("txtPW");
                var aElements = browser.Document.GetElementsByTagName("a");
                var login =
                    aElements.Cast<HtmlElement>().FirstOrDefault(item => item.GetAttribute("className") == "login_btn");
                if (id != null && password != null && login != null)
                {
                    browser.Document.InvokeScript("changeLan", new object[] { "cs" });
                    id.SetAttribute("value", loginName);
                    password.SetAttribute("value", loginPassword);
                    login.InvokeMember("Click");
                }
            }
        }

        protected override void ProcessLogin()
        {
            if (browser != null && browser.Document != null)
            {
                var captchaCodeImage = browser.Document.GetElementById("validateCode");
                var captchaCodeInput = browser.Document.GetElementById("txtCode");
                var aElements = browser.Document.GetElementsByTagName("a");
                var submit = aElements.Cast<HtmlElement>().FirstOrDefault(item => item.InnerHtml == "递交");

                if (browser.Document.Window != null && browser.Document.Window.Parent != null && 
                    captchaCodeImage != null && captchaCodeInput != null && submit != null)
                {
                    browser.Document.Window.Parent.ScrollTo(0, 0);

                    //captchaCodeImage.Style = "position: absolute; z-index: 99999999; top: 0px; left: 0px";
                    var bitmap = new Bitmap(captchaCodeImage.ClientRectangle.Width,
                        captchaCodeImage.ClientRectangle.Height);
                    Rectangle rectangle = new Rectangle(new Point(), captchaCodeImage.ClientRectangle.Size);
                    browser.DrawToBitmap(bitmap, rectangle);

                    var saveFileDialog = new SaveFileDialog
                    {
                        Filter = @"JPEG (*.jpg)|*.jpg|PNG (*.png)|*.png"
                    };
                    saveFileDialog.ShowDialog();

                    bitmap.Save(saveFileDialog.FileName);

                    return;

                    captchaCodeInput.SetAttribute("src", captchaCodeImgUrl.ToString());
                    var code = Recogniser.RecognizeFromImage(captchaCodeImgUrl, browser.Document.Cookie, 4, 3,
                        new HashSet<EnumCaptchaType> { EnumCaptchaType.Number });
                    captchaCodeInput.SetAttribute("value", code);
                    submit.InvokeMember("Click");
                }
            }
        }

        protected override void StartGrabData()
        {
            if (browser != null && browser.Document != null && browser.Document.Window != null &&
                browser.Document.Window.Frames != null && browser.Document.Window.Frames.Count > 0)
            {
                var htmlWindow = browser.Document.Window.Frames["mainFrame"];
                if (htmlWindow != null && htmlWindow.Document != null)
                {
                    var mainTable = htmlWindow.Document.GetElementById("tmplTable");
                    if (mainTable != null && mainTable.Document != null)
                    {
                        var mainTableRows = mainTable.Document.GetElementsByTagName("tr");
                        IDictionary<string, IList<string>> dicData = new Dictionary<string, IList<string>>();
                        var leagueName = string.Empty;
                        foreach (HtmlElement item in mainTableRows)
                        {
                            var spanElements = item.GetElementsByTagName("span");
                            var addToMyFavorite = spanElements.Cast<HtmlElement>()
                                .FirstOrDefault(x => x.GetAttribute("Title") == "加入我的最爱");
                            if (addToMyFavorite != null && addToMyFavorite.Parent != null)
                            {
                                leagueName = addToMyFavorite.Parent.InnerText;
                                if (!string.IsNullOrEmpty(leagueName))
                                {
                                    Console.WriteLine(leagueName);
                                    if (!dicData.ContainsKey(leagueName))
                                    {
                                        dicData.Add(leagueName, new List<string>());
                                    }
                                    else
                                    {
                                        Console.WriteLine(@"联赛名称重复！");
                                    }
                                    continue;
                                }
                            }
                            if (!string.IsNullOrEmpty(leagueName) &&
                                item.GetAttribute("className").Contains("displayOn"))
                            {
                                if (!dicData.ContainsKey(leagueName))
                                {
                                    Console.WriteLine(@"未找到联赛名称！");
                                }
                                else
                                {
                                    var dataElementCollection =
                                        item.GetElementsByTagName("a").Cast<HtmlElement>().Where(x => x.Name == "cvmy");
                                    var elementCollection = dataElementCollection as HtmlElement[] ??
                                                            dataElementCollection.ToArray();
                                    if (elementCollection.Count() % 8 == 1)
                                    {
                                        Console.WriteLine(@"比赛数据不完整！");
                                        continue;
                                    }
                                    foreach (var dataElement in elementCollection)
                                    {
                                        dicData[leagueName].Add(dataElement.InnerHtml);
                                    }
                                }
                            }
                        }
                        EndGrabData(dicData);
                    }
                }
            }
        }
    }
}