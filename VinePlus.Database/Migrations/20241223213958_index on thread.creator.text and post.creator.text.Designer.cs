﻿// <auto-generated />
using System;
using Comicvine.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using VinePlus.Database;

#nullable disable

namespace VinePlus.Database.Migrations
{
    [DbContext(typeof(ComicvineContext))]
    [Migration("20241223213958_index on thread.creator.text and post.creator.text")]
    partial class indexonthreadcreatortextandpostcreatortext
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.3")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Comicvine.Core.Parsers+Post", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("text")
                        .HasColumnName("id");

                    b.Property<string>("Content")
                        .HasColumnType("text")
                        .HasColumnName("content");

                    b.Property<DateTime>("Created")
                        .HasColumnType("timestamp without time zone")
                        .HasColumnName("created");

                    b.Property<Parsers.Link>("Creator")
                        .HasColumnType("jsonb")
                        .HasColumnName("creator");

                    b.Property<bool>("IsComment")
                        .HasColumnType("boolean")
                        .HasColumnName("is_comment");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("boolean")
                        .HasColumnName("is_deleted");

                    b.Property<bool>("IsEdited")
                        .HasColumnType("boolean")
                        .HasColumnName("is_edited");

                    b.Property<bool>("IsModComment")
                        .HasColumnType("boolean")
                        .HasColumnName("is_mod_comment");

                    b.Property<int>("PostNo")
                        .HasColumnType("integer")
                        .HasColumnName("post_no");

                    b.Property<int>("ThreadId")
                        .HasColumnType("integer")
                        .HasColumnName("thread_id");

                    b.HasKey("Id")
                        .HasName("pk_posts");

                    b.HasIndex("Created")
                        .IsDescending()
                        .HasDatabaseName("ix_posts_created");

                    b.HasIndex("ThreadId")
                        .HasDatabaseName("ix_posts_thread_id");

                    b.ToTable("posts", (string)null);
                });

            modelBuilder.Entity("Comicvine.Core.Parsers+Thread", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<Parsers.Link>("Board")
                        .HasColumnType("jsonb")
                        .HasColumnName("board");

                    b.Property<DateTime>("Created")
                        .HasColumnType("timestamp without time zone")
                        .HasColumnName("created");

                    b.Property<Parsers.Link>("Creator")
                        .HasColumnType("jsonb")
                        .HasColumnName("creator");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("boolean")
                        .HasColumnName("is_deleted");

                    b.Property<bool>("IsLocked")
                        .HasColumnType("boolean")
                        .HasColumnName("is_locked");

                    b.Property<bool>("IsPinned")
                        .HasColumnType("boolean")
                        .HasColumnName("is_pinned");

                    b.Property<int>("LastPostNo")
                        .HasColumnType("integer")
                        .HasColumnName("last_post_no");

                    b.Property<int>("LastPostPage")
                        .HasColumnType("integer")
                        .HasColumnName("last_post_page");

                    b.Property<Parsers.Link>("Thread")
                        .HasColumnType("jsonb")
                        .HasColumnName("thread");

                    b.Property<int>("TotalPosts")
                        .HasColumnType("integer")
                        .HasColumnName("total_posts");

                    b.Property<int>("TotalView")
                        .HasColumnType("integer")
                        .HasColumnName("total_view");

                    b.Property<int>("Type")
                        .HasColumnType("integer")
                        .HasColumnName("type");

                    b.HasKey("Id")
                        .HasName("pk_threads");

                    b.HasIndex("Created")
                        .IsDescending()
                        .HasDatabaseName("ix_threads_created");

                    b.ToTable("threads", (string)null);
                });

            modelBuilder.Entity("Comicvine.Core.Parsers+Post", b =>
                {
                    b.HasOne("Comicvine.Core.Parsers+Thread", null)
                        .WithMany("Posts")
                        .HasForeignKey("ThreadId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_posts_threads_thread_id");
                });

            modelBuilder.Entity("Comicvine.Core.Parsers+Thread", b =>
                {
                    b.Navigation("Posts");
                });
#pragma warning restore 612, 618
        }
    }
}