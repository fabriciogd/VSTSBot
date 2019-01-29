using System;
using System.Runtime.Serialization;

namespace VSTSBot.Models
{
    [DataContract]
    [Serializable]
    public class OAuthToken
    {
        [DataMember(Name = "access_token")]
        public string AccessToken { get; set; }

        [DataMember]
        public string AppSecret { get; set; }

        [DataMember]
        public Uri AuthorizeUrl { get; set; }

        [DataMember(Name = "created_on")]
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        [DataMember(Name = "expires_in")]
        public int ExpiresIn { get; set; }

        public DateTime ExpiresOn => this.CreatedOn.AddSeconds(this.ExpiresIn);

        [DataMember(Name = "refresh_token")]
        public string RefreshToken { get; set; }

        [DataMember(Name = "token_type")]
        public string TokenType { get; set; }
    }
}