using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
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
using CustomDrawing;
using CustomDrawing.Core;
using TaoShui.Model;

namespace TaoShui.View
{
    /// <summary>
    /// Interaction logic for WebSiteRelation.xaml
    /// </summary>
    public partial class WebSiteRelation : UserControl
    {
        private Editor _editor;
        private Factory _factory;
        private WpfElement _root;

        public WebSiteRelation()
        {
            InitializeComponent();
            Initialize();
        }

        private void Initialize()
        {
            _editor = new Editor
            {
                Factory = new Factory(),
                Painter = new Painter(),
                Pointer = new WpfPointer(Canvas),
                Redraw = () => _root.Redraw()
            };

            _root = new WpfElement(_editor.Painter);
            _factory = _editor.Factory;

            Canvas.Children.Add(_root);
            Canvas.MouseLeftButtonDown += (sender, e) => _editor.LeftDown(GetPosition(e));
            Canvas.MouseLeftButtonUp += (sender, e) => _editor.LeftUp();
            Canvas.MouseMove += (sender, e) => _editor.Move(GetPosition(e));

            PreviewMouseRightButtonDown += (sender, e) => _editor.RightDown();
        }

        private Point GetPosition(MouseButtonEventArgs e)
        {
            var p = e.GetPosition(Canvas);
            p.X = Math.Round(p.X, 0);
            p.Y = Math.Round(p.Y, 0);
            return p;
        }

        private Point GetPosition(MouseEventArgs e)
        {
            var p = e.GetPosition(Canvas);
            p.X = Math.Round(p.X, 0);
            p.Y = Math.Round(p.Y, 0);
            return p;
        }

        private void WebSiteRelation_OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            ObservableCollection<WebSiteSetting> webSiteSettings = e.NewValue as ObservableCollection<WebSiteSetting>;
            Drawing(webSiteSettings);
        }

        private void WebSiteRelation_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            ObservableCollection<WebSiteSetting> webSiteSettings = DataContext as ObservableCollection<WebSiteSetting>;
            Drawing(webSiteSettings);
        }

        #region Drawing

        private void Drawing(ObservableCollection<WebSiteSetting> webSiteSettings)
        {
            if (webSiteSettings == null)
            {
                return;
            }
            _root.Painter.Elements.Clear();

            var g = CreateCustomGate();

            float x = (float)(Canvas.ActualWidth / 5);
            float y = 30f;
            foreach (var setting in webSiteSettings)
            {
                InsertGate(g, setting.Name, x, y, 0f);
                InsertGate(g, setting.Name, (float)(Canvas.ActualWidth - x - 60f), y, 0f);
                y += 50f;
            }
            _editor.Redraw();
        }

        private void InsertGate(Custom g, string text, float x, float y, float angle)
        {
            var rg = _factory.CreateReference(null, g, x, y);
            var o = g.Variables[0] as Text;
            if (o != null) rg.Variables.Add(o.Id, text);
            rg.Angle = angle;
            CreateConnectorsGate(rg);
            _root.Painter.Elements.Add(rg);
        }

        private Custom CreateCustomGate()
        {
            var custom = _factory.CreateCustom(null, 0f, 0f);

            var p0 = _factory.CreatePin(custom, 0f, 0f, PinTypes.Snap);
            var p1 = _factory.CreatePin(custom, 60f, 0f, PinTypes.Snap);
            var p2 = _factory.CreatePin(custom, 60f, 30f, PinTypes.Snap);
            var p3 = _factory.CreatePin(custom, 0f, 30f, PinTypes.Snap);

            var l0 = _factory.CreateLine(custom, p0, p1);
            var l1 = _factory.CreateLine(custom, p1, p2);
            var l2 = _factory.CreateLine(custom, p2, p3);
            var l3 = _factory.CreateLine(custom, p3, p0);

            custom.Children.Add(l0);
            custom.Children.Add(l1);
            custom.Children.Add(l2);
            custom.Children.Add(l3);

            var text = _factory.CreateText(custom, 30f, 15f, "", HAlign.Center, VAlign.Center);

            custom.Children.Add(text);
            custom.Variables.Add(text);

            return custom;
        }

        private void CreateConnectorsGate(Reference reference)
        {
            reference.Connectors.Add(_factory.CreatePin(reference, 30f, 0f, PinTypes.Connector));
            reference.Connectors.Add(_factory.CreatePin(reference, 60f, 15f, PinTypes.Connector));
            reference.Connectors.Add(_factory.CreatePin(reference, 30f, 30f, PinTypes.Connector));
            reference.Connectors.Add(_factory.CreatePin(reference, 0f, 15f, PinTypes.Connector));
        }

        #endregion
    }
}
