using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SOK.Infrastructure.Persistence.Migrations.Parish
{
    /// <inheritdoc />
    public partial class FixBadIndexesInSchedule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Schedules_Name",
                table: "Schedules");

            migrationBuilder.DropIndex(
                name: "IX_Schedules_PlanId",
                table: "Schedules");

            migrationBuilder.DropIndex(
                name: "IX_Schedules_ShortName",
                table: "Schedules");

            migrationBuilder.CreateIndex(
                name: "IX_Schedules_PlanId_Name",
                table: "Schedules",
                columns: new[] { "PlanId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Schedules_PlanId_ShortName",
                table: "Schedules",
                columns: new[] { "PlanId", "ShortName" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Schedules_PlanId_Name",
                table: "Schedules");

            migrationBuilder.DropIndex(
                name: "IX_Schedules_PlanId_ShortName",
                table: "Schedules");

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
        }
    }
}
