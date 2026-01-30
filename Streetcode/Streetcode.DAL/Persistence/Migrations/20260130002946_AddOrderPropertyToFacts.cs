using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Streetcode.DAL.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderPropertyToFacts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "StreetcodeType",
                schema: "streetcode",
                table: "streetcodes",
                type: "nvarchar(21)",
                maxLength: 21,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<int>(
                name: "Order",
                schema: "streetcode",
                table: "facts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "CoordinateType",
                schema: "add_content",
                table: "coordinates",
                type: "nvarchar(21)",
                maxLength: 21,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Order",
                schema: "streetcode",
                table: "facts");

            migrationBuilder.AlterColumn<string>(
                name: "StreetcodeType",
                schema: "streetcode",
                table: "streetcodes",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(21)",
                oldMaxLength: 21);

            migrationBuilder.AlterColumn<string>(
                name: "CoordinateType",
                schema: "add_content",
                table: "coordinates",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(21)",
                oldMaxLength: 21);
        }
    }
}
