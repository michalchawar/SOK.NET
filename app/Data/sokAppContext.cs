using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using app.Models;
using app.Models.Enums;

namespace app.Data
{
    public class sokAppContext : DbContext
    {
        public sokAppContext(DbContextOptions<sokAppContext> options)
            : base(options)
        {
        }

        public DbSet<app.Models.Address> Addresses { get; set; } = default!;
        public DbSet<app.Models.Agenda> Agendas { get; set; } = default!;
        public DbSet<app.Models.Building> Buildings { get; set; } = default!;
        public DbSet<app.Models.BuildingAssignment> BuildingAssignments { get; set; } = default!;
        public DbSet<app.Models.City> Cities { get; set; } = default!;
        public DbSet<app.Models.Day> Days { get; set; } = default!;
        public DbSet<app.Models.FormSubmission> FormSubmissions { get; set; } = default!;
        public DbSet<app.Models.ParishInfo> ParishInfo { get; set; } = default!;
        public DbSet<app.Models.Plan> Plans { get; set; } = default!;
        public DbSet<app.Models.Schedule> Schedules { get; set; } = default!;
        public DbSet<app.Models.Street> Streets { get; set; } = default!;
        public DbSet<app.Models.StreetSpecifier> StreetSpecifiers { get; set; } = default!;
        public DbSet<app.Models.Submission> Submissions { get; set; } = default!;
        public DbSet<app.Models.SubmissionSnapshot> SubmissionSnapshots { get; set; } = default!;
        public DbSet<app.Models.Submitter> Submitters { get; set; } = default!;
        public DbSet<app.Models.SubmitterSnapshot> SubmitterSnapshots { get; set; } = default!;
        public DbSet<app.Models.User> Users { get; set; } = default!;
        public DbSet<app.Models.Visit> Visits { get; set; } = default!;
        public DbSet<app.Models.VisitSnapshot> VisitSnapshots { get; set; } = default!;

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
        }
    }
}
