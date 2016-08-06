using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Entity;
using Newtonsoft.Json;
using Utils;
using Timer = System.Timers.Timer;

namespace TaoShui
{
    public partial class FormMain : Form
    {
        private readonly Timer _getGrabbedDataTimer;

        public FormMain()
        {
            InitializeComponent();

            SharedMemoryManager.Instance.Write("isDataReady", "false");

            _getGrabbedDataTimer = new Timer(1000);
            _getGrabbedDataTimer.Elapsed += (sender, ev) =>
            {
                var isDataReady = SharedMemoryManager.Instance.Read("isDataReady");
                if (isDataReady == "true")
                {
                    var data = SharedMemoryManager.Instance.Read<WebSiteData>("WebSite.MaxBet");
                    LogHelper.LogInfo(GetType(), @"获取到的数据：" + JsonConvert.SerializeObject(data));
                }
            };
            _getGrabbedDataTimer.AutoReset = true;
            _getGrabbedDataTimer.Enabled = false;
        }

        private void btnTest_Click(object sender, EventArgs e)
        {
            StartProcess("WebSite.MaxBet", "sfb1337952", "Aaaa2234", 4, 60, 1);
            //StartProcess("WebSite.Pinnacle", "hc2at84671", "aaaa2222", 4, 60, 1);

            _getGrabbedDataTimer.Enabled = true;
            _getGrabbedDataTimer.Start();
        }

        private void StartProcess(string webSiteType, string loginName, string loginPassword, int captchaLength,
            int loginTimeOut, int grabDataInterval)
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
                if (Path.Combine(Application.StartupPath, "WebSiteProcess.exe") == p.MainModule.FileName)
                {
                    p.Kill();
                    p.Close();
                }
            }
        }

        private void FormMain_FormClosed(object sender, FormClosedEventArgs e)
        {
            KillProcess();
        }
    }
}