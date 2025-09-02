using app.Models.Parish.Entities;
using app.Services.Parish;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace app.Data
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
        public DbSet<Plan> Plans { get; set; } = default!;
        public DbSet<Schedule> Schedules { get; set; } = default!;
        public DbSet<Street> Streets { get; set; } = default!;
        public DbSet<StreetSpecifier> StreetSpecifiers { get; set; } = default!;
        public DbSet<Submission> Submissions { get; set; } = default!;
        public DbSet<SubmissionSnapshot> SubmissionSnapshots { get; set; } = default!;
        public DbSet<Submitter> Submitters { get; set; } = default!;
        public DbSet<SubmitterSnapshot> SubmitterSnapshots { get; set; } = default!;
        public DbSet<User> Users { get; set; } = default!;
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
            modelBuilder.ApplyConfiguration(new UserEntityTypeConfiguration());
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
                Console.WriteLine($"Connection string for the parish with UID {_currentParishService.ParishUid} is not set.");
            }

            base.OnConfiguring(optionsBuilder);
        }
    }


    // Prosty stub serwisu ICurrentParishService do użycia w czasie projektowania (migracje, narzędzia EF Core)
    public class DesignTimeParishService : ICurrentParishService
    {
        public string? ConnectionString { get; set; } = string.Empty;
        public string? ParishUid { get; set; } = string.Empty;

        public async Task<bool> SetParish(string parishUid)
        {
            return false;
        }
    }

    public class ParishDbContextFactory : IDesignTimeDbContextFactory<ParishDbContext>
    {
        public ParishDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ParishDbContext>();

            // fallback do LocalDB (żeby migracje działały lokalnie)
            var connectionString = "Server=(localdb)\\MSSQLLocalDB;Database=ParishDbDummy;Trusted_Connection=True;";

            optionsBuilder.UseSqlServer(connectionString);

            // podajemy stub serwisu zamiast prawdziwego
            var fakeService = new DesignTimeParishService
            {
                ConnectionString = connectionString
            };

            return new ParishDbContext(optionsBuilder.Options, fakeService);
        }
    }
}
