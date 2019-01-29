using Autofac.Integration.Mvc;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using VSTSBot.App_Start;


namespace VSTSBot
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            var container = Bootstrap.Build();

            GlobalConfiguration.Configure(c => WebApiConfig.Register(c, container));
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));

            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);

            RouteConfig.RegisterRoutes(RouteTable.Routes);
            //BundleConfig.RegisterBundles(BundleTable.Bundles);

            //container.RegisterWebApiFilterProvider(GlobalConfiguration.Configuration);
        }
    }
}
