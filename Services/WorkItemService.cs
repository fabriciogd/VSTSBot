using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using VSTSBot.Extensions;
using VSTSBot.Models;

namespace VSTSBot.Services
{
    [Serializable]
    public class WorkItemService : BaseService, IWorkItemService
    {
        public async Task<IList<string>> GetWorkItems(string assignedTo, string account, string project, OAuthToken token)
        {
            account.ThrowIfNullOrWhiteSpace(nameof(account));
            token.ThrowIfNull(nameof(token));

            using (var client = await this.ConnectAsync<WorkItemTrackingHttpClient>(token, account))
            {
                var query = string.Format(" SELECT [System.Id], [System.WorkItemType], [System.Title]" +
                                          " FROM WorkItems" +
                                          " WHERE [System.TeamProject] = '{0}' AND [System.WorkItemType] IN ('Bug','Task') AND [System.State] = 'Active' AND [System.AssignedTo] CONTAINS '{1}'", project, assignedTo);

                var wiqlQuery = new Wiql() { Query = query };
                var results = await client.QueryByWiqlAsync(wiqlQuery);

                var ids = results.WorkItems.Select(workItemReference => workItemReference.Id);

                if (!ids.Any()) 
                {
                    return Enumerable.Empty<string>().ToList();
                }

                var workItemsForQueryResult = client
                        .GetWorkItemsAsync(
                            ids,
                            expand: WorkItemExpand.All).Result;

                return workItemsForQueryResult.Select(a => a.Fields["System.Title"].ToString()).ToList();
            }
        }
    }
}