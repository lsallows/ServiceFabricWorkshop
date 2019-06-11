using System;
using System.Collections.Generic;
using System.Text;

namespace BinClassRequestProcessor.Types
{
    class BinClassResponseRecord    
    {
        public int Id { get; set; }
        public string SubId { get; set; }
        public string BinClass { get; set; }
        public DateTime TimeStampRequested { get; set; }
        public DateTime TimeStampResponded { get; set; }
        public string CodeVersion { get; set; }
    }
}
