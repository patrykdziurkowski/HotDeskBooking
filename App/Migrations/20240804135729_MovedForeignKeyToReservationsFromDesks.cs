using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace App.Migrations
{
    /// <inheritdoc />
    public partial class MovedForeignKeyToReservationsFromDesks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Desks_Reservations_ReservationId",
                table: "Desks");

            migrationBuilder.DropIndex(
                name: "IX_Desks_ReservationId",
                table: "Desks");

            migrationBuilder.DropColumn(
                name: "ReservationId",
                table: "Desks");

            migrationBuilder.AddColumn<Guid>(
                name: "DeskId",
                table: "Reservations",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_DeskId",
                table: "Reservations",
                column: "DeskId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_Desks_DeskId",
                table: "Reservations",
                column: "DeskId",
                principalTable: "Desks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_Desks_DeskId",
                table: "Reservations");

            migrationBuilder.DropIndex(
                name: "IX_Reservations_DeskId",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "DeskId",
                table: "Reservations");

            migrationBuilder.AddColumn<Guid>(
                name: "ReservationId",
                table: "Desks",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Desks_ReservationId",
                table: "Desks",
                column: "ReservationId",
                unique: true,
                filter: "[ReservationId] IS NOT NULL");

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
