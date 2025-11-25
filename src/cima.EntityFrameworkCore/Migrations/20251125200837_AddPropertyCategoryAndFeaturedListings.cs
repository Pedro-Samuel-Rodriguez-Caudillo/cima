using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace cima.Migrations
{
    /// <inheritdoc />
    public partial class AddPropertyCategoryAndFeaturedListings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Category",
                table: "Listings",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "FeaturedListings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ListingId = table.Column<Guid>(type: "uuid", nullable: false),
                    FeaturedSince = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeaturedListings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FeaturedListings_Listings_ListingId",
                        column: x => x.ListingId,
                        principalTable: "Listings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FeaturedListings_DisplayOrder",
                table: "FeaturedListings",
                column: "DisplayOrder");

            migrationBuilder.CreateIndex(
                name: "IX_FeaturedListings_ListingId",
                table: "FeaturedListings",
                column: "ListingId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FeaturedListings");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "Listings");
        }
    }
}
