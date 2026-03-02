using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Streetcode.DAL.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRepliesAndModerationToComments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ParentId",
                schema: "streetcode",
                table: "comments",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                schema: "streetcode",
                table: "comments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_comments_ParentId",
                schema: "streetcode",
                table: "comments",
                column: "ParentId");

            migrationBuilder.AddForeignKey(
                name: "FK_comments_comments_ParentId",
                schema: "streetcode",
                table: "comments",
                column: "ParentId",
                principalSchema: "streetcode",
                principalTable: "comments",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_comments_comments_ParentId",
                schema: "streetcode",
                table: "comments");

            migrationBuilder.DropIndex(
                name: "IX_comments_ParentId",
                schema: "streetcode",
                table: "comments");

            migrationBuilder.DropColumn(
                name: "ParentId",
                schema: "streetcode",
                table: "comments");

            migrationBuilder.DropColumn(
                name: "Status",
                schema: "streetcode",
                table: "comments");
        }
    }
}
