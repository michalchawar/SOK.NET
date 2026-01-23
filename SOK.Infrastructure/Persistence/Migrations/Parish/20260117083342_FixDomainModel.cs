using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SOK.Infrastructure.Persistence.Migrations.Parish
{
    /// <inheritdoc />
    public partial class FixDomainModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Submissions_Schedules_ScheduleId",
                table: "Submissions");

            migrationBuilder.DropIndex(
                name: "IX_Submissions_ScheduleId",
                table: "Submissions");

            migrationBuilder.DropColumn(
                name: "ScheduleId",
                table: "Submissions");

            migrationBuilder.AlterColumn<int>(
                name: "OrdinalNumber",
                table: "VisitSnapshots",
                type: "int",
                nullable: true,
                oldClrType: typeof(short),
                oldType: "smallint");

            migrationBuilder.AlterColumn<DateOnly>(
                name: "Date",
                table: "VisitSnapshots",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<short>(
                name: "OrdinalNumber",
                table: "VisitSnapshots",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "Date",
                table: "VisitSnapshots",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateOnly),
                oldType: "date",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ScheduleId",
                table: "Submissions",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Submissions_ScheduleId",
                table: "Submissions",
                column: "ScheduleId");

            migrationBuilder.AddForeignKey(
                name: "FK_Submissions_Schedules_ScheduleId",
                table: "Submissions",
                column: "ScheduleId",
                principalTable: "Schedules",
                principalColumn: "Id");
        }
    }
}
