using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Web;
using VSTSBot.Extensions;
using VSTSBot.Resources.Labels;

namespace VSTSBot.Cards
{
    public class LogOnCard: HeroCard
    {
        #region Attributes

        private const string _urlOAuth = "https://app.vssps.visualstudio.com/oauth2/authorize?client_id={0}&response_type=Assertion&state={1}&scope={2}&redirect_uri={3}";

        #endregion

        public LogOnCard(string appId, string appScope, Uri authorizeUrl, IActivity activity)
        {
            appId.ThrowIfNullOrWhiteSpace(nameof(appId));
            appScope.ThrowIfNullOrWhiteSpace(nameof(appScope));
            authorizeUrl.ThrowIfNull(nameof(authorizeUrl));
            activity.ThrowIfNull(nameof(activity));

            string state = this.GetStateParam(activity);

            this.Text = Labels.PleaseLogin;

            var button = new CardAction
            {
                Value = string.Format(CultureInfo.InvariantCulture, _urlOAuth, appId, state, appScope, authorizeUrl),
                Type = string.Equals(activity.ChannelId, ChannelIds.Msteams, StringComparison.Ordinal) ? ActionTypes.OpenUrl : ActionTypes.Signin,
                Title = Labels.AuthenticationRequired
            };

            this.Buttons = new List<CardAction> { button };
        }

        /// <summary>
        /// Encode activity params
        /// </summary>
        /// <param name="activity">Activity instance</param>
        /// <returns>Encoded activity params</returns>
        private string GetStateParam(IActivity activity)
        {
            var queryString = HttpUtility.ParseQueryString(string.Empty);

            queryString["botId"] = activity.Recipient.Id;
            queryString["channelId"] = activity.ChannelId;
            queryString["userId"] = activity.From.Id;
            queryString["conversationId"] = activity.Conversation.Id;
            queryString["serviceUrl"] = activity.ServiceUrl;

            return HttpServerUtility.UrlTokenEncode(Encoding.UTF8.GetBytes(queryString.ToString()));
        }
    }
}