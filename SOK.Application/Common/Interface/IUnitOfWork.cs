namespace SOK.Application.Common.Interface
{
    public interface IUnitOfWork
    {
        Task SaveAsync();
    }

    public interface IUnitOfWorkCentral : IUnitOfWork
    {
        IParishRepository Parishes { get; }
    }

    public interface IUnitOfWorkParish : IUnitOfWork
    {
        IParishInfoRepository ParishInfo { get; }
        ISubmissionRepository Submission { get; }
        ISubmitterRepository Submitter { get; }
        IAddressRepository Address { get; }
        IBuildingRepository Building { get; }
        IStreetRepository Street { get; }
        IScheduleRepository Schedule { get; }
        IVisitRepository Visit { get; }
        IAgendaRepository Agenda { get; }
        IPlanRepository Plan { get; }
        IDayRepository Day { get; }
    }
}
