using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using Utils;
using WcfService;

namespace TaoShui
{
    public partial class FormMain : Form
    {
        public FormMain()
        {
            InitializeComponent();

            var thread = new Thread(() =>
            {
                var grabDataService = new GrabDataService();
                grabDataService.GrabDataSuccess += data =>
                {
                    LogHelper.Instance.LogInfo(GetType(), string.Format("Data Length：{0}, Type: {1}, Time: {2}",
                        data.Data == null ? 0 : data.Data.Length, data.Type,
                        data.GrabbedTime.ToString("yyyy-MM-dd HH:mm:ss:fff")));
                };
            })
            {
                IsBackground = true,
                Name = "ReceiveGrabbedDataThread",
                Priority = ThreadPriority.AboveNormal
            };
            thread.Start();
            StartProcess("WebSite.MaxBet", "sfb1337952", "Aaaa2235", 4, 60, 1);
            StartProcess("WebSite.Pinnacle", "hc2at84671", "aaaa2222", 4, 60, 1);
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