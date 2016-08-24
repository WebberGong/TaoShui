using GalaSoft.MvvmLight;
using TaoShui.DataService;

namespace TaoShui.ViewModel
{
    public class SystemSettingViewModel : ViewModelBase
    {
        private string _name = "SystemSetting";

        public SystemSettingViewModel(IDataService dataService)
        {
        }

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }
    }
}