using GalaSoft.MvvmLight;

namespace TaoShui.ViewModel
{
    public class MatchViewModel : ViewModelBase
    {
        private string _name = "Match";

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }
    }
}