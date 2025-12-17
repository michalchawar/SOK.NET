using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SOK.Infrastructure.Persistence.Migrations.Parish
{
    /// <inheritdoc />
    public partial class AddAutoAssignFlagForBuildingAssignmentAndMultiSubmissionsForAddress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Submissions_AddressId",
                table: "Submissions");

            migrationBuilder.AddColumn<bool>(
                name: "EnableAutoAssign",
                table: "BuildingAssignments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Submissions_AddressId",
                table: "Submissions",
                column: "AddressId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Submissions_AddressId",
                table: "Submissions");

            migrationBuilder.DropColumn(
                name: "EnableAutoAssign",
                table: "BuildingAssignments");

            migrationBuilder.CreateIndex(
                name: "IX_Submissions_AddressId",
                table: "Submissions",
                column: "AddressId",
                unique: true);
        }
    }
}
