using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System;
using System.Threading.Tasks;
using VSTSBot.Attributes;
using VSTSBot.Enums;
using VSTSBot.Exceptions;
using VSTSBot.Extensions;
using VSTSBot.Resources.Labels;
using VSTSBot.Services;

namespace VSTSBot.Dialogs
{
    [CommandMetadata(Dialog.Disconnect)]
    [Serializable]
    public class DisconnectDialog : DialogBase, IDialog<object>
    {
        public DisconnectDialog(IAuthenticationService authenticationService)
            : base(authenticationService) { }

        public async Task StartAsync(IDialogContext context)
        {
            context.ThrowIfNull(nameof(context));

            context.Wait(this.DisconnectAsync);

            await Task.CompletedTask;
        }

        public async Task DisconnectAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            context.ThrowIfNull(nameof(context));
            result.ThrowIfNull(nameof(result));

            var activity = await result;

            var text = activity.Text;

            var isRemovedValue = context.UserData.RemoveValue("userData");

            if (isRemovedValue)
            {
                var reply = context.MakeMessage();
                reply.Text = Labels.Disconnected;

                await context.PostAsync(reply);

                context.Done(reply);
            }
            else
            {
                context.Fail(new UnknownCommandException(text));
            }
        }
    }
}