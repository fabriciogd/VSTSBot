using Microsoft.Bot.Connector;
using System.Collections.Generic;
using System.Linq;
using VSTSBot.Extensions;

namespace VSTSBot.Cards
{
    public class ProjectsCard: HeroCard
    {
        public ProjectsCard(IDictionary<string, string> projects)
        {
            projects.ThrowIfNull(nameof(projects));

            this.Buttons = projects
               .Select(p => new CardAction(ActionTypes.ImBack, p.Value, value: p.Value))
               .ToList();
        }
    }
}