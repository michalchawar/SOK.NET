using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SOK.Infrastructure.Persistence.Migrations.Parish
{
    /// <inheritdoc />
    public partial class ChangedParishUserToParishMember : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FormSubmissions_Users_AuthorId",
                table: "FormSubmissions");

            migrationBuilder.DropForeignKey(
                name: "FK_Plans_Users_AuthorId",
                table: "Plans");

            migrationBuilder.DropForeignKey(
                name: "FK_SubmissionSnapshots_Users_ChangeAuthorId",
                table: "SubmissionSnapshots");

            migrationBuilder.DropForeignKey(
                name: "FK_SubmitterSnapshots_Users_ChangeAuthorId",
                table: "SubmitterSnapshots");

            migrationBuilder.DropForeignKey(
                name: "FK_VisitSnapshots_Users_ChangeAuthorId",
                table: "VisitSnapshots");

            migrationBuilder.DropTable(
                name: "AgendaUser");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.CreateTable(
                name: "Members",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CentralUserId = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Members", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AgendaParishMember",
                columns: table => new
                {
                    AssignedAgendasId = table.Column<int>(type: "int", nullable: false),
                    AssignedMembersId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AgendaParishMember", x => new { x.AssignedAgendasId, x.AssignedMembersId });
                    table.ForeignKey(
                        name: "FK_AgendaParishMember_Agendas_AssignedAgendasId",
                        column: x => x.AssignedAgendasId,
                        principalTable: "Agendas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AgendaParishMember_Members_AssignedMembersId",
                        column: x => x.AssignedMembersId,
                        principalTable: "Members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AgendaParishMember_AssignedMembersId",
                table: "AgendaParishMember",
                column: "AssignedMembersId");

            migrationBuilder.CreateIndex(
                name: "IX_Members_CentralUserId",
                table: "Members",
                column: "CentralUserId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_FormSubmissions_Members_AuthorId",
                table: "FormSubmissions",
                column: "AuthorId",
                principalTable: "Members",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Plans_Members_AuthorId",
                table: "Plans",
                column: "AuthorId",
                principalTable: "Members",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_SubmissionSnapshots_Members_ChangeAuthorId",
                table: "SubmissionSnapshots",
                column: "ChangeAuthorId",
                principalTable: "Members",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_SubmitterSnapshots_Members_ChangeAuthorId",
                table: "SubmitterSnapshots",
                column: "ChangeAuthorId",
                principalTable: "Members",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_VisitSnapshots_Members_ChangeAuthorId",
                table: "VisitSnapshots",
                column: "ChangeAuthorId",
                principalTable: "Members",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FormSubmissions_Members_AuthorId",
                table: "FormSubmissions");

            migrationBuilder.DropForeignKey(
                name: "FK_Plans_Members_AuthorId",
                table: "Plans");

            migrationBuilder.DropForeignKey(
                name: "FK_SubmissionSnapshots_Members_ChangeAuthorId",
                table: "SubmissionSnapshots");

            migrationBuilder.DropForeignKey(
                name: "FK_SubmitterSnapshots_Members_ChangeAuthorId",
                table: "SubmitterSnapshots");

            migrationBuilder.DropForeignKey(
                name: "FK_VisitSnapshots_Members_ChangeAuthorId",
                table: "VisitSnapshots");

            migrationBuilder.DropTable(
                name: "AgendaParishMember");

            migrationBuilder.DropTable(
                name: "Members");

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AgendaUser",
                columns: table => new
                {
                    AssignedAgendasId = table.Column<int>(type: "int", nullable: false),
                    AssignedUsersId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AgendaUser", x => new { x.AssignedAgendasId, x.AssignedUsersId });
                    table.ForeignKey(
                        name: "FK_AgendaUser_Agendas_AssignedAgendasId",
                        column: x => x.AssignedAgendasId,
                        principalTable: "Agendas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AgendaUser_Users_AssignedUsersId",
                        column: x => x.AssignedUsersId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AgendaUser_AssignedUsersId",
                table: "AgendaUser",
                column: "AssignedUsersId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_FormSubmissions_Users_AuthorId",
                table: "FormSubmissions",
                column: "AuthorId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Plans_Users_AuthorId",
                table: "Plans",
                column: "AuthorId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_SubmissionSnapshots_Users_ChangeAuthorId",
                table: "SubmissionSnapshots",
                column: "ChangeAuthorId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_SubmitterSnapshots_Users_ChangeAuthorId",
                table: "SubmitterSnapshots",
                column: "ChangeAuthorId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_VisitSnapshots_Users_ChangeAuthorId",
                table: "VisitSnapshots",
                column: "ChangeAuthorId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
