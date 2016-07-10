using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Windows.Forms;

namespace TaoShui
{
    public partial class FormMain : Form
    {
        public FormMain()
        {
            InitializeComponent();

            ServicePointManager.ServerCertificateValidationCallback = ValidateServerCertificate;

            WebSite maxBet1 = new MaxBet(browser, "pyh667h00a", "A123456a", 60);
            WebSite maxBet2 = new MaxBet(browser, "sfb1337952", "Aaaa2234", 30);
            WebSite pinnacle1 = new Pinnacle(browser, "hc2at84671", "aaaa2222", 30);

            var webBrowserThread = new Thread(maxBet1.Run)
            {
                Priority = ThreadPriority.AboveNormal,
                IsBackground = true
            };
            webBrowserThread.Start();
        }

        private bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain,
            SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }
    }
}