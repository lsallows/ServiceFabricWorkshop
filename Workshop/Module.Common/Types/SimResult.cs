using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Module.Common.Types
{
    [DataContract]
    public class SimResult : IExtensibleDataObject
    {
        [DataMember]
        public float Efficiency { get; set; }
        [DataMember]
        public DateTime TimeStamp { get; set; }

        #region [ IExtensibleDataObject ]

        public ExtensionDataObject ExtensionData { get; set; }

        #endregion
    }
}
