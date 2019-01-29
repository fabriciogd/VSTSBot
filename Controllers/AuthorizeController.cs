using Microsoft.Bot.Builder.Dialogs;
using Microsoft.VisualStudio.Services.Profile;
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using VSTSBot.Factories;
using VSTSBot.Models;
using VSTSBot.Services;

namespace VSTSBot.Controllers
{
    public class AuthorizeController : ApiController
    {
        #region Attributes

        private readonly string _appSecret;
        private readonly Uri _authorizeUrl;
        private readonly IAuthenticationService _authenticationService;
        private readonly IProfileService _profileService;
        private readonly IBotDataFactory _botDataFactory;

        #endregion

        public AuthorizeController(
            string appSecret, 
            Uri authorizeUrl, 
            IAuthenticationService authenticationService, 
            IProfileService profileService, 
            IBotDataFactory botDataFactory)
        {
            this._appSecret = appSecret;
            this._authorizeUrl = authorizeUrl;
            this._authenticationService = authenticationService;
            this._profileService = profileService;
            this._botDataFactory = botDataFactory;
        }

        [HttpGet]
        public async Task<HttpResponseMessage> Index(string code, string state)
        {
            var decoded = Encoding.UTF8.GetString(HttpServerUtility.UrlTokenDecode(state));

            var queryString = HttpUtility.ParseQueryString(decoded);

            var address = new Address(queryString["botId"], 
                queryString["channelId"], 
                queryString["userId"], 
                queryString["conversationId"], 
                queryString["serviceUrl"]);

            var token = await _authenticationService.GetToken(this._appSecret, this._authorizeUrl, code);

            var profile = await _profileService.GetProfile(token);

            var user = CreateProfile(profile, token);

            var botData = _botDataFactory.Create(address);

            await botData.LoadAsync(CancellationToken.None);

            var data = botData.UserData.GetValue<UserData>("userData");

            data.User = user;

            botData.UserData.SetValue("userData", data);

            await botData.FlushAsync(CancellationToken.None);

            var resp = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(string.Format("<html><body>Pin: {0}</body></html>", data.Pin), System.Text.Encoding.UTF8, @"text/html")
            };

            return resp;
        }

        private User CreateProfile(Profile profile, OAuthToken token)
        {
            return new User
            {
                Id = profile.Id,
                Name = profile.DisplayName,
                Token = token
            };
        }
    }
}