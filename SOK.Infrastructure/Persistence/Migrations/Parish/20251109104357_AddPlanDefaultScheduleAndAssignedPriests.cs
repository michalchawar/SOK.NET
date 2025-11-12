using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SOK.Infrastructure.Persistence.Migrations.Parish
{
    /// <inheritdoc />
    public partial class AddPlanDefaultScheduleAndAssignedPriests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DefaultScheduleId",
                table: "Plans",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ParishMemberPlan",
                columns: table => new
                {
                    AssignedPlansId = table.Column<int>(type: "int", nullable: false),
                    AssignedPriestsId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParishMemberPlan", x => new { x.AssignedPlansId, x.AssignedPriestsId });
                    table.ForeignKey(
                        name: "FK_ParishMemberPlan_Members_AssignedPriestsId",
                        column: x => x.AssignedPriestsId,
                        principalTable: "Members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ParishMemberPlan_Plans_AssignedPlansId",
                        column: x => x.AssignedPlansId,
                        principalTable: "Plans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Plans_DefaultScheduleId",
                table: "Plans",
                column: "DefaultScheduleId",
                unique: true,
                filter: "[DefaultScheduleId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ParishMemberPlan_AssignedPriestsId",
                table: "ParishMemberPlan",
                column: "AssignedPriestsId");

            migrationBuilder.AddForeignKey(
                name: "FK_Plans_Schedules_DefaultScheduleId",
                table: "Plans",
                column: "DefaultScheduleId",
                principalTable: "Schedules",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Plans_Schedules_DefaultScheduleId",
                table: "Plans");

            migrationBuilder.DropTable(
                name: "ParishMemberPlan");

            migrationBuilder.DropIndex(
                name: "IX_Plans_DefaultScheduleId",
                table: "Plans");

            migrationBuilder.DropColumn(
                name: "DefaultScheduleId",
                table: "Plans");
        }
    }
}
