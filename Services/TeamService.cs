using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.VisualStudio.Services.WebApi;
using VSTSBot.Extensions;
using VSTSBot.Models;

namespace VSTSBot.Services
{
    [Serializable]
    public class TeamService : BaseService, ITeamService
    {
        public async Task<IList<WebApiTeam>> GetTeams(string account, string projectId, OAuthToken token)
        {
            token.ThrowIfNull(nameof(token));

            using (var client = await this.ConnectAsync<TeamHttpClient>(token, account))
            {
                return await client.GetTeamsAsync(projectId);
            }
        }

        public async Task<IList<IdentityRef>> GetTeamMembers(string account, string projectId, string teamId, OAuthToken token)
        {
            token.ThrowIfNull(nameof(token));

            using (var client = await this.ConnectAsync<TeamHttpClient>(token, account))
            {
                return await client.GetTeamMembersAsync(projectId, teamId);
            }
        }
    }
}