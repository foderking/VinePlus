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
    [Migration("20230101151250_UpdatePosts")]
    partial class UpdatePosts
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.1")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("WebAPI.Models.ForumPost", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Content")
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

                    b.Property<string>("FirstMention")
                        .HasColumnType("text");

                    b.Property<bool>("IsEdited")
                        .HasColumnType("boolean");

                    b.Property<int>("PostNo")
                        .HasColumnType("integer");

                    b.Property<int>("ThreadId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("ThreadId");

                    b.ToTable("Posts");
                });

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

            modelBuilder.Entity("WebAPI.Models.ForumPost", b =>
                {
                    b.HasOne("WebAPI.Models.ForumThread", "Thread")
                        .WithMany("Posts")
                        .HasForeignKey("ThreadId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Thread");
                });

            modelBuilder.Entity("WebAPI.Models.ForumThread", b =>
                {
                    b.Navigation("Posts");
                });
#pragma warning restore 612, 618
        }
    }
}
