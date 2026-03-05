using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AccountingSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddSentEmails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Body",
                table: "SentEmails",
                newName: "BodyHtml");

            migrationBuilder.AddColumn<string>(
                name: "Channel",
                table: "SentEmails",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "OutboxDb");

            migrationBuilder.AddColumn<string>(
                name: "ToUserId",
                table: "SentEmails",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Channel",
                table: "SentEmails");

            migrationBuilder.DropColumn(
                name: "ToUserId",
                table: "SentEmails");

            migrationBuilder.RenameColumn(
                name: "BodyHtml",
                table: "SentEmails",
                newName: "Body");
        }
    }
}
