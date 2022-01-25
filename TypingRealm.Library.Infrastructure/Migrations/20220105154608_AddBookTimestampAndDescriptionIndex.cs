using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TypingRealm.Library.Infrastructure.Migrations;

public partial class AddBookTimestampAndDescriptionIndex : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<string>(
            name: "description",
            table: "book",
            type: "character varying(100)",
            maxLength: 100,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "text");

        migrationBuilder.AddColumn<DateTime>(
            name: "added_at_utc",
            table: "book",
            type: "timestamp with time zone",
            nullable: false,
            defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

        migrationBuilder.CreateIndex(
            name: "ix_book_added_at_utc",
            table: "book",
            column: "added_at_utc");

        migrationBuilder.CreateIndex(
            name: "ix_book_description",
            table: "book",
            column: "description");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "ix_book_added_at_utc",
            table: "book");

        migrationBuilder.DropIndex(
            name: "ix_book_description",
            table: "book");

        migrationBuilder.DropColumn(
            name: "added_at_utc",
            table: "book");

        migrationBuilder.AlterColumn<string>(
            name: "description",
            table: "book",
            type: "text",
            nullable: false,
            oldClrType: typeof(string),
            oldType: "character varying(100)",
            oldMaxLength: 100);
    }
}
