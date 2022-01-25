using Microsoft.EntityFrameworkCore.Migrations;

namespace PokerAPI.Migrations
{
    public partial class Added_Cols_LogError_Table : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Class",
                table: "LogErrors",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "LogErrors",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Line",
                table: "LogErrors",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Method",
                table: "LogErrors",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Class",
                table: "LogErrors");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "LogErrors");

            migrationBuilder.DropColumn(
                name: "Line",
                table: "LogErrors");

            migrationBuilder.DropColumn(
                name: "Method",
                table: "LogErrors");
        }
    }
}
