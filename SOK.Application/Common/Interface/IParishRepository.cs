using SOK.Domain.Entities.Central;

namespace SOK.Application.Common.Interface
{
    public interface IParishRepository : IRepository<ParishEntry>
    {
        void Update(ParishEntry parish);
    }
}