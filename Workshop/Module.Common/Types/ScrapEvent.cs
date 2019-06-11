using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Module.Common.Types
{
    [DataContract]
    public class ScrapEvent : IExtensibleDataObject
    {
        [DataMember]
        public DateTime TimeStamp { get; set; }
        [DataMember]
        public string Location { get; set; }
        [DataMember]
        public int ScrapCode { get; set; }

        #region [ IExtensibleDataObject ]

        public ExtensionDataObject ExtensionData { get; set; }

        #endregion
    }
}
