using System.Configuration;
using System.Web.Mvc;
using Lap_Trinh_Web_Final_Project;

namespace Lap_Trinh_Web_Final_Project.Areas.Store
{
    public class StoreAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Store";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "Store_default",
                "Store/{controller}/{action}/{id}",
                new
                {
                    action = "Index",
                    id = UrlParameter.Optional
                },
                new
                {
                    host = new HostNameConstraint(
                        ConfigurationManager.AppSettings["StoreHost"],
                        "localhost")
                }
            );
        }
    }
}
