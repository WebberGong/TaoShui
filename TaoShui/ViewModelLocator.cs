/*
  In App.xaml:
  <Application.Resources>
      <vm:ViewModelLocator xmlns:vm="clr-namespace:TaoShui"
                           x:Key="Locator" />
  </Application.Resources>
  
  In the View:
  DataContext="{Binding Source={StaticResource Locator}, Path=ViewModelName}"

  You can also use Blend to do all this with the tool's support.
  See http://www.galasoft.ch/mvvm
*/

using GalaSoft.MvvmLight.Ioc;
using Microsoft.Practices.ServiceLocation;
using TaoShui.ViewModel;

namespace TaoShui
{
    public class ViewModelLocator
    {
        public ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            //if (ViewModelBase.IsInDesignModeStatic)
            //{
            //    // Create design time view services and models
            //    SimpleIoc.Default.Register<IDataService, DesignDataService>();
            //}
            //else
            //{
            //    // Create run time view services and models
            //    SimpleIoc.Default.Register<IDataService, DataService>();
            //}

            SimpleIoc.Default.Register<MainViewModel>();
            SimpleIoc.Default.Register<SettingViewModel>();
            SimpleIoc.Default.Register<WebSiteViewModel>();
            SimpleIoc.Default.Register<MatchViewModel>();
            SimpleIoc.Default.Register<RelevanceViewModel>();
            SimpleIoc.Default.Register<WebSiteSettingViewModel>();
            SimpleIoc.Default.Register<SystemSettingViewModel>();
        }

        public MainViewModel Main
        {
            get { return ServiceLocator.Current.GetInstance<MainViewModel>(); }
        }

        public SettingViewModel Setting
        {
            get { return ServiceLocator.Current.GetInstance<SettingViewModel>(); }
        }

        public WebSiteViewModel WebSite
        {
            get { return ServiceLocator.Current.GetInstance<WebSiteViewModel>(); }
        }

        public MatchViewModel Match
        {
            get { return ServiceLocator.Current.GetInstance<MatchViewModel>(); }
        }

        public RelevanceViewModel Relevance
        {
            get { return ServiceLocator.Current.GetInstance<RelevanceViewModel>(); }
        }

        public WebSiteSettingViewModel WebSiteSetting
        {
            get { return ServiceLocator.Current.GetInstance<WebSiteSettingViewModel>(); }
        }

        public SystemSettingViewModel SystemSetting
        {
            get { return ServiceLocator.Current.GetInstance<SystemSettingViewModel>(); }
        }

        public static void Cleanup()
        {
            // TODO Clear the ViewModels
        }
    }
}