using Microsoft.Bot.Connector;
using System.Collections.Generic;
using System.Linq;
using VSTSBot.Extensions;

namespace VSTSBot.Cards
{
    public class TeamsCard : HeroCard
    {
        public TeamsCard(IDictionary<string, string> teams)
        {
            teams.ThrowIfNull(nameof(teams));

            this.Buttons = teams
                .Select(a => new CardAction(ActionTypes.ImBack, a.Value, value: a.Value))
                .ToList();
        }
    }
}