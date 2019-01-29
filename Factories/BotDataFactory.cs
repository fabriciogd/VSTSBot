using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Connector;
using VSTSBot.Extensions;

namespace VSTSBot.Factories
{
    public class BotDataFactory : IBotDataFactory
    {
        private readonly IBotDataStore<BotData> store;

        public BotDataFactory(IBotDataStore<BotData> store)
        {
            store.ThrowIfNull(nameof(store));

            this.store = store;
        }

        public IBotData Create(Address address)
        {
            address.ThrowIfNull(nameof(address));

            return new JObjectBotData(address, this.store);
        }
    }
}