using System.ComponentModel.DataAnnotations;

namespace GrillBot.Core.Validation;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
public class DiscordIdAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var memberName = validationContext.MemberName!;

        return value switch
        {
            null => ValidationResult.Success,
            ulong discordId => Check(discordId, memberName),
            string stringValue => CheckCollection([stringValue], validationContext),
            List<string> stringValues => CheckCollection(stringValues, validationContext),
            List<ulong> ids => CheckCollection(ids.Select(o => o.ToString()), validationContext),
            _ => new ValidationResult("Unsupported type of Discord identifier", [memberName])
        };
    }

    private static ValidationResult? CheckCollection(IEnumerable<string> stringValues, ValidationContext context)
    {
        var i = 0;
        foreach (var value in stringValues.Where(o => !string.IsNullOrEmpty(o)))
        {
            var memberName = $"{context.MemberName}.stringValues[{i}]";
            if (!ulong.TryParse(value, out var discordId))
                return new ValidationResult("Provided value is not discord ID.", [memberName]);

            var checkResult = Check(discordId, memberName);
            if (checkResult != ValidationResult.Success)
                return checkResult;

            i++;
        }

        return ValidationResult.Success;
    }

    private static ValidationResult? Check(ulong id, string memberName)
    {
        if (id == 0 || (id >> 22) == 0)
            return new ValidationResult("Invalid format of discord ID.", [memberName]);

        return ValidationResult.Success;
    }
}
