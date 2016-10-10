using FontAwesomeWPF;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TaoShui.Model;
using TaoShui.ViewModel;

namespace TaoShui.View
{
    /// <summary>
    /// WebSiteRelationView.xaml 的交互逻辑
    /// </summary>
    public partial class WebSiteRelationView : UserControl, INotifyPropertyChanged
    {
        public WebSiteRelationView()
        {
            InitializeComponent();

            DataContextChanged += (sender, e) =>
            {
                WebSiteSettingViewModel vm = e.NewValue as WebSiteSettingViewModel;
                if (vm != null)
                {
                    OnSourceChanged(vm.WebSiteSettings);
                }
            };

            SizeChanged += (sender, e) =>
            {
                OnSourceChanged(DataContext as ObservableCollection<WebSiteSetting>);
            };
        }

        private void OnSourceChanged(ObservableCollection<WebSiteSetting> source)
        {
            if (source != null)
            {
                var totalWidth = ActualWidth - Margin.Left - Margin.Right - 60 + source.Count * (1 + 4 / (double)source.Count);
                var width = totalWidth / (double)source.Count;
                if (width < 0)
                {
                    width = 0;
                }
                Panel1.Children.Clear();
                Panel2.Children.Clear();
                Panel3.Children.Clear();
                foreach (var item1 in source)
                {
                    Border b1 = new Border
                    {
                        Width = width,
                        Margin = new Thickness(-1, 0, 0, 1)
                    };
                    TextBlock t1 = new TextBlock { Text = item1.Name };
                    b1.Child = t1;
                    Panel1.Children.Add(b1);

                    Border b2 = new Border
                    {
                        Height = 60,
                        Margin = new Thickness(0, -1, 1, 0)
                    };
                    TextBlock t2 = new TextBlock { Text = item1.Name };
                    b2.Child = t2;
                    Panel2.Children.Add(b2);

                    StackPanel panel = new StackPanel
                    {
                        Height = 59,
                        Orientation = Orientation.Horizontal
                    };

                    foreach (var item2 in item1.RelatedWebSiteSettings)
                    {
                        Border b3 = new Border
                        {
                            Width = width,
                            Margin = new Thickness(-1, -1, 0, 0)
                        };

                        if (item1.Id == item2.Object.Id)
                        {
                            TextBlock t3 = new TextBlock
                            {
                                Text = Fa.Remove,
                                Style =
                                    new Style(typeof(TextBlock),
                                        Application.Current.Resources["FontAwesomeStyle"] as Style),
                                HorizontalAlignment = HorizontalAlignment.Center,
                                VerticalAlignment = VerticalAlignment.Center,
                                FontSize = 25,
                                Foreground = new SolidColorBrush() { Color = Colors.Red }
                            };
                            b3.Child = t3;
                            panel.Children.Add(b3);
                        }
                        else
                        {
                            CheckBox c3 = new CheckBox();
                            var isCheckedBinding = new Binding
                            {
                                Source = item2,
                                Path = new PropertyPath("IsSelected"),
                                Mode = BindingMode.TwoWay,
                                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                            };
                            BindingOperations.SetBinding(c3, ToggleButton.IsCheckedProperty, isCheckedBinding);

                            b3.Child = c3;
                            panel.Children.Add(b3);
                        }
                    }
                    Panel3.Children.Add(panel);
                }
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
