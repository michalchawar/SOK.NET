using SOK.Domain.Entities.Parish;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SOK.Application.Common.Interface
{
    /// <summary>
    /// Reprezentuje repozytorium wizyt.
    /// </summary>
    public interface IVisitRepository : IUpdatableRepository<Visit>
    {
        Task SetToUnplannedAsync(int visitId);
        Task WithdrawAsync(int visitId);
    }
}
