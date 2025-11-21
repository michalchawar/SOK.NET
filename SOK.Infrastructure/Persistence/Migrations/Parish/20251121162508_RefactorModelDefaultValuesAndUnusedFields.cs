using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SOK.Infrastructure.Persistence.Migrations.Parish
{
    /// <inheritdoc />
    public partial class RefactorModelDefaultValuesAndUnusedFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Buildings_StreetId_Number_Letter",
                table: "Buildings");

            migrationBuilder.DropIndex(
                name: "IX_Addresses_BuildingId_ApartmentNumber_ApartmentLetter",
                table: "Addresses");

            migrationBuilder.DropColumn(
                name: "Diocese",
                table: "SubmissionSnapshots");

            migrationBuilder.CreateIndex(
                name: "IX_Buildings_StreetId_Number_Letter",
                table: "Buildings",
                columns: new[] { "StreetId", "Number", "Letter" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Addresses_BuildingId_ApartmentNumber_ApartmentLetter",
                table: "Addresses",
                columns: new[] { "BuildingId", "ApartmentNumber", "ApartmentLetter" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Buildings_StreetId_Number_Letter",
                table: "Buildings");

            migrationBuilder.DropIndex(
                name: "IX_Addresses_BuildingId_ApartmentNumber_ApartmentLetter",
                table: "Addresses");

            migrationBuilder.AddColumn<string>(
                name: "Diocese",
                table: "SubmissionSnapshots",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Buildings_StreetId_Number_Letter",
                table: "Buildings",
                columns: new[] { "StreetId", "Number", "Letter" },
                unique: true,
                filter: "[Letter] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Addresses_BuildingId_ApartmentNumber_ApartmentLetter",
                table: "Addresses",
                columns: new[] { "BuildingId", "ApartmentNumber", "ApartmentLetter" },
                unique: true,
                filter: "[ApartmentNumber] IS NOT NULL AND [ApartmentLetter] IS NOT NULL");
        }
    }
}
