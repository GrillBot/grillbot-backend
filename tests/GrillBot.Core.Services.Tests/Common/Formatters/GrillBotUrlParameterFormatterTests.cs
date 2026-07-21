using GrillBot.Core.Services.Common.Formatters;
using PointsService;
using PointsService.Enums;
using NSubstitute;
using System.Reflection;

namespace GrillBot.Core.Services.Tests.Common.Formatters;

[TestClass]
public class GrillBotUrlParameterFormatterTests
{
    private GrillBotUrlParameterFormatter _formatter = null!;
    private ICustomAttributeProvider _attributeProvider = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        _formatter = new();
        _attributeProvider = Substitute.For<ICustomAttributeProvider>();
    }

    [TestMethod]
    public void Format_NullValue()
    {
        Assert.IsNull(_formatter.Format(null, null!, null!));
    }

    [TestMethod]
    public void Format_DefaultBehavior()
    {
        var result = _formatter.Format("TODO", _attributeProvider, typeof(string));
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public void Format_EnumWithoutFlags()
    {
        const LeaderboardSortOptions ENUM = LeaderboardSortOptions.ByTotalDescending;

        var result = _formatter.Format(ENUM, _attributeProvider, typeof(IPointsServiceClient));
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public void Format_EnumWithFlags()
    {
        const LeaderboardColumnFlag ENUM = LeaderboardColumnFlag.YearBack | LeaderboardColumnFlag.MonthBack;
        const string EXPECTED_RESULT = "3";

        var result = _formatter.Format(ENUM, _attributeProvider, typeof(IPointsServiceClient));

        Assert.IsNotNull(result);
        Assert.AreEqual(EXPECTED_RESULT, result);
    }
}
