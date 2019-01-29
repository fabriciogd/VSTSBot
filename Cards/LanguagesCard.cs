using Microsoft.Bot.Connector;
using System.Collections.Generic;
using System.Linq;
using VSTSBot.Extensions;

namespace VSTSBot.Cards
{
    public class LanguagesCard: HeroCard
    {
        public LanguagesCard(IDictionary<string, string> languages)
        {
            languages.ThrowIfNull(nameof(languages));

            this.Buttons = languages
                .Select(a => new CardAction(ActionTypes.ImBack, a.Key, value: a.Value))
                .ToList();
        }
    }
}