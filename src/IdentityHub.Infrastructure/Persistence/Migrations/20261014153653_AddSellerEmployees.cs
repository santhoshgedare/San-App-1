using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdentityHub.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSellerEmployees : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SellerEmployees",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SellerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    InviteEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    InviteTokenHash = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    InviteExpiresAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    InviteSentAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SellerEmployees", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SellerEmployees_SellerProfiles_SellerId",
                        column: x => x.SellerId,
                        principalTable: "SellerProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SellerEmployees_InviteTokenHash",
                table: "SellerEmployees",
                column: "InviteTokenHash");

            migrationBuilder.CreateIndex(
                name: "IX_SellerEmployees_SellerId",
                table: "SellerEmployees",
                column: "SellerId");

            migrationBuilder.CreateIndex(
                name: "IX_SellerEmployees_UserId",
                table: "SellerEmployees",
                column: "UserId",
                unique: true,
                filter: "[UserId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SellerEmployees");
        }
    }
}
