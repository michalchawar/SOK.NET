using SOK.Domain.Entities.Parish;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SOK.Application.Common.Interface
{
    /// <summary>
    /// Reprezentuje repozytorium ulic.
    /// </summary>
    public interface IStreetRepository : IUpdatableRepository<Street>
    {
    }
}
