using System;
using System.Threading;
using System.Threading.Tasks;
using BinClass.Common.Types;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Remoting.FabricTransport;
using Microsoft.ServiceFabric.Services.Remoting;
using Module.Common.Types;
using ModuleActorService.Interfaces.Types;

[assembly: FabricTransportActorRemotingProvider(RemotingListenerVersion = RemotingListenerVersion.V2, RemotingClientVersion = RemotingClientVersion.V2)]
namespace ModuleActorService.Interfaces
{
    /// <summary>
    /// This interface defines the methods exposed by an actor.
    /// Clients use this interface to interact with the actor that implements it.
    /// </summary>
    public interface IModules : IActor
    {
        /// <summary>
        /// Gets the <see cref="ModuleInfo"/> for the module
        /// </summary>
        /// <param name="cancellationToken">Token to cancel the operation</param>
        /// <returns>The <see cref="ModuleInfo"/></returns>
        Task<ModuleInfo> GetModuleInfo(CancellationToken cancellationToken);

        Task AddProducedEvent(ProducedEvent producedEvent, CancellationToken cancellationToken);

        Task AddScrapEvent(ScrapEvent scrapEvent, CancellationToken cancellationToken);

        Task SetProductStatus(ProductStatus productStatus, string location, DateTime dateTime, CancellationToken cancellationToken);

        Task SetSimData(SimResult simResult, CancellationToken cancellationToken);

        Task<SimResult> GetSimData(CancellationToken cancellationToken);

        Task SetBinClass(BinClassResult binClassResult, CancellationToken cancellationToken);
    }
}
