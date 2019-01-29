using System;
using System.Web.Configuration;

namespace VSTSBot
{
    public static class Config
    {
        public static  string ApplicationId => WebConfigurationManager.AppSettings["VSSPSAppId"];

        public static string ApplicationSecret => WebConfigurationManager.AppSettings["VSSPSSecret"];

        public static string ApplicationScope => WebConfigurationManager.AppSettings["VSSPSScope"];

        public static Uri AuthorizeUrl => new Uri(WebConfigurationManager.AppSettings["AuthorizeUrl"]);
    }
}