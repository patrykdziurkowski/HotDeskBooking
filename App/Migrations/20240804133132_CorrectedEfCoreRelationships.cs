using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace App.Migrations
{
    /// <inheritdoc />
    public partial class CorrectedEfCoreRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Desks_Reservations_ReservationId",
                table: "Desks");

            migrationBuilder.AddForeignKey(
                name: "FK_Desks_Reservations_ReservationId",
                table: "Desks",
                column: "ReservationId",
                principalTable: "Reservations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Desks_Reservations_ReservationId",
                table: "Desks");

            migrationBuilder.AddForeignKey(
                name: "FK_Desks_Reservations_ReservationId",
                table: "Desks",
                column: "ReservationId",
                principalTable: "Reservations",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
