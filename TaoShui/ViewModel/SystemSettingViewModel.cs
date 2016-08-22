using GalaSoft.MvvmLight;

namespace TaoShui.ViewModel
{
    public class SystemSettingViewModel : ViewModelBase
    {
        private string _name = "SystemSetting";

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }
    }
}