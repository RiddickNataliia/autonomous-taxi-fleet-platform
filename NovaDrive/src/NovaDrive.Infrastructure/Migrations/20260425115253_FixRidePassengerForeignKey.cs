using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NovaDrive.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixRidePassengerForeignKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Rides_Users_PassengerId",
                table: "Rides");

            migrationBuilder.AddForeignKey(
                name: "FK_Rides_Passengers_PassengerId",
                table: "Rides",
                column: "PassengerId",
                principalTable: "Passengers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Rides_Passengers_PassengerId",
                table: "Rides");

            migrationBuilder.AddForeignKey(
                name: "FK_Rides_Users_PassengerId",
                table: "Rides",
                column: "PassengerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
