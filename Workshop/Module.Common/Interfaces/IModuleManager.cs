using Microsoft.ServiceFabric.Services.Remoting;
using Microsoft.ServiceFabric.Services.Remoting.FabricTransport;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

[assembly: FabricTransportServiceRemotingProvider(RemotingListenerVersion = RemotingListenerVersion.V2, RemotingClientVersion = RemotingClientVersion.V2)]
namespace Module.Common
{
    public interface IModuleManager : IService
    {
        /// <summary>
        /// Returns a list of all module Ids
        /// </summary>
        /// <param name="cancellationToken">Token used to cancel the operation</param>
        /// <returns>List of module Ids</returns>
        Task<List<string>> GetModuleIds(CancellationToken cancellationToken);
    }
}
