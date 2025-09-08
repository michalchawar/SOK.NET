using Microsoft.EntityFrameworkCore;
using SOK.Application.Common.Interface;
using SOK.Infrastructure.Persistence.Context;

namespace SOK.Infrastructure.Repositories
{
    public class UnitOfWork<T> : IUnitOfWork where T : DbContext
    {
        protected readonly T _db;
        public UnitOfWork(T db)
        {
            _db = db;
        }
        public async Task SaveAsync()
        {
            await _db.SaveChangesAsync();
        }
    }

    public class UnitOfWorkCentral : UnitOfWork<CentralDbContext>, IUnitOfWorkCentral
    {
        public IParishRepository Parishes { get; private set; }
        
        public UnitOfWorkCentral(CentralDbContext db) : base(db)
        {
            Parishes = new ParishRepository(db);
        }
    }

    public class UnitOfWorkParish : UnitOfWork<ParishDbContext>, IUnitOfWorkParish
    {
        public IParishInfoRepository ParishInfo { get; private set; }

        public UnitOfWorkParish(ParishDbContext db) : base(db)
        {
            ParishInfo = new ParishInfoRepository(db);
        }
    }
}
