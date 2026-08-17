using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TicketManager.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddResolutionNotes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ResolutionNotes",
                table: "Tickets",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ResolutionNotes",
                table: "Tickets");
        }
    }
}
