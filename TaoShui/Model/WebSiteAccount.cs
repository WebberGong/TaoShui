using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using GalaSoft.MvvmLight;

namespace TaoShui.Model
{
    public class WebSiteAccount : ObservableObject, IModelBase
    {
        private long _id;
        private string _loginName;
        private string _name;
        private string _password;
        private WebSiteSetting _setting;
        private long _settingId;
        private ObservableCollection<WebSiteSetting> _settings;

        public WebSiteAccount()
        {
        }

        public WebSiteAccount(ObservableCollection<WebSiteSetting> settings)
        {
            _settings = settings;
        }

        [DisplayName(@"网站名")]
        [ForeignKey("Setting")]
        public ObservableCollection<WebSiteSetting> Settings
        {
            get { return _settings; }
            set
            {
                if ((value != null) && (_settings != value))
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
                    if (Settings != null)
                    {
                        var setting = Settings.FirstOrDefault(x => x.Id == _settingId);
                        Setting = setting;
                    }
                    RaisePropertyChanged();
                }
            }
        }

        public WebSiteSetting Setting
        {
            get { return _setting; }
            set
            {
                if ((value != null) && (_setting != value))
                {
                    _setting = value;
                    SettingId = _setting.Id;
                    Name = _setting.Name;
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
    }
}