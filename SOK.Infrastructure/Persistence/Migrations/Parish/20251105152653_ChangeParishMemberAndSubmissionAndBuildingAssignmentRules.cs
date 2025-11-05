using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SOK.Infrastructure.Persistence.Migrations.Parish
{
    /// <inheritdoc />
    public partial class ChangeParishMemberAndSubmissionAndBuildingAssignmentRules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BuildingAssignments_Agendas_AgendaId",
                table: "BuildingAssignments");

            migrationBuilder.RenameColumn(
                name: "AgendaId",
                table: "BuildingAssignments",
                newName: "DayId");

            migrationBuilder.AlterColumn<int>(
                name: "PlanId",
                table: "Submissions",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DisplayName",
                table: "Members",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_BuildingAssignments_Days_DayId",
                table: "BuildingAssignments",
                column: "DayId",
                principalTable: "Days",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BuildingAssignments_Days_DayId",
                table: "BuildingAssignments");

            migrationBuilder.DropColumn(
                name: "DisplayName",
                table: "Members");

            migrationBuilder.RenameColumn(
                name: "DayId",
                table: "BuildingAssignments",
                newName: "AgendaId");

            migrationBuilder.AlterColumn<int>(
                name: "PlanId",
                table: "Submissions",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_BuildingAssignments_Agendas_AgendaId",
                table: "BuildingAssignments",
                column: "AgendaId",
                principalTable: "Agendas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
