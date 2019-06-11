using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BinClass.Common.Interfaces;
using BinClass.Common.Types;
using BinLogicService.Types;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using ModuleActorService.Interfaces;

namespace BinLogicService
{
    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class BinLogicService : StatelessService, IBinClassLogic
    {
        private const string ModuleServiceUri = "fabric:/ModuleApp/ModulesActorService";

        public BinLogicService(StatelessServiceContext context)
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

        public async Task<BinClassResult> CalculateBinClass(string subId, CancellationToken cancellationToken)
        {
            var actorId = new ActorId(subId);
            var actor = ActorProxy.Create<IModules>(actorId, new Uri(ModuleServiceUri));

            var simData = await actor.GetSimData(cancellationToken);

            var binClass = BinLogic.CalculateBin(simData);

            var binResult = new BinClassResult()
            {
                BinClass = binClass,
                TimeStamp = DateTime.Now,
                CodeVersion = this.Context.CodePackageActivationContext.CodePackageVersion
            };

            await actor.SetBinClass(binResult, cancellationToken);

            return binResult;
        }
    }
}
