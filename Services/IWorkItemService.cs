using System.Collections.Generic;
using System.Threading.Tasks;
using VSTSBot.Models;

namespace VSTSBot.Services
{
    public interface IWorkItemService
    {
        Task<IList<string>> GetWorkItems(string assignedTo, string account, string project, OAuthToken token);
    }
}