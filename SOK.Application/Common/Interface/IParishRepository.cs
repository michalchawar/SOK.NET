using SOK.Domain.Entities.Central;

namespace SOK.Application.Common.Interface
{
    /// <summary>
    /// Reprezentuje repozytorium parafii.
    /// </summary>
    public interface IParishRepository : IUpdatableRepository<ParishEntry>
    {
    }
}