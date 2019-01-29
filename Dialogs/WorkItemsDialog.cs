using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VSTSBot.Attributes;
using VSTSBot.Cards;
using VSTSBot.Enums;
using VSTSBot.Extensions;
using VSTSBot.Models;
using VSTSBot.Resources.Labels;
using VSTSBot.Services;

namespace VSTSBot.Dialogs
{
    [CommandMetadata(Dialog.WorkItems)]
    [Serializable]
    public class WorkItemsDialog : DialogBase, IDialog<object>
    {
        #region Attributes

        private readonly string CommandMatchWorkItems = $"{Dialog.WorkItems.GetDescription()} *(\\S*)";

        private readonly IWorkItemService workIteService;

        public string Member { get; set; }

        #endregion

        public WorkItemsDialog(IAuthenticationService authenticationService, IWorkItemService workIteService)
            : base(authenticationService)
        {
            this.workIteService = workIteService;
        }

        public async Task StartAsync(IDialogContext context)
        {
            context.ThrowIfNull(nameof(context));

            context.Wait(this.WorkItemsAsync);

            await Task.CompletedTask;
        }

        public virtual async Task WorkItemsAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            context.ThrowIfNull(nameof(context));
            result.ThrowIfNull(nameof(result));

            var activity = await result;

            var text = activity.RemoveRecipientMention().Trim().ToLowerInvariant();

            var match = Regex.Match(text, CommandMatchWorkItems, RegexOptions.IgnoreCase);

            if (match.Success)
            {
                this.Member = match.Groups[1].Value;
            }

            await this.ContinueProcess(context, activity);
        }

        public virtual async Task ContinueProcess(IDialogContext context, IMessageActivity result)
        {
            context.ThrowIfNull(nameof(context));
            result.ThrowIfNull(nameof(result));

            if (string.IsNullOrWhiteSpace(this.Member))
            {
                await this.SelectMemberAsync(context, result);
                return;
            }

            var data = context.UserData.GetValue<UserData>("userData");

            var resultService = await this.workIteService.GetWorkItems(this.Member, data.Account.Value, data.Project.Value, data.User.Token);

            var reply = context.MakeMessage();

            if (resultService.Count == 0)
            {
                reply.Text = string.Format(Labels.NoWorkItems, this.Member);
            }
            else
            {
                reply.Text = string.Join(Environment.NewLine, resultService);
            }

            await context.PostAsync(reply);
            context.Done(reply);

            return;
        }

        public virtual async Task SelectMemberAsync(IDialogContext context, IMessageActivity result)
        {
            context.ThrowIfNull(nameof(context));
            result.ThrowIfNull(nameof(result));

            var reply = context.MakeMessage();

            reply.Text = Labels.Member;

            await context.PostAsync(reply);

            context.Wait(this.MemberReceivedAsync);
        }

        public virtual async Task MemberReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            context.ThrowIfNull(nameof(context));
            result.ThrowIfNull(nameof(result));

            var activity = await result;

            var text = activity.RemoveRecipientMention().Trim().ToLowerInvariant();

            var isCancel = await this.IsCancelMessage(text, context);

            if (isCancel)
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(text))
            {
                this.Member = text;

                await this.ContinueProcess(context, activity);

                return;
            }

            await context.PostAsync(Labels.InvalidMember);

            context.Wait(this.MemberReceivedAsync);
        }
    }
}