using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace cima.Migrations
{
    /// <inheritdoc />
    public partial class RefactorListingDomain : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Listings_Location",
                table: "Listings");

            migrationBuilder.DropIndex(
                name: "IX_Listings_Status_Location",
                table: "Listings");

            migrationBuilder.DropColumn(
                name: "NextImageId",
                table: "ListingImages");

            migrationBuilder.DropColumn(
                name: "PreviousImageId",
                table: "ListingImages");

            migrationBuilder.AddColumn<int>(
                name: "SortOrder",
                table: "ListingImages",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SortOrder",
                table: "ListingImages");

            migrationBuilder.AddColumn<Guid>(
                name: "NextImageId",
                table: "ListingImages",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PreviousImageId",
                table: "ListingImages",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Listings_Location",
                table: "Listings",
                column: "Location");

            migrationBuilder.CreateIndex(
                name: "IX_Listings_Status_Location",
                table: "Listings",
                columns: new[] { "Status", "Location" });
        }
    }
}
