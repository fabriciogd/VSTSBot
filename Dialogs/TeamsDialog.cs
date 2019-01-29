using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System;
using System.Linq;
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
    [CommandMetadata(Dialog.Teams)]
    [Serializable]
    public class TeamsDialog : DialogBase, IDialog<object>
    {
        #region Attributes

        private readonly ITeamService teamService;

        #endregion

        public TeamsDialog(IAuthenticationService authenticationService, ITeamService teamService)
            : base(authenticationService)
        {
            this.teamService = teamService;
        }

        public async Task StartAsync(IDialogContext context)
        {
            context.ThrowIfNull(nameof(context));

            context.Wait(this.TeamAsync);

            await Task.CompletedTask;
        }

        public async Task TeamAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            context.ThrowIfNull(nameof(context));
            result.ThrowIfNull(nameof(result));

            var data = context.UserData.GetValue<UserData>("userData");

            var resultService = await this.teamService.GetTeams(data.Account.Value, data.Project.Key, data.User.Token);

            var teams = resultService.ToDictionary(a => a.Id.ToString(), a => a.Name);

            var reply = context.MakeMessage();

            var accountsCard = new TeamsCard(teams);

            reply.Text = Labels.Teams;
            reply.Attachments.Add(accountsCard.ToAttachment());

            await context.PostAsync(reply);
            context.Done(reply);
        }
    }
}