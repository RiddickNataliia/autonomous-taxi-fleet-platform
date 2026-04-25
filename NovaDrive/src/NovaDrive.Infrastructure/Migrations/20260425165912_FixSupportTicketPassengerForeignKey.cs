using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NovaDrive.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixSupportTicketPassengerForeignKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SupportTickets_Users_PassengerId",
                table: "SupportTickets");

            migrationBuilder.AddForeignKey(
                name: "FK_SupportTickets_Passengers_PassengerId",
                table: "SupportTickets",
                column: "PassengerId",
                principalTable: "Passengers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SupportTickets_Passengers_PassengerId",
                table: "SupportTickets");

            migrationBuilder.AddForeignKey(
                name: "FK_SupportTickets_Users_PassengerId",
                table: "SupportTickets",
                column: "PassengerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
