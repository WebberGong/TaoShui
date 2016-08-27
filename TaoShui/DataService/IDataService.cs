using System.Collections.ObjectModel;
using GalaSoft.MvvmLight;
using TaoShui.Model;

namespace TaoShui.DataService
{
    public interface IDataService<TModel, TDto> 
        where TModel : ObservableObject, IModelBase 
        where TDto : new()
    {
        ObservableCollection<TModel> SelectAll();

        DbResult Update(TModel model);

        DbResult Delete(TModel model);

        DbResult Insert(TModel model);
    }
}