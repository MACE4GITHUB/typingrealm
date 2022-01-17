using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TypingRealm.Library.Infrastructure.Migrations
{
    public partial class ChangeIsProcessedIntoStatusEnum : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "processing_status",
                table: "book",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.Sql(@"
UPDATE book
SET processing_status = 3
WHERE is_processed
");

            migrationBuilder.DropIndex(
                name: "ix_book_is_processed",
                table: "book");

            migrationBuilder.DropColumn(
                name: "is_processed",
                table: "book");

            migrationBuilder.CreateIndex(
                name: "ix_book_processing_status",
                table: "book",
                column: "processing_status");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_processed",
                table: "book",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.Sql(@"
UPDATE book
SET is_processed TRUE
WHERE processing_status = 3
");

            migrationBuilder.DropIndex(
                name: "ix_book_processing_status",
                table: "book");

            migrationBuilder.DropColumn(
                name: "processing_status",
                table: "book");

            migrationBuilder.CreateIndex(
                name: "ix_book_is_processed",
                table: "book",
                column: "is_processed");
        }
    }
}
