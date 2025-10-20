using SOK.Domain.Entities.Parish;

namespace SOK.Application.Common.Interface
{
    public interface IBuildingRepository :  IRepository<Building>
    {
        void Update(Building building);
    }
}
