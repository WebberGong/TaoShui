using System;
using System.Runtime.InteropServices;
using System.Security.Permissions;

namespace WebBrowserWaiter
{
    [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
    [ComVisible(true)]
    public class MessageHandler
    {
        private readonly Action<string> popupMsgHandler;

        public MessageHandler(Action<string> popupMsg)
        {
            popupMsgHandler = popupMsg;
        }

        public void PopupMsgHandler(string msg)
        {
            if (popupMsgHandler != null)
            {
                popupMsgHandler(msg);
            }
        }
    }
}