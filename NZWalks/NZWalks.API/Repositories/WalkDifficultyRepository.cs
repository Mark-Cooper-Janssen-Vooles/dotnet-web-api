using Microsoft.EntityFrameworkCore;
using NZWalks.API.Data;
using NZWalks.API.Models.Domain;
using NZWalks.API.Models.DTO;

namespace NZWalks.API.Repositories
{
    public class WalkDifficultyRepository : IWalkDifficultyRepository
    {
        private readonly NZWalksDbContext dbContext;

        public WalkDifficultyRepository(NZWalksDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<Models.Domain.WalkDifficulty> CreateAsync(Models.Domain.WalkDifficulty walkDifficulty)
        {
            walkDifficulty.Id = Guid.NewGuid();
            await dbContext.WalkDifficulty.AddAsync(walkDifficulty);
            await dbContext.SaveChangesAsync();

            return walkDifficulty;
        }

        public async Task<Models.Domain.WalkDifficulty> DeleteAsync(Guid id)
        {
            var walkDifficulty = await GetAsync(id);
            if (walkDifficulty == null)
            {
                return null;
            }

            dbContext.WalkDifficulty.Remove(walkDifficulty);
            await dbContext.SaveChangesAsync();

            return walkDifficulty;
        }

        public async Task<IEnumerable<Models.Domain.WalkDifficulty>> GetAllAsync()
        {
            return await dbContext.WalkDifficulty.ToListAsync();
        }

        public async Task<Models.Domain.WalkDifficulty> GetAsync(Guid id)
        {
            return await dbContext.WalkDifficulty.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<Models.Domain.WalkDifficulty> UpdateAsync(Guid id, Models.Domain.WalkDifficulty walkDifficulty)
        {
            var existingWalkDifficulty = await GetAsync(id);
            if (existingWalkDifficulty == null)
            {
                return null;
            }

            existingWalkDifficulty.Code = walkDifficulty.Code;

            await dbContext.SaveChangesAsync();

            return await GetAsync(id);
        }
    }
}
