using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TypingRealm.Library.Infrastructure.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "book",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    content = table.Column<string>(type: "text", nullable: false),
                    is_processed = table.Column<bool>(type: "boolean", nullable: false),
                    is_archived = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_book", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "sentence",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    book_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    index_in_book = table.Column<int>(type: "integer", nullable: false),
                    value = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_sentence", x => x.id);
                    table.ForeignKey(
                        name: "fk_sentence_book_book_id",
                        column: x => x.book_id,
                        principalTable: "book",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "word",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    sentence_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    index_in_sentence = table.Column<int>(type: "integer", nullable: false),
                    value = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    raw_value = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    count_in_sentence = table.Column<int>(type: "integer", nullable: false),
                    raw_count_in_sentence = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_word", x => x.id);
                    table.ForeignKey(
                        name: "fk_word_sentence_sentence_id",
                        column: x => x.sentence_id,
                        principalTable: "sentence",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "key_pair",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    index_in_word = table.Column<int>(type: "integer", nullable: false),
                    value = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    count_in_word = table.Column<int>(type: "integer", nullable: false),
                    count_in_sentence = table.Column<int>(type: "integer", nullable: false),
                    word_id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_key_pair", x => x.id);
                    table.ForeignKey(
                        name: "fk_key_pair_word_word_id",
                        column: x => x.word_id,
                        principalTable: "word",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_book_is_archived",
                table: "book",
                column: "is_archived");

            migrationBuilder.CreateIndex(
                name: "ix_book_is_processed",
                table: "book",
                column: "is_processed");

            migrationBuilder.CreateIndex(
                name: "ix_key_pair_count_in_sentence",
                table: "key_pair",
                column: "count_in_sentence");

            migrationBuilder.CreateIndex(
                name: "ix_key_pair_count_in_word",
                table: "key_pair",
                column: "count_in_word");

            migrationBuilder.CreateIndex(
                name: "ix_key_pair_index_in_word",
                table: "key_pair",
                column: "index_in_word");

            migrationBuilder.CreateIndex(
                name: "ix_key_pair_value",
                table: "key_pair",
                column: "value");

            migrationBuilder.CreateIndex(
                name: "ix_key_pair_word_id",
                table: "key_pair",
                column: "word_id");

            migrationBuilder.CreateIndex(
                name: "ix_sentence_book_id",
                table: "sentence",
                column: "book_id");

            migrationBuilder.CreateIndex(
                name: "ix_sentence_index_in_book",
                table: "sentence",
                column: "index_in_book");

            migrationBuilder.CreateIndex(
                name: "ix_word_count_in_sentence",
                table: "word",
                column: "count_in_sentence");

            migrationBuilder.CreateIndex(
                name: "ix_word_index_in_sentence",
                table: "word",
                column: "index_in_sentence");

            migrationBuilder.CreateIndex(
                name: "ix_word_raw_count_in_sentence",
                table: "word",
                column: "raw_count_in_sentence");

            migrationBuilder.CreateIndex(
                name: "ix_word_raw_value",
                table: "word",
                column: "raw_value");

            migrationBuilder.CreateIndex(
                name: "ix_word_sentence_id",
                table: "word",
                column: "sentence_id");

            migrationBuilder.CreateIndex(
                name: "ix_word_value",
                table: "word",
                column: "value");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "key_pair");

            migrationBuilder.DropTable(
                name: "word");

            migrationBuilder.DropTable(
                name: "sentence");

            migrationBuilder.DropTable(
                name: "book");
        }
    }
}
