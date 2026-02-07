using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace dixanh.Data.Migrations
{
    /// <inheritdoc />
    public partial class FK_VehiclePlate_VehicleChassic_Status : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Vehicles_LicensePlate",
                table: "Vehicles");

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_LicensePlate",
                table: "Vehicles",
                column: "LicensePlate");

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_LicensePlate_ChassisNumber",
                table: "Vehicles",
                columns: new[] { "LicensePlate", "ChassisNumber" },
                unique: true,
                filter: "[StatusId] <> 2");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Vehicles_LicensePlate",
                table: "Vehicles");

            migrationBuilder.DropIndex(
                name: "IX_Vehicles_LicensePlate_ChassisNumber",
                table: "Vehicles");

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_LicensePlate",
                table: "Vehicles",
                column: "LicensePlate",
                unique: true);
        }
    }
}
