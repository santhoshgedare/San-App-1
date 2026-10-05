using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdentityHub.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSellerInvitesAndPayment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BankDetails",
                table: "SellerProfiles",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InviteEmail",
                table: "SellerProfiles",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "InviteExpiresAt",
                table: "SellerProfiles",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "InviteSentAt",
                table: "SellerProfiles",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InviteTokenHash",
                table: "SellerProfiles",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PayeeName",
                table: "SellerProfiles",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpiId",
                table: "SellerProfiles",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SellerProfiles_InviteTokenHash",
                table: "SellerProfiles",
                column: "InviteTokenHash");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SellerProfiles_InviteTokenHash",
                table: "SellerProfiles");

            migrationBuilder.DropColumn(
                name: "BankDetails",
                table: "SellerProfiles");

            migrationBuilder.DropColumn(
                name: "InviteEmail",
                table: "SellerProfiles");

            migrationBuilder.DropColumn(
                name: "InviteExpiresAt",
                table: "SellerProfiles");

            migrationBuilder.DropColumn(
                name: "InviteSentAt",
                table: "SellerProfiles");

            migrationBuilder.DropColumn(
                name: "InviteTokenHash",
                table: "SellerProfiles");

            migrationBuilder.DropColumn(
                name: "PayeeName",
                table: "SellerProfiles");

            migrationBuilder.DropColumn(
                name: "UpiId",
                table: "SellerProfiles");
        }
    }
}
