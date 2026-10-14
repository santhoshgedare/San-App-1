using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdentityHub.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderAdvanceAndChargeAcceptance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "AdvanceAmount",
                table: "Orders",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "AdvanceReferenceNumber",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AdvanceStatus",
                table: "Orders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ChargesAcceptedAt",
                table: "Orders",
                type: "datetimeoffset",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdvanceAmount",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "AdvanceReferenceNumber",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "AdvanceStatus",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ChargesAcceptedAt",
                table: "Orders");
        }
    }
}
