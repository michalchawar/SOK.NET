using SOK.Application.Common.Interface;
using SOK.Domain.Entities.Parish;
using SOK.Infrastructure.Persistence.Context;

namespace SOK.Infrastructure.Repositories
{
    /// <inheritdoc />
    public class BuildingRepository : UpdatableRepository<Building, ParishDbContext>, IBuildingRepository
    {
        public BuildingRepository(ParishDbContext db) : base(db) {}
    }
}
