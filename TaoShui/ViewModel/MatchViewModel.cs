using GalaSoft.MvvmLight;
using TaoShui.DataService;

namespace TaoShui.ViewModel
{
    public class MatchViewModel : ViewModelBase
    {
        private string _name = "Match";

        public MatchViewModel()
        {
        }

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }
    }
}