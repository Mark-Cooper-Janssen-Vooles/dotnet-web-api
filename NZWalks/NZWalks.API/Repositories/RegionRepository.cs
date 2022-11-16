using Microsoft.EntityFrameworkCore;
using NZWalks.API.Data;
using NZWalks.API.Models.Domain;

namespace NZWalks.API.Repositories
{
    public class RegionRepository : IRegionRepository
    {
        private readonly NZWalksDbContext dbContext;

        public RegionRepository(NZWalksDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<Region> AddAsync(Region region)
        {
            region.Id = Guid.NewGuid();
            await dbContext.AddAsync(region);
            await dbContext.SaveChangesAsync();
            return region;
        }

        public async Task<Region> DeleteAsync(Guid id)
        {
            var region = await GetAsync(id);
            if (region == null)
            {
                return null;
            }

            // Delete the region
            dbContext.Regions.Remove(region);
            await dbContext.SaveChangesAsync();

            return region;
        }

        public async Task<IEnumerable<Region>> GetAllAsync()
        {
            return await dbContext.Regions.ToListAsync();
        }

        public async Task<Region> GetAsync(Guid id)
        {
            return await dbContext.Regions.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<Region> UpdateAsync(Guid id, Region updatedRegion)
        {
            var existingRegion = await GetAsync(id);
            if (existingRegion == null)
            {
                return null;
            }

            existingRegion.Code = updatedRegion.Code;
            existingRegion.Name = updatedRegion.Name;
            existingRegion.Area = updatedRegion.Area;
            existingRegion.Lat = updatedRegion.Lat;
            existingRegion.Long = updatedRegion.Long;
            existingRegion.Population = updatedRegion.Population;
            await dbContext.SaveChangesAsync();

            return existingRegion;
        }
    }
}
