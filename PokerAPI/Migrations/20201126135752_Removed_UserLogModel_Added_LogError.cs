using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace PokerAPI.Migrations
{
    public partial class Removed_UserLogModel_Added_LogError : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RoundUsersLogs_UsersLogModels_UserLogId",
                table: "RoundUsersLogs");

            migrationBuilder.DropTable(
                name: "UsersLogModels");

            migrationBuilder.DropIndex(
                name: "IX_RoundUsersLogs_UserLogId",
                table: "RoundUsersLogs");

            migrationBuilder.DropColumn(
                name: "UserLogId",
                table: "RoundUsersLogs");

            migrationBuilder.AddColumn<string>(
                name: "FullName",
                table: "RoundUsersLogs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "LogErrors",
                columns: table => new
                {
                    LogErrorId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DateOccurred = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LogErrors", x => x.LogErrorId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LogErrors");

            migrationBuilder.DropColumn(
                name: "FullName",
                table: "RoundUsersLogs");

            migrationBuilder.AddColumn<int>(
                name: "UserLogId",
                table: "RoundUsersLogs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "UsersLogModels",
                columns: table => new
                {
                    UserLogId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PlayerId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsersLogModels", x => x.UserLogId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RoundUsersLogs_UserLogId",
                table: "RoundUsersLogs",
                column: "UserLogId");

            migrationBuilder.AddForeignKey(
                name: "FK_RoundUsersLogs_UsersLogModels_UserLogId",
                table: "RoundUsersLogs",
                column: "UserLogId",
                principalTable: "UsersLogModels",
                principalColumn: "UserLogId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
