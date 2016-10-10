using System.ComponentModel;
using GalaSoft.MvvmLight;
using System.Collections.ObjectModel;
using System.Text;

namespace TaoShui.Model
{
    public class WebSiteSetting : ObservableObject, IModelBase
    {
        private int _captchaLength;
        private int _grabDataInterval;
        private long _id;
        private int _loginTimeOut;
        private string _name;
        private string _url;
        private ObservableCollection<SelectableObject<WebSiteSetting>> _relatedWebSiteSettings;
        private string _relatedWebSiteSettingsString;

        [DisplayName(@"网址")]
        public string Url
        {
            get { return _url; }
            set
            {
                if (_url != value)
                {
                    _url = value;
                    RaisePropertyChanged();
                }
            }
        }

        [DisplayName(@"验证码长度")]
        public int CaptchaLength
        {
            get { return _captchaLength; }
            set
            {
                if (_captchaLength != value)
                {
                    _captchaLength = value;
                    RaisePropertyChanged();
                }
            }
        }

        [DisplayName(@"登录超时时间")]
        public int LoginTimeOut
        {
            get { return _loginTimeOut; }
            set
            {
                if (_loginTimeOut != value)
                {
                    _loginTimeOut = value;
                    RaisePropertyChanged();
                }
            }
        }

        [DisplayName(@"抓取数据时间间隔")]
        public int GrabDataInterval
        {
            get { return _grabDataInterval; }
            set
            {
                if (_grabDataInterval != value)
                {
                    _grabDataInterval = value;
                    RaisePropertyChanged();
                }
            }
        }

        public long Id
        {
            get { return _id; }
            set
            {
                if (_id != value)
                {
                    _id = value;
                    RaisePropertyChanged();
                }
            }
        }

        [DisplayName(@"网站名")]
        public string Name
        {
            get { return _name; }
            set
            {
                if (_name != value)
                {
                    _name = value;
                    RaisePropertyChanged();
                }
            }
        }

        public ObservableCollection<SelectableObject<WebSiteSetting>> RelatedWebSiteSettings
        {
            get { return _relatedWebSiteSettings; }
            set
            {
                if (_relatedWebSiteSettings != value)
                {
                    _relatedWebSiteSettings = value;
                    foreach (var setting in _relatedWebSiteSettings)
                    {
                        setting.PropertyChanged += (sender, e) =>
                        {
                            if (e.PropertyName == "IsSelected")
                            {
                                RelatedWebSiteSettingsString = GetRelatedWebSiteSettingsString();
                            }
                        };
                    }
                    RaisePropertyChanged();
                }
            }
        }

        public string RelatedWebSiteSettingsString
        {
            get { return _relatedWebSiteSettingsString; }
            set
            {
                if (_relatedWebSiteSettingsString != value)
                {
                    _relatedWebSiteSettingsString = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string GetRelatedWebSiteSettingsString()
        {
            StringBuilder result = new StringBuilder();
            if (RelatedWebSiteSettings != null)
            {
                foreach (var item in RelatedWebSiteSettings)
                {
                    if (item.IsSelected)
                    {
                        result.Append(item.Object.Id + ",");
                    }
                }
            }
            string settingString = result.ToString();
            if (settingString.Length > 0)
            {
                settingString = settingString.Substring(0, settingString.Length - 1);
            }
            return settingString;
        }
    }
}