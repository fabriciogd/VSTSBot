using System;
using System.Threading.Tasks;
using VSTSBot.Models;

namespace VSTSBot.Services
{
    public interface IAuthenticationService
    {
        Task<OAuthToken> GetToken(string appSecret, Uri authorizeUrl, string code);

        Task<OAuthToken> GetToken(OAuthToken token);
    }
}
