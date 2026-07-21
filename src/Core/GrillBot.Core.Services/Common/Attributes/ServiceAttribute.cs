namespace GrillBot.Core.Services.Common.Attributes;

[AttributeUsage(AttributeTargets.Interface)]
public class ServiceAttribute(string serviceName) : Attribute
{
    public string ServiceName => serviceName;
}
