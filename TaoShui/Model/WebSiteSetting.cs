using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;

namespace TaoShui.Model
{
    public class WebSiteSetting : ObservableObject
    {
        private string _name;
        private string _url;
        private string _loginName;
        private string _password;
        private int _captchaLength;
        private int _loginTimeOut;
        private int _grabDataInterval;

        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                if (_name != value)
                {
                    _name = value;
                    RaisePropertyChanged();
                }
            }
        }

        public string Url
        {
            get
            {
                return _url;
            }
            set
            {
                if (_url != value)
                {
                    _url = value;
                    RaisePropertyChanged();
                }
            }
        }

        public string LoginName
        {
            get
            {
                return _loginName;
            }
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
            get
            {
                return _password;
            }
            set
            {
                if (_password != value)
                {
                    _password = value;
                    RaisePropertyChanged();
                }
            }
        }

        public int CaptchaLength
        {
            get
            {
                return _captchaLength;
            }
            set
            {
                if (_captchaLength != value)
                {
                    _captchaLength = value;
                    RaisePropertyChanged();
                }
            }
        }

        public int LoginTimeOut
        {
            get
            {
                return _loginTimeOut;
            }
            set
            {
                if (_loginTimeOut != value)
                {
                    _loginTimeOut = value;
                    RaisePropertyChanged();
                }
            }
        }

        public int GrabDataInterval
        {
            get
            {
                return _grabDataInterval;
            }
            set
            {
                if (_grabDataInterval != value)
                {
                    _grabDataInterval = value;
                    RaisePropertyChanged();
                }
            }
        }
    }
}
