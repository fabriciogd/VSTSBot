using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
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
    [CommandMetadata(Dialog.TeamMembers)]
    [Serializable]
    public class TeamMembersDialog : DialogBase, IDialog<object>
    {
        #region Attributes

        private readonly string CommandMatchMembers = $"{Dialog.TeamMembers.GetDescription()} *(\\S*)";

        private readonly ITeamService teamService;

        private Dictionary<string, string> teams;

        public string Team { get; set; }

        #endregion

        public TeamMembersDialog(IAuthenticationService authenticationService, ITeamService teamService)
            : base(authenticationService)
        {
            this.teamService = teamService;
        }

        public async Task StartAsync(IDialogContext context)
        {
            context.ThrowIfNull(nameof(context));

            context.Wait(this.TeamsAsync);

            await Task.CompletedTask;
        }

        public virtual async Task TeamsAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            context.ThrowIfNull(nameof(context));
            result.ThrowIfNull(nameof(result));

            var activity = await result;

            var text = activity.RemoveRecipientMention().Trim().ToLowerInvariant();

            var match = Regex.Match(text, CommandMatchMembers, RegexOptions.IgnoreCase);

            if (match.Success)
            {
                this.Team = match.Groups[1].Value;
            }

            await this.ContinueProcess(context, activity);
        }

        public virtual async Task ContinueProcess(IDialogContext context, IMessageActivity result)
        {
            context.ThrowIfNull(nameof(context));
            result.ThrowIfNull(nameof(result));

            var data = context.UserData.GetValue<UserData>("userData");

            var resultService = await this.teamService.GetTeams(data.Account.Value, data.Project.Key, data.User.Token);

            teams = resultService.ToDictionary(a => a.Id.ToString(), a => a.Name);

            if (string.IsNullOrWhiteSpace(this.Team))
            {
                await this.SelectTeamAsync(context, result);
                return;
            }

            var team = teams.FirstOrDefault(a => string.Equals(a.Value, this.Team, StringComparison.OrdinalIgnoreCase));

            if (!team.Equals(default(KeyValuePair<string, string>)))
            {
                await this.ShowMembers(context, result);
                return;
            }

            await context.PostAsync(Labels.InvalidTeam);
            context.Done(result);

            return;
        }

        public virtual async Task ShowMembers(IDialogContext context, IMessageActivity result)
        {
            var data = context.UserData.GetValue<UserData>("userData");

            var resultService = await this.teamService.GetTeamMembers(data.Account.Value, data.Project.Key, this.Team, data.User.Token);

            var members = resultService.Select(a => a.DisplayName);

            var reply = context.MakeMessage();

            if (!members.Any())
            {
                reply.Text = Labels.NoTeamMembers;

                await context.PostAsync(reply);

                context.Done(reply);

                return;
            }

            reply.Text = string.Join(", ", members.ToArray());

            await context.PostAsync(reply);

            context.Done(reply);

            return;
        }

        public virtual async Task SelectTeamAsync(IDialogContext context, IMessageActivity result)
        {
            context.ThrowIfNull(nameof(context));
            result.ThrowIfNull(nameof(result));

            var reply = context.MakeMessage();

            if (!teams.Any())
            {
                reply.Text = Labels.NoTeams;
                await context.PostAsync(reply);
                context.Done(reply);
                return;
            }

            var accountsCard = new TeamsCard(teams);

            reply.Text = Labels.Teams;
            reply.Attachments.Add(accountsCard.ToAttachment());

            await context.PostAsync(reply);

            context.Wait(this.TeamReceivedAsync);
        }

        public virtual async Task TeamReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
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

            var team = teams.FirstOrDefault(a => string.Equals(a.Value, text, StringComparison.OrdinalIgnoreCase));

            if (!team.Equals(default(KeyValuePair<string, string>)))
            {
                this.Team = team.Value;

                await this.ContinueProcess(context, activity);

                return;
            }

            await context.PostAsync(Labels.InvalidTeam);

            context.Wait(this.TeamReceivedAsync);
        }
    }
}