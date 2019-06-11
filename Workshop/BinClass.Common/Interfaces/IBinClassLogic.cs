using BinClass.Common.Types;
using Microsoft.ServiceFabric.Services.Remoting;
using Microsoft.ServiceFabric.Services.Remoting.FabricTransport;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

[assembly: FabricTransportServiceRemotingProvider(RemotingListenerVersion = RemotingListenerVersion.V2, RemotingClientVersion = RemotingClientVersion.V2)]
namespace BinClass.Common.Interfaces
{
    public interface IBinClassLogic : IService
    {
        Task<BinClassResult> CalculateBinClass(string subId, CancellationToken cancellationToken);
    }
}
