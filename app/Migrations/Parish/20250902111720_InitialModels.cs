using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace app.Migrations.Parish
{
    /// <inheritdoc />
    public partial class InitialModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Cities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ParishInfo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(96)", maxLength: 96, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParishInfo", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StreetSpecifiers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FullName = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Abbreviation = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StreetSpecifiers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Submitters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UniqueId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "CONVERT(varchar(64), HASHBYTES('SHA2_256', CAST(NEWID() AS varchar(36))), 2)"),
                    Name = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Surname = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Submitters", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Streets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    PostalCode = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: true),
                    StreetSpecifierId = table.Column<int>(type: "int", nullable: false),
                    CityId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Streets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Streets_Cities_CityId",
                        column: x => x.CityId,
                        principalTable: "Cities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Streets_StreetSpecifiers_StreetSpecifierId",
                        column: x => x.StreetSpecifierId,
                        principalTable: "StreetSpecifiers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Plans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    AuthorId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Plans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Plans_Users_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "SubmitterSnapshots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UniqueId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Surname = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    ChangeTime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    ChangeAuthorId = table.Column<int>(type: "int", nullable: true),
                    SubmitterId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubmitterSnapshots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubmitterSnapshots_Submitters_SubmitterId",
                        column: x => x.SubmitterId,
                        principalTable: "Submitters",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SubmitterSnapshots_Users_ChangeAuthorId",
                        column: x => x.ChangeAuthorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Buildings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Number = table.Column<int>(type: "int", nullable: false),
                    Letter = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: true),
                    FloorCount = table.Column<int>(type: "int", nullable: false),
                    ApartmentCount = table.Column<int>(type: "int", nullable: false),
                    HighestApartmentNumber = table.Column<int>(type: "int", nullable: false),
                    HasElevator = table.Column<bool>(type: "bit", nullable: false),
                    AllowSelection = table.Column<bool>(type: "bit", nullable: false),
                    StreetId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Buildings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Buildings_Streets_StreetId",
                        column: x => x.StreetId,
                        principalTable: "Streets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Days",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    StartHour = table.Column<TimeOnly>(type: "time", nullable: false),
                    EndHour = table.Column<TimeOnly>(type: "time", nullable: false),
                    PlanId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Days", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Days_Plans_PlanId",
                        column: x => x.PlanId,
                        principalTable: "Plans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Schedules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ShortName = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: false),
                    PlanId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Schedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Schedules_Plans_PlanId",
                        column: x => x.PlanId,
                        principalTable: "Plans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Addresses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApartmentNumber = table.Column<int>(type: "int", nullable: true),
                    ApartmentLetter = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: true),
                    BuildingId = table.Column<int>(type: "int", nullable: false),
                    StreetId = table.Column<int>(type: "int", nullable: false),
                    CityId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Addresses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Addresses_Buildings_BuildingId",
                        column: x => x.BuildingId,
                        principalTable: "Buildings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Addresses_Cities_CityId",
                        column: x => x.CityId,
                        principalTable: "Cities",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Addresses_Streets_StreetId",
                        column: x => x.StreetId,
                        principalTable: "Streets",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Agendas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UniqueId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AccessToken = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false, defaultValueSql: "CONVERT(varchar(64), HASHBYTES('SHA2_256', CAST(NEWID() AS varchar(36))), 2)"),
                    StartHourOverride = table.Column<TimeOnly>(type: "time", nullable: true),
                    EndHourOverride = table.Column<TimeOnly>(type: "time", nullable: true),
                    GatheredFunds = table.Column<float>(type: "real", nullable: true),
                    ShowsAssignment = table.Column<bool>(type: "bit", nullable: false),
                    ShowHours = table.Column<bool>(type: "bit", nullable: false),
                    DayId = table.Column<int>(type: "int", nullable: false),
                    ScheduleId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Agendas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Agendas_Days_DayId",
                        column: x => x.DayId,
                        principalTable: "Days",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Agendas_Schedules_ScheduleId",
                        column: x => x.ScheduleId,
                        principalTable: "Schedules",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Submissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UniqueId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "CONVERT(varchar(64), HASHBYTES('SHA2_256', CAST(NEWID() AS varchar(36))), 2)"),
                    AccessToken = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    SubmitterNotes = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    AdminMessage = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    AdminNotes = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    NotesStatus = table.Column<int>(type: "int", nullable: false),
                    SubmitterId = table.Column<int>(type: "int", nullable: false),
                    AddressId = table.Column<int>(type: "int", nullable: false),
                    PlanId = table.Column<int>(type: "int", nullable: true),
                    ScheduleId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Submissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Submissions_Addresses_AddressId",
                        column: x => x.AddressId,
                        principalTable: "Addresses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Submissions_Plans_PlanId",
                        column: x => x.PlanId,
                        principalTable: "Plans",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Submissions_Schedules_ScheduleId",
                        column: x => x.ScheduleId,
                        principalTable: "Schedules",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Submissions_Submitters_SubmitterId",
                        column: x => x.SubmitterId,
                        principalTable: "Submitters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AgendaUser",
                columns: table => new
                {
                    AssignedAgendasId = table.Column<int>(type: "int", nullable: false),
                    AssignedUsersId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AgendaUser", x => new { x.AssignedAgendasId, x.AssignedUsersId });
                    table.ForeignKey(
                        name: "FK_AgendaUser_Agendas_AssignedAgendasId",
                        column: x => x.AssignedAgendasId,
                        principalTable: "Agendas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AgendaUser_Users_AssignedUsersId",
                        column: x => x.AssignedUsersId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BuildingAssignments",
                columns: table => new
                {
                    AgendaId = table.Column<int>(type: "int", nullable: false),
                    BuildingId = table.Column<int>(type: "int", nullable: false),
                    ScheduleId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuildingAssignments", x => new { x.AgendaId, x.BuildingId, x.ScheduleId });
                    table.ForeignKey(
                        name: "FK_BuildingAssignments_Agendas_AgendaId",
                        column: x => x.AgendaId,
                        principalTable: "Agendas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BuildingAssignments_Buildings_BuildingId",
                        column: x => x.BuildingId,
                        principalTable: "Buildings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BuildingAssignments_Schedules_ScheduleId",
                        column: x => x.ScheduleId,
                        principalTable: "Schedules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FormSubmissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Surname = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    SubmitterNotes = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    ScheduleName = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Apartment = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    Building = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    StreetSpecifier = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Street = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    City = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Diocese = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Method = table.Column<int>(type: "int", nullable: false),
                    IP = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    SubmitTime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    AuthorId = table.Column<int>(type: "int", nullable: true),
                    SubmissionId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FormSubmissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FormSubmissions_Submissions_SubmissionId",
                        column: x => x.SubmissionId,
                        principalTable: "Submissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FormSubmissions_Users_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "SubmissionSnapshots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UniqueId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AccessToken = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    SubmitterNotes = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    AdminMessage = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    AdminNotes = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    NotesStatus = table.Column<int>(type: "int", nullable: false),
                    Apartment = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    Building = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    StreetSpecifier = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Street = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    City = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Diocese = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ChangeTime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    ChangeAuthorId = table.Column<int>(type: "int", nullable: true),
                    SubmissionId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubmissionSnapshots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubmissionSnapshots_Submissions_SubmissionId",
                        column: x => x.SubmissionId,
                        principalTable: "Submissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SubmissionSnapshots_Users_ChangeAuthorId",
                        column: x => x.ChangeAuthorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Visits",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrdinalNumber = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    AgendaId = table.Column<int>(type: "int", nullable: true),
                    ScheduleId = table.Column<int>(type: "int", nullable: true),
                    SubmissionId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Visits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Visits_Agendas_AgendaId",
                        column: x => x.AgendaId,
                        principalTable: "Agendas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Visits_Schedules_ScheduleId",
                        column: x => x.ScheduleId,
                        principalTable: "Schedules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Visits_Submissions_SubmissionId",
                        column: x => x.SubmissionId,
                        principalTable: "Submissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VisitSnapshots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrdinalNumber = table.Column<short>(type: "smallint", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ScheduleName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateVisibility = table.Column<bool>(type: "bit", nullable: true),
                    PredictedTime = table.Column<TimeOnly>(type: "time", nullable: true),
                    PredictedTimeVisibility = table.Column<bool>(type: "bit", nullable: true),
                    ChangeTime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    ChangeAuthorId = table.Column<int>(type: "int", nullable: true),
                    VisitId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VisitSnapshots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VisitSnapshots_Users_ChangeAuthorId",
                        column: x => x.ChangeAuthorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_VisitSnapshots_Visits_VisitId",
                        column: x => x.VisitId,
                        principalTable: "Visits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Addresses_BuildingId_ApartmentNumber_ApartmentLetter",
                table: "Addresses",
                columns: new[] { "BuildingId", "ApartmentNumber", "ApartmentLetter" },
                unique: true,
                filter: "[ApartmentNumber] IS NOT NULL AND [ApartmentLetter] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Addresses_CityId",
                table: "Addresses",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_Addresses_StreetId",
                table: "Addresses",
                column: "StreetId");

            migrationBuilder.CreateIndex(
                name: "IX_Agendas_DayId",
                table: "Agendas",
                column: "DayId");

            migrationBuilder.CreateIndex(
                name: "IX_Agendas_ScheduleId",
                table: "Agendas",
                column: "ScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_Agendas_UniqueId",
                table: "Agendas",
                column: "UniqueId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AgendaUser_AssignedUsersId",
                table: "AgendaUser",
                column: "AssignedUsersId");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingAssignments_BuildingId",
                table: "BuildingAssignments",
                column: "BuildingId");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingAssignments_ScheduleId",
                table: "BuildingAssignments",
                column: "ScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_Buildings_StreetId_Number_Letter",
                table: "Buildings",
                columns: new[] { "StreetId", "Number", "Letter" },
                unique: true,
                filter: "[Letter] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Days_PlanId",
                table: "Days",
                column: "PlanId");

            migrationBuilder.CreateIndex(
                name: "IX_FormSubmissions_AuthorId",
                table: "FormSubmissions",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_FormSubmissions_SubmissionId",
                table: "FormSubmissions",
                column: "SubmissionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ParishInfo_Name",
                table: "ParishInfo",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Plans_AuthorId",
                table: "Plans",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_Schedules_Name",
                table: "Schedules",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Schedules_PlanId",
                table: "Schedules",
                column: "PlanId");

            migrationBuilder.CreateIndex(
                name: "IX_Schedules_ShortName",
                table: "Schedules",
                column: "ShortName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Streets_CityId",
                table: "Streets",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_Streets_Name",
                table: "Streets",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Streets_StreetSpecifierId",
                table: "Streets",
                column: "StreetSpecifierId");

            migrationBuilder.CreateIndex(
                name: "IX_StreetSpecifiers_FullName",
                table: "StreetSpecifiers",
                column: "FullName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Submissions_AddressId",
                table: "Submissions",
                column: "AddressId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Submissions_PlanId",
                table: "Submissions",
                column: "PlanId");

            migrationBuilder.CreateIndex(
                name: "IX_Submissions_ScheduleId",
                table: "Submissions",
                column: "ScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_Submissions_SubmitterId",
                table: "Submissions",
                column: "SubmitterId");

            migrationBuilder.CreateIndex(
                name: "IX_Submissions_UniqueId",
                table: "Submissions",
                column: "UniqueId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SubmissionSnapshots_ChangeAuthorId",
                table: "SubmissionSnapshots",
                column: "ChangeAuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_SubmissionSnapshots_SubmissionId",
                table: "SubmissionSnapshots",
                column: "SubmissionId");

            migrationBuilder.CreateIndex(
                name: "IX_Submitters_UniqueId",
                table: "Submitters",
                column: "UniqueId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SubmitterSnapshots_ChangeAuthorId",
                table: "SubmitterSnapshots",
                column: "ChangeAuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_SubmitterSnapshots_SubmitterId",
                table: "SubmitterSnapshots",
                column: "SubmitterId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Visits_AgendaId",
                table: "Visits",
                column: "AgendaId");

            migrationBuilder.CreateIndex(
                name: "IX_Visits_ScheduleId_OrdinalNumber",
                table: "Visits",
                columns: new[] { "ScheduleId", "OrdinalNumber" },
                unique: true,
                filter: "[ScheduleId] IS NOT NULL AND [OrdinalNumber] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Visits_SubmissionId",
                table: "Visits",
                column: "SubmissionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VisitSnapshots_ChangeAuthorId",
                table: "VisitSnapshots",
                column: "ChangeAuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_VisitSnapshots_VisitId",
                table: "VisitSnapshots",
                column: "VisitId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AgendaUser");

            migrationBuilder.DropTable(
                name: "BuildingAssignments");

            migrationBuilder.DropTable(
                name: "FormSubmissions");

            migrationBuilder.DropTable(
                name: "ParishInfo");

            migrationBuilder.DropTable(
                name: "SubmissionSnapshots");

            migrationBuilder.DropTable(
                name: "SubmitterSnapshots");

            migrationBuilder.DropTable(
                name: "VisitSnapshots");

            migrationBuilder.DropTable(
                name: "Visits");

            migrationBuilder.DropTable(
                name: "Agendas");

            migrationBuilder.DropTable(
                name: "Submissions");

            migrationBuilder.DropTable(
                name: "Days");

            migrationBuilder.DropTable(
                name: "Addresses");

            migrationBuilder.DropTable(
                name: "Schedules");

            migrationBuilder.DropTable(
                name: "Submitters");

            migrationBuilder.DropTable(
                name: "Buildings");

            migrationBuilder.DropTable(
                name: "Plans");

            migrationBuilder.DropTable(
                name: "Streets");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Cities");

            migrationBuilder.DropTable(
                name: "StreetSpecifiers");
        }
    }
}
