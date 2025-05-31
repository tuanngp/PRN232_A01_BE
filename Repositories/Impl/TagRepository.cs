using System.Collections.Generic;
using System.Threading.Tasks;
using BusinessObject;
using Microsoft.EntityFrameworkCore;
using Repositories.Common;
using Repositories.Interface;

namespace Repositories.Impl
{
    public class TagRepository : GenericRepository<Tag>, ITagRepository
    {
        public TagRepository(FUNewsDbContext context)
            : base(context) { }

        public async Task<IEnumerable<Tag>> GetByNameAsync(string name)
        {
            return await _dbSet
                .Where(t => t.TagName.ToLower().Contains(name.ToLower()))
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<bool> IsNameExistAsync(string name)
        {
            return await _dbSet.AnyAsync(t => t.TagName.ToLower() == name.ToLower());
        }
    }
}
