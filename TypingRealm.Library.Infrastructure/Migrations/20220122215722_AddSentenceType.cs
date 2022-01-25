using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TypingRealm.Library.Infrastructure.Migrations;

public partial class AddSentenceType : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>(
            name: "type",
            table: "sentence",
            type: "integer",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.CreateIndex(
            name: "ix_sentence_type",
            table: "sentence",
            column: "type");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "ix_sentence_type",
            table: "sentence");

        migrationBuilder.DropColumn(
            name: "type",
            table: "sentence");
    }
}
