using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using Microsoft.Owin.Security.OAuth;
using Newtonsoft.Json.Serialization;

namespace TJC.Applink.RestWebAPIService
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services

            // Web API routes
            config.MapHttpAttributeRoutes();


            config.Routes.MapHttpRoute(
             name: "DefaultApi",
             routeTemplate: "{action}/{id}",
             defaults: new { controller = "AppLink", action = "GetAppLinkInformation", id = RouteParameter.Optional }
            );
        }
    }
}
