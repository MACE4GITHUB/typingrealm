using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TypingRealm.Library.Infrastructure.Migrations
{
    public partial class SplitBookInfoAndContent : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "book_content",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    content = table.Column<string>(type: "text", nullable: false),
                    book_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_book_content", x => x.id);
                });

            migrationBuilder.AddColumn<string>(
                name: "content_id",
                table: "book",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql(@"
INSERT INTO book_content (id, book_id, content)
SELECT id, id, content
FROM book");

            migrationBuilder.Sql(@"
UPDATE book
SET content_id = id");

            migrationBuilder.DropColumn(
                name: "content",
                table: "book");

            migrationBuilder.CreateIndex(
                name: "ix_book_content_id",
                table: "book",
                column: "content_id");

            migrationBuilder.CreateIndex(
                name: "ix_book_content_book_id",
                table: "book_content",
                column: "book_id");

            migrationBuilder.AddForeignKey(
                name: "fk_book_book_content_content_id",
                table: "book",
                column: "content_id",
                principalTable: "book_content",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_book_book_content_content_id",
                table: "book");

            migrationBuilder.DropTable(
                name: "book_content");

            migrationBuilder.DropIndex(
                name: "ix_book_content_id",
                table: "book");

            migrationBuilder.DropColumn(
                name: "content_id",
                table: "book");

            migrationBuilder.AddColumn<string>(
                name: "content",
                table: "book",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
