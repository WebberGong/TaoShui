using GalaSoft.MvvmLight;
using TaoShui.DataService;

namespace TaoShui.ViewModel
{
    public class RelevanceViewModel : ViewModelBase
    {
        private string _name = "Relevance";

        public RelevanceViewModel(IDataService dataService)
        {
        }

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }
    }
}