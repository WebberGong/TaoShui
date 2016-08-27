using System.Collections.ObjectModel;
using AutoMapper;
using GalaSoft.MvvmLight;
using TaoShui.Model;

namespace TaoShui.DataService
{
    public interface IDataService<TModel, TDto> 
        where TModel : ObservableObject, IModelBase 
        where TDto : new()
    {
        ObservableCollection<TModel> SelectAllModel();

        TModel SelectModelById(long id);

        ObservableCollection<TDto> SelectAllDto();

        TDto SelectDtoById(long id);

        DbResult<TDto> Update(TModel model);

        DbResult<TDto> Delete(TModel model);

        DbResult<TDto> Insert(TModel model);

        IMapper Mapper { get; }
    }
}