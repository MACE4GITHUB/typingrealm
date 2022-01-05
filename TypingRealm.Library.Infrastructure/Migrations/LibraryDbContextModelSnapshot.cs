﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using TypingRealm.Library.Infrastructure.DataAccess;

#nullable disable

namespace TypingRealm.Library.Infrastructure.Migrations
{
    [DbContext(typeof(LibraryDbContext))]
    partial class LibraryDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("TypingRealm.Library.Infrastructure.DataAccess.Entities.BookDao", b =>
                {
                    b.Property<string>("Id")
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)")
                        .HasColumnName("id");

                    b.Property<string>("Content")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("content");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("description");

                    b.Property<bool>("IsArchived")
                        .HasColumnType("boolean")
                        .HasColumnName("is_archived");

                    b.Property<bool>("IsProcessed")
                        .HasColumnType("boolean")
                        .HasColumnName("is_processed");

                    b.HasKey("Id")
                        .HasName("pk_book");

                    b.HasIndex("IsArchived")
                        .HasDatabaseName("ix_book_is_archived");

                    b.HasIndex("IsProcessed")
                        .HasDatabaseName("ix_book_is_processed");

                    b.ToTable("book", (string)null);
                });

            modelBuilder.Entity("TypingRealm.Library.Infrastructure.DataAccess.Entities.KeyPairDao", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<int>("CountInSentence")
                        .HasColumnType("integer")
                        .HasColumnName("count_in_sentence");

                    b.Property<int>("CountInWord")
                        .HasColumnType("integer")
                        .HasColumnName("count_in_word");

                    b.Property<int>("IndexInWord")
                        .HasColumnType("integer")
                        .HasColumnName("index_in_word");

                    b.Property<string>("Value")
                        .IsRequired()
                        .HasMaxLength(10)
                        .HasColumnType("character varying(10)")
                        .HasColumnName("value");

                    b.Property<long>("WordId")
                        .HasColumnType("bigint")
                        .HasColumnName("word_id");

                    b.HasKey("Id")
                        .HasName("pk_key_pair");

                    b.HasIndex("CountInSentence")
                        .HasDatabaseName("ix_key_pair_count_in_sentence");

                    b.HasIndex("CountInWord")
                        .HasDatabaseName("ix_key_pair_count_in_word");

                    b.HasIndex("IndexInWord")
                        .HasDatabaseName("ix_key_pair_index_in_word");

                    b.HasIndex("Value")
                        .HasDatabaseName("ix_key_pair_value");

                    b.HasIndex("WordId")
                        .HasDatabaseName("ix_key_pair_word_id");

                    b.ToTable("key_pair", (string)null);
                });

            modelBuilder.Entity("TypingRealm.Library.Infrastructure.DataAccess.Entities.SentenceDao", b =>
                {
                    b.Property<string>("Id")
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)")
                        .HasColumnName("id");

                    b.Property<string>("BookId")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)")
                        .HasColumnName("book_id");

                    b.Property<int>("IndexInBook")
                        .HasColumnType("integer")
                        .HasColumnName("index_in_book");

                    b.Property<string>("Value")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("value");

                    b.HasKey("Id")
                        .HasName("pk_sentence");

                    b.HasIndex("BookId")
                        .HasDatabaseName("ix_sentence_book_id");

                    b.HasIndex("IndexInBook")
                        .HasDatabaseName("ix_sentence_index_in_book");

                    b.ToTable("sentence", (string)null);
                });

            modelBuilder.Entity("TypingRealm.Library.Infrastructure.DataAccess.Entities.WordDao", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<int>("CountInSentence")
                        .HasColumnType("integer")
                        .HasColumnName("count_in_sentence");

                    b.Property<int>("IndexInSentence")
                        .HasColumnType("integer")
                        .HasColumnName("index_in_sentence");

                    b.Property<string>("SentenceId")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)")
                        .HasColumnName("sentence_id");

                    b.Property<string>("Value")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)")
                        .HasColumnName("value");

                    b.HasKey("Id")
                        .HasName("pk_word");

                    b.HasIndex("CountInSentence")
                        .HasDatabaseName("ix_word_count_in_sentence");

                    b.HasIndex("IndexInSentence")
                        .HasDatabaseName("ix_word_index_in_sentence");

                    b.HasIndex("SentenceId")
                        .HasDatabaseName("ix_word_sentence_id");

                    b.HasIndex("Value")
                        .HasDatabaseName("ix_word_value");

                    b.ToTable("word", (string)null);
                });

            modelBuilder.Entity("TypingRealm.Library.Infrastructure.DataAccess.Entities.KeyPairDao", b =>
                {
                    b.HasOne("TypingRealm.Library.Infrastructure.DataAccess.Entities.WordDao", "Word")
                        .WithMany("KeyPairs")
                        .HasForeignKey("WordId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_key_pair_word_word_id");

                    b.Navigation("Word");
                });

            modelBuilder.Entity("TypingRealm.Library.Infrastructure.DataAccess.Entities.SentenceDao", b =>
                {
                    b.HasOne("TypingRealm.Library.Infrastructure.DataAccess.Entities.BookDao", "Book")
                        .WithMany()
                        .HasForeignKey("BookId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_sentence_book_book_id");

                    b.Navigation("Book");
                });

            modelBuilder.Entity("TypingRealm.Library.Infrastructure.DataAccess.Entities.WordDao", b =>
                {
                    b.HasOne("TypingRealm.Library.Infrastructure.DataAccess.Entities.SentenceDao", "Sentence")
                        .WithMany("Words")
                        .HasForeignKey("SentenceId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_word_sentence_sentence_id");

                    b.Navigation("Sentence");
                });

            modelBuilder.Entity("TypingRealm.Library.Infrastructure.DataAccess.Entities.SentenceDao", b =>
                {
                    b.Navigation("Words");
                });

            modelBuilder.Entity("TypingRealm.Library.Infrastructure.DataAccess.Entities.WordDao", b =>
                {
                    b.Navigation("KeyPairs");
                });
#pragma warning restore 612, 618
        }
    }
}