using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Ambev.DeveloperEvaluation.ORM.Repositories;

public class SaleRepository : ISaleRepository
{
    private readonly DefaultContext _context;

    public SaleRepository(DefaultContext context)
    {
        _context = context;
    }

    public async Task<Sale> CreateAsync(Sale sale, CancellationToken cancellationToken = default)
    {
        await _context.Sales.AddAsync(sale, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return sale;
    }

    public async Task<Sale?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Sales
            .Include(s => s.Items)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<(IReadOnlyList<Sale> Items, int TotalCount)> ListAsync(
        SaleListCriteria criteria,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Sales
            .Include(s => s.Items)
            .AsQueryable();

        query = ApplyFilters(query, criteria);
        query = ApplyOrdering(query, criteria.OrderBy);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((criteria.Page - 1) * criteria.Size)
            .Take(criteria.Size)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<Sale> UpdateAsync(Sale sale, CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
        return sale;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var sale = await _context.Sales.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
        if (sale is null)
            return false;

        _context.Sales.Remove(sale);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static IQueryable<Sale> ApplyFilters(IQueryable<Sale> query, SaleListCriteria criteria)
    {
        if (criteria.CustomerId.HasValue)
            query = query.Where(s => s.CustomerId == criteria.CustomerId.Value);

        if (criteria.BranchId.HasValue)
            query = query.Where(s => s.BranchId == criteria.BranchId.Value);

        if (criteria.IsCancelled.HasValue)
            query = query.Where(s => s.IsCancelled == criteria.IsCancelled.Value);

        if (!string.IsNullOrWhiteSpace(criteria.CustomerName))
            query = ApplyStringFilter(query, s => s.CustomerName, criteria.CustomerName);

        if (!string.IsNullOrWhiteSpace(criteria.BranchName))
            query = ApplyStringFilter(query, s => s.BranchName, criteria.BranchName);

        if (criteria.MinSaleDate.HasValue)
            query = query.Where(s => s.SaleDate >= criteria.MinSaleDate.Value);

        if (criteria.MaxSaleDate.HasValue)
            query = query.Where(s => s.SaleDate <= criteria.MaxSaleDate.Value);

        if (criteria.MinTotalAmount.HasValue)
            query = query.Where(s => s.TotalAmount >= criteria.MinTotalAmount.Value);

        if (criteria.MaxTotalAmount.HasValue)
            query = query.Where(s => s.TotalAmount <= criteria.MaxTotalAmount.Value);

        if (criteria.SaleNumber.HasValue)
            query = query.Where(s => s.SaleNumber == criteria.SaleNumber.Value);

        return query;
    }

    private static IQueryable<Sale> ApplyStringFilter(
        IQueryable<Sale> query,
        System.Linq.Expressions.Expression<Func<Sale, string>> property,
        string filterValue)
    {
        var trimmed = filterValue.Trim('"');

        if (trimmed.StartsWith('*') && trimmed.EndsWith('*') && trimmed.Length > 1)
        {
            var value = trimmed.Trim('*');
            return query.Where(BuildContainsFilter(property, value));
        }

        if (trimmed.EndsWith('*'))
        {
            var value = trimmed.TrimEnd('*');
            return query.Where(BuildStartsWithFilter(property, value));
        }

        if (trimmed.StartsWith('*'))
        {
            var value = trimmed.TrimStart('*');
            return query.Where(BuildEndsWithFilter(property, value));
        }

        return query.Where(BuildEqualsFilter(property, trimmed));
    }

    private static System.Linq.Expressions.Expression<Func<Sale, bool>> BuildContainsFilter(
        System.Linq.Expressions.Expression<Func<Sale, string>> property,
        string value)
    {
        var parameter = property.Parameters[0];
        var body = System.Linq.Expressions.Expression.Call(
            property.Body,
            typeof(string).GetMethod(nameof(string.Contains), [typeof(string)])!,
            System.Linq.Expressions.Expression.Constant(value));
        return System.Linq.Expressions.Expression.Lambda<Func<Sale, bool>>(body, parameter);
    }

    private static System.Linq.Expressions.Expression<Func<Sale, bool>> BuildStartsWithFilter(
        System.Linq.Expressions.Expression<Func<Sale, string>> property,
        string value)
    {
        var parameter = property.Parameters[0];
        var body = System.Linq.Expressions.Expression.Call(
            property.Body,
            typeof(string).GetMethod(nameof(string.StartsWith), [typeof(string)])!,
            System.Linq.Expressions.Expression.Constant(value));
        return System.Linq.Expressions.Expression.Lambda<Func<Sale, bool>>(body, parameter);
    }

    private static System.Linq.Expressions.Expression<Func<Sale, bool>> BuildEndsWithFilter(
        System.Linq.Expressions.Expression<Func<Sale, string>> property,
        string value)
    {
        var parameter = property.Parameters[0];
        var body = System.Linq.Expressions.Expression.Call(
            property.Body,
            typeof(string).GetMethod(nameof(string.EndsWith), [typeof(string)])!,
            System.Linq.Expressions.Expression.Constant(value));
        return System.Linq.Expressions.Expression.Lambda<Func<Sale, bool>>(body, parameter);
    }

    private static System.Linq.Expressions.Expression<Func<Sale, bool>> BuildEqualsFilter(
        System.Linq.Expressions.Expression<Func<Sale, string>> property,
        string value)
    {
        var parameter = property.Parameters[0];
        var body = System.Linq.Expressions.Expression.Equal(
            property.Body,
            System.Linq.Expressions.Expression.Constant(value));
        return System.Linq.Expressions.Expression.Lambda<Func<Sale, bool>>(body, parameter);
    }

    private static IQueryable<Sale> ApplyOrdering(IQueryable<Sale> query, string? orderBy)
    {
        if (string.IsNullOrWhiteSpace(orderBy))
            return query.OrderByDescending(s => s.SaleDate);

        IOrderedQueryable<Sale>? orderedQuery = null;

        foreach (var part in orderBy.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var tokens = part.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var field = tokens[0].Trim('"');
            var descending = tokens.Length > 1 && tokens[1].Equals("desc", StringComparison.OrdinalIgnoreCase);

            orderedQuery = ApplyOrderField(query, orderedQuery, field, descending);
            query = orderedQuery ?? query;
        }

        return orderedQuery ?? query.OrderByDescending(s => s.SaleDate);
    }

    private static IOrderedQueryable<Sale>? ApplyOrderField(
        IQueryable<Sale> query,
        IOrderedQueryable<Sale>? orderedQuery,
        string field,
        bool descending)
    {
        return field.ToLowerInvariant() switch
        {
            "salenumber" => ApplyOrder(query, orderedQuery, s => s.SaleNumber, descending),
            "saledate" => ApplyOrder(query, orderedQuery, s => s.SaleDate, descending),
            "customername" => ApplyOrder(query, orderedQuery, s => s.CustomerName, descending),
            "branchname" => ApplyOrder(query, orderedQuery, s => s.BranchName, descending),
            "totalamount" => ApplyOrder(query, orderedQuery, s => s.TotalAmount, descending),
            "createdat" => ApplyOrder(query, orderedQuery, s => s.CreatedAt, descending),
            _ => orderedQuery
        };
    }

    private static IOrderedQueryable<Sale> ApplyOrder<TKey>(
        IQueryable<Sale> query,
        IOrderedQueryable<Sale>? orderedQuery,
        System.Linq.Expressions.Expression<Func<Sale, TKey>> keySelector,
        bool descending)
    {
        if (orderedQuery is null)
            return descending ? query.OrderByDescending(keySelector) : query.OrderBy(keySelector);

        return descending
            ? orderedQuery.ThenByDescending(keySelector)
            : orderedQuery.ThenBy(keySelector);
    }
}
