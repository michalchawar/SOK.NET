using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SOK.Infrastructure.Persistence.Migrations.Parish
{
    /// <inheritdoc />
    public partial class ChangeAssignedToActivePriestsInPlan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ParishMemberPlan_Members_AssignedPriestsId",
                table: "ParishMemberPlan");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ParishMemberPlan",
                table: "ParishMemberPlan");

            migrationBuilder.DropIndex(
                name: "IX_ParishMemberPlan_AssignedPriestsId",
                table: "ParishMemberPlan");

            migrationBuilder.RenameColumn(
                name: "AssignedPriestsId",
                table: "ParishMemberPlan",
                newName: "ActivePriestsId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ParishMemberPlan",
                table: "ParishMemberPlan",
                columns: new[] { "ActivePriestsId", "AssignedPlansId" });

            migrationBuilder.CreateIndex(
                name: "IX_ParishMemberPlan_AssignedPlansId",
                table: "ParishMemberPlan",
                column: "AssignedPlansId");

            migrationBuilder.AddForeignKey(
                name: "FK_ParishMemberPlan_Members_ActivePriestsId",
                table: "ParishMemberPlan",
                column: "ActivePriestsId",
                principalTable: "Members",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ParishMemberPlan_Members_ActivePriestsId",
                table: "ParishMemberPlan");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ParishMemberPlan",
                table: "ParishMemberPlan");

            migrationBuilder.DropIndex(
                name: "IX_ParishMemberPlan_AssignedPlansId",
                table: "ParishMemberPlan");

            migrationBuilder.RenameColumn(
                name: "ActivePriestsId",
                table: "ParishMemberPlan",
                newName: "AssignedPriestsId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ParishMemberPlan",
                table: "ParishMemberPlan",
                columns: new[] { "AssignedPlansId", "AssignedPriestsId" });

            migrationBuilder.CreateIndex(
                name: "IX_ParishMemberPlan_AssignedPriestsId",
                table: "ParishMemberPlan",
                column: "AssignedPriestsId");

            migrationBuilder.AddForeignKey(
                name: "FK_ParishMemberPlan_Members_AssignedPriestsId",
                table: "ParishMemberPlan",
                column: "AssignedPriestsId",
                principalTable: "Members",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
