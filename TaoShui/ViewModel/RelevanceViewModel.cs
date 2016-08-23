using GalaSoft.MvvmLight;
using TaoShui.DataService;

namespace TaoShui.ViewModel
{
    public class RelevanceViewModel : ViewModelBase
    {
        public RelevanceViewModel(IDataService dataService)
        {
        }

        private string _name = "Relevance";

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }
    }
}