using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity.Validation;
using System.Linq;
using System.Windows;
using AutoMapper;
using Newtonsoft.Json;
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

        public ObservableCollection<WebSiteSetting> GetWebSiteSettings()
        {
            using (var context = new DatabaseContext())
            {
                var list = context.WebSiteSettings.ToList();
                var results = new ObservableCollection<WebSiteSetting>();
                list.ForEach(x => results.Add(_mapper.Map<WebSiteSettingDto, WebSiteSetting>(x)));
                return results;
            }
        }

        public ObservableCollection<WebSite> GetWebSites()
        {
            using (var context = new DatabaseContext())
            {
                var list = context.WebSites.ToList();
                var results = new ObservableCollection<WebSite>();
                list.ForEach(x => results.Add(_mapper.Map<WebSiteDto, WebSite>(x)));
                return results;
            }
        }

        public void SaveWebSiteSetting(WebSiteSetting setting)
        {
            using (var context = new DatabaseContext())
            {
                var dto = context.WebSiteSettings.FirstOrDefault(x => x.Id == setting.Id);
                if (dto != null)
                {
                    var serialized = JsonConvert.SerializeObject(dto);
                    _mapper.Map(setting, dto);
                    var errors = context.GetValidationErrors();
                    var results = errors as IList<DbEntityValidationResult> ?? errors.ToList();
                    var errorMsgs = GetValidationErrors(results);
                    if (string.IsNullOrEmpty(errorMsgs))
                    {
                        context.SaveChanges();
                    }
                    else
                    {
                        MessageBox.Show(errorMsgs);
                        var obj =
                            JsonConvert.DeserializeObject(serialized, typeof(WebSiteSettingDto)) as WebSiteSettingDto;
                        _mapper.Map(obj, setting);
                    }
                }
            }
        }

        public void SaveWebSite(WebSite site)
        {
            using (var context = new DatabaseContext())
            {
                var dto = context.WebSites.FirstOrDefault(x => x.Id == site.Id);
                if (dto != null)
                {
                    var serialized = JsonConvert.SerializeObject(dto);
                    _mapper.Map(site, dto);
                    var errors = context.GetValidationErrors();
                    var results = errors as IList<DbEntityValidationResult> ?? errors.ToList();
                    var errorMsgs = GetValidationErrors(results);
                    if (string.IsNullOrEmpty(errorMsgs))
                    {
                        context.SaveChanges();
                    }
                    else
                    {
                        MessageBox.Show(errorMsgs);
                        var obj = JsonConvert.DeserializeObject(serialized, typeof(WebSiteDto)) as WebSiteDto;
                        _mapper.Map(obj, site);
                    }
                }
            }
        }

        private string GetValidationErrors(IList<DbEntityValidationResult> results)
        {
            var errorMsg = string.Empty;
            if (results.Any())
                foreach (var result in results)
                    if (!result.IsValid)
                        foreach (var error in result.ValidationErrors)
                            errorMsg += error.ErrorMessage;
            return errorMsg;
        }
    }
}