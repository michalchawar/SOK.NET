using SOK.Application.Common.DTO;
using SOK.Domain.Entities.Parish;
using System.Linq.Expressions;

namespace SOK.Application.Services.Interface
{
    public interface ISubmissionService
    {
        public Task<Submission?> GetSubmissionByIdAsync(int id);

        public Task<Submission?> GetSubmissionByUidAsync(string submissionUid);

        public Task<List<Submission>> GetSubmissionsPaginated(
            Expression<Func<Submission, bool>>? filter = null,
            int page = 1,
            int pageSize = 1);

        public Task CreateSubmissionAsync(SubmissionCreationRequestDto submissionDto);

        public Task<bool> DeleteSubmissionAsync(int id);

        public Task UpdateSubmissionAsync(Submission submission);
    }
}