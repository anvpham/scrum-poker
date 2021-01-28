﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using scrum_poker_server.Data;

namespace scrum_poker_server.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20210119102026_UpdateStoryPointDataType")]
    partial class UpdateStoryPointDataType
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .UseIdentityColumns()
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("ProductVersion", "5.0.1");

            modelBuilder.Entity("scrum_poker_server.Models.Account", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .UseIdentityColumn();

                    b.HasKey("Id");

                    b.ToTable("Accounts");
                });

            modelBuilder.Entity("scrum_poker_server.Models.Room", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .UseIdentityColumn();

                    b.Property<int?>("AccountId")
                        .HasColumnType("int");

                    b.Property<string>("Code")
                        .HasColumnType("char(6)");

                    b.Property<string>("Description")
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<string>("Name")
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("AccountId");

                    b.HasIndex("UserId");

                    b.ToTable("Rooms");
                });

            modelBuilder.Entity("scrum_poker_server.Models.Story", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .UseIdentityColumn();

                    b.Property<string>("Content")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("IsJiraStory")
                        .HasColumnType("bit");

                    b.Property<string>("JiraIssueId")
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)");

                    b.Property<double>("Point")
                        .HasColumnType("float");

                    b.Property<int>("RoomId")
                        .HasColumnType("int");

                    b.Property<string>("Title")
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)");

                    b.Property<int?>("UserId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("RoomId");

                    b.HasIndex("UserId");

                    b.ToTable("Stories");
                });

            modelBuilder.Entity("scrum_poker_server.Models.SubmittedPointByUser", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .UseIdentityColumn();

                    b.Property<float>("Point")
                        .HasColumnType("real");

                    b.Property<int>("StoryId")
                        .HasColumnType("int");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("StoryId");

                    b.HasIndex("UserId");

                    b.ToTable("SubmittedPointByUsers");
                });

            modelBuilder.Entity("scrum_poker_server.Models.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .UseIdentityColumn();

                    b.Property<int?>("AccountId")
                        .HasColumnType("int");

                    b.Property<string>("Email")
                        .HasColumnType("varchar(255)");

                    b.Property<string>("JiraDomain")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("JiraToken")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .HasMaxLength(25)
                        .HasColumnType("nvarchar(25)");

                    b.Property<string>("Password")
                        .HasColumnType("varchar(255)");

                    b.HasKey("Id");

                    b.HasIndex("AccountId")
                        .IsUnique()
                        .HasFilter("[AccountId] IS NOT NULL");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("scrum_poker_server.Models.UserRoom", b =>
                {
                    b.Property<int>("UserID")
                        .HasColumnType("int");

                    b.Property<int>("RoomId")
                        .HasColumnType("int");

                    b.Property<int>("Role")
                        .HasColumnType("int");

                    b.HasKey("UserID", "RoomId");

                    b.HasIndex("RoomId");

                    b.ToTable("UserRooms");
                });

            modelBuilder.Entity("scrum_poker_server.Models.Room", b =>
                {
                    b.HasOne("scrum_poker_server.Models.Account", "Account")
                        .WithMany("Rooms")
                        .HasForeignKey("AccountId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("scrum_poker_server.Models.User", "Owner")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Account");

                    b.Navigation("Owner");
                });

            modelBuilder.Entity("scrum_poker_server.Models.Story", b =>
                {
                    b.HasOne("scrum_poker_server.Models.Room", "Room")
                        .WithMany("Stories")
                        .HasForeignKey("RoomId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("scrum_poker_server.Models.User", "Assignee")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.Navigation("Assignee");

                    b.Navigation("Room");
                });

            modelBuilder.Entity("scrum_poker_server.Models.SubmittedPointByUser", b =>
                {
                    b.HasOne("scrum_poker_server.Models.Story", "Story")
                        .WithMany("SubmittedPointByUsers")
                        .HasForeignKey("StoryId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("scrum_poker_server.Models.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Story");

                    b.Navigation("User");
                });

            modelBuilder.Entity("scrum_poker_server.Models.User", b =>
                {
                    b.HasOne("scrum_poker_server.Models.Account", "Account")
                        .WithOne("User")
                        .HasForeignKey("scrum_poker_server.Models.User", "AccountId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.Navigation("Account");
                });

            modelBuilder.Entity("scrum_poker_server.Models.UserRoom", b =>
                {
                    b.HasOne("scrum_poker_server.Models.Room", "Room")
                        .WithMany("UserRooms")
                        .HasForeignKey("RoomId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("scrum_poker_server.Models.User", "User")
                        .WithMany("UserRooms")
                        .HasForeignKey("UserID")
                        .OnDelete(DeleteBehavior.Restrict)
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
