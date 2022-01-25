using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace PokerAPI.Migrations
{
    public partial class Removed_UserBan_Renamed_Properties : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Banns_AspNetUsers_ReporterID",
                table: "Banns");

            migrationBuilder.DropTable(
                name: "UsersBanned");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Banns",
                table: "Banns");

            migrationBuilder.DropIndex(
                name: "IX_Banns_ReporterID",
                table: "Banns");

            migrationBuilder.RenameColumn(
                name: "ReporterID",
                table: "Banns",
                newName: "AdUserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Banns",
                table: "Banns",
                columns: new[] { "AdUserId", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_Banns_UserId",
                table: "Banns",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Banns_AspNetUsers_AdUserId",
                table: "Banns",
                column: "AdUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Banns_AspNetUsers_AdUserId",
                table: "Banns");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Banns",
                table: "Banns");

            migrationBuilder.DropIndex(
                name: "IX_Banns_UserId",
                table: "Banns");

            migrationBuilder.RenameColumn(
                name: "AdUserId",
                table: "Banns",
                newName: "ReporterID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Banns",
                table: "Banns",
                columns: new[] { "UserId", "ReporterID" });

            migrationBuilder.CreateTable(
                name: "UsersBanned",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoomId = table.Column<int>(type: "int", nullable: false),
                    Registered = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsersBanned", x => new { x.UserId, x.RoomId });
                    table.ForeignKey(
                        name: "FK_UsersBanned_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UsersBanned_Rooms_RoomId",
                        column: x => x.RoomId,
                        principalTable: "Rooms",
                        principalColumn: "RoomId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Banns_ReporterID",
                table: "Banns",
                column: "ReporterID");

            migrationBuilder.CreateIndex(
                name: "IX_UsersBanned_RoomId",
                table: "UsersBanned",
                column: "RoomId");

            migrationBuilder.AddForeignKey(
                name: "FK_Banns_AspNetUsers_ReporterID",
                table: "Banns",
                column: "ReporterID",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
