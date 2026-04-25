using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace REPOSITORY.Repositories
{
    public interface IGenericRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAll();
        Task<IEnumerable<T>> GetAllAsync();

        Task<int> CountAsync(Expression<Func<T, bool>> predicate);
        Task<T?> GetFirstOrDefaultAsync(Expression<Func<T, bool>> filter);
        Task<IEnumerable<T>> GetAllAsyncWithPagination(int page, int pageSize);
        Task<IEnumerable<T>> GetAllAsyncWithPaginationByDesc<TKey>(int page, int pageSize, Expression<Func<T, TKey>> orderBy);
        Task<IEnumerable<T>> GetByConditionWithPaginationByDesc<TKey>(Expression<Func<T, bool>> expression, int page, int pageSize, Expression<Func<T, TKey>> orderBy);
        Task<T?> GetById(int id);
        Task<T?> GetByGuid(Guid id);
        Task<T?> GetByIdAsync(int id);
        Task Add(T entity);
        IQueryable<T> GetByExpEagerly(Expression<Func<T, bool>> predicate, params string[] navigationProperties);
        Task AddMultiple(IEnumerable<T> entity);
        void Update(T entity);
        void Attach(T entity);
        void UpdateMultiple(IEnumerable<T> entity);
        void Delete(T entity);
        void DeleteMultiple(IEnumerable<T> entity);
        Task<IEnumerable<T>> GetByCondition(Expression<Func<T, bool>> expression);
        Task<IEnumerable<T>> GetByConditionWithPagination(Expression<Func<T, bool>> expression, int page, int pageSize);
        Task<bool> Create(T entity);
        List<T> GetByExp(Expression<Func<T, bool>> predicate);
        IQueryable<T> GetByExpAsync(Expression<Func<T, bool>> predicate);
        IQueryable<T> GetByExpIQueryable(Expression<Func<T, bool>> predicate);
        IQueryable<T> Query();
        Task<bool> CreateRange(List<T> entityList);
        Task<bool> UpdateRange(List<T> entityList);
    }
}
