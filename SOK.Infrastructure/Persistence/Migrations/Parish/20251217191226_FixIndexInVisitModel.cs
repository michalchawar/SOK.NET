using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SOK.Infrastructure.Persistence.Migrations.Parish
{
    /// <inheritdoc />
    public partial class FixIndexInVisitModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Visits_AgendaId",
                table: "Visits");

            migrationBuilder.DropIndex(
                name: "IX_Visits_ScheduleId_OrdinalNumber",
                table: "Visits");

            migrationBuilder.CreateIndex(
                name: "IX_Visits_AgendaId_OrdinalNumber",
                table: "Visits",
                columns: new[] { "AgendaId", "OrdinalNumber" },
                unique: true,
                filter: "[AgendaId] IS NOT NULL AND [OrdinalNumber] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Visits_ScheduleId",
                table: "Visits",
                column: "ScheduleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Visits_AgendaId_OrdinalNumber",
                table: "Visits");

            migrationBuilder.DropIndex(
                name: "IX_Visits_ScheduleId",
                table: "Visits");

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
        }
    }
}
