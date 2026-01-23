using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SOK.Infrastructure.Persistence.Migrations.Parish
{
    /// <inheritdoc />
    public partial class AddOfficialToAgenda : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsOfficial",
                table: "Agendas",
                type: "bit",
                nullable: false,
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsOfficial",
                table: "Agendas");
        }
    }
}
