using Microsoft.EntityFrameworkCore;

#pragma warning disable RCS1158 // Static member in generic type should use a type parameter
namespace GrillBot.Core.Models.Pagination;

public class PaginatedResponse<TModel>
{
    public List<TModel> Data { get; set; } = [];
    public int Page { get; set; }
    public long TotalItemsCount { get; set; }

    public static async Task<PaginatedResponse<TEntity>> CreateWithEntityAsync<TEntity>(IQueryable<TEntity> query, PaginatedParams @params, CancellationToken cancellationToken = default)
    {
        var result = new PaginatedResponse<TEntity>
        {
            Page = Math.Max(@params.Page, 0),
            TotalItemsCount = await query.CountAsync(cancellationToken)
        };

        if (result.TotalItemsCount == 0 || @params.OnlyCount)
            return result;

        query = query.Skip(@params.Skip()).Take(@params.PageSize);
        result.Data.AddRange(await query.ToListAsync(cancellationToken));

        return result;
    }

    public static async Task<PaginatedResponse<TModel>> CopyAndMapAsync<TEntity>(PaginatedResponse<TEntity> resultWithEntity,
        Func<TEntity, Task<TModel>> converter)
    {
        var result = new PaginatedResponse<TModel>
        {
            Page = resultWithEntity.Page,
            TotalItemsCount = resultWithEntity.TotalItemsCount
        };

        foreach (var item in resultWithEntity.Data)
            result.Data.Add(await converter(item));

        return result;
    }

    public static PaginatedResponse<TModel> Create(List<TModel> data, PaginatedParams request)
    {
        var result = new PaginatedResponse<TModel>
        {
            Page = Math.Max(request.Page, 0),
            TotalItemsCount = data.Count
        };

        if (result.TotalItemsCount == 0 || request.OnlyCount)
            return result;

        result.Data = [.. data.Skip(request.Skip()).Take(request.PageSize)];
        return result;
    }
}
