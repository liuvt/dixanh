using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace dixanh.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddVehicleCodeHistoryFilteredIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CurrentVehicleCode",
                table: "Vehicles",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "VehicleCodeHistories",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(36)", unicode: false, maxLength: 36, nullable: false),
                    VehicleId = table.Column<string>(type: "varchar(36)", unicode: false, maxLength: 36, nullable: false),
                    VehicleCode = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    OperatingArea = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ValidFrom = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ValidTo = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ChangeReason = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ChangedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ChangedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VehicleCodeHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VehicleCodeHistories_Vehicles_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "Vehicles",
                        principalColumn: "VehicleId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_CreatedAt",
                table: "Vehicles",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_CurrentVehicleCode",
                table: "Vehicles",
                column: "CurrentVehicleCode");

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_StatusId",
                table: "Vehicles",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleCodeHistories_VehicleId",
                table: "VehicleCodeHistories",
                column: "VehicleId",
                unique: true,
                filter: "[ValidTo] IS NULL");

            migrationBuilder.CreateIndex(
                name: "UX_VehicleCodeHistories_ActiveCodePerArea",
                table: "VehicleCodeHistories",
                columns: new[] { "OperatingArea", "VehicleCode" },
                unique: true,
                filter: "[ValidTo] IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VehicleCodeHistories");

            migrationBuilder.DropIndex(
                name: "IX_Vehicles_CreatedAt",
                table: "Vehicles");

            migrationBuilder.DropIndex(
                name: "IX_Vehicles_CurrentVehicleCode",
                table: "Vehicles");

            migrationBuilder.DropIndex(
                name: "IX_Vehicles_StatusId",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "CurrentVehicleCode",
                table: "Vehicles");
        }
    }
}
