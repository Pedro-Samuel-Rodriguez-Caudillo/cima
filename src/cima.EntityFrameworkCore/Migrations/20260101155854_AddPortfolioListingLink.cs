using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace cima.Migrations
{
    /// <inheritdoc />
    public partial class AddPortfolioListingLink : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ListingId",
                table: "PortfolioProjects",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PortfolioProjects_ListingId",
                table: "PortfolioProjects",
                column: "ListingId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_PortfolioProjects_Listings_ListingId",
                table: "PortfolioProjects",
                column: "ListingId",
                principalTable: "Listings",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PortfolioProjects_Listings_ListingId",
                table: "PortfolioProjects");

            migrationBuilder.DropIndex(
                name: "IX_PortfolioProjects_ListingId",
                table: "PortfolioProjects");

            migrationBuilder.DropColumn(
                name: "ListingId",
                table: "PortfolioProjects");
        }
    }
}
