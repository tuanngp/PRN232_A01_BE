using BusinessObject;
using Repositories.Common;

namespace Repositories.Interface
{
    public interface ITagRepository : IGenericRepository<Tag>
    {
        Task<IEnumerable<Tag>> GetByNameAsync(string name);
        Task<bool> IsNameExistAsync(string name);
    }
}
