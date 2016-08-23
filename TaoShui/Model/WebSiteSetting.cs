using GalaSoft.MvvmLight;

namespace TaoShui.Model
{
    public class WebSiteSetting : ObservableObject
    {
        private long _id;
        private string _name;
        private string _url;
        private int _captchaLength;
        private int _grabDataInterval;
        private int _loginTimeOut;

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
    }
}