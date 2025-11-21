using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SOK.Infrastructure.Migrations.Central
{
    /// <inheritdoc />
    public partial class RefactorModelDefaultValuesAndUnusedFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MigrationVersion",
                table: "Parishes");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MigrationVersion",
                table: "Parishes",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
