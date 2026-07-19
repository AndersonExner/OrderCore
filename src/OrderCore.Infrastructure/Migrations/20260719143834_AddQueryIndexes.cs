using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrderCore.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddQueryIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_products_Name",
                table: "products",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_outbox_messages_Type",
                table: "outbox_messages",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_orders_CreatedAtUtc",
                table: "orders",
                column: "CreatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_orders_CustomerId_CreatedAtUtc",
                table: "orders",
                columns: new[] { "CustomerId", "CreatedAtUtc" },
                descending: new[] { false, true });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_products_Name",
                table: "products");

            migrationBuilder.DropIndex(
                name: "IX_outbox_messages_Type",
                table: "outbox_messages");

            migrationBuilder.DropIndex(
                name: "IX_orders_CreatedAtUtc",
                table: "orders");

            migrationBuilder.DropIndex(
                name: "IX_orders_CustomerId_CreatedAtUtc",
                table: "orders");
        }
    }
}
