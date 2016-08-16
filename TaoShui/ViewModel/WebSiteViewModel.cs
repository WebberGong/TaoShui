using GalaSoft.MvvmLight;

namespace TaoShui.ViewModel
{
    public class WebSiteViewModel : ViewModelBase
    {
        private string _name = "WebSite";

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }
    }
}