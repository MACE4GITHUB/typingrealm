using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TypingRealm.Typing.Infrastructure.Migrations;

public partial class Initial : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "text",
            columns: table => new
            {
                id = table.Column<string>(type: "text", nullable: false),
                value = table.Column<string>(type: "text", nullable: false),
                created_by_user = table.Column<string>(type: "text", nullable: false),
                created_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                is_public = table.Column<bool>(type: "boolean", nullable: false),
                is_archived = table.Column<bool>(type: "boolean", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_text", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "typing_session",
            columns: table => new
            {
                id = table.Column<string>(type: "text", nullable: false),
                created_by_user = table.Column<string>(type: "text", nullable: false),
                created_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_typing_session", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "typing_session_text",
            columns: table => new
            {
                row_id = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                text_id = table.Column<string>(type: "text", nullable: false),
                value = table.Column<string>(type: "text", nullable: false),
                index_in_typing_session = table.Column<int>(type: "integer", nullable: false),
                typing_session_id = table.Column<string>(type: "text", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_typing_session_text", x => x.row_id);
                table.ForeignKey(
                    name: "fk_typing_session_text_text_text_id",
                    column: x => x.text_id,
                    principalTable: "text",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "fk_typing_session_text_typing_session_typing_session_id",
                    column: x => x.typing_session_id,
                    principalTable: "typing_session",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "user_session",
            columns: table => new
            {
                id = table.Column<string>(type: "text", nullable: false),
                user_id = table.Column<string>(type: "text", nullable: false),
                created_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                user_time_zone_offset_minutes = table.Column<int>(type: "integer", nullable: false),
                typing_session_id = table.Column<string>(type: "text", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_user_session", x => x.id);
                table.ForeignKey(
                    name: "fk_user_session_typing_session_typing_session_id",
                    column: x => x.typing_session_id,
                    principalTable: "typing_session",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "text_typing_result",
            columns: table => new
            {
                id = table.Column<string>(type: "text", nullable: false),
                typing_session_text_index = table.Column<int>(type: "integer", nullable: false),
                started_typing_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                submitted_results_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                user_session_id = table.Column<string>(type: "text", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_text_typing_result", x => x.id);
                table.ForeignKey(
                    name: "fk_text_typing_result_user_session_user_session_id",
                    column: x => x.user_session_id,
                    principalTable: "user_session",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "key_press_event",
            columns: table => new
            {
                id = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                order = table.Column<int>(type: "integer", nullable: false),
                index = table.Column<int>(type: "integer", nullable: false),
                key_action = table.Column<int>(type: "integer", nullable: false),
                key = table.Column<string>(type: "text", nullable: false),
                absolute_delay = table.Column<decimal>(type: "numeric", nullable: false),
                text_typing_result_id = table.Column<string>(type: "text", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_key_press_event", x => x.id);
                table.ForeignKey(
                    name: "fk_key_press_event_text_typing_result_text_typing_result_id",
                    column: x => x.text_typing_result_id,
                    principalTable: "text_typing_result",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "ix_key_press_event_order",
            table: "key_press_event",
            column: "order");

        migrationBuilder.CreateIndex(
            name: "ix_key_press_event_text_typing_result_id",
            table: "key_press_event",
            column: "text_typing_result_id");

        migrationBuilder.CreateIndex(
            name: "ix_text_created_by_user",
            table: "text",
            column: "created_by_user");

        migrationBuilder.CreateIndex(
            name: "ix_text_created_utc",
            table: "text",
            column: "created_utc");

        migrationBuilder.CreateIndex(
            name: "ix_text_is_archived",
            table: "text",
            column: "is_archived");

        migrationBuilder.CreateIndex(
            name: "ix_text_is_public",
            table: "text",
            column: "is_public");

        migrationBuilder.CreateIndex(
            name: "ix_text_typing_result_started_typing_utc",
            table: "text_typing_result",
            column: "started_typing_utc");

        migrationBuilder.CreateIndex(
            name: "ix_text_typing_result_submitted_results_utc",
            table: "text_typing_result",
            column: "submitted_results_utc");

        migrationBuilder.CreateIndex(
            name: "ix_text_typing_result_typing_session_text_index",
            table: "text_typing_result",
            column: "typing_session_text_index");

        migrationBuilder.CreateIndex(
            name: "ix_text_typing_result_user_session_id",
            table: "text_typing_result",
            column: "user_session_id");

        migrationBuilder.CreateIndex(
            name: "ix_typing_session_created_by_user",
            table: "typing_session",
            column: "created_by_user");

        migrationBuilder.CreateIndex(
            name: "ix_typing_session_created_utc",
            table: "typing_session",
            column: "created_utc");

        migrationBuilder.CreateIndex(
            name: "ix_typing_session_text_index_in_typing_session_typing_session_",
            table: "typing_session_text",
            columns: new[] { "index_in_typing_session", "typing_session_id" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_typing_session_text_text_id",
            table: "typing_session_text",
            column: "text_id");

        migrationBuilder.CreateIndex(
            name: "ix_typing_session_text_typing_session_id",
            table: "typing_session_text",
            column: "typing_session_id");

        migrationBuilder.CreateIndex(
            name: "ix_user_session_created_utc_user_time_zone_offset_minutes",
            table: "user_session",
            columns: new[] { "created_utc", "user_time_zone_offset_minutes" });

        migrationBuilder.CreateIndex(
            name: "ix_user_session_typing_session_id",
            table: "user_session",
            column: "typing_session_id");

        migrationBuilder.CreateIndex(
            name: "ix_user_session_user_id",
            table: "user_session",
            column: "user_id");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "key_press_event");

        migrationBuilder.DropTable(
            name: "typing_session_text");

        migrationBuilder.DropTable(
            name: "text_typing_result");

        migrationBuilder.DropTable(
            name: "text");

        migrationBuilder.DropTable(
            name: "user_session");

        migrationBuilder.DropTable(
            name: "typing_session");
    }
}
