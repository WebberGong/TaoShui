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

            this.SizeChanged += (sender, e) =>
            {
                OnSourceChanged(this.DataContext as ObservableCollection<WebSiteSetting>);
            };
        }

        private void OnSourceChanged(ObservableCollection<WebSiteSetting> source)
        {
            if (source != null)
            {
                var totalWidth = this.ActualWidth - this.Margin.Left - this.Margin.Right - 60 + source.Count * (1 + 4 / (double)source.Count);
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
                    Border b1 = new Border();
                    b1.Width = width;
                    b1.Margin = new Thickness(-1, 0, 0, 1);
                    TextBlock t1 = new TextBlock();
                    t1.Text = item1.Name;
                    b1.Child = t1;
                    panel1.Children.Add(b1);

                    Border b2 = new Border();
                    b2.Height = 60;
                    b2.Margin = new Thickness(0, -1, 1, 0);
                    TextBlock t2 = new TextBlock();
                    t2.Text = item1.Name;
                    b2.Child = t2;
                    panel2.Children.Add(b2);

                    StackPanel panel = new StackPanel();
                    panel.Height = 59;
                    panel.Orientation = Orientation.Horizontal;

                    foreach (var item2 in source)
                    {
                        Border b3 = new Border();
                        b3.Width = width;
                        b3.Margin = new Thickness(-1, -1, 0, 0);
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
