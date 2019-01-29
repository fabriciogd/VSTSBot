using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VSTSBot.Attributes;
using VSTSBot.Cards;
using VSTSBot.Enums;
using VSTSBot.Extensions;
using VSTSBot.Resources.Labels;
using VSTSBot.Utils;

namespace VSTSBot.Dialogs
{
    [CommandMetadata(Dialog.Language)]
    [Serializable]
    public class LanguageChoiceDialog : DialogBase, IDialog<object>
    {
        #region Atributos

        public const string Choise = "LCID";

        #endregion

        public Task StartAsync(IDialogContext context)
        {
            context.ThrowIfNull(nameof(context));

            context.Wait(SelectLanguageAsync);

            return Task.CompletedTask;
        }

        public virtual async Task SelectLanguageAsync(IDialogContext context, IAwaitable<object> result)
        {
            context.ThrowIfNull(nameof(context));
            result.ThrowIfNull(nameof(result));

            var instance = new Language();

            var languages = instance.GetDescriptions().ToDictionary(a => a.Value, a => a.Key.GetStringValue());

            var languagesCard = new LanguagesCard(languages);

            var reply = context.MakeMessage();

            reply.Text = Labels.ConnectToAccount;
            reply.Attachments.Add(languagesCard.ToAttachment());

            await context.PostAsync(reply);

            context.Wait(this.LanguageReceivedAsync);
        }

        public virtual async Task LanguageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var activity = await result;

            var text = activity.RemoveRecipientMention().Trim().ToLowerInvariant();

            var isCancel = await this.IsCancelMessage(text, context);

            if (isCancel)
            {
                return;
            }

            var instance = new Language();

            var descriptions = instance.GetDescriptions().ToDictionary(a => a.Value, a => a.Key.GetStringValue());

            var language = descriptions.FirstOrDefault(a => string.Equals(a.Value, text, StringComparison.OrdinalIgnoreCase));

            if (!language.Equals(default(KeyValuePair<string, string>)))
            {
                context.PrivateConversationData.SetValue(Choise, language.Value);

                Localize.SetAmbientThreadCulture(language.Value);

                var reply = context.MakeMessage();
                reply.Text = Labels.LanguageSelected;

                await context.PostAsync(reply);

                context.Done(reply);

                return;
            }
            else
            {
                context.Wait(this.LanguageReceivedAsync);
            }
        }
    }
}