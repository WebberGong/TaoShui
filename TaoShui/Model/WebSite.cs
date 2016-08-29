using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using GalaSoft.MvvmLight;

namespace TaoShui.Model
{
    public class WebSite : ObservableObject, IModelBase
    {
        private long _id;
        private string _loginName;
        private string _password;
        private long _settingId;
        private string _name;
        private WebSiteSetting _setting;
        private ObservableCollection<WebSiteSetting> _settings;

        public WebSite()
        {       
        }

        public WebSite(ObservableCollection<WebSiteSetting> settings)
        {
            _settings = settings;
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

        [DisplayName(@"网站名")]
        [ForeignKey("Setting")]
        public ObservableCollection<WebSiteSetting> Settings
        {
            get { return _settings; }
            set
            {
                if (value != null && _settings != value)
                {
                    _settings = value;
                    RaisePropertyChanged();
                }
            }
        }

        [DisplayName(@"用户名")]
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

        [DisplayName(@"密码")]
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
                if (value != null && _setting != value)
                {
                    _setting = value;
                    SettingId = _setting.Id;
                    Name = _setting.Name;
                    RaisePropertyChanged();
                }
            }
        }
    }
}