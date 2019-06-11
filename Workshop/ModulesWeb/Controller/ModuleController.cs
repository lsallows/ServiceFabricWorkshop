using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ModulesWeb.DTO;
using ModulesWeb.Services;

namespace ModuleManagerWeb.Controllers
{
    [Route("api/module")]
    [ApiController]
    public class ModuleController : ControllerBase
    {
        private readonly IModuleService _moduleService;
        public ModuleController(IModuleService moduleService)
        {
            this._moduleService = moduleService;
        }

        // GET: api/Module
        [HttpGet]
        public async Task<IEnumerable<string>> Get(CancellationToken cancellationToken)
        {
            return await this._moduleService.GetAll(cancellationToken);
        }

        // GET: api/Module/5
        [HttpGet("{id}", Name = "Get")]
        public async Task<ModuleInfoDTO> Get(string id, CancellationToken cancellationToken)
        {
            return await this._moduleService.GetModule(id, cancellationToken);
        }
    }
}
