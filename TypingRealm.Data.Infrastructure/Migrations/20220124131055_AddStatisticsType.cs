using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TypingRealm.Data.Infrastructure.Migrations
{
    public partial class AddStatisticsType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "generation_text_type",
                table: "text",
                newName: "text_generation_type");

            migrationBuilder.Sql(@"
UPDATE text
SET text_generation_type = 0
WHERE text_generation_type = 0");

            migrationBuilder.Sql(@"
UPDATE text
SET text_generation_type = 5
WHERE text_generation_type != 0");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "text_generation_type",
                table: "text",
                newName: "generation_text_type");

            migrationBuilder.Sql(@"
UPDATE text
SET generation_text_type = 1
WHERE generation_text_type = 1 or generation_text_type = 3");

            migrationBuilder.Sql(@"
UPDATE text
SET generation_text_type = 2
WHERE generation_text_type = 2 or generation_text_type = 4");

            migrationBuilder.Sql(@"
UPDATE text
SET generation_text_type = 0
WHERE generation_text_type != 1 and generation_text_type != 2 and generation_text_type != 3 and generation_text_type != 4");
        }
    }
}
