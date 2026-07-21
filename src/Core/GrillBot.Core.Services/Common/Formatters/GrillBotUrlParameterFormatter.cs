using Refit;
using System.Reflection;

namespace GrillBot.Core.Services.Common.Formatters;

internal class GrillBotUrlParameterFormatter : DefaultUrlParameterFormatter
{
    public override string? Format(object? parameterValue, ICustomAttributeProvider attributeProvider, Type type)
    {
        if (parameterValue is null)
            return null;

        var parameterType = parameterValue.GetType();
        if (parameterType.IsEnum && parameterType.GetCustomAttribute<FlagsAttribute>() is not null)
            return ((int)parameterValue).ToString();

        return base.Format(parameterValue, attributeProvider, type);
    }
}
