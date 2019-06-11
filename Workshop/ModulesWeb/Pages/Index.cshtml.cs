using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ModulesWeb.DTO;

namespace ModulesWeb.Pages
{
    public class IndexModel : PageModel
    {
        public string Id { get; set; }
        public IList<string> Modules { get; private set; }
        public ModuleInfoDTO ModuleInfo { get; private set; }
    }
}
