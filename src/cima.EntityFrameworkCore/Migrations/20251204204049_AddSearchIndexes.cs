using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace cima.Migrations
{
    /// <inheritdoc />
    public partial class AddSearchIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Listings_Location",
                table: "Listings",
                column: "Location");

            migrationBuilder.CreateIndex(
                name: "IX_Listings_Price",
                table: "Listings",
                column: "Price");

            migrationBuilder.CreateIndex(
                name: "IX_Listings_Status",
                table: "Listings",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Listings_Status_Location",
                table: "Listings",
                columns: new[] { "Status", "Location" });

            migrationBuilder.CreateIndex(
                name: "IX_Listings_Status_Type_Transaction",
                table: "Listings",
                columns: new[] { "Status", "Type", "TransactionType" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Listings_Location",
                table: "Listings");

            migrationBuilder.DropIndex(
                name: "IX_Listings_Price",
                table: "Listings");

            migrationBuilder.DropIndex(
                name: "IX_Listings_Status",
                table: "Listings");

            migrationBuilder.DropIndex(
                name: "IX_Listings_Status_Location",
                table: "Listings");

            migrationBuilder.DropIndex(
                name: "IX_Listings_Status_Type_Transaction",
                table: "Listings");
        }
    }
}
