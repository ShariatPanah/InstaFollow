using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstaFollowModels.Core
{
    public class ApplicationContext : DbContext
    {
        private string DatabasePath { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<LoggedInUsers> LoggedInUsers { get; set; }

        public ApplicationContext()
        {

        }

        public ApplicationContext(string dbPath)
        {
            this.DatabasePath = dbPath;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite($"Filename={DatabasePath}");
            //base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Follower>()
            .HasKey(t => new { t.FollowedLoggedInUserId, t.LoggedInUserFollowedByUserId });

            modelBuilder.Entity<Following>()
            .HasKey(t => new { t.FollowedByLoggedInUserId, t.FollowedUserId });

            modelBuilder.Entity<LostFollower>()
            .HasKey(t => new { t.LoggedInUserFollowedByUserId, t.FollowedLoggedInUserId });

            modelBuilder.Entity<GainedFollower>()
            .HasKey(t => new { t.LoggedInUserFollowedByUserId, t.FollowedLoggedInUserId });


            modelBuilder.Entity<Follower>()
                .HasOne(follower => follower.FollowedLoggedUder)
                .WithMany(fl => fl.Followers)
                .HasForeignKey(follower => follower.FollowedLoggedInUserId);

            modelBuilder.Entity<Follower>()
                .HasOne(follower => follower.LoggedInUserFollowedByUser)
                .WithMany(fl => fl.FollowingLoggedInUsers)
                .HasForeignKey(follower => follower.LoggedInUserFollowedByUserId);

            modelBuilder.Entity<Following>()
               .HasOne(follower => follower.FollowedByLoggedInUser)
               .WithMany(fl => fl.Followings)
               .HasForeignKey(follower => follower.FollowedByLoggedInUserId);

            modelBuilder.Entity<Following>()
                .HasOne(follower => follower.FollowedUser)
                .WithMany(fl => fl.FollowedByLoggedInUsers)
                .HasForeignKey(follower => follower.FollowedUserId);

            modelBuilder.Entity<LostFollower>()
               .HasOne(follower => follower.LoggedInUserFollowedByUser)
               .WithMany(fl => fl.UnfollowedLoggedInUsers)
               .HasForeignKey(follower => follower.LoggedInUserFollowedByUserId);

            modelBuilder.Entity<LostFollower>()
                .HasOne(follower => follower.FollowedLoggedUder)
                .WithMany(fl => fl.LostFollowers)
                .HasForeignKey(follower => follower.FollowedLoggedInUserId);

            modelBuilder.Entity<GainedFollower>()
               .HasOne(follower => follower.LoggedInUserFollowedByUser)
               .WithMany(fl => fl.GainedFollowingLoggedInUsers)
               .HasForeignKey(follower => follower.LoggedInUserFollowedByUserId);

            modelBuilder.Entity<GainedFollower>()
                .HasOne(follower => follower.FollowedLoggedUder)
                .WithMany(fl => fl.GainedFollowers)
                .HasForeignKey(follower => follower.FollowedLoggedInUserId);
        }
    }
}
