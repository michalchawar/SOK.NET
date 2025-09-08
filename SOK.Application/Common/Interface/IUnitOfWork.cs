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
    }
}
