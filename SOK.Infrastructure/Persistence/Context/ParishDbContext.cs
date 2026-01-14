using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using SOK.Application.Services.Interface;
using SOK.Domain.Entities.Central;
using SOK.Domain.Entities.Parish;
using SOK.Domain.Enums;
using SOK.Infrastructure.Persistence.Configurations.Parish;

namespace SOK.Infrastructure.Persistence.Context
{
    public class ParishDbContext : DbContext
    {
        private readonly ICurrentParishService _currentParishService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public string OverrideConnectionString = string.Empty;

        public ParishDbContext(
            DbContextOptions<ParishDbContext> options, 
            ICurrentParishService currentParishService,
            IHttpContextAccessor httpContextAccessor)
            : base(options)
        {
            _currentParishService = currentParishService;
            _httpContextAccessor = httpContextAccessor;
        }

        public DbSet<Address> Addresses { get; set; } = default!;
        public DbSet<Agenda> Agendas { get; set; } = default!;
        public DbSet<Building> Buildings { get; set; } = default!;
        public DbSet<BuildingAssignment> BuildingAssignments { get; set; } = default!;
        public DbSet<City> Cities { get; set; } = default!;
        public DbSet<Day> Days { get; set; } = default!;
        public DbSet<FormSubmission> FormSubmissions { get; set; } = default!;
        public DbSet<ParishInfo> ParishInfo { get; set; } = default!;
        public DbSet<ParishMember> Members { get; set; } = default!;
        public DbSet<Plan> Plans { get; set; } = default!;
        public DbSet<Schedule> Schedules { get; set; } = default!;
        public DbSet<Street> Streets { get; set; } = default!;
        public DbSet<StreetSpecifier> StreetSpecifiers { get; set; } = default!;
        public DbSet<Submission> Submissions { get; set; } = default!;
        public DbSet<SubmissionSnapshot> SubmissionSnapshots { get; set; } = default!;
        public DbSet<Submitter> Submitters { get; set; } = default!;
        public DbSet<SubmitterSnapshot> SubmitterSnapshots { get; set; } = default!;
        public DbSet<Visit> Visits { get; set; } = default!;
        public DbSet<VisitSnapshot> VisitSnapshots { get; set; } = default!;
        public DbSet<EmailLog> EmailLogs { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Rejestracja konfiguracji encji
            modelBuilder.ApplyConfiguration(new AddressEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new AgendaEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new BuildingEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new BuildingAssignmentEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new CityEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new DayEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new FormSubmissionEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new ParishInfoEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new PlanEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new ScheduleEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new StreetEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new StreetSpecifierEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new SubmissionEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new SubmissionSnapshotEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new SubmitterEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new SubmitterSnapshotEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new ParishMemberEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new VisitEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new VisitSnapshotEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new EmailLogEntityTypeConfiguration());

            base.OnModelCreating(modelBuilder);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (optionsBuilder.IsConfigured)
            {
                return;
            }

            // Dynamiczne ustawiamy connection string na podstawie aktualnej parafii
            string? parishConnectionString = _currentParishService.ConnectionString;

            //  Jeśli ustawiono OverrideConnectionString, to go używamy
            if (!string.IsNullOrEmpty(OverrideConnectionString))
            {
                parishConnectionString = OverrideConnectionString;
            }

            // Jeśli connection string jest ustawiony, to konfigurujemy DbContext
            if (!string.IsNullOrEmpty(parishConnectionString))
            {
                optionsBuilder.UseSqlServer(parishConnectionString);
            }
            else
            {
                optionsBuilder.UseSqlServer("");
                Console.WriteLine($"Connection string for the parish with UID {_currentParishService.ParishUid} is not set.");
            }

            base.OnConfiguring(optionsBuilder);
        }

        public async Task<ParishEntry?> GetCurrentParishAsync()
        {
            return await _currentParishService.GetCurrentParishAsync();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await CreateSnapshotsForModifiedEntitiesAsync();
            return await base.SaveChangesAsync(cancellationToken);
        }

        public override int SaveChanges()
        {
            CreateSnapshotsForModifiedEntitiesAsync().GetAwaiter().GetResult();
            return base.SaveChanges();
        }

        private async Task CreateSnapshotsForModifiedEntitiesAsync()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Modified)
                .ToList();

            // Pobierz aktualnie zalogowanego użytkownika raz dla wszystkich snapshotów
            int? currentUserId = GetCurrentUserId();

