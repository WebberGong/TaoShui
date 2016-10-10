using System.Collections.ObjectModel;
using System.Linq;
using Newtonsoft.Json;
using Repository;
using Repository.Dto;
using TaoShui.Model;

namespace TaoShui.DataService
{
    internal class WebSiteAccountDataService : DataServiceBase<WebSiteAccount, WebSiteAccountDto>
    {
        public override ObservableCollection<WebSiteAccount> SelectAllModel()
        {
            var dtoList = SelectAllDto().ToList();
            var results = new ObservableCollection<WebSiteAccount>();
            dtoList.ForEach(x => results.Add(_mapper.Map<WebSiteAccountDto, WebSiteAccount>(x)));
            return results;
        }

        public override WebSiteAccount SelectModelById(long id)
        {
            var dto = SelectDtoById(id);
            var result = _mapper.Map<WebSiteAccountDto, WebSiteAccount>(dto);
            return result;
        }

        public override ObservableCollection<WebSiteAccountDto> SelectAllDto()
        {
            using (var context = new DatabaseContext())
            {
                var results = context.WebSiteAccounts.ToList();
                return new ObservableCollection<WebSiteAccountDto>(results);
            }
        }

        public override WebSiteAccountDto SelectDtoById(long id)
        {
            using (var context = new DatabaseContext())
            {
                var result = context.WebSiteAccounts.FirstOrDefault(x => x.Id == id);
                return result;
            }
        }

        public override bool SaveAllModel(ObservableCollection<WebSiteAccount> models)
        {
            using (var context = new DatabaseContext())
            {
                foreach (var dto in context.WebSiteAccounts)
                {
                    var model = models.FirstOrDefault(x => x.Id == dto.Id);
                    _mapper.Map(model, dto);
                }
                int result = context.SaveChanges();
                if (result > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public override DbResult<WebSiteAccountDto> Update(WebSiteAccount model)
        {
            if (model == null)
                return new DbResult<WebSiteAccountDto>(false, null, EnumErrorMsg.参数为空.ToString(), null);
            using (var context = new DatabaseContext())
            {
                var dto = context.WebSiteAccounts.FirstOrDefault(x => x.Id == model.Id);
                if (dto != null)
                {
                    var serialized = JsonConvert.SerializeObject(dto);
                    _mapper.Map(model, dto);
                    var errors = context.GetValidationErrors().ToList();
                    var errorMsgs = DbResultBase.GetValidationErrors(errors);
                    if (string.IsNullOrEmpty(errorMsgs))
                    {
                        context.SaveChanges();
                        return new DbResult<WebSiteAccountDto>(true, null, EnumOkMsg.修改数据成功.ToString(), dto);
                    }
                    var obj = JsonConvert.DeserializeObject(serialized, typeof(WebSiteAccountDto)) as WebSiteAccountDto;
                    _mapper.Map(obj, model);
                    return new DbResult<WebSiteAccountDto>(false, errors, EnumErrorMsg.数据校验失败.ToString(), dto);
                }
                return new DbResult<WebSiteAccountDto>(false, null, EnumErrorMsg.未找到要修改的数据.ToString(), null);
            }
        }

        public override DbResult<WebSiteAccountDto> Delete(WebSiteAccount model)
        {
            if (model == null)
                return new DbResult<WebSiteAccountDto>(false, null, EnumErrorMsg.参数为空.ToString(), null);
            using (var context = new DatabaseContext())
            {
                var dto = context.WebSiteAccounts.FirstOrDefault(x => x.Id == model.Id);
                if (dto != null)
                {
                    context.WebSiteAccounts.Remove(dto);
                    context.SaveChanges();
                    return new DbResult<WebSiteAccountDto>(true, null, EnumOkMsg.删除数据成功.ToString(), dto);
                }
                return new DbResult<WebSiteAccountDto>(false, null, EnumErrorMsg.未找到要删除的数据.ToString(), null);
            }
        }

        public override DbResult<WebSiteAccountDto> Insert(WebSiteAccount model)
        {
            if (model == null)
                return new DbResult<WebSiteAccountDto>(false, null, EnumErrorMsg.参数为空.ToString(), null);
            using (var context = new DatabaseContext())
            {
                var dto = _mapper.Map<WebSiteAccountDto>(model);
                context.WebSiteAccounts.Add(dto);
                var errors = context.GetValidationErrors().ToList();
                var errorMsgs = DbResultBase.GetValidationErrors(errors);
                if (string.IsNullOrEmpty(errorMsgs))
                {
                    context.SaveChanges();
                    model.Id = dto.Id;
                    return new DbResult<WebSiteAccountDto>(true, null, EnumOkMsg.添加数据成功.ToString(), dto);
                }
                return new DbResult<WebSiteAccountDto>(false, errors, EnumErrorMsg.数据校验失败.ToString(), dto);
            }
        }
    }
}