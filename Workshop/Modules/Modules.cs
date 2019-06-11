using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using Module.Common.Types;
using ModuleActorService.Interfaces;
using ModuleActorService.Interfaces.Types;
using BinClass.Common.Types;

namespace Modules
{
    /// <remarks>
    /// This class represents an actor.
    /// Every ActorID maps to an instance of this class.
    /// The StatePersistence attribute determines persistence and replication of actor state:
    ///  - Persisted: State is written to disk and replicated.
    ///  - Volatile: State is kept in memory only and replicated.
    ///  - None: State is kept in memory only and not replicated.
    /// </remarks>
    [StatePersistence(StatePersistence.Persisted)]
    internal class Modules : Actor, IModules
    {
        #region [ Private Constants ]

        private const string MODULEINFOSTATENAME = "ModuleInfoState";
        private const string SIMDATASTATENAME = "SimDataState";
        private const string BINCLASSSTATENAME = "BinClassState";

        #endregion

        /// <summary>
        /// Initializes a new instance of Modules
        /// </summary>
        /// <param name="actorService">The Microsoft.ServiceFabric.Actors.Runtime.ActorService that will host this actor instance.</param>
        /// <param name="actorId">The Microsoft.ServiceFabric.Actors.ActorId for this actor instance.</param>
        public Modules(ActorService actorService, ActorId actorId)
            : base(actorService, actorId)
        {
        }

        public async Task AddProducedEvent(ProducedEvent producedEvent, CancellationToken cancellationToken)
        {
            ActorEventSource.Current.ActorMessage(this, "AddProducedEvent called");
            var newInfo = new ModuleInfo(GetSubId()) { ProducedEvents = new List<ProducedEvent>() { producedEvent }, ProductStatus = producedEvent.ProductStatus };

            await this.StateManager.AddOrUpdateStateAsync(MODULEINFOSTATENAME, newInfo, (key, value) =>
            {
                value.ProducedEvents.Add(producedEvent);
                return value;
            }, cancellationToken);
        }

        public async Task AddScrapEvent(ScrapEvent scrapEvent, CancellationToken cancellationToken)
        {
            ActorEventSource.Current.ActorMessage(this, "AddScrapEvent called");
            var newInfo = new ModuleInfo(GetSubId())
            {
                ScrapEvents = new List<ScrapEvent>() { scrapEvent },
                ProductStatusLocation = scrapEvent.Location,
                ProductStatus = ProductStatus.Scrap,
                ProductStatusTimestamp = scrapEvent.TimeStamp
            };

            await this.StateManager.AddOrUpdateStateAsync(MODULEINFOSTATENAME, newInfo, (key, value) =>
            {
                value.ProductStatusTimestamp = scrapEvent.TimeStamp;
                value.ProductStatus = ProductStatus.Scrap;
                value.ProductStatusLocation = scrapEvent.Location;

                return value;
            }, cancellationToken);
        }

        public async Task<ModuleInfo> GetModuleInfo(CancellationToken cancellationToken)
        {
            ActorEventSource.Current.ActorMessage(this, "GetModuleInfo called");
            var info = await this.StateManager.TryGetStateAsync<ModuleInfo>(MODULEINFOSTATENAME, cancellationToken);
            return info.HasValue ? info.Value : ModuleInfo.DefaultModuleInfo(GetSubId());
        }

        public async Task SetProductStatus(ProductStatus productStatus, string location, DateTime dateTime, CancellationToken cancellationToken)
        {
            ActorEventSource.Current.ActorMessage(this, "SetProductStatus called");
            var newInfo = new ModuleInfo(GetSubId())
            {
                ProductStatusLocation = location,
                ProductStatus = ProductStatus.Scrap,
                ProductStatusTimestamp = dateTime
            };

            await this.StateManager.AddOrUpdateStateAsync(MODULEINFOSTATENAME, newInfo, (key, value) =>
            {
                value.ProductStatusTimestamp = dateTime;
                value.ProductStatus = ProductStatus.Scrap;
                value.ProductStatusLocation = location;

                return value;
            }, cancellationToken);
        }

        public async Task SetSimData(SimResult simResult, CancellationToken cancellationToken)
        {
            ActorEventSource.Current.ActorMessage(this, "SetSimData called");
            await this.StateManager.AddOrUpdateStateAsync(SIMDATASTATENAME, simResult, (key, value) => simResult, cancellationToken);
        }

        public async Task<SimResult> GetSimData(CancellationToken cancellationToken)
        {
            ActorEventSource.Current.ActorMessage(this, "GetSimData called");
            var result = await this.StateManager.TryGetStateAsync<SimResult>(SIMDATASTATENAME, cancellationToken);
            return result.HasValue ? result.Value : null;
        }

        public async Task SetBinClass(BinClassResult binClassResult, CancellationToken cancellationToken)
        {
            ActorEventSource.Current.ActorMessage(this, "SetBinClass called");
            await this.StateManager.AddOrUpdateStateAsync(BINCLASSSTATENAME, binClassResult, (key, value) => binClassResult, cancellationToken);
        }


        /// <summary>
        /// This method is called whenever an actor is activated.
        /// An actor is activated the first time any of its methods are invoked.
        /// </summary>
        protected override Task OnActivateAsync()
        {
            ActorEventSource.Current.ActorMessage(this, "Actor activated.");

            // The StateManager is this actor's private state store.
            // Data stored in the StateManager will be replicated for high-availability for actors that use volatile or persisted state storage.
            // Any serializable object can be saved in the StateManager.
            // For more information, see https://aka.ms/servicefabricactorsstateserialization

            return this.StateManager.TryAddStateAsync(MODULEINFOSTATENAME, ModuleInfo.DefaultModuleInfo(GetSubId()));
        }

        private string GetSubId()
        {
            return this.GetActorId().GetStringId();
        }
    }
}
