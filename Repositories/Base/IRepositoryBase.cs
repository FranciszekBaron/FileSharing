using System.Linq.Expressions;


public interface IRepositoryBase<T>
{
    Task<IEnumerable<T>> GetAllAsync();
    Task<IEnumerable<T>> GetByConditionAsync(Expression<Func<T, bool>> condition);

    void Create(T entity);
    void Delete(T entity);
    void Update(T entity);

}
