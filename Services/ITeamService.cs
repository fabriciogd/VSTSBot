using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.VisualStudio.Services.WebApi;
using System.Collections.Generic;
using System.Threading.Tasks;
using VSTSBot.Models;

namespace VSTSBot.Services
{
    public interface ITeamService
    {
        Task<IList<WebApiTeam>> GetTeams(string account, string projectId, OAuthToken token);

        Task<IList<IdentityRef>> GetTeamMembers(string account, string projectId, string teamId, OAuthToken token);
    }
}
