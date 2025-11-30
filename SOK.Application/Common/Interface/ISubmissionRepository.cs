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
            bool plan = false,
            bool tracked = false);

        Task<(IEnumerable<Submission> submissions, int totalCount)> GetPaginatedWithSortingAsync(
            Expression<Func<Submission, bool>>? filter,
            string sortBy = "time",
            string order = "desc",
            int pageSize = 1,
            int page = 1,
            bool submitter = false,
            bool address = false,
            bool addressFull = false,
            bool visit = false,
            bool history = false,
            bool formSubmission = false,
            bool plan = false,
            bool tracked = false);

        Task<Submission?> GetRandomAsync();
    }
}