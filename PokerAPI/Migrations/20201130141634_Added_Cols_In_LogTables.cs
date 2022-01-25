using Microsoft.EntityFrameworkCore.Migrations;

namespace PokerAPI.Migrations
{
    public partial class Added_Cols_In_LogTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "GameFeeCharge",
                table: "RoundWinners",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<double>(
                name: "FeePercentage",
                table: "Rooms",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GameFeeCharge",
                table: "RoundWinners");

            migrationBuilder.DropColumn(
                name: "FeePercentage",
                table: "Rooms");
        }
    }
}
