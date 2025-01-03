﻿// <auto-generated />
using System;
using Comicvine.Core;
using VinePlus.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace VinePlus.Database.Migrations
{
    [DbContext(typeof(ComicvineContext))]
    [Migration("20230426200040_minor change in thread schema")]
    partial class minorchangeinthreadschema
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.3")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

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
                        .HasColumnType("timestamp with time zone")
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

                    b.Property<int[]>("Posts")
                        .HasColumnType("integer[]")
                        .HasColumnName("posts");

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

                    b.ToTable("threads", (string)null);
                });
#pragma warning restore 612, 618
        }
    }
}
