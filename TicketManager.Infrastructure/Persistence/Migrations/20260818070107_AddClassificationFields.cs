using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TicketManager.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddClassificationFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "Tickets",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "GroundedInHistory",
                table: "Tickets",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "SuggestedResponse",
                table: "Tickets",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Category",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "GroundedInHistory",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "SuggestedResponse",
                table: "Tickets");
        }
    }
}
