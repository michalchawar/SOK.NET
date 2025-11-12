using SOK.Domain.Entities.Central;
using SOK.Domain.Entities.Parish;
using System.Linq.Expressions;

namespace SOK.Application.Common.Interface
{
    public interface IParishMemberRepository : IRepository<ParishMember>
    {
        void Update(ParishMember parishMember);

        Task<IEnumerable<ParishMember>> GetPaginatedAsync(
            Expression<Func<ParishMember, bool>>? filter,
            int pageSize = 1,
            int page = 1,
            bool assignedAgendas = false,
            bool enteredSubmissions = false,
            bool tracked = false);

        Task<User> GenerateUserAsync(string displayName);
    }
}
