using System.Collections.ObjectModel;
using TaoShui.Model;

namespace TaoShui.DataService
{
    public interface IDataService
    {
        ObservableCollection<WebSiteSetting> GetWebSiteSettings();

        ObservableCollection<WebSite> GetWebSites();

        void SaveWebSiteSetting(WebSiteSetting setting);

        void SaveWebSite(WebSite site);
    }
}