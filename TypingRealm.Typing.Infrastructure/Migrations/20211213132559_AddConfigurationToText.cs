using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TypingRealm.Typing.Infrastructure.Migrations;

public partial class AddConfigurationToText : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>(
            name: "generation_length",
            table: "text",
            type: "integer",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "generation_should_contain",
            table: "text",
            type: "text",
            nullable: true);

        migrationBuilder.AddColumn<int>(
            name: "generation_text_type",
            table: "text",
            type: "integer",
            nullable: true);

        migrationBuilder.AddColumn<int>(
            name: "text_type",
            table: "text",
            type: "integer",
            nullable: false,
            defaultValue: 2); // Make all old texts "User" by default.
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "generation_length",
            table: "text");

        migrationBuilder.DropColumn(
            name: "generation_should_contain",
            table: "text");

        migrationBuilder.DropColumn(
            name: "generation_text_type",
            table: "text");

        migrationBuilder.DropColumn(
            name: "text_type",
            table: "text");
    }
}
