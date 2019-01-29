using Microsoft.VisualStudio.Services.Account;
using Microsoft.VisualStudio.Services.Account.Client;
using Microsoft.VisualStudio.Services.Profile;
using Microsoft.VisualStudio.Services.Profile.Client;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VSTSBot.Extensions;
using VSTSBot.Models;

namespace VSTSBot.Services
{
    [Serializable]
    public class ProfileService : BaseService, IProfileService
    {
        public async Task<Profile> GetProfile(OAuthToken token)
        {
            token.ThrowIfNull(nameof(token));

            using (var client = await this.ConnectAsync<ProfileHttpClient>(token))
            {
                return await client.GetProfileAsync(new ProfileQueryContext(AttributesScope.Core));
            }
        }

        public async Task<IList<Account>> GetAccounts(OAuthToken token, Guid memberId)
        {
            token.ThrowIfNull(nameof(token));

            using (var client = await this.ConnectAsync<AccountHttpClient>(token))
            {
                return await client.GetAccountsByMemberAsync(memberId);
            }
        }
    }
}