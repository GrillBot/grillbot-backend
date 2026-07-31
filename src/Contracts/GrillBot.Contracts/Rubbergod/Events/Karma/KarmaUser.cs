namespace GrillBot.Contracts.Rubbergod.Events.Karma;

public sealed record KarmaUser(string MemberId, int Karma, int Positive, int Negative);
