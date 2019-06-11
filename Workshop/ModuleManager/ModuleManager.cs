using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors.Client;
using Microsoft.ServiceFabric.Actors.Query;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Module.Common;

namespace ModuleManager
{
    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class ModuleManager : StatelessService, IModuleManager
    {
        private const string MODULEACTORSERVICE = "ModulesActorService";

        public ModuleManager(StatelessServiceContext context)
            : base(context)
        { }

        /// <summary>
        /// Optional override to create listeners (e.g., TCP, HTTP) for this service replica to handle client or user requests.
        /// </summary>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return this.CreateServiceRemotingInstanceListeners();
        }

        public async Task<List<string>> GetModuleIds(CancellationToken cancellationToken)
        {
            var serviceUri = new Uri($"{this.Context.CodePackageActivationContext.ApplicationName}/{MODULEACTORSERVICE}");
            var client = new FabricClient();
            var partitions = await client.QueryManager.GetPartitionListAsync(serviceUri);
            List<ActorInformation> actorInformationList = new List<ActorInformation>();
            foreach (var partition in partitions)
            {
                var actorProxy = ActorServiceProxy.Create(serviceUri, ((Int64RangePartitionInformation)partition.PartitionInformation).LowKey);
                ContinuationToken continuationToken = null;
                do
                {
                    var page = await actorProxy.GetActorsAsync(continuationToken, cancellationToken);
                    actorInformationList.AddRange(page.Items);
                    continuationToken = page.ContinuationToken;
                    cancellationToken.ThrowIfCancellationRequested();
                } while (continuationToken != null);

            }

            var aList = actorInformationList.Select<ActorInformation, string>(a => a.ActorId.GetStringId()).ToList();
            aList.Sort();

            return aList;
        }

    }
}
