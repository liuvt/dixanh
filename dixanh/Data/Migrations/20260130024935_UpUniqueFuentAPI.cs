using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace dixanh.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpUniqueFuentAPI : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_VehicleStatusHistories_VehicleId",
                table: "VehicleStatusHistories");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleStatusHistories_VehicleId_ChangedAt",
                table: "VehicleStatusHistories",
                columns: new[] { "VehicleId", "ChangedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_VehicleStatusHistories_VehicleId_ChangedAt",
                table: "VehicleStatusHistories");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleStatusHistories_VehicleId",
                table: "VehicleStatusHistories",
                column: "VehicleId");
        }
    }
}
