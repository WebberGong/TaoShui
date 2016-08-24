using System;
using System.ServiceModel;
using Utils;

namespace WcfService
{
    [ServiceBehavior(AddressFilterMode = AddressFilterMode.Any, InstanceContextMode = InstanceContextMode.Single)]
    public class GrabDataService : IGrabDataService
    {
        public GrabDataService()
        {
            CheckHost();
        }

        public ServiceHost Host { get; private set; }

        public event Action<GrabbedData> GrabDataSuccess;

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
                if ((Host == null) || (Host.State != CommunicationState.Opened))
                {
                    Host = new ServiceHost(this, new Uri(GrabDataConstants.ServiceBase));
                    Host.AddServiceEndpoint(
                        typeof(IGrabDataService),
                        new NetNamedPipeBinding {MaxReceivedMessageSize = int.MaxValue},
                        GrabDataConstants.ServiceName);
                    Host.Open();
                }
            }
            catch (Exception ex)
            {
                LogHelper.Instance.LogError(GetType(), "打开抓取数据服务端异常!", ex);
            }
        }
    }
}