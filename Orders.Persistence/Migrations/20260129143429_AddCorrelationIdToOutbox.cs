using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Orders.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCorrelationIdToOutbox : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "correlation_id",
                table: "outbox_messages",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "ix_outbox_correlation_id",
                table: "outbox_messages",
                column: "correlation_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_outbox_correlation_id",
                table: "outbox_messages");

            migrationBuilder.DropColumn(
                name: "correlation_id",
                table: "outbox_messages");
        }
    }
}
