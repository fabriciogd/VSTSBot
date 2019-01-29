using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace VSTSBot.Extensions
{
    public static class ContextExtensions
    {
        public static async Task SendTyping(this IDialogContext context)
        {
            var reply = context.MakeMessage();
            reply.Type = ActivityTypes.Typing;
            reply.Text = "";

            await context.PostAsync(reply);
        }
    }
}