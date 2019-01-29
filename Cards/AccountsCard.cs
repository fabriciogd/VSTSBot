using Microsoft.Bot.Connector;
using System.Collections.Generic;
using System.Linq;
using VSTSBot.Extensions;

namespace VSTSBot.Cards
{
    public class AccountsCard: HeroCard
    {
        public AccountsCard(IDictionary<string, string> accounts)
        {
            accounts.ThrowIfNull(nameof(accounts));

            this.Buttons = accounts
                .Select(a => new CardAction(ActionTypes.ImBack, a.Value, value: a.Value))
                .ToList();
        }
    }
}