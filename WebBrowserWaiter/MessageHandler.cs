using System;
using System.Runtime.InteropServices;
using System.Security.Permissions;

namespace WebBrowserWaiter
{
    [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
    [ComVisible(true)]
    public class MessageHandler
    {
        private readonly Action<string> _popupMsg;
        private readonly Action<string> _sendData;

        public MessageHandler(Action<string> popupMsg, Action<string> sendData)
        {
            _popupMsg = popupMsg;
            _sendData = sendData;
        }

        public void PopupMsg(string msg)
        {
            if (_popupMsg != null)
            {
                _popupMsg(msg);
            }
        }

        public void SendData(string data)
        {
            if (_sendData != null)
            {
                _sendData(data);
            }
        }
    }
}