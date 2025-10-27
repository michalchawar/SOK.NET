using SOK.Domain.Entities.Parish;

namespace SOK.Application.Common.Interface
{
    public interface IParishMemberRepository : IRepository<ParishMember>
    {
        void Update(ParishMember parishMember);
    }
}
