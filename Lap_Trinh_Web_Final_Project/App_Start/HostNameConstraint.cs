using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Routing;

namespace Lap_Trinh_Web_Final_Project
{
    /// <summary>
    /// Limits an MVC route to one of its configured host names.
    /// URL generation stays relative so links keep the current subdomain.
    /// </summary>
    public sealed class HostNameConstraint : IRouteConstraint
    {
        private readonly HashSet<string> allowedHosts;

        public HostNameConstraint(string primaryHost, params string[] aliases)
        {
            allowedHosts = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (!string.IsNullOrWhiteSpace(primaryHost))
            {
                allowedHosts.Add(primaryHost.Trim());
            }

            if (aliases == null) return;

            foreach (string alias in aliases)
            {
                if (!string.IsNullOrWhiteSpace(alias))
                {
                    allowedHosts.Add(alias.Trim());
                }
            }
        }

        public bool Match(
            HttpContextBase httpContext,
            Route route,
            string parameterName,
            RouteValueDictionary values,
            RouteDirection routeDirection)
        {
            if (routeDirection == RouteDirection.UrlGeneration)
            {
                return true;
            }

            string host = httpContext == null || httpContext.Request == null || httpContext.Request.Url == null
                ? null
                : httpContext.Request.Url.Host;

            return !string.IsNullOrEmpty(host) && allowedHosts.Contains(host);
        }
    }
}
