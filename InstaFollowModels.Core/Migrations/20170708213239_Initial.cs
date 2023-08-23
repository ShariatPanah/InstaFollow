using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace InstaFollowModels.Core
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LoggedInUsers",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false),
                    AccessToken = table.Column<string>(maxLength: 70, nullable: true),
                    Username = table.Column<string>(maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoggedInUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false),
                    FullName = table.Column<string>(maxLength: 50, nullable: true),
                    ProfilePicture = table.Column<string>(maxLength: 160, nullable: true),
                    Username = table.Column<string>(maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Follower",
                columns: table => new
                {
                    FollowedLoggedInUserId = table.Column<long>(nullable: false),
                    LoggedInUserFollowedByUserId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Follower", x => new { x.FollowedLoggedInUserId, x.LoggedInUserFollowedByUserId });
                    table.ForeignKey(
                        name: "FK_Follower_LoggedInUsers_FollowedLoggedInUserId",
                        column: x => x.FollowedLoggedInUserId,
                        principalTable: "LoggedInUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Follower_Users_LoggedInUserFollowedByUserId",
                        column: x => x.LoggedInUserFollowedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Following",
                columns: table => new
                {
                    FollowedByLoggedInUserId = table.Column<long>(nullable: false),
                    FollowedUserId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Following", x => new { x.FollowedByLoggedInUserId, x.FollowedUserId });
                    table.ForeignKey(
                        name: "FK_Following_LoggedInUsers_FollowedByLoggedInUserId",
                        column: x => x.FollowedByLoggedInUserId,
                        principalTable: "LoggedInUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Following_Users_FollowedUserId",
                        column: x => x.FollowedUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GainedFollower",
                columns: table => new
                {
                    LoggedInUserFollowedByUserId = table.Column<long>(nullable: false),
                    FollowedLoggedInUserId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GainedFollower", x => new { x.LoggedInUserFollowedByUserId, x.FollowedLoggedInUserId });
                    table.UniqueConstraint("AK_GainedFollower_FollowedLoggedInUserId_LoggedInUserFollowedByUserId", x => new { x.FollowedLoggedInUserId, x.LoggedInUserFollowedByUserId });
                    table.ForeignKey(
                        name: "FK_GainedFollower_LoggedInUsers_FollowedLoggedInUserId",
                        column: x => x.FollowedLoggedInUserId,
                        principalTable: "LoggedInUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GainedFollower_Users_LoggedInUserFollowedByUserId",
                        column: x => x.LoggedInUserFollowedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LostFollower",
                columns: table => new
                {
                    LoggedInUserFollowedByUserId = table.Column<long>(nullable: false),
                    FollowedLoggedInUserId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LostFollower", x => new { x.LoggedInUserFollowedByUserId, x.FollowedLoggedInUserId });
                    table.UniqueConstraint("AK_LostFollower_FollowedLoggedInUserId_LoggedInUserFollowedByUserId", x => new { x.FollowedLoggedInUserId, x.LoggedInUserFollowedByUserId });
                    table.ForeignKey(
                        name: "FK_LostFollower_LoggedInUsers_FollowedLoggedInUserId",
                        column: x => x.FollowedLoggedInUserId,
                        principalTable: "LoggedInUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LostFollower_Users_LoggedInUserFollowedByUserId",
                        column: x => x.LoggedInUserFollowedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Follower_LoggedInUserFollowedByUserId",
                table: "Follower",
                column: "LoggedInUserFollowedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Following_FollowedUserId",
                table: "Following",
                column: "FollowedUserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Follower");

            migrationBuilder.DropTable(
                name: "Following");

            migrationBuilder.DropTable(
                name: "GainedFollower");

            migrationBuilder.DropTable(
                name: "LostFollower");

            migrationBuilder.DropTable(
                name: "LoggedInUsers");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
