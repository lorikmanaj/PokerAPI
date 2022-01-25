using Microsoft.EntityFrameworkCore.Migrations;

namespace PokerAPI.Migrations
{
    public partial class updatedoubletolong : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "FeePercentage",
                table: "Rooms",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<double>(
                name: "FeePercentage",
                table: "Rooms",
                type: "float",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");
        }
    }
}
