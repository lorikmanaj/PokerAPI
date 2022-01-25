using Microsoft.EntityFrameworkCore.Migrations;

namespace PokerAPI.Migrations
{
    public partial class Renamed_Cols_Table : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "AddressType",
                table: "IpRecords",
                newName: "MacAddress");

            migrationBuilder.AddColumn<bool>(
                name: "BadIp",
                table: "IpRecords",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BadIp",
                table: "IpRecords");

            migrationBuilder.RenameColumn(
                name: "MacAddress",
                table: "IpRecords",
                newName: "AddressType");
        }
    }
}
