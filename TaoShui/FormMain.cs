using System;
using System.Windows.Forms;
using CaptchaRecogniser;
using WebSite;

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
            //WebSite maxBet1 = new MaxBet("pyh667h00a", "A123456a", 4, 30);
            //WebSite maxBet2 = new MaxBet("sfb1337952", "Aaaa2234", 4, 30);
            //WebSite pinnacle1 = new Pinnacle("hc2at84671", "aaaa2222", 4, 30);

            WebSiteBase maxBet1 = new MaxBet("sfb1337952", "Aaaa2234", 4, 600, 3);
            maxBet1.Run();

            //WebSite.WebSite pinnacle1 = new Pinnacle("hc2at84671111", "aaaa2222111", 4, 30);
            //pinnacle1.Run();
        }

        private void FormMain_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (Recogniser.Instance != null)
            {
                Recogniser.Instance.Dispose();
            }
        }
    }
}