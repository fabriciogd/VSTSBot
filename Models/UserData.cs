using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace VSTSBot.Models
{
    [DataContract]
    public class UserData
    {
        [DataMember]
        public KeyValuePair<string, string> Account { get; set; }

        [DataMember]
        public KeyValuePair<string, string> Project { get; set; }

        [DataMember]
        public string Pin { get; set; }

        [DataMember]
        public User User { get; set; }
    }
}