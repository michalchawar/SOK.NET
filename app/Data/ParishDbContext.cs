using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using app.Models.Enums;
using app.Services.ParishService;
using app.Models.Parish;

namespace app.Data
{
    public class ParishDbContext : DbContext
    {
        private readonly ICurrentParishService _currentParishService;

        public ParishDbContext(DbContextOptions<ParishDbContext> options, ICurrentParishService currentParishService)
            : base(options)
        {
            _currentParishService = currentParishService;
        }

        public DbSet<Parish.Address> Addresses { get; set; } = default!;
        public DbSet<Parish.Agenda> Agendas { get; set; } = default!;
        public DbSet<Parish.Building> Buildings { get; set; } = default!;
        public DbSet<Parish.BuildingAssignment> BuildingAssignments { get; set; } = default!;
        public DbSet<Parish.City> Cities { get; set; } = default!;
        public DbSet<Parish.Day> Days { get; set; } = default!;
        public DbSet<Parish.FormSubmission> FormSubmissions { get; set; } = default!;
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
            modelBuilder.ApplyConfiguration(new VisitSnapshotEntityTypeConfiguration());,

            base.OnModelCreating(modelBuilder);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string parishConnectionString = _currentParishService.ConnectionString 
                ?? throw new InvalidOperationException($"Connection string for the parish with UID {_currentParishService.ParishUid} is not set.");

            if (!string.IsNullOrEmpty(parishConnectionString))
            {
                optionsBuilder.UseSqlServer(parishConnectionString);
            }

            base.OnConfiguring(optionsBuilder);
        }
    }
}
