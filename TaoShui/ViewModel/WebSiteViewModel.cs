using GalaSoft.MvvmLight;
using TaoShui.DataService;

namespace TaoShui.ViewModel
{
    public class WebSiteViewModel : ViewModelBase
    {
        public WebSiteViewModel(IDataService dataService)
        {
        }

        private string _name = "WebSite";

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }
    }
}