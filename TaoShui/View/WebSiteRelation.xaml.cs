using FontAwesomeWPF;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace TaoShui.View
{
    /// <summary>
    /// WebSiteRelation.xaml 的交互逻辑
    /// </summary>
    public partial class WebSiteRelation : UserControl
    {
        public WebSiteRelation()
        {
            InitializeComponent();

            DataContextChanged += (sender, e) => 
            {
                ObservableCollection<WebSiteSetting> settings = e.NewValue as ObservableCollection<WebSiteSetting>;
                OnSourceChanged(settings);
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
                panel1.Children.Clear();
                panel2.Children.Clear();
                panel3.Children.Clear();
                foreach (var item1 in source)
                {
                    Border b1 = new Border
                    {
                        Width = width,
                        Margin = new Thickness(-1, 0, 0, 1)
                    };
                    TextBlock t1 = new TextBlock {Text = item1.Name};
                    b1.Child = t1;
                    panel1.Children.Add(b1);

                    Border b2 = new Border
                    {
                        Height = 60,
                        Margin = new Thickness(0, -1, 1, 0)
                    };
                    TextBlock t2 = new TextBlock {Text = item1.Name};
                    b2.Child = t2;
                    panel2.Children.Add(b2);

                    StackPanel panel = new StackPanel
                    {
                        Height = 59,
                        Orientation = Orientation.Horizontal
                    };

                    foreach (var item2 in source)
                    {
                        Border b3 = new Border
                        {
                            Width = width,
                            Margin = new Thickness(-1, -1, 0, 0)
                        };
                        CheckBox c3 = new CheckBox();
                        b3.Child = c3;
                        panel.Children.Add(b3);
                    }
                    panel3.Children.Add(panel);
                }
            }
        }
    }
}
