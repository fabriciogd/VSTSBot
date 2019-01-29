using Microsoft.TeamFoundation.Core.WebApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VSTSBot.Extensions;
using VSTSBot.Models;

namespace VSTSBot.Services
{
    [Serializable]
    public class ProjectService : BaseService, IProjectService
    {
        public async Task<IList<TeamProjectReference>> GetProjects(string account, OAuthToken token)
        {
            account.ThrowIfNullOrWhiteSpace(nameof(account));
            token.ThrowIfNull(nameof(token));

            using (var client = await this.ConnectAsync<ProjectHttpClient>(token, account))
            {
                var results = await client.GetProjects();
                return results.ToList();
            }
        }
    }
}