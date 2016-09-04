using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using TaoShui.Model;

namespace TaoShui.Shared
{
    public partial class ComboBoxItem : UserControl, INotifyPropertyChanged
    {
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string),
            typeof(ComboBoxItem));

        public static readonly DependencyProperty ItemsProperty = DependencyProperty.Register("Items",
            typeof(IEnumerable<IModelBase>), typeof(ComboBoxItem));

        public static readonly DependencyProperty SelectedProperty = DependencyProperty.Register("Selected",
            typeof(IModelBase), typeof(ComboBoxItem));

        public ComboBoxItem()
        {
            InitializeComponent();
        }

        public string Text
        {
            get { return GetValue(TextProperty) as string; }
            set
            {
                SetValue(TextProperty, value);
                OnPropertyChanged("Text");
            }
        }

        public IEnumerable<IModelBase> Items
        {
            get { return GetValue(ItemsProperty) as IEnumerable<IModelBase>; }
            set
            {
                SetValue(ItemsProperty, value);
                OnPropertyChanged("Items");
            }
        }

        public IModelBase Selected
        {
            get { return GetValue(SelectedProperty) as IModelBase; }
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
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}