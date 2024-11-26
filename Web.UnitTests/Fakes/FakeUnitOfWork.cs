using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Web.Model.Repository;
using Web.Types;
using System.Linq;

namespace Web.UnitTests.Fakes;
public class FakeUnitOfWork : IUnitOfWork
{
    readonly Dictionary<Type, object> repositories = [];

    public FakeRepository<TEntity> Repository<TEntity>() where TEntity : class
    {
        var type = typeof(TEntity);

        if (!repositories.TryGetValue(type, out var untypedRepository) || untypedRepository is not FakeRepository<TEntity> repository)
        {
            repository = new FakeRepository<TEntity>();
            repositories[type] = repository;
        }

        return repository;
    }

    public Task CompleteAsync(CancellationToken cancellationToken = default) => Task.FromResult(1);
    IRepository<TEntity> IUnitOfWork.Repository<TEntity>() => Repository<TEntity>();
}

public class FakeRepository<TEntity> : IRepository<TEntity> where TEntity : class
{
    readonly Dictionary<Snowflake, TEntity> data = [];

    public IReadOnlyList<TEntity> Data => [.. data.Values];

    public Task<TEntity?> GetByIdAsync(Snowflake id, CancellationToken cancellationToken = default) => Task.FromResult(data.GetValueOrDefault(id));

    public Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default) => Task.FromResult(data.Values.AsEnumerable());

    public IQueryable<TEntity> Query() => data.Values.AsQueryable();

    public void Add(TEntity entity) => data.Add(FakeRepository<TEntity>.GetKey(entity), entity);

    public void AddRange(IEnumerable<TEntity> entities)
    {
        foreach(var entity in entities)
            Add(entity);
    }

    public void Delete(TEntity entity) => data.Remove(FakeRepository<TEntity>.GetKey(entity));

    static Snowflake GetKey(TEntity entity)
    {
        var likelyKeyName = typeof(TEntity).Name + "Id";
        var properties = entity.GetType().GetProperties();
        var keyProperty = properties.FirstOrDefault(property => property.Name == likelyKeyName || property.Name == "Id");
        return keyProperty?.GetValue(entity) as Snowflake?
            ?? throw new Exception();
    }
}
