using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdentityHub.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddApprovalRevisionNumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsCurrent",
                table: "Approvals",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "RevisionNumber",
                table: "Approvals",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Approvals_EntityType_EntityId_IsCurrent",
                table: "Approvals",
                columns: new[] { "EntityType", "EntityId", "IsCurrent" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Approvals_EntityType_EntityId_IsCurrent",
                table: "Approvals");

            migrationBuilder.DropColumn(
                name: "IsCurrent",
                table: "Approvals");

            migrationBuilder.DropColumn(
                name: "RevisionNumber",
                table: "Approvals");
        }
    }
}
