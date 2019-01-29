using Microsoft.VisualStudio.Services.Account;
using Microsoft.VisualStudio.Services.Profile;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VSTSBot.Models;

namespace VSTSBot.Services
{
    public interface IProfileService
    {
        Task<Profile> GetProfile(OAuthToken token);

        Task<IList<Account>> GetAccounts(OAuthToken token, Guid memberId);
    }
}
