using System;
using System.Runtime.Serialization;

namespace WebResourceManager.Models
{
    [DataContract]
    public class ImposterProfile
    {
        [DataMember(Name = "profileId")]
        public Guid ProfileId { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }
    }
}
