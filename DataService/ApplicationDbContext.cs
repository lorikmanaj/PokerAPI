using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ModelService;
using ModelService.GameLogModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DataService
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<IdentityRole>().HasData(
                    new { Id = "1", Name = "Administrator", NormalizedName = "ADMINISTRATOR", RoleName = "Administrator", Handle = "administrator", RoleIcon = "/uploads/roles/icons/deafult/role.png", IsActive = true },
                    new { Id = "2", Name = "Player", NormalizedName = "PLAYER", RoleName = "player", Handle = "Player", RoleIcon = "/uploads/roles/icons/default/role.png", IsActive = true }
                );

            //builder.Entity<ApplicationUser>(entity =>
            //{
            //    entity.HasKey("UserId");
            //    entity.ToTable("ApplicationUsers");
            //});

            builder.Entity<Warning>()
                .HasKey(t => new { t.UserId, t.ReporterId });

            //builder.Entity<UserBan>()
            //    .HasKey(t => new { t.UserId, t.RoomId });

            builder.Entity<Friendship>()
                .HasKey(t => new { t.InvitingUser, t.InvitedUser });

            builder.Entity<Ban>()
                .HasKey(t => new { t.AdUserId, t.UserId }); 

            builder.Entity<UserInRoom>()
                .HasKey(t => new { t.UserId, t.RoomId });
        }

        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<TokenModel> Tokens { get; set; }
        public DbSet<ActivityModel> Activities { get; set; }
        public DbSet<Ban> Banns { get; set; }
        public DbSet<Challenge> Challenges { get; set; }
        public DbSet<Friendship> Friendships { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Session> Sessions { get; set; }
        public DbSet<Warning> Warnings { get; set; }
        public DbSet<Report> Reports { get; set; }
        //public DbSet<UserBan> UsersBanned { get; set; }
        public DbSet<UserInRoom> UsersInRooms { get; set; }

        //Game Logs
        public DbSet<RoundLog> RoundLogs { get; set; }
        public DbSet<RoomRoundLog> RoomRoundLogs { get; set; }
        public DbSet<RoundWinner> RoundWinners { get; set; }
        public DbSet<RoundUserLog> RoundUsersLogs { get; set; }
        public DbSet<CardModel> CardModels { get; set; }
        //public DbSet<UserLogModel> UsersLogModels { get; set; }
        
        //Errors
        public DbSet<LogError> LogErrors { get; set; }
        
        //IpInfo
        public DbSet<UserIpInfo> IpRecords { get; set; }
    }
}
