using Microsoft.EntityFrameworkCore.Migrations;

namespace PokerAPI.Migrations
{
    public partial class Renamed_Col_Reports : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reports_AspNetUsers_ReporterId",
                table: "Reports");

            migrationBuilder.RenameColumn(
                name: "ReporterId",
                table: "Reports",
                newName: "LastReporterId");

            migrationBuilder.RenameIndex(
                name: "IX_Reports_ReporterId",
                table: "Reports",
                newName: "IX_Reports_LastReporterId");

            migrationBuilder.AddForeignKey(
                name: "FK_Reports_AspNetUsers_LastReporterId",
                table: "Reports",
                column: "LastReporterId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reports_AspNetUsers_LastReporterId",
                table: "Reports");

            migrationBuilder.RenameColumn(
                name: "LastReporterId",
                table: "Reports",
                newName: "ReporterId");

            migrationBuilder.RenameIndex(
                name: "IX_Reports_LastReporterId",
                table: "Reports",
                newName: "IX_Reports_ReporterId");

            migrationBuilder.AddForeignKey(
                name: "FK_Reports_AspNetUsers_ReporterId",
                table: "Reports",
                column: "ReporterId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
