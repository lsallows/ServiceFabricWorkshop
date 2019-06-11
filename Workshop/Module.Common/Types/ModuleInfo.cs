using ModuleActorService.Interfaces.Types;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Module.Common.Types
{
    [DataContract]
    public class ModuleInfo : IExtensibleDataObject
    {
        public static ModuleInfo DefaultModuleInfo(string subId) { return new ModuleInfo(subId); }

        #region [ Constructor ]

        public ModuleInfo(string subId)
        {
            this.SubId = subId;
            this.ProductStatus = ProductStatus.Unknown;
            this.ProducedEvents = new List<ProducedEvent>();
            this.ScrapEvents = new List<ScrapEvent>();
        }

        #endregion

        #region [ Public Properties ]

        [DataMember]
        public string SubId { get; set; }

        [DataMember]
        public List<ProducedEvent> ProducedEvents { get; set; }

        [DataMember]
        public List<ScrapEvent> ScrapEvents { get; set; }

        [DataMember]
        public ProductStatus ProductStatus { get; set; }

        [DataMember]
        public DateTime ProductStatusTimestamp { get; set; }

        [DataMember]
        public string ProductStatusLocation { get; set; }

        #endregion

        #region [ IExtensibleDataObject ]

        public ExtensionDataObject ExtensionData { get; set; }

        #endregion
    }
}
