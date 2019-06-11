using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace BinClass.Common.Types
{
    [DataContract]
    public class BinClassResult : IExtensibleDataObject
    {
        [DataMember]
        public string BinClass { get; set; }
        [DataMember]
        public DateTime TimeStamp { get; set; }
        [DataMember]
        public string CodeVersion { get; set; }

        #region [ IExtensibleDataObject ]

        public ExtensionDataObject ExtensionData { get; set; }

        #endregion
    }
}
