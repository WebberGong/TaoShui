using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Newtonsoft.Json;
using TaoShui.ViewModel;
using Utils;
using WcfService;
using ThreadState = System.Threading.ThreadState;

namespace TaoShui
{
    public class MainViewModel : ViewModelBase
    {
        private readonly MatchViewModel _matchViewModel = new MatchViewModel();
        private readonly double _minHeight = SystemParameters.PrimaryScreenHeight/5*4;
        private readonly double _minWidth = SystemParameters.PrimaryScreenWidth/5*4;
        private readonly Thread _receiveGrabbedDataThread;
        private readonly RelevanceViewModel _relevanceViewModel = new RelevanceViewModel();
        private readonly SettingViewModel _settingViewModel = new SettingViewModel();
        private readonly WebSiteViewModel _webSiteViewModel = new WebSiteViewModel();
        private ViewModelBase _currentViewModel;
        private bool _isAutoBet;
        private bool _isAutoRun;

        public MainViewModel()
        {
            _currentViewModel = _settingViewModel;
            ClosedCommand = new RelayCommand(ExecuteClosedCommand);
            SettingViewCommand = new RelayCommand(ExecuteSettingViewCommand);
            WebSiteViewCommand = new RelayCommand(ExecuteWebSiteViewCommand);
            MatchViewCommand = new RelayCommand(ExecuteMatchViewCommand);
            RelevanceViewCommand = new RelayCommand(ExecuteRelevanceViewCommand);
            AutoBetCommand = new RelayCommand<bool>(ExecuteAutoBetCommand);
            AutoRunCommand = new RelayCommand<bool>(ExecuteAutoRunCommand);

            _receiveGrabbedDataThread = new Thread(() =>
            {
                var grabDataService = new GrabDataService();
                grabDataService.GrabDataSuccess += data =>
                {
                    if (IsAutoRun)
                    {
                        LogHelper.Instance.LogInfo(GetType(), string.Format("Data Count£º{0}, Type: {1}, Time: {2}",
                            data.Data == null ? 0 : data.Data.Length, data.Type,
                            data.GrabbedTime.ToString("yyyy-MM-dd HH:mm:ss:fff")));
                        Console.WriteLine(JsonConvert.SerializeObject(data));
                    }
                };
            })
            {
                IsBackground = true,
                Name = "ReceiveGrabbedDataThread",
                Priority = ThreadPriority.AboveNormal
            };
        }

        public ICommand ClosedCommand { get; private set; }

        public ICommand SettingViewCommand { get; private set; }

        public ICommand WebSiteViewCommand { get; private set; }

        public ICommand MatchViewCommand { get; private set; }

        public ICommand RelevanceViewCommand { get; private set; }

        public ICommand AutoBetCommand { get; private set; }

        public ICommand AutoRunCommand { get; private set; }

        public ViewModelBase CurrentViewModel
        {
            get { return _currentViewModel; }
            set
            {
                if (_currentViewModel != value)
                {
                    _currentViewModel = value;
                    RaisePropertyChanged();
                }
            }
        }

        public bool IsAutoBet
        {
            get { return _isAutoBet; }
            set
            {
                if (_isAutoBet != value)
                {
                    _isAutoBet = value;
                    RaisePropertyChanged();
                }
            }
        }

        public bool IsAutoRun
        {
            get { return _isAutoRun; }
            set
            {
                if (_isAutoRun != value)
                {
                    _isAutoRun = value;
                    RaisePropertyChanged();
                }
            }
        }

        public double MinWidth
        {
            get { return _minWidth; }
        }

        public double MinHeight
        {
            get { return _minHeight; }
        }

        private void ExecuteClosedCommand()
        {
            KillProcess();
        }

        private void ExecuteSettingViewCommand()
        {
            CurrentViewModel = _settingViewModel;
        }

        private void ExecuteWebSiteViewCommand()
        {
            CurrentViewModel = _webSiteViewModel;
        }

        private void ExecuteMatchViewCommand()
        {
            CurrentViewModel = _matchViewModel;
        }

        private void ExecuteRelevanceViewCommand()
        {
            CurrentViewModel = _relevanceViewModel;
        }

        private void ExecuteAutoBetCommand(bool isChecked)
        {
        }

        private void ExecuteAutoRunCommand(bool isChecked)
        {
            if (isChecked)
            {
                if ((_receiveGrabbedDataThread.ThreadState & ThreadState.Unstarted) == ThreadState.Unstarted)
                {
                    _receiveGrabbedDataThread.Start();
                    StartProcess("WebSite.MaxBet", "sfb1337952", "Aaaa2235", 4, 60, 1);
                    StartProcess("WebSite.Pinnacle", "hh7d1hi061", "ss123456@", 4, 60, 1);
                    StartProcess("WebSite.BetIsn", "zb999111", "sss123456", 4, 60, 1);
                }
            }
        }

        private void StartProcess(string webSiteType, string loginName, string loginPassword,
            int captchaLength, int loginTimeOut, int grabDataInterval)
        {
            var process = new Process();
            var start = new ProcessStartInfo("WebSiteProcess.exe")
            {
                CreateNoWindow = false,
                UseShellExecute = true,
                Arguments =
                    string.Format("{0} {1} {2} {3} {4} {5}", webSiteType, loginName, loginPassword, captchaLength,
                        loginTimeOut, grabDataInterval)
            };
            process.StartInfo = start;
            process.Start();
        }

        private void KillProcess()
        {
            var processes = Process.GetProcessesByName("WebSiteProcess");
            foreach (var p in processes)
            {
                if (Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "WebSiteProcess.exe") == p.MainModule.FileName)
                {
                    p.Kill();
                    p.Close();
                }
            }
        }
    }
}