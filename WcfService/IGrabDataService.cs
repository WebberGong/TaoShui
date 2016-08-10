using System;
using System.ServiceModel;

namespace WcfService
{
    [ServiceContract]
    public interface IGrabDataService
    {
        event Action<GrabbedData> GrabDataSuccess;

        [OperationContract]
        void ReceiveData(GrabbedData data);
    }
}