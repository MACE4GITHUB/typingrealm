using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TypingRealm.Library.Infrastructure.Migrations;

public partial class AddTechnicalTrackableFieldsToBookDao : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<DateTime>(
            name: "created_at_utc",
            table: "book",
            type: "timestamp with time zone",
            nullable: false,
            defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

        migrationBuilder.AddColumn<string>(
            name: "created_by",
            table: "book",
            type: "text",
            nullable: true);

        migrationBuilder.AddColumn<DateTime>(
            name: "updated_at_utc",
            table: "book",
            type: "timestamp with time zone",
            nullable: false,
            defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

        migrationBuilder.AddColumn<string>(
            name: "updated_by",
            table: "book",
            type: "text",
            nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "created_at_utc",
            table: "book");

        migrationBuilder.DropColumn(
            name: "created_by",
            table: "book");

        migrationBuilder.DropColumn(
            name: "updated_at_utc",
            table: "book");

        migrationBuilder.DropColumn(
            name: "updated_by",
            table: "book");
    }
}
