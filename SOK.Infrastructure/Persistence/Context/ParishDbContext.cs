using Microsoft.EntityFrameworkCore;
using SOK.Application.Services.Interface;
using SOK.Domain.Entities.Central;
using SOK.Domain.Entities.Parish;
using SOK.Infrastructure.Persistence.Configurations.Parish;

namespace SOK.Infrastructure.Persistence.Context
{
    public class ParishDbContext : DbContext
    {
        private readonly ICurrentParishService _currentParishService;

        public string OverrideConnectionString = string.Empty;

        public ParishDbContext(DbContextOptions<ParishDbContext> options, ICurrentParishService currentParishService)
            : base(options)
        {
            _currentParishService = currentParishService;
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
    }
}
