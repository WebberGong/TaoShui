using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Utils
{
    public class MyMessageBox
    {
        public static DialogResult ShowInformationDialog(string msg)
        {
            return MessageBox.Show(msg, "消息", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public static DialogResult ShowWarningDialog(string msg)
        {
            return MessageBox.Show(msg, "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        public static DialogResult ShowErrorDialog(string msg)
        {
            return MessageBox.Show(msg, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        public static DialogResult ShowQuestionDialog(string msg)
        {
            return MessageBox.Show(msg, "询问", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
        }

    }
}
