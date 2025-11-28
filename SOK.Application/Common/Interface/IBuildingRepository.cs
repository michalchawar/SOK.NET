using SOK.Domain.Entities.Parish;

namespace SOK.Application.Common.Interface
{
    /// <summary>
    /// Reprezentuje repozytorium budynków (bram i domów).
    /// </summary>
    public interface IBuildingRepository :  IUpdatableRepository<Building>
    {
    }
}
