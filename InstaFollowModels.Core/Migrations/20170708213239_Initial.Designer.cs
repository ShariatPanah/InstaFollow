using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using InstaFollowModels.Core;

namespace InstaFollowModels.Core
{
    [DbContext(typeof(ApplicationContext))]
    [Migration("20170708213239_Initial")]
    partial class Initial
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.1.2");

            modelBuilder.Entity("InstaFollowModels.Core.Follower", b =>
                {
                    b.Property<long>("FollowedLoggedInUserId");

                    b.Property<long>("LoggedInUserFollowedByUserId");

                    b.HasKey("FollowedLoggedInUserId", "LoggedInUserFollowedByUserId");

                    b.HasIndex("LoggedInUserFollowedByUserId");

                    b.ToTable("Follower");
                });

            modelBuilder.Entity("InstaFollowModels.Core.Following", b =>
                {
                    b.Property<long>("FollowedByLoggedInUserId");

                    b.Property<long>("FollowedUserId");

                    b.HasKey("FollowedByLoggedInUserId", "FollowedUserId");

                    b.HasIndex("FollowedUserId");

                    b.ToTable("Following");
                });

            modelBuilder.Entity("InstaFollowModels.Core.GainedFollower", b =>
                {
                    b.Property<long>("LoggedInUserFollowedByUserId");

                    b.Property<long>("FollowedLoggedInUserId");

                    b.HasKey("LoggedInUserFollowedByUserId", "FollowedLoggedInUserId");

                    b.HasAlternateKey("FollowedLoggedInUserId", "LoggedInUserFollowedByUserId");

                    b.ToTable("GainedFollower");
                });

            modelBuilder.Entity("InstaFollowModels.Core.LoggedInUsers", b =>
                {
                    b.Property<long>("Id");

                    b.Property<string>("AccessToken")
                        .HasMaxLength(70);

                    b.Property<string>("Username")
                        .HasMaxLength(50);

                    b.HasKey("Id");

                    b.ToTable("LoggedInUsers");
                });

            modelBuilder.Entity("InstaFollowModels.Core.LostFollower", b =>
                {
                    b.Property<long>("LoggedInUserFollowedByUserId");

                    b.Property<long>("FollowedLoggedInUserId");

                    b.HasKey("LoggedInUserFollowedByUserId", "FollowedLoggedInUserId");

                    b.HasAlternateKey("FollowedLoggedInUserId", "LoggedInUserFollowedByUserId");

                    b.ToTable("LostFollower");
                });

            modelBuilder.Entity("InstaFollowModels.Core.User", b =>
                {
                    b.Property<long>("Id");

                    b.Property<string>("FullName")
                        .HasMaxLength(50);

                    b.Property<string>("ProfilePicture")
                        .HasMaxLength(160);

                    b.Property<string>("Username")
                        .HasMaxLength(50);

                    b.HasKey("Id");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("InstaFollowModels.Core.Follower", b =>
                {
                    b.HasOne("InstaFollowModels.Core.LoggedInUsers", "FollowedLoggedUder")
                        .WithMany("Followers")
                        .HasForeignKey("FollowedLoggedInUserId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("InstaFollowModels.Core.User", "LoggedInUserFollowedByUser")
                        .WithMany("FollowingLoggedInUsers")
                        .HasForeignKey("LoggedInUserFollowedByUserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("InstaFollowModels.Core.Following", b =>
                {
                    b.HasOne("InstaFollowModels.Core.LoggedInUsers", "FollowedByLoggedInUser")
                        .WithMany("Followings")
                        .HasForeignKey("FollowedByLoggedInUserId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("InstaFollowModels.Core.User", "FollowedUser")
                        .WithMany("FollowedByLoggedInUsers")
                        .HasForeignKey("FollowedUserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("InstaFollowModels.Core.GainedFollower", b =>
                {
                    b.HasOne("InstaFollowModels.Core.LoggedInUsers", "FollowedLoggedUder")
                        .WithMany("GainedFollowers")
                        .HasForeignKey("FollowedLoggedInUserId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("InstaFollowModels.Core.User", "LoggedInUserFollowedByUser")
                        .WithMany("GainedFollowingLoggedInUsers")
                        .HasForeignKey("LoggedInUserFollowedByUserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("InstaFollowModels.Core.LostFollower", b =>
                {
                    b.HasOne("InstaFollowModels.Core.LoggedInUsers", "FollowedLoggedUder")
                        .WithMany("LostFollowers")
                        .HasForeignKey("FollowedLoggedInUserId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("InstaFollowModels.Core.User", "LoggedInUserFollowedByUser")
                        .WithMany("UnfollowedLoggedInUsers")
                        .HasForeignKey("LoggedInUserFollowedByUserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
        }
    }
}
