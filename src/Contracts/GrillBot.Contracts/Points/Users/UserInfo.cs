namespace GrillBot.Contracts.Points.Users;

public class UserInfo
{
    public bool NoActivity { get; set; }
    public bool PointsDisabled { get; set; }
    public int? PointsPostion { get; set; }
    public PointsStatus? Status { get; set; }
}
