using System;
using System.ServiceModel;
using Utils;

namespace WcfService
{
    public class GrabDataClient : IGrabDataClient, IDisposable
    {
        private readonly ChannelFactory<IGrabDataService> _myServiceFactory;
        private IGrabDataService _channel;

        public GrabDataClient()
        {
            _myServiceFactory =
                new ChannelFactory<IGrabDataService>(
                    new NetNamedPipeBinding {MaxReceivedMessageSize = int.MaxValue},
                    new EndpointAddress(GrabDataConstants.ServiceBase + "/" + GrabDataConstants.ServiceName));
            _channel = _myServiceFactory.CreateChannel();
        }

        public IGrabDataService Channel
        {
            get
            {
                if (_myServiceFactory.State != CommunicationState.Opened)
                    try
                    {
                        LogHelper.Instance.LogError(GetType(), "抓取数据客户端已关闭,正在重连...");
                        _channel = _myServiceFactory.CreateChannel();
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Instance.LogError(GetType(), "重连抓取数据客户端异常!", ex);
                    }
                return _channel;
            }
        }

        public void Dispose()
        {
            _myServiceFactory.Close();
        }

        public void SendData(GrabbedData data)
        {
            Channel.ReceiveData(data);
        }
    }
}