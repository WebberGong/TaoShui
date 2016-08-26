using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using AutoMapper.Execution;
using FontAwesome.Sharp;
using GalaSoft.MvvmLight;
using TaoShui.Model;

namespace TaoShui.Shared
{
    public class NewEntityWindow<T> : Window where T : ObservableObject, IModelBase
    {
        public NewEntityWindow(T entity, int width = 500)
        {
            Type type = typeof(T);
            var props = type.GetProperties();
            var panel = new StackPanel { Margin = new Thickness(20) };
            foreach (var p in props)
            {
                var item = new InputItem();
                var displayNameAttrs = p.GetCustomAttributes(typeof(DisplayNameAttribute), false);
                if (displayNameAttrs.Any())
                {
                    var displayNameAttr = displayNameAttrs.FirstOrDefault() as DisplayNameAttribute;
                    if (displayNameAttr != null) item.Text = displayNameAttr.DisplayName + ":";
                    var bindingMode = BindingMode.TwoWay;
                    if (p.SetMethod == null)
                    {
                        bindingMode = BindingMode.OneWay;
                        item.IsEnabled = false;
                    }
                    var value = p.GetValue(entity);
                    item.Value = value;
                    var valueType = p.PropertyType;
                    item.ValueType = valueType;
                    if (valueType.IsPrimitive || valueType == typeof(string))
                    {
                        Binding valueBinding = new Binding()
                        {
                            Source = entity,
                            Path = new PropertyPath(p.Name),
                            Mode = bindingMode,
                            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                        };
                        BindingOperations.SetBinding(item, InputItem.ValueProperty, valueBinding);
                    }
                    else if (valueType.IsArray || valueType.GetInterface("IEnumerable") != null)
                    {
                        var foreignKeyAttrs = p.GetCustomAttributes(typeof(ForeignKeyAttribute), false);
                        if (foreignKeyAttrs.Any())
                        {
                            var foreignKeyAttr = foreignKeyAttrs.FirstOrDefault() as ForeignKeyAttribute;
                            if (foreignKeyAttr != null)
                            {
                                Binding valueBinding = new Binding()
                                {
                                    Source = entity,
                                    Path = new PropertyPath(p.Name),
                                    Mode = BindingMode.OneWay,
                                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                                };
                                BindingOperations.SetBinding(item, InputItem.ValueProperty, valueBinding);

                                Binding selectedBinding = new Binding()
                                {
                                    Source = entity,
                                    Path = new PropertyPath(foreignKeyAttr.Name),
                                    Mode = BindingMode.TwoWay,
                                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                                };
                                BindingOperations.SetBinding(item, InputItem.SelectedProperty, selectedBinding);
                            }
                        }
                    }
                    else
                    {
                        continue;
                    }
                    panel.Children.Add(item);
                }
            }
            Content = panel;
            Width = width;
            SizeToContent = SizeToContent.Height;
            WindowStyle = WindowStyle.SingleBorderWindow;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            ResizeMode = ResizeMode.NoResize;
            Topmost = true;
            ShowInTaskbar = false;
            Icon = IconChar.PlusCircle.ToImageSource(new SolidColorBrush(Colors.ForestGreen), 100);
            FontFamily = new FontFamily("Microsoft YaHei");
            FontSize = 12;
        }
    }
}
