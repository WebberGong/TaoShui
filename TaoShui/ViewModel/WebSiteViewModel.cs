using GalaSoft.MvvmLight;

namespace TaoShui.ViewModel
{
    public class WebSiteAccountViewModel : ViewModelBase
    {
        private string _name = "WebSiteAccount";

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }
    }
}