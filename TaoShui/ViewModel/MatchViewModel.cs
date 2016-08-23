using GalaSoft.MvvmLight;
using TaoShui.DataService;

namespace TaoShui.ViewModel
{
    public class MatchViewModel : ViewModelBase
    {
        public MatchViewModel(IDataService dataService)
        {
        }

        private string _name = "Match";

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }
    }
}