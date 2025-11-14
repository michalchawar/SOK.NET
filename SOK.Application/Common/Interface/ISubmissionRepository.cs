using SOK.Domain.Entities.Parish;
using System.Linq.Expressions;

namespace SOK.Application.Common.Interface
{
    /// <summary>
    /// Reprezentuje repozytorium zgłoszeń.
    /// </summary>
    public interface ISubmissionRepository : IUpdatableRepository<Submission>
    {
        Task<IEnumerable<Submission>> GetPaginatedAsync(
            Expression<Func<Submission, bool>>? filter,
            int pageSize = 1,
            int page = 1,
            bool submitter = false,
            bool address = false,
            bool addressFull = false,
            bool visit = false,
            bool history = false,
            bool formSubmission = false,
            bool tracked = false);

        Task<Submission?> GetRandomAsync();
    }
}