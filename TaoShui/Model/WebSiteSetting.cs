using System.ComponentModel;
using GalaSoft.MvvmLight;

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
    }
}