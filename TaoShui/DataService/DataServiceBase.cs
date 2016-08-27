using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity.Validation;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using GalaSoft.MvvmLight;
using Repository.Dto;
using TaoShui.Model;

namespace TaoShui.DataService
{
    public abstract class DataServiceBase<TModel, TDto> : IDataService<TModel, TDto>
        where TModel : ObservableObject, IModelBase
        where TDto : new()
    {
        protected readonly IMapper _mapper;

        protected DataServiceBase()
        {
            var mapperConfig = new MapperConfiguration(config =>
            {
                config.CreateMap<TModel, TDto>();
                config.CreateMap<TDto, TModel>();
            });
            _mapper = mapperConfig.CreateMapper();
        }

        public abstract ObservableCollection<TModel> SelectAllModel();

        public abstract TModel SelectModelById(long id);

        public abstract ObservableCollection<TDto> SelectAllDto();

        public abstract TDto SelectDtoById(long id);

        public abstract DbResult<TDto> Update(TModel model);

        public abstract DbResult<TDto> Delete(TModel model);

        public abstract DbResult<TDto> Insert(TModel model);

        public IMapper Mapper
        {
            get { return _mapper; }
        }
    }
}
