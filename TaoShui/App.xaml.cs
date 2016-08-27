using System.Globalization;
using System.Threading;
using System.Windows;
using log4net;

namespace TaoShui
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            var culture = CultureInfo.GetCultureInfo("zh-cn");
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;
            Thread.CurrentThread.CurrentCulture = culture;
        }
    }
}