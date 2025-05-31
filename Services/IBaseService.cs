namespace Services
{
    public interface IBaseService<TEntity, ID>
        where TEntity : class
    {
        Task<TEntity?> GetByIdAsync(ID id);
        Task<IEnumerable<TEntity>> GetAllAsync();
        Task<TEntity> AddAsync(TEntity entity);
        Task<TEntity> UpdateAsync(TEntity entity);
        Task<bool> DeleteAsync(ID id);
    }
}
