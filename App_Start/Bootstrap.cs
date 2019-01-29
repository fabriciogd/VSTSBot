using Autofac;
using Autofac.Extras.AttributeMetadata;
using Autofac.Integration.Mvc;
using Autofac.Integration.WebApi;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Connector;
using System.Linq;
using VSTSBot.Controllers;
using VSTSBot.Dialogs;
using VSTSBot.Factories;
using VSTSBot.Services;

namespace VSTSBot.App_Start
{
    public static class Bootstrap
    {
        public static IContainer Build()
        {
            Conversation.UpdateContainer(Build);

            return Conversation.Container;
        }

        private static void Build(ContainerBuilder builder)
        {
            builder
                .RegisterModule<AttributedMetadataModule>();

            builder
               .RegisterType<BotDataFactory>()
               .As<IBotDataFactory>()
               .InstancePerDependency();

            builder
               .RegisterType<AuthenticationService>()
               .As<IAuthenticationService>();

            builder
               .RegisterType<ProfileService>()
               .As<IProfileService>();

            builder
                .RegisterType<ProjectService>()
                .As<IProjectService>();

            builder
                .RegisterType<TeamService>()
                .As<ITeamService>();

            builder
                .RegisterType<WorkItemService>()
                .As<IWorkItemService>();

            builder
              .RegisterAssemblyTypes(typeof(Bootstrap).Assembly)
              .Where(t => t.GetInterfaces().Any(i => i.IsAssignableFrom(typeof(IDialog<object>))))
              .Except<RootDialog>()
              .Except<ConnectDialog>()
              .AsImplementedInterfaces();

            builder
                .RegisterType<ConnectDialog>()
                .WithParameter("appId", Config.ApplicationId)
                .WithParameter("appScope", Config.ApplicationScope)
                .WithParameter("authorizeUrl", Config.AuthorizeUrl)
                .AsImplementedInterfaces();

            builder
              .RegisterControllers(typeof(Bootstrap).Assembly)
              .Except<AuthorizeController>();

            builder
                .RegisterApiControllers(typeof(Bootstrap).Assembly);

            builder
               .RegisterType<AuthorizeController>()
               .WithParameter("appSecret", Config.ApplicationSecret)
               .WithParameter("authorizeUrl", Config.AuthorizeUrl)
               .AsSelf();

            builder
               .RegisterType<BotState>()
               .AsImplementedInterfaces();

            var store = new InMemoryDataStore();

            builder.Register(c => new CachingBotDataStore(store, CachingBotDataStoreConsistencyPolicy.LastWriteWins))
                .As<IBotDataStore<BotData>>()
                .AsSelf()
                .SingleInstance();

        }
    }
}