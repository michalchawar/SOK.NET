using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using SOK.Application.Common.Interface;
using SOK.Domain.Entities.Central;
using SOK.Infrastructure.Persistence.Context;

namespace SOK.Infrastructure.Repositories
{
    /// <inheritdoc cref="IUnitOfWork" />
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

        public async Task<ITransaction> BeginTransactionAsync()
        {
            return new Transaction(await _db.Database.BeginTransactionAsync());
        }

        /// <inheritdoc cref="ITransaction" />
        private class Transaction : ITransaction
        {
            IDbContextTransaction _innerTransaction;

            public Transaction(IDbContextTransaction transaction)
            {
                _innerTransaction = transaction;
            }

            public async Task CommitAsync()
            {
                await _innerTransaction.CommitAsync();
            }

            public async Task RollbackAsync()
            {
                await _innerTransaction.RollbackAsync();
            }

            public async ValueTask DisposeAsync()
            {
                await _innerTransaction.DisposeAsync();
            }
        }
    }

    /// <inheritdoc cref="IUnitOfWorkCentral" />
    public class UnitOfWorkCentral : UnitOfWork<CentralDbContext>, IUnitOfWorkCentral
    {
        public IParishRepository Parishes { get; private set; }
        
        public UnitOfWorkCentral(CentralDbContext db) : base(db)
        {
            Parishes = new ParishRepository(db);
        }
    }

    /// <inheritdoc cref="IUnitOfWorkParish" />
    public class UnitOfWorkParish : UnitOfWork<ParishDbContext>, IUnitOfWorkParish
    {
        public IParishInfoRepository ParishInfo { get; private set; }
        public ISubmissionRepository Submission { get; private set; }
        public ISubmitterRepository Submitter { get; private set; }
        public IAddressRepository Address { get; private set; }
        public IBuildingRepository Building { get; private set; }
        public IStreetRepository Street { get; private set; }
        public IScheduleRepository Schedule { get; private set; }
        public IVisitRepository Visit { get; private set; }
        public IAgendaRepository Agenda { get; private set; }
        public IPlanRepository Plan { get; private set; }
        public IDayRepository Day { get; private set; }
        public IParishMemberRepository ParishMember { get; private set; }

        public UnitOfWorkParish(ParishDbContext db, UserManager<User> userManager) : base(db)
        {
            ParishInfo = new ParishInfoRepository(db);
            Submission = new SubmissionRepository(db);
            Submitter = new SubmitterRepository(db);
            Address = new AddressRepository(db);
            Building = new BuildingRepository(db);
            Street = new StreetRepository(db);
            Schedule = new ScheduleRepository(db);
            Visit = new VisitRepository(db);
            Agenda = new AgendaRepository(db);
            Plan = new PlanRepository(db);
            Day = new DayRepository(db);
            ParishMember = new ParishMemberRepository(db, userManager);
        }
    }
}
