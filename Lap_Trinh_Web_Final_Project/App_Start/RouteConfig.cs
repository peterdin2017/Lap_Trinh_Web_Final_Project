using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Lap_Trinh_Web_Final_Project
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            var storeHomeRoute = routes.MapRoute(
                "StoreHostHome",
                "",
                new
                {
                    area = "Store",
                    controller = "Home",
                    action = "Index"
                },
                new
                {
                    host = new HostNameConstraint(
                        ConfigurationManager.AppSettings["StoreHost"],
                        "localhost")
                },
                new[] { "Lap_Trinh_Web_Final_Project.Areas.Store.Controllers" }
            );

            storeHomeRoute.DataTokens["area"] = "Store";
            storeHomeRoute.DataTokens["UseNamespaceFallback"] = false;
        }
    }
}
