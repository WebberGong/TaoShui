using System;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using Utils;

namespace WcfService
{
    [ServiceBehavior(AddressFilterMode = AddressFilterMode.Any, InstanceContextMode = InstanceContextMode.Single)]
    public class GrabDataService : IGrabDataService
    {
        private ServiceHost _host;

        public event Action<GrabbedData> GrabDataSuccess;

        public GrabDataService()
        {
            CheckHost();
        }

        public ServiceHost Host
        {
            get
            {
                return _host;
            }
        }

        public void ReceiveData(GrabbedData data)
        {
            if (GrabDataSuccess != null)
            {
                CheckHost();
                GrabDataSuccess(data);
            }
        }

        private void CheckHost()
        {
            try
            {
                if (_host == null || _host.State != CommunicationState.Opened)
                {
                    _host = new ServiceHost(this, new Uri(GrabDataConstants.ServiceBase));
                    _host.AddServiceEndpoint(typeof(IGrabDataService), new NetNamedPipeBinding(),
                        GrabDataConstants.ServiceName);
                    _host.Open();
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogError(GetType(), "打开抓取数据服务端异常!", ex);
            }
        }
    }
}