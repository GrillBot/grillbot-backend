using System.Linq.Expressions;

namespace GrillBot.Services.Common.Archivation;

/// <summary>
/// A named retention rule: records matching <see cref="Match"/> are kept for <see cref="Retention"/>.
/// <see cref="Match"/> must be translatable by the query provider (EF Core) and must not
/// dereference a navigation without a null check.
/// </summary>
public sealed record RetentionRule<TEntity>(string Name, Expression<Func<TEntity, bool>> Match, RetentionPeriod Retention);
