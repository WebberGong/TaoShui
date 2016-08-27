using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity.Validation;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using AutoMapper;
using Newtonsoft.Json;
using Repository;
using Repository.Dto;
using TaoShui.Model;

namespace TaoShui.DataService
{
    class WebSiteDataService : DataServiceBase<WebSite, WebSiteDto>
    {
        public override ObservableCollection<WebSite> SelectAllModel()
        {
            var dtoList = SelectAllDto().ToList();
            var results = new ObservableCollection<WebSite>();
            dtoList.ForEach(x => results.Add(_mapper.Map<WebSiteDto, WebSite>(x)));
            return results;
        }

        public override WebSite SelectModelById(long id)
        {
            var dto = SelectDtoById(id);
            var result = _mapper.Map<WebSiteDto, WebSite>(dto);
            return result;
        }

        public override ObservableCollection<WebSiteDto> SelectAllDto()
        {
            using (var context = new DatabaseContext())
            {
                var results = context.WebSites.ToList();
                return new ObservableCollection<WebSiteDto>(results);
            }
        }

        public override WebSiteDto SelectDtoById(long id)
        {
            using (var context = new DatabaseContext())
            {
                var result = context.WebSites.FirstOrDefault(x => x.Id == id);
                return result;
            }
        }

        public override DbResult<WebSiteDto> Update(WebSite model)
        {
            if (model == null)
                return new DbResult<WebSiteDto>(false, null, EnumErrorMsg.参数为空.ToString(), null);
            using (var context = new DatabaseContext())
            {
                var dto = context.WebSites.FirstOrDefault(x => x.Id == model.Id);
                if (dto != null)
                {
                    var serialized = JsonConvert.SerializeObject(dto);
                    _mapper.Map(model, dto);
                    var errors = context.GetValidationErrors().ToList();
                    var errorMsgs = DbResultBase.GetValidationErrors(errors);
                    if (string.IsNullOrEmpty(errorMsgs))
                    {
                        context.SaveChanges();
                        return new DbResult<WebSiteDto>(true, null, EnumOkMsg.修改数据成功.ToString(), dto);
                    }
                    var obj = JsonConvert.DeserializeObject(serialized, typeof(WebSiteDto)) as WebSiteDto;
                    _mapper.Map(obj, model);
                    return new DbResult<WebSiteDto>(false, errors, EnumErrorMsg.数据校验失败.ToString(), dto);
                }
                return new DbResult<WebSiteDto>(false, null, EnumErrorMsg.未找到要修改的数据.ToString(), null);
            }
        }

        public override DbResult<WebSiteDto> Delete(WebSite model)
        {
            if (model == null)
                return new DbResult<WebSiteDto>(false, null, EnumErrorMsg.参数为空.ToString(), null);
            using (var context = new DatabaseContext())
            {
                var dto = context.WebSites.FirstOrDefault(x => x.Id == model.Id);
                if (dto != null)
                {
                    context.WebSites.Remove(dto);
                    context.SaveChanges();
                    return new DbResult<WebSiteDto>(true, null, EnumOkMsg.删除数据成功.ToString(), dto);
                }
                return new DbResult<WebSiteDto>(false, null, EnumErrorMsg.未找到要删除的数据.ToString(), null);
            }
        }

        public override DbResult<WebSiteDto> Insert(WebSite model)
        {
            if (model == null)
                return new DbResult<WebSiteDto>(false, null, EnumErrorMsg.参数为空.ToString(), null);
            using (var context = new DatabaseContext())
            {
                var dto = _mapper.Map<WebSiteDto>(model);
                context.WebSites.Add(dto);
                var errors = context.GetValidationErrors().ToList();
                var errorMsgs = DbResultBase.GetValidationErrors(errors);
                if (string.IsNullOrEmpty(errorMsgs))
                {
                    context.SaveChanges();
                    return new DbResult<WebSiteDto>(true, null, EnumOkMsg.添加数据成功.ToString(), dto);
                }
                return new DbResult<WebSiteDto>(false, errors, EnumErrorMsg.数据校验失败.ToString(), dto);
            }
        }
    }
}
