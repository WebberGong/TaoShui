using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Repository;
using Repository.Dto;
using TaoShui.Model;

namespace TaoShui.DataService
{
    public class DataService : IDataService
    {
        private readonly IMapper _mapper;

        public DataService()
        {
            var mapperConfig = new MapperConfiguration(config =>
            {
                config.CreateMap<WebSiteSettingDto, WebSiteSetting>();
                config.CreateMap<WebSiteDto, WebSite>();

                config.CreateMap<WebSiteSetting, WebSiteSettingDto>();
                config.CreateMap<WebSite, WebSiteDto>();
            });
            _mapper = mapperConfig.CreateMapper();
        }

        public ObservableCollection<WebSiteSettingDto> GetWebSiteSettings()
        {
            using (var context = new DatabaseContext())
            {
                var list = context.WebSiteSettings.ToList();
                return new ObservableCollection<WebSiteSettingDto>(list);
                //var results = new ObservableCollection<WebSiteSetting>();
                //list.ForEach(x => results.Add(_mapper.Map<WebSiteSettingDto, WebSiteSetting>(x)));
                //return results;
            }
        }

        public ObservableCollection<WebSiteDto> GetWebSites()
        {
            using (var context = new DatabaseContext())
            {
                var list = context.WebSites.ToList();
                return new ObservableCollection<WebSiteDto>(list);
                //var results = new ObservableCollection<WebSite>();
                //list.ForEach(x => results.Add(_mapper.Map<WebSiteDto, WebSite>(x)));
                //return results;
            }
        }

        public void SaveWebSiteSetting(WebSiteSettingDto setting)
        {
            using (var context = new DatabaseContext())
            {
                var dto = context.WebSiteSettings.FirstOrDefault(x => x.Id == setting.Id);
                dto = setting;
                context.SaveChanges();
                //var dto = context.WebSiteSettings.FirstOrDefault(x => x.Id == setting.Id);
                //if (dto != null)
                //{
                //    dto = _mapper.Map<WebSiteSetting, WebSiteSettingDto>(setting);
                //    context.SaveChanges();
                //}
            }
        }
    }
}
