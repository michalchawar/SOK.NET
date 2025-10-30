using SOK.Application.Common.Interface;
using SOK.Application.Services.Interface;
using SOK.Domain.Entities.Parish;
using System.Linq.Expressions;

namespace SOK.Application.Services.Implementation
{
    /// <inheritdoc />
    public class ParishMemberService : IParishMemberService
    {
        private readonly IUnitOfWorkParish _uow;

        public ParishMemberService(IUnitOfWorkParish uow)
        {
            _uow = uow;
        }

        /// <inheritdoc />
        public async Task<ParishMember?> GetParishMemberAsync(int id)
        {
            return await _uow.ParishMember.GetAsync(pm => pm.Id == id);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<ParishMember>> GetParishMembersPaginatedAsync(
            Expression<Func<ParishMember, bool>>? filter = null,
            int page = 1,
            int pageSize = 1)
        {
            if (pageSize < 1) throw new ArgumentException("Page size must be positive.");
            if (page < 1) throw new ArgumentException("Page must be positive.");

            return await _uow.ParishMember.GetPaginatedAsync(filter, pageSize: pageSize, page: page);
        }

        /// <inheritdoc />
        public async Task UpdateParishMemberAsync(ParishMember parishMember)
        {
            _uow.ParishMember.Update(parishMember);
            await _uow.SaveAsync();
        }
    }
}