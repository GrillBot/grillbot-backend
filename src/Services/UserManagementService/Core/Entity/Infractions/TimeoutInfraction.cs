using Microsoft.EntityFrameworkCore;

namespace UserManagementService.Core.Entity.Infractions;

[Index(nameof(ExternalId), IsUnique = true)]
public class TimeoutInfraction : InfractionBase
{
    public DateTime ValidTo { get; set; }
    public long ExternalId { get; set; }
}
