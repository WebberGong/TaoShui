using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace TaoShui
{
    public partial class FormMain : Form
    {
        public FormMain()
        {
            InitializeComponent();
        }

        private void btnTest_Click(object sender, EventArgs e)
        {
            StartProcess("WebSite.MaxBet", "pyh667h00a111", "A123456a111", 4, 60, 1);
            StartProcess("WebSite.Pinnacle", "hc2at84671", "aaaa2222", 4, 60, 1);
        }

        private void StartProcess(string webSiteType, string loginName, string loginPassword, int captchaLength,
            int loginTimeOut, int grabDataInterval)
        {
            var process = new Process();
            var start = new ProcessStartInfo("WebSiteProcess.exe")
            {
                //CreateNoWindow = false,
                //UseShellExecute = true,
                CreateNoWindow = true,
                UseShellExecute = false,
                Arguments = webSiteType + "|" + loginName + "|" + loginPassword +
                    "|" + captchaLength + "|" + loginTimeOut + "|" + grabDataInterval,
            };
            process.StartInfo = start;
            process.Start();
        }

        private void KillProcess()
        {
            Process[] processes = Process.GetProcessesByName("WebSiteProcess");
            foreach (Process p in processes)
            {
                if (System.IO.Path.Combine(Application.StartupPath, "WebSiteProcess.exe") == p.MainModule.FileName)
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