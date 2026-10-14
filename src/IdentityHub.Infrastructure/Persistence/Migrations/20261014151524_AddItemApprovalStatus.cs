using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdentityHub.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddItemApprovalStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ApprovalChangedAt",
                table: "Items",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApprovalNote",
                table: "Items",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApprovalStatus",
                table: "Items",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Approved");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApprovalChangedAt",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "ApprovalNote",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "ApprovalStatus",
                table: "Items");
        }
    }
}
