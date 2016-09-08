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
using Line = CustomDrawing.Core.Line;

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
            ReDrawing(webSiteSettings);
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

            float x = (float)(Canvas.ActualWidth / 8);
            float y = 30f;
            foreach (var setting in webSiteSettings)
            {
                InsertGate(g, setting.Name, x, y, 0f, Postion.Left);
                InsertGate(g, setting.Name, (float)(Canvas.ActualWidth - x - 60f), y, 0f, Postion.Right);
                y += 40f;
            }
            _editor.Redraw();
        }

        private void ReDrawing(ObservableCollection<WebSiteSetting> webSiteSettings)
        {
            if (webSiteSettings == null)
            {
                return;
            }
            float x = (float)(Canvas.ActualWidth / 8);

            foreach (var elem in _root.Painter.Elements)
            {
                if (elem is Reference)
                {
                    var reference = elem as Reference;
                    switch (reference.Position)
                    {
                        case Postion.Top:
                            break;
                        case Postion.Bottom:
                            break;
                        case Postion.Left:
                            reference.Origin.X = x;
                            break;
                        case Postion.Right:
                            reference.Origin.X = (float)(Canvas.ActualWidth - x - 60f);
                            break;
                        default:
                            break;
                    }
                }
                else if (elem is Line)
                {

                }
            }
            _editor.Redraw();
        }

        private void InsertGate(Custom g, string text, float x, float y, float angle, Postion postion)
        {
            var rg = _factory.CreateReference(null, g, x, y);
            var o = g.Variables[0] as Text;
            if (o != null)
            {
                rg.Variables.Add(o.Id, text);
                rg.Position = postion;
            }
            rg.Angle = angle;
            switch (postion)
            {
                case Postion.Top:
                    rg.Connectors.Add(_factory.CreatePin(rg, 30f, 30f, PinTypes.Connector));
                    break;
                case Postion.Bottom:
                    rg.Connectors.Add(_factory.CreatePin(rg, 30f, 0f, PinTypes.Connector));
                    break;
                case Postion.Left:
                    rg.Connectors.Add(_factory.CreatePin(rg, 60f, 15f, PinTypes.Connector));
                    break;
                case Postion.Right:
                    rg.Connectors.Add(_factory.CreatePin(rg, 0f, 15f, PinTypes.Connector));
                    break;
                default:
                    break;
            }
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

        #endregion
    }
}
