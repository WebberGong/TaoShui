﻿using System.Collections.Generic;
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
    public class WebSiteSettingDataService : DataServiceBase<WebSiteSetting, WebSiteSettingDto>
    {
        public override ObservableCollection<WebSiteSetting> SelectAll()
        {
            using (var context = new DatabaseContext())
            {
                var list = context.WebSiteSettings.ToList();
                var results = new ObservableCollection<WebSiteSetting>();
                list.ForEach(x => results.Add(_mapper.Map<WebSiteSettingDto, WebSiteSetting>(x)));
                return results;
            }
        }

        public override DbResult Update(WebSiteSetting model)
        {
            if (model == null)
                return new DbResult(false, null, EnumErrorMsg.参数为空.ToString());
            using (var context = new DatabaseContext())
            {
                var dto = context.WebSiteSettings.FirstOrDefault(x => x.Id == model.Id);
                if (dto != null)
                {
                    var serialized = JsonConvert.SerializeObject(dto);
                    _mapper.Map(model, dto);
                    var errors = context.GetValidationErrors().ToList();
                    var errorMsgs = DbResult.GetValidationErrors(errors);
                    if (string.IsNullOrEmpty(errorMsgs))
                    {
                        context.SaveChanges();
                        return new DbResult(true, null, EnumOkMsg.修改数据成功.ToString());
                    }
                    var obj = JsonConvert.DeserializeObject(serialized, typeof(WebSiteSettingDto)) as WebSiteSettingDto;
                    _mapper.Map(obj, model);
                    return new DbResult(false, errors, EnumErrorMsg.数据校验失败.ToString());
                }
                return new DbResult(false, null, EnumErrorMsg.未找到要修改的数据.ToString());
            }
        }

        public override DbResult Delete(WebSiteSetting model)
        {
            if (model == null)
                return new DbResult(false, null, EnumErrorMsg.参数为空.ToString());
            using (var context = new DatabaseContext())
            {
                var dto = context.WebSiteSettings.FirstOrDefault(x => x.Id == model.Id);
                if (dto != null)
                {
                    context.WebSiteSettings.Remove(dto);
                    context.WebSites.RemoveRange(context.WebSites.Where(x => x.SettingId == dto.Id));
                    context.SaveChanges();
                    return new DbResult(true, null, EnumOkMsg.删除数据成功.ToString());
                }
                return new DbResult(false, null, EnumErrorMsg.未找到要删除的数据.ToString());
            }
        }

        public override DbResult Insert(WebSiteSetting model)
        {
            if (model == null)
                return new DbResult(false, null, EnumErrorMsg.参数为空.ToString());
            using (var context = new DatabaseContext())
            {
                var dto = _mapper.Map<WebSiteSettingDto>(model);
                context.WebSiteSettings.Add(dto);
                var errors = context.GetValidationErrors().ToList();
                var errorMsgs = DbResult.GetValidationErrors(errors);
                if (string.IsNullOrEmpty(errorMsgs))
                {
                    context.SaveChanges();
                    return new DbResult(true, null, EnumOkMsg.添加数据成功.ToString());
                }
                return new DbResult(false, errors, EnumErrorMsg.数据校验失败.ToString());
            }
        }
    }
}