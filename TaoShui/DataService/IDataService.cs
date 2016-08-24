using System.Collections.ObjectModel;
using TaoShui.Model;

namespace TaoShui.DataService
{
    public interface IDataService
    {
        ObservableCollection<WebSiteSetting> SelectAllWebSiteSettings();

        ObservableCollection<WebSite> SelectAllWebSites();

        bool UpdateWebSiteSetting(WebSiteSetting setting);

        bool UpdateWebSite(WebSite site);

        bool DeleteWebSiteSetting(WebSiteSetting setting);

        bool DeleteWebSite(WebSite site);

        bool InsertWebSiteSetting(WebSiteSetting setting);

        bool InsertWebSite(WebSite site);
    }
}