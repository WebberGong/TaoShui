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
using GalaSoft.MvvmLight.CommandWpf;
using TaoShui.DataService;
using TaoShui.Model;
using Utils;

namespace TaoShui.Shared
{
    public class NewModelWindow<TModel, TDto> : Window
        where TModel : ObservableObject, IModelBase
        where TDto : new()
    {
        private readonly IDataService<TModel, TDto> _dataService;
        private readonly TModel _model;
        private readonly Action<DbResult, TModel> _callBackAction;

        public NewModelWindow(TModel model, IDataService<TModel, TDto> dataService, Action<DbResult, TModel> callBackAction, int width = 450)
        {
            _model = model;
            _dataService = dataService;
            _callBackAction = callBackAction;

            Type type = typeof(TModel);
            var props = type.GetProperties();
            var panelMain = new StackPanel();
            var panelInput = new StackPanel() { Margin = new Thickness(20) };
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
                    var value = p.GetValue(_model);
                    item.Value = value;
                    var valueType = p.PropertyType;
                    item.ValueType = valueType;
                    if (valueType.IsPrimitive || valueType == typeof(string))
                    {
                        Binding valueBinding = new Binding()
                        {
                            Source = _model,
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
                                    Source = _model,
                                    Path = new PropertyPath(p.Name),
                                    Mode = BindingMode.OneWay,
                                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                                };
                                BindingOperations.SetBinding(item, InputItem.ValueProperty, valueBinding);

                                Binding selectedBinding = new Binding()
                                {
                                    Source = _model,
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
                    panelInput.Children.Add(item);
                }
            }
            panelMain.Children.Add(panelInput);

            SaveFooter footer = new SaveFooter
            {
                SaveCommand = new RelayCommand(ExecuteSaveCommand),
                CancelCommand = new RelayCommand(ExecuteCancelCommand)
            };
            panelMain.Children.Add(footer);

            Content = panelMain;
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

        private void ExecuteSaveCommand()
        {
            var result = _dataService.Insert(_model);

            if (_callBackAction != null)
            {
                _callBackAction(result, _model);
            }

            if (result.IsSuccess)
            {
                MyMessageBox.ShowInformationDialog(result.CombinedMsg);
            }
            else
            {
                MyMessageBox.ShowWarningDialog(result.CombinedMsg);
            }

            if (result.IsValidationFailed)
            {
                return;
            }

            if (result.IsSuccess)
            {
                Close();
            }
        }

        private void ExecuteCancelCommand()
        {
            if (_callBackAction != null)
            {
                _callBackAction(new DbResult(false, null, "操作已取消"), _model);
            }
            Close();
        }
    }
}
