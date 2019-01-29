using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace VSTSBot.Models
{
    [DataContract]
    [Serializable]
    public class User
    {
        [DataMember]
        public OAuthToken Token { get; set; }

        [DataMember]
        public Guid Id { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public IDictionary<string, string> Accounts { get; set; }

        [DataMember]
        public IDictionary<string, string> Projects { get; set; }
    }
}