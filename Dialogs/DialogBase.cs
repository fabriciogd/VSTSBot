using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Threading.Tasks;
using VSTSBot.Models;
using VSTSBot.Resources.Labels;
using VSTSBot.Services;

namespace VSTSBot.Dialogs
{
    [Serializable]
    public abstract class DialogBase
    {
        #region Attributes

        [NonSerialized]
        private readonly IAuthenticationService _authenticationService;

        #endregion

        public DialogBase()
        {
        }

        protected DialogBase(IAuthenticationService authenticationServicee)
        {
            this._authenticationService = authenticationServicee;
        }

        protected async Task<User> GetValidatedProfile(IBotDataBag dataBag)
        {
            if (!dataBag.TryGetValue("userData", out UserData data))
            {
                return null;
            }

            var profile = data.User;

            if (profile != null && profile.Token.ExpiresOn.AddMinutes(-5) <= DateTime.UtcNow)
            {
                profile.Token = await this._authenticationService.GetToken(profile.Token);

                dataBag.SetValue("userData", data);
            }

            return profile;
        }

        protected async Task<bool> IsCancelMessage(string text, IDialogContext context)
        {
            if (text.Equals("cancel", StringComparison.OrdinalIgnoreCase))
            {
                var reply = context.MakeMessage();
                reply.Text = Labels.Cancel;

                await context.PostAsync(reply);

                context.Done(reply);

                return true;
            }

            return false;
        }
    }
}