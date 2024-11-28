namespace Web.Model.Repository;

public interface IUnitOfWork
{
    IRepository<TEntity> Repository<TEntity>() where TEntity : class;
    Task CompleteAsync(CancellationToken cancellationToken = default);
}
