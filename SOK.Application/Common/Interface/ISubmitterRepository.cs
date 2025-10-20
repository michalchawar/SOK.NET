using SOK.Domain.Entities.Parish;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SOK.Application.Common.Interface
{
    public interface ISubmitterRepository : IRepository<Submitter>
    {
        Task<Submitter?> GetRandomAsync();
        void Update(Submitter submitter);
    }
}
