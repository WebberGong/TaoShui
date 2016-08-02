using System;
using System.Windows.Forms;
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
            //WebSiteBase maxBet1 = new MaxBet("sfb1337952", "Aaaa2234", 4, 200, 1);
            //maxBet1.Run();
            WebSiteBase maxBet2 = new MaxBet("pyh667h00a111", "A123456a111", 4, 60, 1);
            maxBet2.Run();
            //WebSiteBase pinnacle1 = new Pinnacle("hc2at84671111", "aaaa2222111", 4, 60, 1);
            //pinnacle1.Run();
        }
    }
}