using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TaoShui.Model;

namespace TaoShui.Shared
{
    public partial class InputItem : UserControl, INotifyPropertyChanged
    {
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(InputItem));
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(object), typeof(InputItem));
        public static readonly DependencyProperty ValueTypeProperty = DependencyProperty.Register("ValueType", typeof(Type), typeof(InputItem));
        public static readonly DependencyProperty ValueTemplateProperty = DependencyProperty.Register("ValueTemplate", typeof(DataTemplate), typeof(InputItem));
        public static readonly DependencyProperty SelectedProperty = DependencyProperty.Register("Selected", typeof(IModelBase), typeof(InputItem));

        public InputItem()
        {
            InitializeComponent();
        }

        public string Text
        {
            get
            {
                return GetValue(TextProperty) as string;
            }
            set
            {
                SetValue(TextProperty, value);
                OnPropertyChanged("Text");
            }
        }

        public object Value
        {
            get
            {
                return GetValue(ValueProperty);
            }
            set
            {
                SetValue(ValueProperty, value);
                OnPropertyChanged("Value");
            }
        }

        public Type ValueType
        {
            get
            {
                return GetValue(ValueTypeProperty) as Type;
            }
            set
            {
                SetValue(ValueTypeProperty, value);
                OnPropertyChanged("ValueType");
                ValueTemplate = GetValueTemplate();
            }
        }

        public DataTemplate ValueTemplate
        {
            get
            {
                return GetValue(ValueTemplateProperty) as DataTemplate;
            }
            set
            {
                SetValue(ValueTemplateProperty, value);
                OnPropertyChanged("ValueTemplate");
            }
        }

        public IModelBase Selected
        {
            get
            {
                return GetValue(SelectedProperty) as IModelBase;
            }
            set
            {
                SetValue(SelectedProperty, value);
                OnPropertyChanged("Selected");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private DataTemplate GetValueTemplate()
        {
            if (ValueType == null)
            {
                return null;
            }
            if (ValueType.IsPrimitive || ValueType == typeof(string))
            {
                return Resources["TextBoxDataTemplate"] as DataTemplate;
            }
            else if (ValueType.IsArray || ValueType.GetInterface("IEnumerable") != null)
            {
                return Resources["ComboBoxDataTemplate"] as DataTemplate;
            }
            else
            {
                return null;
            }
        }
    }
}
