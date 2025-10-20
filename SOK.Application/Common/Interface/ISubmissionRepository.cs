using SOK.Domain.Entities.Parish;
using System.Linq.Expressions;

namespace SOK.Application.Common.Interface
{
    public interface ISubmissionRepository : IRepository<Submission>
    {
        void Update(Submission submission);

        Task<List<Submission>> GetWithIncludesAsync(
            Expression<Func<Submission, bool>>? filter,
            int pageSize = 1,
            int page = 1,
            bool submitter = false,
            bool address = false,
            bool visit = false,
            bool history = false,
            bool formSubmission = false,
            bool tracked = false);
    }
}