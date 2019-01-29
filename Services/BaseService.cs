using Microsoft.VisualStudio.Services.OAuth;
using Microsoft.VisualStudio.Services.WebApi;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using VSTSBot.Models;

namespace VSTSBot.Services
{
    [Serializable]
    public class BaseService
    {
        #region Attributes

        private const string VstsUrl = "https://{0}.visualstudio.com";
        private const string VstsRmUrl = "https://{0}.vsrm.visualstudio.com";

        private readonly Uri vstsAppUrl = new Uri("https://app.vssps.visualstudio.com");

        #endregion

        protected async Task<T> ConnectAsync<T>(OAuthToken token, string account = null, bool isRm = false)
            where T : VssHttpClientBase
        {
            var credentials = new VssOAuthAccessTokenCredential(new VssOAuthAccessToken(token.AccessToken));

            var uri = this.vstsAppUrl;

            if (!string.IsNullOrWhiteSpace(account))
            {
                uri = isRm
                    ? new Uri(string.Format(CultureInfo.InvariantCulture, VstsRmUrl, HttpUtility.UrlEncode(account)))
                    : new Uri(string.Format(CultureInfo.InvariantCulture, VstsUrl, HttpUtility.UrlEncode(account)));
            }

            return await new VssConnection(uri, credentials).GetClientAsync<T>();
        }
    }
}