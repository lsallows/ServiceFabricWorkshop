using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Dapper;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Module.Common.Types;
using ModuleActorService.Interfaces;
using Newtonsoft.Json;
using SimDataProcessor.Types;

namespace SimDataProcessor
{
    /// <summary>
    /// An instance of this class is created for each service replica by the Service Fabric runtime.
    /// </summary>
    internal sealed class SimDataProcessor : StatefulService
    {
        private const string PROPERTIESDICTIONARYNAME = "Properties";
        private const string LASTINDEXPROPERTYNAME = "LastIndex";
        private const string CONNECTSTRINGNAME = "WorkshopDBConnectString";
        private const string ModuleServiceUri = "fabric:/ModuleApp/ModulesActorService";
        private const int MAXEVENTSPERITERATION = 10;

        private string connectionString = "";
        private BufferBlock<SimDataRecord> _simDataBuffer = new BufferBlock<SimDataRecord>();
        private IReliableDictionary<string, int> _propertiesDictionary;

        public SimDataProcessor(StatefulServiceContext context)
            : base(context)
        { }

        /// <summary>
        /// Optional override to create listeners (e.g., HTTP, Service Remoting, WCF, etc.) for this service replica to handle client or user requests.
        /// </summary>
        /// <remarks>
        /// For more information on service communication, see https://aka.ms/servicefabricservicecommunication
        /// </remarks>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return new ServiceReplicaListener[0];
        }

        /// <summary>
        /// This is the main entry point for your service replica.
        /// This method executes when this replica of your service becomes primary and has write status.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service replica.</param>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            this.connectionString = Environment.GetEnvironmentVariable(CONNECTSTRINGNAME);

            _propertiesDictionary = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, int>>(PROPERTIESDICTIONARYNAME);

            var lastIndexRead = -1;

            using (var tx = this.StateManager.CreateTransaction())
            {
                var result = await _propertiesDictionary.TryGetValueAsync(tx, LASTINDEXPROPERTYNAME);

                lastIndexRead = result.HasValue ? result.Value : -1;
            }
            //Start consumer task
            var consumeTask = Task.Run(() => this.ConsumeBufferBlock(cancellationToken));

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                lastIndexRead = await GetSQLRecords(lastIndexRead, cancellationToken);

                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
        }

        private async Task<int> GetSQLRecords(int lastIndex, CancellationToken cancellationToken)
        {
            var sqlSelect = "SELECT TOP 50 * from dbo.SimData WHERE [Id] > @Id ORDER BY [Id]";

            try
            {
                using (var connection = new System.Data.SqlClient.SqlConnection(this.connectionString))
                {
                    var events = (await connection.QueryAsync<SimDataRecord>(
                        new CommandDefinition(commandText: sqlSelect, parameters: new { Id = lastIndex }, cancellationToken: cancellationToken))).ToList();

                    if (events.Count == 0)
                        return lastIndex;

                    foreach (var evt in events)
                    {
                        this._simDataBuffer.Post(evt);
                    }

                    return events.Last().Id;
                }
            }
            catch (Exception ex)
            {
                ServiceEventSource.Current.ServiceMessage(this.Context, ex.ToString());
            }
            return lastIndex;
        }

        private async Task ConsumeBufferBlock(CancellationToken cancellationToken)
        {
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (await this._simDataBuffer.OutputAvailableAsync(cancellationToken))
                {
                    var processedCount = 0;
                    while (true)
                    {
                        using (var tx = this.StateManager.CreateTransaction())
                        {
                            var evt = this._simDataBuffer.Receive();
                            await this.ProcessEvent(evt, cancellationToken);
                            await _propertiesDictionary.AddOrUpdateAsync(tx, LASTINDEXPROPERTYNAME, evt.Id, (key, value) => evt.Id);

                            if (processedCount++ >= MAXEVENTSPERITERATION ||
                                this._simDataBuffer.Count == 0)
                            {
                                await tx.CommitAsync();
                                break;
                            }
                        }
                    }
                }
            }
        }

        private async Task ProcessEvent(SimDataRecord simDataRecord, CancellationToken cancellationToken)
        {
            try
            {
                var actorId = new ActorId(simDataRecord.SubId);
                var actor = ActorProxy.Create<IModules>(actorId, new Uri(ModuleServiceUri));
                var simData = JsonConvert.DeserializeObject<SimData>(simDataRecord.SimData);
                await actor.SetSimData(new SimResult() { Efficiency = simData.Efficiency, TimeStamp = simDataRecord.TimeStamp }, cancellationToken);
            }
            catch (Exception ex)
            {
                ServiceEventSource.Current.ServiceMessage(this.Context, ex.ToString());
            }
        }
    }
}
