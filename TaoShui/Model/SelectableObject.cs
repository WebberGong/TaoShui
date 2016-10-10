using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;

namespace TaoShui.Model
{
    public class SelectableObject<T> : ObservableObject
    {
        private bool _isSelected;
        private T _object;

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    RaisePropertyChanged();
                }
            }
        }

        public T Object
        {
            get { return _object; }
            set
            {
                if (value != null && !value.Equals(_object))
                {
                    _object = value;
                    RaisePropertyChanged();
                }
            }
        }
    }
}
