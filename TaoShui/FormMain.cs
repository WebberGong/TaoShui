using System;
using System.Windows.Forms;

namespace TaoShui
{
    public partial class FormMain : Form
    {
        public FormMain()
        {
            InitializeComponent();

            MyRequest.DoRequest();

            WebSite maxBet1 = new MaxBet(null, "pyh667h00a", "A123456a", new Uri("http://www.maxbet.com/Default.aspx"),
                new Uri("http://www.maxbet.com/login_code.aspx?1"), "Default.aspx", "ProcessLogin.aspx", "main.aspx", 15);
            //WebSite maxBet2 = new MaxBet(null, "sfb1337950", "Aaaa2234", new Uri("http://www.maxbet.com/Default.aspx"),
            //    new Uri("http://www.maxbet.com/login_code.aspx?1"), "Default.aspx", "ProcessLogin.aspx", "main.aspx", 15);
            maxBet1.Run();
        }
    }
}