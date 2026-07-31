using GrillBot.Contracts.Unverify.Requests;

namespace GrillBot.Contracts.Unverify.Events;

/// <summary>
/// Carries an <see cref="UnverifyRequest"/> across the broker. It composes the REST
/// request rather than inheriting it - a record cannot derive from a class, and the
/// request is a live REST contract that must not change shape.
/// </summary>
public sealed record SetUnverifyMessage(UnverifyRequest Request);
