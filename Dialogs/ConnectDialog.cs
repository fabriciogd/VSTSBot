using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
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
    [CommandMetadata(Dialog.Connect)]
    [Serializable]
    public class ConnectDialog : DialogBase, IDialog<object>
    {
        #region Attributes

        private const string CommandMatchPin = @"(\d{4})";

        private readonly string appId;
        private readonly string appScope;
        private readonly string authorizeUrl;

        private readonly IProfileService profileService;
        private readonly IProjectService projectService;

        #endregion

        public ConnectDialog(
            string appId, 
            string appScope, 
            Uri authorizeUrl, 
            IAuthenticationService authenticationService,
            IProfileService profileService,
            IProjectService projectService): base(authenticationService)
        {
            appId.ThrowIfNullOrWhiteSpace(nameof(appId));
            appScope.ThrowIfNullOrWhiteSpace(nameof(appScope));
            authorizeUrl.ThrowIfNull(nameof(authorizeUrl));

            this.appId = appId;
            this.appScope = appScope;
            this.authorizeUrl = authorizeUrl.ToString();
            this.profileService = profileService;
            this.projectService = projectService;
        }

        public Task StartAsync(IDialogContext context)
        {
            context.ThrowIfNull(nameof(context));

            context.Wait(ConnectAsync);

            return Task.CompletedTask;
        }

        private async Task ConnectAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var activity = await result;

            await this.ContinueProcess(context, activity);
        }

        public virtual async Task ContinueProcess(IDialogContext context, IMessageActivity result)
        {
            context.ThrowIfNull(nameof(context));
            result.ThrowIfNull(nameof(result));

            if (!context.UserData.TryGetValue("userData", out UserData data))
            {
                data = new UserData();
            }

            var user = await this.GetValidatedProfile(context.UserData);

            if (user == null)
            {
                await this.LogOnAsync(context, result);
                return;
            }

            var account = data.Account;

            if (account.Equals(default(KeyValuePair<string, string>)))
            {
                await this.SelectAccountAsync(context, result);
                return;
            }

            var project = data.Project;

            if (project.Equals(default(KeyValuePair<string, string>)))
            {
                await this.SelectProjectAsync(context, result);
                return;
            }

            var reply = context.MakeMessage();
            reply.Text = string.Format(Labels.ConnectedTo, data.User.Name, data.Account.Value, data.Project.Value);

            await context.PostAsync(reply);

            context.Done(reply);
        }

        public virtual async Task LogOnAsync(IDialogContext context, IMessageActivity result)
        {
            context.ThrowIfNull(nameof(context));
            result.ThrowIfNull(nameof(result));

            if (!context.UserData.TryGetValue("userData", out UserData data))
            {
                data = new UserData();
            }

            data.Pin = this.GeneratePin();

            context.UserData.SetValue("userData", data);

            var card = new LogOnCard(this.appId, this.appScope, new Uri(this.authorizeUrl), result);

            var reply = context.MakeMessage();
            reply.Attachments.Add(card.ToAttachment());

            await context.PostAsync(reply);

            context.Wait(this.PinReceivedAsync);
        }

        public virtual async Task PinReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
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

            var data = context.UserData.GetValue<UserData>("userData");

            var match = Regex.Match(text, CommandMatchPin);

            if (match.Success && string.Equals(data.Pin, text, StringComparison.OrdinalIgnoreCase))
            {
                data.Pin = string.Empty;

                context.UserData.SetValue("userData", data);

                await this.ContinueProcess(context, activity);

                return;
            }

            await context.PostAsync(Labels.InvalidPin);

            context.Wait(this.PinReceivedAsync);
        }

        public virtual async Task SelectAccountAsync(IDialogContext context, IMessageActivity result)
        {
            context.ThrowIfNull(nameof(context));
            result.ThrowIfNull(nameof(result));

            context.UserData.TryGetValue("userData", out UserData data);

            var resultService = await this.profileService.GetAccounts(data.User.Token, data.User.Id);

            var accounts = resultService.ToDictionary(a => a.AccountId.ToString(), a => a.AccountName);

            data.User.Accounts = accounts;

            context.UserData.SetValue("userData", data);

            var reply = context.MakeMessage();

            if (!accounts.Any())
            {
                reply.Text = Labels.NoAccounts;

                await context.PostAsync(reply);

                context.Done(reply);
                return;
            }

            var accountsCard = new AccountsCard(accounts);

            reply.Text = Labels.ConnectToAccount;
            reply.Attachments.Add(accountsCard.ToAttachment());

            await context.PostAsync(reply);

            context.Wait(this.AccountReceivedAsync);
        }

        public virtual async Task AccountReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
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

            context.UserData.TryGetValue("userData", out UserData data);

            var account = data.User.Accounts.FirstOrDefault(a => string.Equals(a.Value, text, StringComparison.OrdinalIgnoreCase));

            if (!account.Equals(default(KeyValuePair<string, string>)))
            {
                data.Account = account;

                context.UserData.SetValue("userData", data);

                await this.ContinueProcess(context, activity);

                return;
            }

            await context.PostAsync(Labels.InvalidAccount);

            context.Wait(this.AccountReceivedAsync);
        }

        public virtual async Task SelectProjectAsync(IDialogContext context, IMessageActivity result)
        {
            context.ThrowIfNull(nameof(context));
            result.ThrowIfNull(nameof(result));

            context.UserData.TryGetValue("userData", out UserData data);

            var resultService = await this.projectService.GetProjects(data.Account.Value, data.User.Token);

            var projects = resultService.ToDictionary(a => a.Id.ToString(), a => a.Name);

            data.User.Projects = projects;

            context.UserData.SetValue("userData", data);

            var reply = context.MakeMessage();

            if (!projects.Any())
            {
                reply.Text = Labels.NoTeamProjects;
                await context.PostAsync(reply);
                context.Done(reply);
                return;
            }

            var projectsCard = new ProjectsCard(projects);

            reply.Text = Labels.ConnectToProject;
            reply.Attachments.Add(projectsCard.ToAttachment());

            await context.PostAsync(reply);

            context.Wait(this.ProjectReceivedAsync);
        }

        public virtual async Task ProjectReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
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

            context.UserData.TryGetValue("userData", out UserData data);

            var project = data.User.Projects.FirstOrDefault(a => string.Equals(a.Value, text, StringComparison.OrdinalIgnoreCase));

            if (!project.Equals(default(KeyValuePair<string, string>)))
            {
                data.Project = project;

                context.UserData.SetValue("userData", data);

                await this.ContinueProcess(context, activity);

                return;
            }

            await context.PostAsync(Labels.InvalidProject);

            context.Wait(this.ProjectReceivedAsync);
        }

        private string GeneratePin()
        {
            using (var generator = new RNGCryptoServiceProvider())
            {
                var data = new byte[4];

                generator.GetBytes(data);

                var value = BitConverter.ToUInt32(data, 0) % 100000;

                return value.ToString("00000", CultureInfo.InvariantCulture);
            }
        }
    }
}