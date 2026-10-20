using GrillBot.Data.Models.API.Infractions;

namespace GrillBot.Data.Models.API.UserMeasures;

/// <summary>
/// Request body of the legacy API V2 endpoint api/user/measures/timeout/create.
/// Kept under its original name so the public API schema does not change for existing clients.
/// </summary>
public class CreateUserMeasuresTimeoutParams : CreateInfractionTimeoutParams;
