﻿using System.Net;
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

            for (var i = 0; i < 1; i++)
            {
                WebSite test = new MaxBet(null, "pyh667h00a", "A123456a", 4, 30);
                //WebSite maxBet1 = new MaxBet(null, "pyh667h00a", "A123456a", 4, 30);
                //WebSite maxBet2 = new MaxBet(null, "sfb1337952", "Aaaa2234", 4, 30);
                //WebSite pinnacle1 = new Pinnacle(null, "hc2at84671", "aaaa2222", 4, 30);

                var webBrowserThread = new Thread(test.Run)
                {
                    Priority = ThreadPriority.AboveNormal,
                    IsBackground = true
                };
                webBrowserThread.Start();
            }
        }

        private bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain,
            SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }
    }
}