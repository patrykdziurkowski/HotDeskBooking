using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace App.Migrations
{
    /// <inheritdoc />
    public partial class RestrictLocationDeleteWhenDeskExists : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Desks_Locations_LocationId",
                table: "Desks");

            migrationBuilder.DropForeignKey(
                name: "FK_Desks_Reservations_ReservationId",
                table: "Desks");

            migrationBuilder.AddForeignKey(
                name: "FK_Desks_Locations_LocationId",
                table: "Desks",
                column: "LocationId",
                principalTable: "Locations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Desks_Reservations_ReservationId",
                table: "Desks",
                column: "ReservationId",
                principalTable: "Reservations",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Desks_Locations_LocationId",
                table: "Desks");

            migrationBuilder.DropForeignKey(
                name: "FK_Desks_Reservations_ReservationId",
                table: "Desks");

            migrationBuilder.AddForeignKey(
                name: "FK_Desks_Locations_LocationId",
                table: "Desks",
                column: "LocationId",
                principalTable: "Locations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Desks_Reservations_ReservationId",
                table: "Desks",
                column: "ReservationId",
                principalTable: "Reservations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
