using GalaSoft.MvvmLight;

namespace TaoShui.ViewModel
{
    public class RelevanceViewModel : ViewModelBase
    {
        private string _name = "Relevance";

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }
    }
}