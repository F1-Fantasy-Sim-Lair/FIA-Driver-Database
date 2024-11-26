using Microsoft.EntityFrameworkCore;
using Web.Model.EF;
using Web.Types;

namespace Web.Model.Repository;

public class DefaultRepository<TEntity>(DriverDatabaseContext context) : IRepository<TEntity> where TEntity : class
{
    readonly DbSet<TEntity> dbSet = context.Set<TEntity>();

    public async Task<TEntity?> GetByIdAsync(Snowflake id, CancellationToken cancellationToken) => await dbSet.FindAsync([id], cancellationToken: cancellationToken);

    public async Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken) => await dbSet.ToListAsync(cancellationToken: cancellationToken);

    public IQueryable<TEntity> Query() => dbSet.AsQueryable();

    public void Add(TEntity entity) => dbSet.Add(entity);

    public void Delete(TEntity entity) => dbSet.Remove(entity);
}