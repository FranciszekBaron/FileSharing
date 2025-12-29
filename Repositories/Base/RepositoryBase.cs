using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

public class RepositoryBase<T> : IRepositoryBase<T> where T : class
{
    private readonly FileSharingDbContext _dbContext;
    

    // tutaj klasycznie dependency injection 
    public RepositoryBase(FileSharingDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async void Create(T entity)
    {
        _dbContext.Set<T>().Add(entity);
    }

    public void Delete(T entity)
    {
        _dbContext.Set<T>().Remove(entity);
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _dbContext.Set<T>().AsNoTracking().ToListAsync();
    }

    public async Task<IEnumerable<T>> GetByConditionAsync(Expression<Func<T, bool>> condition)
    {
        return await _dbContext.Set<T>().Where(condition).AsNoTracking().ToListAsync();
        //AsNoTracking bo brak sledzenia zmian dla 
    }

    public void Update(T entity)
    {
        _dbContext.Set<T>().Update(entity);
    }
}