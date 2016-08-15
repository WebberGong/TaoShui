using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using GalaSoft.MvvmLight;
using TaoShui.Model;

namespace TaoShui.ViewModel
{
    public class SettingViewModel : ViewModelBase
    {
        private string _name = "Setting";

        public SettingViewModel()
        {
            WebSiteSettings = new ObservableCollection<WebSiteSetting>
            {
                new WebSiteSetting()
                {
                    Name = "沙巴",
                    Url = "http://www.maxbet.com/",
                    LoginName = "test1",
                    Password = "test1",
                    CaptchaLength = 4,
                    GrabDataInterval = 1,
                    LoginTimeOut = 10
                },
                new WebSiteSetting()
                {
                    Name = "智博",
                    Url = "http://www.isn99.com/",
                    LoginName = "test2",
                    Password = "test2",
                    CaptchaLength = 4,
                    GrabDataInterval = 1,
                    LoginTimeOut = 10
                },
                new WebSiteSetting()
                {
                    Name = "沙巴",
                    Url = "http://www.maxbet.com/",
                    LoginName = "test1",
                    Password = "test1",
                    CaptchaLength = 4,
                    GrabDataInterval = 1,
                    LoginTimeOut = 10
                },
                new WebSiteSetting()
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

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public ObservableCollection<WebSiteSetting> WebSiteSettings { get; set; }
    }
}
