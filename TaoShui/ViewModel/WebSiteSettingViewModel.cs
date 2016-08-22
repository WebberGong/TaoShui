using System.Collections.ObjectModel;
using GalaSoft.MvvmLight;
using TaoShui.Model;

namespace TaoShui.ViewModel
{
    public class WebSiteSettingViewModel : ViewModelBase
    {
        public WebSiteSettingViewModel()
        {
            WebSiteSettings = new ObservableCollection<WebSiteSetting>
            {
                new WebSiteSetting
                {
                    Name = "沙巴",
                    Url = "http://www.maxbet.com/",
                    LoginName = "test1",
                    Password = "test1",
                    CaptchaLength = 4,
                    GrabDataInterval = 1,
                    LoginTimeOut = 10
                },
                new WebSiteSetting
                {
                    Name = "智博",
                    Url = "http://www.isn99.com/",
                    LoginName = "test2",
                    Password = "test2",
                    CaptchaLength = 4,
                    GrabDataInterval = 1,
                    LoginTimeOut = 10
                },
                new WebSiteSetting
                {
                    Name = "沙巴",
                    Url = "http://www.maxbet.com/",
                    LoginName = "test1",
                    Password = "test1",
                    CaptchaLength = 4,
                    GrabDataInterval = 1,
                    LoginTimeOut = 10
                },
                new WebSiteSetting
                {
                    Name = "智博",
                    Url = "http://www.isn99.com/",
                    LoginName = "test2",
                    Password = "test2",
                    CaptchaLength = 4,
                    GrabDataInterval = 1,
                    LoginTimeOut = 10
                }
            };
        }

        public ObservableCollection<WebSiteSetting> WebSiteSettings { get; set; }
    }
}