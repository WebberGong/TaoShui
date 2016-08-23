using GalaSoft.MvvmLight;
using TaoShui.DataService;

namespace TaoShui.ViewModel
{
    public class SystemSettingViewModel : ViewModelBase
    {

        public SystemSettingViewModel(IDataService dataService)
        {
        }

        private string _name = "SystemSetting";

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }
    }
}