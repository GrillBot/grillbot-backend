using GrillBot.Contracts.UserManagement.Responses;

namespace UserManagementService.Models.Infractions;

public class InternalDashboardRow : InfractionDashboardRow
{
    public DateTime CreatedAtUtc { get; set; }
}
