using System.Linq.Expressions;

namespace GrillBot.Services.Common.Archivation;

/// <summary>
/// Ordered retention rules with a default. The first matching rule decides how long a record is kept;
/// records no rule matches use <see cref="DefaultRetention"/>. The policy compiles into one predicate
/// that EF Core translates to SQL, with the cutoffs computed once and passed as parameters.
/// </summary>
public sealed class RetentionPolicy<TEntity>
{
    private readonly Expression<Func<TEntity, DateTime>> _createdAt;
    private readonly Expression<Func<TEntity, bool>>? _alwaysExpired;

    public IReadOnlyList<RetentionRule<TEntity>> Rules { get; }
    public RetentionPeriod DefaultRetention { get; }

    /// <summary>
    /// Creates a policy from ordered rules and a default retention.
    /// </summary>
    /// <param name="createdAt">Selects the moment retention is counted from.</param>
    /// <param name="defaultRetention">Retention of records no rule matches.</param>
    /// <param name="rules">Rules in priority order; put conditional rules before broader ones for the same records.</param>
    /// <param name="alwaysExpired">Optional condition that makes a record expired regardless of its age.</param>
    public RetentionPolicy(
        Expression<Func<TEntity, DateTime>> createdAt,
        RetentionPeriod defaultRetention,
        IEnumerable<RetentionRule<TEntity>>? rules = null,
        Expression<Func<TEntity, bool>>? alwaysExpired = null
    )
    {
        _createdAt = createdAt;
        _alwaysExpired = alwaysExpired;
        DefaultRetention = defaultRetention;
        Rules = rules?.ToList() ?? [];

        var duplicate = Rules.GroupBy(o => o.Name).FirstOrDefault(o => o.Count() > 1);
        if (duplicate is not null)
            throw new ArgumentException($"Retention rule '{duplicate.Key}' is defined more than once.", nameof(rules));
    }

    /// <summary>
    /// Builds <c>alwaysExpired || (createdAt &lt;= newestCutoff &amp;&amp; ((rule1 &amp;&amp; createdAt &lt;= cutoff1) || (!rule1 &amp;&amp; rule2 &amp;&amp; createdAt &lt;= cutoff2) || ... || (!anyRule &amp;&amp; createdAt &lt;= defaultCutoff)))</c>.
    /// </summary>
    public Expression<Func<TEntity, bool>> BuildExpiredPredicate(DateTime nowUtc)
    {
        var parameter = _createdAt.Parameters[0];
        var createdAt = _createdAt.Body;

        Expression? rulesResult = null;
        Expression? noPreviousMatch = null;

        foreach (var rule in Rules)
        {
            var match = Rebind(rule.Match, parameter);
            Expression branch = Expression.AndAlso(match, IsCreatedBefore(createdAt, rule.Retention.GetCutoff(nowUtc)));
            if (noPreviousMatch is not null)
                branch = Expression.AndAlso(noPreviousMatch, branch);

            rulesResult = rulesResult is null ? branch : Expression.OrElse(rulesResult, branch);

            var notMatch = Expression.Not(match);
            noPreviousMatch = noPreviousMatch is null ? notMatch : Expression.AndAlso(noPreviousMatch, notMatch);
        }

        Expression defaultBranch = IsCreatedBefore(createdAt, DefaultRetention.GetCutoff(nowUtc));
        if (noPreviousMatch is not null)
            defaultBranch = Expression.AndAlso(noPreviousMatch, defaultBranch);

        rulesResult = rulesResult is null ? defaultBranch : Expression.OrElse(rulesResult, defaultBranch);

        // Nothing newer than the newest cutoff can be expired by age. A plain range condition up front
        // lets the database use the CreatedAt index before it evaluates the rules.
        if (Rules.Count > 0)
        {
            var newestCutoff = Rules.Select(o => o.Retention.GetCutoff(nowUtc)).Append(DefaultRetention.GetCutoff(nowUtc)).Max();
            rulesResult = Expression.AndAlso(IsCreatedBefore(createdAt, newestCutoff), rulesResult);
        }

        var result = _alwaysExpired is null ? rulesResult : Expression.OrElse(Rebind(_alwaysExpired, parameter), rulesResult);
        return Expression.Lambda<Func<TEntity, bool>>(result, parameter);
    }

    /// <summary>The rule that applies to the record, or null when the default applies.</summary>
    public RetentionRule<TEntity>? FindRule(TEntity entity)
        => Rules.FirstOrDefault(o => o.Match.Compile()(entity));

    private static BinaryExpression IsCreatedBefore(Expression createdAt, DateTime cutoff)
    {
        // A field on a closure-like holder makes EF Core send the cutoff as a query parameter instead of a literal.
        var holder = new CutoffHolder { Value = cutoff };
        var cutoffExpression = Expression.Field(Expression.Constant(holder), nameof(CutoffHolder.Value));

        return Expression.LessThanOrEqual(createdAt, cutoffExpression);
    }

    private static Expression Rebind(Expression<Func<TEntity, bool>> expression, ParameterExpression parameter)
        => new ParameterReplacer(expression.Parameters[0], parameter).Visit(expression.Body);

    private sealed class CutoffHolder
    {
        public DateTime Value;
    }

    private sealed class ParameterReplacer(ParameterExpression from, ParameterExpression to) : ExpressionVisitor
    {
        protected override Expression VisitParameter(ParameterExpression node)
            => node == from ? to : base.VisitParameter(node);
    }
}
