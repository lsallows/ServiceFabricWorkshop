using AutoMapper;
using Module.Common.Types;
using System;

namespace ModulesWeb.DTO
{
    [AutoMap(typeof(ModuleInfo))]
    public class ModuleInfoDTO
    {        
        public string SubId { get; set; }
        public string ProductStatus { get; set; }
        public string LastSeenLocation { get; set; }
        public DateTime LastSeenTimestamp { get; set; }
        public DateTime ProductStatusTimestamp { get; set; }
        public string ProductStatusLocation { get; set; }
    }
}