            foreach (var entry in entries)
            {
                switch (entry.Entity)
                {
                    case Submission submission:
                        await CreateSubmissionSnapshotAsync(entry, submission, currentUserId);
                        break;
                    case Submitter submitter:
                        await CreateSubmitterSnapshotAsync(entry, submitter, currentUserId);
                        break;
                    case Visit visit:
                        await CreateVisitSnapshotAsync(entry, visit, currentUserId);
                        break;
                }
            }
        }

        private int? GetCurrentUserId()
        {
            try
            {
                var user = _httpContextAccessor.HttpContext?.User;
                if (user == null || !user.Identity?.IsAuthenticated == true)
                    return null;

                // Pobierz ID użytkownika bezpośrednio z Claims (bez circular dependency na IParishMemberService)
                var userIdClaim = user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                    return null;

                return userId;
            }
            catch
            {
                // W przypadku błędu (np. brak HttpContext w background job), zwróć null
                return null;
            }
        }

        private async Task CreateSubmissionSnapshotAsync(EntityEntry entry, Submission submission, int? currentUserId)
        {
            if (
                entry.OriginalValues.GetValue<Guid>(nameof(Submission.UniqueId)) 
                    == entry.CurrentValues.GetValue<Guid>(nameof(Submission.UniqueId)) &&
                entry.OriginalValues.GetValue<string>(nameof(Submission.AccessToken)) 
                    == entry.CurrentValues.GetValue<string>(nameof(Submission.AccessToken)) &&
                entry.OriginalValues.GetValue<string?>(nameof(Submission.SubmitterNotes)) 
                    == entry.CurrentValues.GetValue<string?>(nameof(Submission.SubmitterNotes)) &&
                entry.OriginalValues.GetValue<string?>(nameof(Submission.AdminMessage)) 
                    == entry.CurrentValues.GetValue<string?>(nameof(Submission.AdminMessage)) &&
                entry.OriginalValues.GetValue<string?>(nameof(Submission.AdminNotes)) 
                    == entry.CurrentValues.GetValue<string?>(nameof(Submission.AdminNotes)) &&
                entry.OriginalValues.GetValue<NotesFulfillmentStatus>(nameof(Submission.NotesStatus)) 
                    == entry.CurrentValues.GetValue<NotesFulfillmentStatus>(nameof(Submission.NotesStatus))
            )
            {
                return;
            }

            // Pobierz oryginalne wartości dla prostych właściwości Submission
            var originalValues = entry.OriginalValues;
            var originalUniqueId = originalValues.GetValue<Guid>(nameof(Submission.UniqueId));
            var originalAccessToken = originalValues.GetValue<string>(nameof(Submission.AccessToken));
            var originalSubmitterNotes = originalValues.GetValue<string?>(nameof(Submission.SubmitterNotes));
            var originalAdminMessage = originalValues.GetValue<string?>(nameof(Submission.AdminMessage));
            var originalAdminNotes = originalValues.GetValue<string?>(nameof(Submission.AdminNotes));
            var originalNotesStatus = originalValues.GetValue<NotesFulfillmentStatus>(nameof(Submission.NotesStatus));

            // Pobierz oryginalny AddressId (żeby wiedzieć czy adres się zmienił)
            var originalAddressId = originalValues.GetValue<int>(nameof(Submission.AddressId));

            // TERAZ załaduj powiązane encje dla oryginalnego adresu
            Address? originalAddress = null;
            if (originalAddressId > 0)
            {
                // Załaduj oryginalny adres z bazy (nie z submission.Address, bo to może być już nowy!)
                originalAddress = await Addresses
                    .Include(a => a.Building)
                        .ThenInclude(b => b.Street)
                            .ThenInclude(s => s.City)
                    .Include(a => a.Building)
                        .ThenInclude(b => b.Street)
                            .ThenInclude(s => s.Type)
                    .AsNoTracking() // Nie śledź tego - to tylko do odczytu snapshota
                    .FirstOrDefaultAsync(a => a.Id == originalAddressId);
            }

            var snapshot = new SubmissionSnapshot
            {
                SubmissionId = submission.Id,
                UniqueId = originalUniqueId,
                AccessToken = originalAccessToken,
                SubmitterNotes = originalSubmitterNotes,
                AdminMessage = originalAdminMessage,
                AdminNotes = originalAdminNotes,
                NotesStatus = originalNotesStatus,
                
                // Dane adresowe - z ORYGINALNEGO adresu (przed zmianą)
                Apartment = (originalAddress?.ApartmentNumber.HasValue ?? false)
                    ? $"{originalAddress.ApartmentNumber}{originalAddress.ApartmentLetter ?? string.Empty}"
                    : string.Empty,
                Building = (originalAddress?.Building?.Letter != null)
                    ? $"{originalAddress.Building.Number}{originalAddress.Building.Letter}"
                    : originalAddress?.Building?.Number.ToString() ?? string.Empty,
                StreetSpecifier = originalAddress?.Building?.Street?.Type?.Abbreviation ?? string.Empty,
                Street = originalAddress?.Building?.Street?.Name ?? string.Empty,
                City = originalAddress?.Building?.Street?.City?.Name ?? string.Empty,

                ChangeAuthorId = currentUserId,
            };

            await SubmissionSnapshots.AddAsync(snapshot);
        }

        private async Task CreateSubmitterSnapshotAsync(EntityEntry entry, Submitter submitter, int? currentUserId)
        {
            // Sprawdź, czy zaszły istotne zmiany, aby utworzyć snapshot
            if (
                entry.OriginalValues.GetValue<Guid>(nameof(Submitter.UniqueId)) 
                    == entry.CurrentValues.GetValue<Guid>(nameof(Submitter.UniqueId)) &&
                entry.OriginalValues.GetValue<string>(nameof(Submitter.Name)) 
                    == entry.CurrentValues.GetValue<string>(nameof(Submitter.Name)) &&
                entry.OriginalValues.GetValue<string>(nameof(Submitter.Surname)) 
                    == entry.CurrentValues.GetValue<string>(nameof(Submitter.Surname)) &&
                entry.OriginalValues.GetValue<string?>(nameof(Submitter.Email)) 
                    == entry.CurrentValues.GetValue<string?>(nameof(Submitter.Email)) &&
                entry.OriginalValues.GetValue<string?>(nameof(Submitter.Phone)) 
                    == entry.CurrentValues.GetValue<string?>(nameof(Submitter.Phone))
            )
            {
                return;
            }

            var originalValues = entry.OriginalValues;

            var snapshot = new SubmitterSnapshot
            {
                UniqueId = originalValues.GetValue<Guid>(nameof(Submitter.UniqueId)),
                Name = originalValues.GetValue<string>(nameof(Submitter.Name)),
                Surname = originalValues.GetValue<string>(nameof(Submitter.Surname)),
                Email = originalValues.GetValue<string?>(nameof(Submitter.Email)),
                Phone = originalValues.GetValue<string?>(nameof(Submitter.Phone)),
                ChangeAuthorId = currentUserId,
            };
            
            await SubmitterSnapshots.AddAsync(snapshot);
        }

        private async Task CreateVisitSnapshotAsync(EntityEntry entry, Visit visit, int? currentUserId)
        {
            // Sprawdź, czy zaszły istotne zmiany, aby utworzyć snapshot
            if (
                entry.OriginalValues.GetValue<int?>(nameof(Visit.OrdinalNumber)) 
                    == entry.CurrentValues.GetValue<int?>(nameof(Visit.OrdinalNumber)) &&
                entry.OriginalValues.GetValue<VisitStatus>(nameof(Visit.Status)) 
                    == entry.CurrentValues.GetValue<VisitStatus>(nameof(Visit.Status)) &&
                entry.OriginalValues.GetValue<int?>(nameof(Visit.PeopleCount)) 
                    == entry.CurrentValues.GetValue<int?>(nameof(Visit.PeopleCount)) &&
                entry.OriginalValues.GetValue<int?>(nameof(Visit.AgendaId)) 
                    == entry.CurrentValues.GetValue<int?>(nameof(Visit.AgendaId))
            )
            {
                return;
            }

            var originalValues = entry.OriginalValues;

            // Załaduj Schedule jeśli nie jest załadowany (potrzebujemy Schedule.Name)
            if (visit.ScheduleId.HasValue && !entry.Reference(nameof(Visit.Schedule)).IsLoaded)
                await entry.Reference(nameof(Visit.Schedule)).LoadAsync();

            // Załaduj Agenda jeśli nie jest załadowana
            if (visit.AgendaId.HasValue && !entry.Reference(nameof(Visit.Agenda)).IsLoaded)
                await entry.Reference(nameof(Visit.Agenda)).LoadAsync();

            // Pobierz datę z Day (przez Agenda)
            DateTime? visitDate = null;
            bool? dateVisibility = null;

            if (visit.Agenda != null)
            {
                var agendaEntry = Entry(visit.Agenda);
                if (!agendaEntry.Reference(nameof(Agenda.Day)).IsLoaded)
                    await agendaEntry.Reference(nameof(Agenda.Day)).LoadAsync();

                if (visit.Agenda.Day != null)
                {
                    visitDate = visit.Agenda.Day.Date.ToDateTime(TimeOnly.MinValue);
                    dateVisibility = !visit.Agenda.HideVisits;
                }
            }

            var snapshot = new VisitSnapshot
            {
                VisitId = visit.Id,
                OrdinalNumber = (short)(originalValues.GetValue<int?>(nameof(Visit.OrdinalNumber)) ?? 0),
                Status = originalValues.GetValue<VisitStatus>(nameof(Visit.Status)),
                PeopleCount = originalValues.GetValue<int?>(nameof(Visit.PeopleCount)),
                ScheduleName = visit.Schedule?.Name,
                AgendaId = originalValues.GetValue<int?>(nameof(Visit.AgendaId)),
                Date = visitDate,
                DateVisibility = dateVisibility,
                PredictedTime = null, // TODO: Dodaj jeśli będziesz mieć przewidywany czas wizyty
                PredictedTimeVisibility = visit.Agenda?.ShowHours,
                ChangeAuthorId = currentUserId,
            };

            await VisitSnapshots.AddAsync(snapshot);
        }
    }
}
