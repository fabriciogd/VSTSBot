using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Internals;

namespace VSTSBot.Factories
{
    public interface IBotDataFactory
    {
        IBotData Create(Address address);
    }
}
