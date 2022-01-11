using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TypingRealm.Library.Infrastructure.Migrations
{
    public partial class AddLanguageToBook : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "language",
                table: "book",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "en");

            migrationBuilder.CreateIndex(
                name: "ix_book_language",
                table: "book",
                column: "language");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_book_language",
                table: "book");

            migrationBuilder.DropColumn(
                name: "language",
                table: "book");
        }
    }
}
