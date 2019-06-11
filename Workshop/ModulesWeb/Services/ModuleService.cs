using AutoMapper;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Module.Common;
using ModuleActorService.Interfaces;
using ModulesWeb.DTO;
using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ModulesWeb.Services
{
    public interface IModuleService
    {
        Task<List<string>> GetAll(CancellationToken cancellationToken);
        Task<ModuleInfoDTO> GetModule(string subId, CancellationToken cancellationToken);
    }

    public class ModuleService : IModuleService
    {
        private const string MODULESERVICE = "ModulesActorService";
        private const string MODULEMANAGERSERVICE = "ModuleManager";

        private string _applicationName = null;
        private static ServiceProxyFactory _proxyFactory = new ServiceProxyFactory();
        private readonly IMapper _mapper;        

        public ModuleService(StatelessServiceContext statelessServiceContext, IMapper mapper)
        {
            this._applicationName = statelessServiceContext.CodePackageActivationContext.ApplicationName;
            this._mapper = mapper;
        }

        public async Task<List<string>> GetAll(CancellationToken cancellationToken)
        {            
            var address = new Uri($"{this._applicationName}/{MODULEMANAGERSERVICE}");
            var service = _proxyFactory.CreateServiceProxy<IModuleManager>(address);
            return await service.GetModuleIds(cancellationToken);
        }

        public async Task<ModuleInfoDTO> GetModule(string subId, CancellationToken cancellationToken)
        {
            var actorId = new ActorId(subId);
            
            var actor = ActorProxy.Create<IModules>(actorId, new Uri($"{this._applicationName}/{MODULESERVICE}"));
            
            var modInfo = await actor.GetModuleInfo(CancellationToken.None);

            return this._mapper.Map<ModuleInfoDTO>(modInfo);            
        }
    }
}
