using SOK.Application.Common.Interface;
using SOK.Domain.Entities.Parish;
using SOK.Infrastructure.Persistence.Context;

namespace SOK.Infrastructure.Repositories
{
    public class BuildingRepository : Repository<Building, ParishDbContext>, IBuildingRepository
    {
        private readonly ParishDbContext _db;

        public BuildingRepository(ParishDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(Building building)
        {
            dbSet.Update(building);
        }
    }
}
