using System.ComponentModel;
using GalaSoft.MvvmLight;

namespace TaoShui.Model
{
    public class WebSite : ObservableObject
    {
        private long _id;
        private string _loginName;
        private string _password;
        private long _settingId;
        private WebSiteSetting _setting;

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

        public string LoginName
        {
            get { return _loginName; }
            set
            {
                if (_loginName != value)
                {
                    _loginName = value;
                    RaisePropertyChanged();
                }
            }
        }

        public string Password
        {
            get { return _password; }
            set
            {
                if (_password != value)
                {
                    _password = value;
                    RaisePropertyChanged();
                }
            }
        }

        public long SettingId
        {
            get { return _settingId; }
            set
            {
                if (_settingId != value)
                {
                    _settingId = value;
                    RaisePropertyChanged();
                }
            }
        }

        public WebSiteSetting Setting
        {
            get { return _setting; }
            set
            {
                if (_setting != value)
                {
                    _setting = value;
                    RaisePropertyChanged();
                }
            }
        }
    }
}