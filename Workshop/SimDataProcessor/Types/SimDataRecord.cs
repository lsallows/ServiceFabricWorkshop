using System;

namespace SimDataProcessor.Types
{
    class SimDataRecord
    {
        public int Id { get; set; }
        public string SubId { get; set; }
        public string SimData { get; set; }
        public DateTime TimeStamp { get; set; }
    }
}
