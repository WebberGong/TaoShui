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
using Repository.Dto;
using TaoShui.DataService;
using TaoShui.Model;
using TaoShui.ViewModel;

namespace TaoShui
{
    public class ViewModelLocator
    {
        public ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            SimpleIoc.Default.Register<MainViewModel>();
            SimpleIoc.Default.Register<SettingViewModel>();
            SimpleIoc.Default.Register<WebSiteViewModel>();
            SimpleIoc.Default.Register<MatchViewModel>();
            SimpleIoc.Default.Register<RelevanceViewModel>();
            SimpleIoc.Default.Register<WebSiteSettingViewModel>();
            SimpleIoc.Default.Register<SystemSettingViewModel>();

            SimpleIoc.Default.Register<IDataService<WebSiteSetting, WebSiteSettingDto>, WebSiteSettingDataService>();
            SimpleIoc.Default.Register<IDataService<WebSite, WebSiteDto>, WebSiteDataService>();
        }

        public MainViewModel Main
        {
            get { return ServiceLocator.Current.GetInstance<MainViewModel>(); }
        }
    }
}