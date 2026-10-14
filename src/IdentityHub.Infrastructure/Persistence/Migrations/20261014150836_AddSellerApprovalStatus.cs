using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdentityHub.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSellerApprovalStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "SellerProfiles",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "SellerProfiles",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Approved");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "StatusChangedAt",
                table: "SellerProfiles",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StatusNote",
                table: "SellerProfiles",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "SellerProfiles");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "SellerProfiles");

            migrationBuilder.DropColumn(
                name: "StatusChangedAt",
                table: "SellerProfiles");

            migrationBuilder.DropColumn(
                name: "StatusNote",
                table: "SellerProfiles");
        }
    }
}
