﻿// <auto-generated />
using System;
using ComicVine.API.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ComicVine.API.Migrations
{
    [DbContext(typeof(ForumContext))]
    [Migration("20221230000613_removeComicvineuserReference")]
    partial class removeComicvineuserReference
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.1")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("WebAPI.Models.ForumThread", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("BoardLink")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("BoardName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("CreatorLink")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("CreatorName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("DateCreated")
                        .HasColumnType("timestamp with time zone");

                    b.Property<bool>("IsLocked")
                        .HasColumnType("boolean");

                    b.Property<bool>("IsPinned")
                        .HasColumnType("boolean");

                    b.Property<int>("MostRecentPostNo")
                        .HasColumnType("integer");

                    b.Property<int>("MostRecentPostPage")
                        .HasColumnType("integer");

                    b.Property<int>("NoPost")
                        .HasColumnType("integer");

                    b.Property<int>("NoViews")
                        .HasColumnType("integer");

                    b.Property<string>("ThreadLink")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("ThreadTitle")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("ThreadType")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Threads");
                });
#pragma warning restore 612, 618
        }
    }
}
