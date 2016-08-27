using GalaSoft.MvvmLight;
using TaoShui.DataService;

namespace TaoShui.ViewModel
{
    public class WebSiteViewModel : ViewModelBase
    {
        private string _name = "WebSite";

        public WebSiteViewModel()
        {
        }

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }
    }
}