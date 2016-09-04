using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace TaoShui.Shared
{
    public partial class InputItem : UserControl, INotifyPropertyChanged
    {
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string),
            typeof(InputItem));

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(string),
            typeof(InputItem));

        public InputItem()
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

        public string Value
        {
            get { return GetValue(ValueProperty) as string; }
            set
            {
                SetValue(ValueProperty, value);
                OnPropertyChanged("Value");
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