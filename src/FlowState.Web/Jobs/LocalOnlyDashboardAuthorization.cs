using Hangfire.Dashboard;

namespace FlowState.Web.Jobs;

/// <summary>
/// Restricts the Hangfire dashboard to local requests until real auth lands (Slice 10).
/// Good enough for a single-instance demo; the dashboard is a great interview screenshot.
/// </summary>
public class LocalOnlyDashboardAuthorization : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();
        var remote = httpContext.Connection.RemoteIpAddress;
        if (remote is null) return false;
        return System.Net.IPAddress.IsLoopback(remote);
    }
}
