using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NovaDrive.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDiscountCodeToRide : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DiscountCodeUsed",
                table: "Rides",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DiscountCodeUsed",
                table: "Rides");
        }
    }
}
