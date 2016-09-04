using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace TaoShui.Shared
{
    /// <summary>
    ///     Interaction logic for SaveFooter.xaml
    /// </summary>
    public partial class SaveFooter : UserControl, INotifyPropertyChanged
    {
        public static readonly DependencyProperty SaveCommandProperty = DependencyProperty.Register("SaveCommand",
            typeof(ICommand), typeof(SaveFooter));

        public static readonly DependencyProperty CancelCommandProperty = DependencyProperty.Register("CancelCommand",
            typeof(ICommand), typeof(SaveFooter));

        public SaveFooter()
        {
            InitializeComponent();
        }

        public ICommand SaveCommand
        {
            get { return GetValue(SaveCommandProperty) as ICommand; }
            set
            {
                SetValue(SaveCommandProperty, value);
                OnPropertyChanged("SaveCommand");
            }
        }

        public ICommand CancelCommand
        {
            get { return GetValue(CancelCommandProperty) as ICommand; }
            set
            {
                SetValue(CancelCommandProperty, value);
                OnPropertyChanged("CancelCommand");
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