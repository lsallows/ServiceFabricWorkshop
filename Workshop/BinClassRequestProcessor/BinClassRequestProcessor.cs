using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using BinClass.Common.Interfaces;
using BinClassRequestProcessor.Types;
using Dapper;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Microsoft.ServiceFabric.Services.Runtime;

namespace BinClassRequestProcessor
{
    /// <summary>
    /// An instance of this class is created for each service replica by the Service Fabric runtime.
    /// </summary>
    internal sealed class BinClassRequestProcessor : StatefulService
    {
        private const string PROPERTIESDICTIONARYNAME = "Properties";
        private const string LASTINDEXPROPERTYNAME = "LastIndex";
        private const string CONNECTSTRINGNAME = "WorkshopDBConnectString";

        private const string BINLOGICSERVICENAME = "BinLogicService";
        private const int MAXEVENTSPERITERATION = 10;

        private string connectionString = "";
        private BufferBlock<BinClassRequestRecord> _requestBuffer = new BufferBlock<BinClassRequestRecord>();
        private IReliableDictionary<string, int> _propertiesDictionary;

        private ServiceProxyFactory _serviceProxyFactory = new ServiceProxyFactory();

        public BinClassRequestProcessor(StatefulServiceContext context)
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
            var sqlSelect = "SELECT TOP 50 * from dbo.BinClassRequest WHERE [Id] > @Id ORDER BY [Id]";

            try
            {
                using (var connection = new System.Data.SqlClient.SqlConnection(this.connectionString))
                {
                    var events = (await connection.QueryAsync<BinClassRequestRecord>(
                        new CommandDefinition(commandText: sqlSelect, parameters: new { Id = lastIndex }, cancellationToken: cancellationToken))).ToList();

                    if (events.Count == 0)
                        return lastIndex;

                    foreach (var evt in events)
                    {
                        this._requestBuffer.Post(evt);
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
                if (await this._requestBuffer.OutputAvailableAsync(cancellationToken))
                {
                    var processedCount = 0;
                    while (true)
                    {
                        using (var tx = this.StateManager.CreateTransaction())
                        {
                            var evt = this._requestBuffer.Receive();
                            await this.ProcessEvent(evt, cancellationToken);
                            await _propertiesDictionary.AddOrUpdateAsync(tx, LASTINDEXPROPERTYNAME, evt.Id, (key, value) => evt.Id);

                            if (processedCount++ >= MAXEVENTSPERITERATION ||
                                this._requestBuffer.Count == 0)
                            {
                                await tx.CommitAsync();
                                break;
                            }
                        }
                    }
                }
            }
        }

        private async Task ProcessEvent(BinClassRequestRecord binClassRequestRecord, CancellationToken cancellationToken)
        {
            try
            {
                var binLogicServiceUri = $"{this.Context.CodePackageActivationContext.ApplicationName}/{BINLOGICSERVICENAME}";
                var proxy = this._serviceProxyFactory.CreateServiceProxy<IBinClassLogic>(new Uri(binLogicServiceUri));
                var result = await proxy.CalculateBinClass(binClassRequestRecord.SubId, cancellationToken);

                var sqlInsert = "INSERT INTO dbo.BinClassResponse([SubId], [BinClass], [TimeStampRequested], [TimeStampResponded], [CodeVersion]) VALUES(@SubId, @BinClass, @TimeStampRequested, @TimeStampResponded, @CodeVersion)";


                using (var connection = new System.Data.SqlClient.SqlConnection(this.connectionString))
                {
                    var sqlResult = connection.Execute(new CommandDefinition(commandText: sqlInsert,
                        parameters: new
                        {
                            SubId = binClassRequestRecord.SubId,
                            BinClass = result.BinClass,
                            TimeStampRequested = binClassRequestRecord.TimeStamp,
                            TimeStampResponded = result.TimeStamp,
                            CodeVersion = result.CodeVersion
                        },
                        cancellationToken: cancellationToken));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
