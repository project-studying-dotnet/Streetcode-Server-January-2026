using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Streetcode.Email.DAL.Migrations
{
    /// <inheritdoc />
    public partial class RenameFeedbackIntoEmail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "Feedbacks",
                newName: "Emails");

            migrationBuilder.RenameColumn(
                name: "Email",
                table: "Emails",
                newName: "From");

            migrationBuilder.RenameColumn(
                name: "Message",
                table: "Emails",
                newName: "Content");

            migrationBuilder.AlterColumn<string>(
        name: "Content",
        table: "Emails",
        type: "nvarchar(1000)",
        maxLength: 1000,
        nullable: false,
        oldClrType: typeof(string),
        oldType: "nvarchar(100)",
        oldMaxLength: 100);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "Emails",
                newName: "Feedbacks");

            migrationBuilder.RenameColumn(
                name: "From",
                table: "Feedbacks",
                newName: "Email");

            migrationBuilder.RenameColumn(
                name: "Content",
                table: "Feedbacks",
                newName: "Message");

            migrationBuilder.AlterColumn<string>(
        name: "Content",
        table: "Emails",
        type: "nvarchar(1000)",
        maxLength: 1000,
        nullable: false,
        oldClrType: typeof(string),
        oldType: "nvarchar(100)",
        oldMaxLength: 100);
        }
    }
}
