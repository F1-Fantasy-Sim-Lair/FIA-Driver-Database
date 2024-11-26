using Microsoft.EntityFrameworkCore;
using Web.Model.EF;

namespace Web.Model.Repository;

public class DefaultUnitOfWork(DriverDatabaseContext context, IServiceProvider serviceProvider) : IUnitOfWork
{
    readonly DriverDatabaseContext context = context;
    readonly IServiceProvider serviceProvider = serviceProvider;
    readonly Dictionary<Type, object> repositories = [];

    public IRepository<TEntity> Repository<TEntity>() where TEntity : class
    {
        var entityType = typeof(TEntity);

        if (!repositories.TryGetValue(entityType, out var untypedRepository) || untypedRepository is not IRepository<TEntity> repository)
        {
            repository = serviceProvider.GetService< IRepository<TEntity>>()
                ?? new DefaultRepository<TEntity>(context);

            repositories[entityType] = repository;
        }

        return repository;
    }

    public async Task CompleteAsync(CancellationToken cancellationToken) => await context.SaveChangesAsync(cancellationToken);

    public void Dispose() => context.Dispose();
}
