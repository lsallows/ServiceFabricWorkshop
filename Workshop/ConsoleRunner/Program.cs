using EasyConsole;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Module.Common;
using ModuleActorService.Interfaces;
using System;
using System.Threading;

namespace ConsoleRunner
{
    class Program
    {
        private const string ModuleServiceUri = "fabric:/ModuleApp/ModulesActorService";
        private const string ModuleManagerUri = "fabric:/ModuleApp/ModuleManager";

        static void Main(string[] args)
        {
            var exit = false;
            while (true)
            {
                var menu = new EasyConsole.Menu()
                    .Add("Get module info", () => GetActorInfo())
                    .Add("Add produced event", () => AddProducedEvent())
                    .Add("Get sim data", () => GetSimData())
                    .Add("Get all modules", () => GetAllModules())
                    .Add("Quit", () => exit = true);
                menu.Display();

                if (exit)
                    break;
            }
        }

        static void GetActorInfo()
        {
            var actor = GetActor();
            var cts = new CancellationTokenSource(15000);
            var info = actor.GetModuleInfo(cts.Token).Result;

            Console.WriteLine($"ProductStatus: {info.ProductStatus}");
            Console.WriteLine($"Produced Events: {info.ProducedEvents.Count}");
            info.ProducedEvents.ForEach(p =>
            {
                Console.WriteLine($"Produced event location: {p.Location} timestamp: {p.TimeStamp}");
            });
        }

        static void AddProducedEvent()
        {
            var actor = GetActor();
            var cts = new CancellationTokenSource(15000);
            actor.AddProducedEvent(new Module.Common.Types.ProducedEvent() { Location = "Console", TimeStamp = DateTime.Now }, cts.Token);
        }

        static void GetSimData()
        {
            var actor = GetActor();
            var cts = new CancellationTokenSource(15000);
            var simData = actor.GetSimData(cts.Token).Result;
            if (simData == null)
            {
                Console.WriteLine("SimData is null");
            }
            else
            {
                Console.WriteLine($"SimData efficiency: {simData.Efficiency}");
                Console.WriteLine($"SimData timestamp: {simData.TimeStamp}");
            }
        }

        static void GetAllModules()
        {
            var address = new Uri(ModuleManagerUri);
            var proxy = new ServiceProxyFactory();
            var service = proxy.CreateServiceProxy<IModuleManager>(address);
            var cts = new CancellationTokenSource(15000);
            var result = service.GetModuleIds(cts.Token).Result;
            Console.WriteLine($"Returned {result.Count} modules");
        }

        static IModules GetActor()
        {
            var name = Input.ReadString("Enter module ID");
            var actorId = new ActorId(name);
            return ActorProxy.Create<IModules>(actorId, new Uri(ModuleServiceUri));
        }
    }
}
