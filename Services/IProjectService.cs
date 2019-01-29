using Microsoft.TeamFoundation.Core.WebApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using VSTSBot.Models;

namespace VSTSBot.Services
{
    public interface IProjectService
    {
        Task<IList<TeamProjectReference>> GetProjects(string account, OAuthToken token);
    }
}