using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SOK.Infrastructure.Persistence.Migrations.Parish
{
    /// <inheritdoc />
    public partial class FixAccessTokens : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "AccessToken",
                table: "Submissions",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                defaultValueSql: "CONVERT(NVARCHAR(64), HASHBYTES('SHA2_256', CAST(NEWID() AS NVARCHAR(36))), 2)",
                oldClrType: typeof(string),
                oldType: "nvarchar(64)",
                oldMaxLength: 64,
                oldDefaultValueSql: "CONVERT(varchar(64), HASHBYTES('SHA2_256', CAST(NEWID() AS varchar(36))), 2)");

            migrationBuilder.AlterColumn<string>(
                name: "AccessToken",
                table: "Agendas",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                defaultValueSql: "CONVERT(NVARCHAR(64), HASHBYTES('SHA2_256', CAST(NEWID() AS NVARCHAR(36))), 2)",
                oldClrType: typeof(string),
                oldType: "nvarchar(64)",
                oldMaxLength: 64,
                oldDefaultValueSql: "CONVERT(varchar(64), HASHBYTES('SHA2_256', CAST(NEWID() AS varchar(36))), 2)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "AccessToken",
                table: "Submissions",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                defaultValueSql: "CONVERT(varchar(64), HASHBYTES('SHA2_256', CAST(NEWID() AS varchar(36))), 2)",
                oldClrType: typeof(string),
                oldType: "nvarchar(64)",
                oldMaxLength: 64,
                oldDefaultValueSql: "CONVERT(NVARCHAR(64), HASHBYTES('SHA2_256', CAST(NEWID() AS NVARCHAR(36))), 2)");

            migrationBuilder.AlterColumn<string>(
                name: "AccessToken",
                table: "Agendas",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                defaultValueSql: "CONVERT(varchar(64), HASHBYTES('SHA2_256', CAST(NEWID() AS varchar(36))), 2)",
                oldClrType: typeof(string),
                oldType: "nvarchar(64)",
                oldMaxLength: 64,
                oldDefaultValueSql: "CONVERT(NVARCHAR(64), HASHBYTES('SHA2_256', CAST(NEWID() AS NVARCHAR(36))), 2)");
        }
    }
}
