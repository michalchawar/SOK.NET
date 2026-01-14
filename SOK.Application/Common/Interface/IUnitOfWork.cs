namespace SOK.Application.Common.Interface
{
    /// <summary>
    /// Reprezentuje zbiór repozytoriów.
    /// </summary>
    public interface IUnitOfWork
    {
        /// <summary>
        /// Zapisuje stan repozytoriów w bazie danych.
        /// </summary>
        /// <returns>
        /// Obiekt <see cref="Task"/>, reprezentujący asynchroniczną operację.
        /// </returns>
        Task SaveAsync();

        /// <summary>
        /// Rozpoczyna transakcję bazy danych.
        /// </summary>
        /// <returns>
        /// Obiekt <see cref="Task"/>, reprezentujący asynchroniczną operację,
        /// którego wartością jest obiekt transakcji, implementujący interfejs <see cref="ITransaction"/>.
        /// </returns>
        Task<ITransaction> BeginTransactionAsync();

        /// <summary>
        /// Wykonuje operację w ramach transakcji z obsługą strategii ponownych prób.
        /// </summary>
        /// <typeparam name="TResult">Typ wyniku operacji.</typeparam>
        /// <param name="operation">Operacja do wykonania w ramach transakcji.</param>
        /// <returns>
        /// Obiekt <see cref="Task{TResult}"/>, reprezentujący asynchroniczną operację.
        /// </returns>
        Task<TResult> ExecuteInTransactionAsync<TResult>(Func<Task<TResult>> operation);
    }

    /// <summary>
    /// Reprezentuje transakcję bazy danych.
    /// </summary>
    public interface ITransaction : IAsyncDisposable
    {
        /// <summary>
        /// Zapisuje wszystkie zmiany i kończy transakcję.
        /// </summary>
        /// <returns>
        /// Obiekt <see cref="Task"/>, reprezentujący asynchroniczną operację.
        /// </returns>
        Task CommitAsync();

        /// <summary>
        /// Cofa wszystkie zmiany i kończy transakcję.
        /// </summary>
        /// <returns>
        /// Obiekt <see cref="Task"/>, reprezentujący asynchroniczną operację.
        /// </returns>
        Task RollbackAsync();
    }

    /// <summary>
    /// Reprezentuje zbiór repozytoriów, związanych z centralną bazą danych.
    /// </summary>
    public interface IUnitOfWorkCentral : IUnitOfWork
    {
        /// <summary>
        /// Repozytorium parafii.
        /// </summary>
        IParishRepository Parishes { get; }
    }

    /// <summary>
    /// Reprezentuje zbiór repozytoriów, związanych z parafialną bazą danych.
    /// </summary>
    public interface IUnitOfWorkParish : IUnitOfWork
    {
        /// <summary>
        /// Repozytorium informacji o parafii.
        /// </summary>
        IParishInfoRepository ParishInfo { get; }

        /// <summary>
        /// Repozytorium zgłoszeń.
        /// </summary>
        ISubmissionRepository Submission { get; }

        /// <summary>
        /// Repozytorium zgłaszających.
        /// </summary>
        ISubmitterRepository Submitter { get; }

        /// <summary>
        /// Repozytorium adresów.
        /// </summary>
        IAddressRepository Address { get; }

        /// <summary>
        /// Repozytorium budynków (bram i domów).
        /// </summary>
        IBuildingRepository Building { get; }

        /// <summary>
        /// Repozytorium ulic.
        /// </summary>
        IStreetRepository Street { get; }
        
        /// <summary>
        /// Repozytorium typów ulic.
        /// </summary>
        IStreetSpecifierRepository StreetSpecifier { get; }

        /// <summary>
        /// Repozytorium miast.
        /// </summary>
        ICityRepository City { get; }

        /// <summary>
        /// Repozytorium harmonogramów.
        /// </summary>
        IScheduleRepository Schedule { get; }

        /// <summary>
        /// Repozytorium wizyt.
        /// </summary>
        IVisitRepository Visit { get; }

        /// <summary>
        /// Repozytorium agend.
        /// </summary>
        IAgendaRepository Agenda { get; }

        /// <summary>
        /// Repozytorium planów.
        /// </summary>
        IPlanRepository Plan { get; }

        /// <summary>
        /// Repozytorium dni kolędowych.
        /// </summary>
        IDayRepository Day { get; }

        /// <summary>
        /// Repozytorium przypisań budynków do dni.
        /// </summary>
        IBuildingAssignmentRepository BuildingAssignment { get; }

        /// <summary>
        /// Repozytorium członków parafii.
        /// </summary>
        IParishMemberRepository ParishMember { get; }

        /// <summary>
        /// Repozytorium logów emaili.
        /// </summary>
        IEmailLogRepository EmailLog { get; }
    }
}
