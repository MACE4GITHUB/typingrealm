using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TypingRealm.Data.Infrastructure.Migrations
{
    public partial class AddLanguageToText : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "language",
                table: "text",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "en"); // Set to English for all old already existing texts.

            migrationBuilder.CreateIndex(
                name: "ix_text_language",
                table: "text",
                column: "language");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_text_language",
                table: "text");

            migrationBuilder.DropColumn(
                name: "language",
                table: "text");
        }
    }
}
