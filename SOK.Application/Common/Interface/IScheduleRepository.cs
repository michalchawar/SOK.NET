using SOK.Domain.Entities.Parish;

namespace SOK.Application.Common.Interface
{
    /// <summary>
    /// Reprezentuje repozytorium harmonogramów.
    /// </summary>
    public interface IScheduleRepository : IUpdatableRepository<Schedule>
    {
    }
}
