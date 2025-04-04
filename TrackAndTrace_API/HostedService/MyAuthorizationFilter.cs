using Hangfire.Dashboard;

namespace TrackAndTrace_API.HostedService
{
    public class MyAuthorizationFilter : IDashboardAuthorizationFilter
    {
        private readonly string[] _roles;

        public MyAuthorizationFilter(params string[] roles)
        {
            _roles = roles;
        }

        public bool Authorize(DashboardContext context)
        {
            var httpContext = ((AspNetCoreDashboardContext)context).HttpContext;

            //Your authorization logic goes here.

            return true; //I'am returning true for simplicity
        }
    }

}
