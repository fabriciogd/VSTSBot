using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Builder.Internals.Fibers;
using Microsoft.Bot.Connector;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VSTSBot.Exceptions;
using VSTSBot.Extensions;
using VSTSBot.Models;
using VSTSBot.Resources.Labels;
using VSTSBot.Utils;

namespace VSTSBot.Dialogs
{
    [Serializable]
    public class RootDialog : IDialog<object>
    {
        public Task StartAsync(IDialogContext context)
        {
            context.ThrowIfNull(nameof(context));

            context.Wait(HandleActivityAsync);

            return Task.CompletedTask;
        }

        private async Task HandleActivityAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            context.ThrowIfNull(nameof(context));
            result.ThrowIfNull(nameof(result));

            var activity = await result;

            if (activity.Type == ActivityTypes.ConversationUpdate)
            {
                await this.WelcomeAsync(context, activity);
            }
            else
            {
                await this.HandleCommandAsync(context, activity);
            }
        }

        public virtual async Task WelcomeAsync(IDialogContext context, IMessageActivity result)
        {
            context.ThrowIfNull(nameof(context));
            result.ThrowIfNull(nameof(result));

            var message = result as IConversationUpdateActivity;

            if (message == null)
            {
                return;
            }

            if (!message.MembersAdded.Any() || message.MembersAdded.All(m => m.Id.Equals(message.Recipient.Id, StringComparison.OrdinalIgnoreCase)))
            {
                await context.PostAsync(Labels.Welcome);
            }
        }

        public virtual async Task HandleCommandAsync(IDialogContext context, IMessageActivity result)
        {
            context.ThrowIfNull(nameof(context));
            result.ThrowIfNull(nameof(result));

            var text = result.RemoveRecipientMention();

            await context.SendTyping();

            using (var scope = DialogModule.BeginLifetimeScope(Conversation.Container, result))
            {
                this.SetLocale(context, result);

                var dialog = scope.Find(text);

                var reply = context.MakeMessage();

                if (dialog == null)
                {
                    reply.Text = Labels.UnknownCommand;

                    await context.PostAsync(reply);

                    context.Wait(this.HandleActivityAsync);
                }
                else if (!(dialog is ConnectDialog) && !IsConnected(context.UserData))
                {
                    reply.Text = Labels.Connect;

                    await context.PostAsync(reply);

                    context.Wait(this.HandleActivityAsync);
                }
                else
                {
                    await context.Forward(dialog, this.ResumeAfterChildDialog, result, CancellationToken.None);
                }
            }
        }

        public virtual async Task ResumeAfterChildDialog(IDialogContext context, IAwaitable<object> result)
        {
            context.ThrowIfNull(nameof(context));
            result.ThrowIfNull(nameof(result));

            try
            {
                await result;
            }
            catch (UnknownCommandException)
            {
                await this.HandleCommandAsync(context, context.Activity as IMessageActivity);
            }
            catch (Exception ex)
            { 
                await context.PostAsync(string.Format(Labels.ErrorOccurred, ex.Message));
            }
        }

        private void SetLocale(IDialogContext context, IMessageActivity result)
        {
            if (context.PrivateConversationData.ContainsKey(LanguageChoiceDialog.Choise))
            {
                var lcid = context.PrivateConversationData.GetValueOrDefault<string>(LanguageChoiceDialog.Choise);

                result.Locale = lcid;

                Localize.SetAmbientThreadCulture(lcid);
            }
        }

        private bool IsConnected(IBotDataBag dataBag)
        {
            if (!dataBag.TryGetValue("userData", out UserData data))
            {
                return false;
            }

            var profile = data.User;

            if (profile != null && profile.Token != null)
            {
                return true;
            }

            return false;
        }
    }
}