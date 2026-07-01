using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace GreenNode.WebAPI.Hubs;

public static class DashboardHubMethods
{
    public const string QuoteCreated = "QuoteCreated";
    public const string QuoteApproved = "QuoteApproved";
    public const string QuoteRejected = "QuoteRejected";
    public const string EvidenceUploaded = "EvidenceUploaded";
    public const string ProjectCompleted = "ProjectCompleted";
    public const string CertificateGenerated = "CertificateGenerated";
    public const string DashboardUpdated = "DashboardUpdated";
}

[Authorize(Roles = "Admin")]
public class DashboardHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "Admins");
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "Admins");
        await base.OnDisconnectedAsync(exception);
    }
}
