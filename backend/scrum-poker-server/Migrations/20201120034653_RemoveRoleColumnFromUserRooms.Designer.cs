﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using scrum_poker_server.Data;

namespace scrum_poker_server.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20201120034653_RemoveRoleColumnFromUserRooms")]
    partial class RemoveRoleColumnFromUserRooms
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .UseIdentityByDefaultColumns()
                .HasAnnotation("Relational:MaxIdentifierLength", 63)
                .HasAnnotation("ProductVersion", "5.0.0");

            modelBuilder.Entity("scrum_poker_server.Models.Account", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .UseIdentityByDefaultColumn();

                    b.HasKey("Id");

                    b.ToTable("Accounts");
                });

            modelBuilder.Entity("scrum_poker_server.Models.Room", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .UseIdentityByDefaultColumn();

                    b.Property<int?>("AccountId")
                        .HasColumnType("integer");

                    b.Property<string>("Code")
                        .HasColumnType("char(6)");

                    b.Property<string>("Description")
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<string>("Name")
                        .HasMaxLength(20)
                        .HasColumnType("character varying(20)");

                    b.Property<int>("UserId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("AccountId");

                    b.HasIndex("UserId");

                    b.ToTable("Rooms");
                });

            modelBuilder.Entity("scrum_poker_server.Models.Story", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .UseIdentityByDefaultColumn();

                    b.Property<string>("Content")
                        .HasColumnType("text");

                    b.Property<int?>("Point")
                        .HasColumnType("integer");

                    b.Property<int>("RoomId")
                        .HasColumnType("integer");

                    b.Property<string>("Title")
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<int?>("UserId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("RoomId");

                    b.HasIndex("UserId");

                    b.ToTable("Stories");
                });

            modelBuilder.Entity("scrum_poker_server.Models.SubmittedPointByUser", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .UseIdentityByDefaultColumn();

                    b.Property<int>("Point")
                        .HasColumnType("integer");

                    b.Property<int>("StoryId")
                        .HasColumnType("integer");

                    b.Property<int>("UserId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("StoryId");

                    b.HasIndex("UserId");

                    b.ToTable("SubmittedPointByUsers");
                });

            modelBuilder.Entity("scrum_poker_server.Models.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .UseIdentityByDefaultColumn();

                    b.Property<int?>("AccountId")
                        .HasColumnType("integer");

                    b.Property<string>("Email")
                        .HasColumnType("varchar(255)");

                    b.Property<string>("Name")
                        .HasMaxLength(25)
                        .HasColumnType("character varying(25)");

                    b.Property<string>("Password")
                        .HasColumnType("varchar(255)");

                    b.HasKey("Id");

                    b.HasIndex("AccountId")
                        .IsUnique();

                    b.ToTable("Users");
                });

            modelBuilder.Entity("scrum_poker_server.Models.UserRoom", b =>
                {
                    b.Property<int>("UserID")
                        .HasColumnType("integer");

                    b.Property<int>("RoomId")
                        .HasColumnType("integer");

                    b.HasKey("UserID", "RoomId");

                    b.HasIndex("RoomId");

                    b.ToTable("UserRooms");
                });

            modelBuilder.Entity("scrum_poker_server.Models.Room", b =>
                {
                    b.HasOne("scrum_poker_server.Models.Account", "Account")
                        .WithMany("Rooms")
                        .HasForeignKey("AccountId");

                    b.HasOne("scrum_poker_server.Models.User", "Owner")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Account");

                    b.Navigation("Owner");
                });

            modelBuilder.Entity("scrum_poker_server.Models.Story", b =>
                {
                    b.HasOne("scrum_poker_server.Models.Room", "Room")
                        .WithMany("Stories")
                        .HasForeignKey("RoomId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("scrum_poker_server.Models.User", "Assignee")
                        .WithMany()
                        .HasForeignKey("UserId");

                    b.Navigation("Assignee");

                    b.Navigation("Room");
                });

            modelBuilder.Entity("scrum_poker_server.Models.SubmittedPointByUser", b =>
                {
                    b.HasOne("scrum_poker_server.Models.Story", "Story")
                        .WithMany("SubmittedPointByUsers")
                        .HasForeignKey("StoryId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("scrum_poker_server.Models.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Story");

                    b.Navigation("User");
                });

            modelBuilder.Entity("scrum_poker_server.Models.User", b =>
                {
                    b.HasOne("scrum_poker_server.Models.Account", "Account")
                        .WithOne("User")
                        .HasForeignKey("scrum_poker_server.Models.User", "AccountId");

                    b.Navigation("Account");
                });

            modelBuilder.Entity("scrum_poker_server.Models.UserRoom", b =>
                {
                    b.HasOne("scrum_poker_server.Models.Room", "Room")
                        .WithMany("UserRooms")
                        .HasForeignKey("RoomId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("scrum_poker_server.Models.User", "User")
                        .WithMany("UserRooms")
                        .HasForeignKey("UserID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Room");

                    b.Navigation("User");
                });

            modelBuilder.Entity("scrum_poker_server.Models.Account", b =>
                {
                    b.Navigation("Rooms");

                    b.Navigation("User");
                });

            modelBuilder.Entity("scrum_poker_server.Models.Room", b =>
                {
                    b.Navigation("Stories");

                    b.Navigation("UserRooms");
                });

            modelBuilder.Entity("scrum_poker_server.Models.Story", b =>
                {
                    b.Navigation("SubmittedPointByUsers");
                });

            modelBuilder.Entity("scrum_poker_server.Models.User", b =>
                {
                    b.Navigation("UserRooms");
                });
#pragma warning restore 612, 618
        }
    }
}
