using Web.Types;

namespace Web.Model.Repository;

public interface IRepository<TEntity>
{
    Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);
    IQueryable<TEntity> Query();
    Task<TEntity?> GetByIdAsync(Snowflake id, CancellationToken cancellationToken = default);
    void Add(TEntity entity);
    void Delete(TEntity entity);
}
