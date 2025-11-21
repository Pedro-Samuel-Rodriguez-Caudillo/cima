using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace cima.Migrations
{
    /// <inheritdoc />
    public partial class AddRepliedAtToContactRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TransactionType",
                table: "Listings",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "Listings",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "RepliedAt",
                table: "ContactRequests",
                type: "timestamp without time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TransactionType",
                table: "Listings");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Listings");

            migrationBuilder.DropColumn(
                name: "RepliedAt",
                table: "ContactRequests");
        }
    }
}
