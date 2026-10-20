using Microsoft.EntityFrameworkCore;

namespace UserManagementService.Core.Entity.Infractions;

[Index(nameof(LogSetId), IsUnique = true)]
public class UnverifyInfraction : InfractionBase
{
    public DateTime ValidTo { get; set; }
    public long LogSetId { get; set; }
}
