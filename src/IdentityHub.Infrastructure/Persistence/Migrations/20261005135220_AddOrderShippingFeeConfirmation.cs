using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdentityHub.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderShippingFeeConfirmation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ShippingFeeConfirmed",
                table: "Orders",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "ShippingFeeNote",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShippingFeeConfirmed",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ShippingFeeNote",
                table: "Orders");
        }
    }
}
