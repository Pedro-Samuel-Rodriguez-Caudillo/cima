using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace cima.Migrations
{
    /// <inheritdoc />
    public partial class RefactorDDD_BoundedContexts_AreasAndStats : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Bio",
                table: "Architects");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Architects");

            migrationBuilder.RenameColumn(
                name: "Area",
                table: "Listings",
                newName: "LandArea");

            migrationBuilder.AddColumn<decimal>(
                name: "ConstructionArea",
                table: "Listings",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "ActiveListings",
                table: "Architects",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Architects",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "RegistrationDate",
                table: "Architects",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "TotalListingsPublished",
                table: "Architects",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConstructionArea",
                table: "Listings");

            migrationBuilder.DropColumn(
                name: "ActiveListings",
                table: "Architects");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Architects");

            migrationBuilder.DropColumn(
                name: "RegistrationDate",
                table: "Architects");

            migrationBuilder.DropColumn(
                name: "TotalListingsPublished",
                table: "Architects");

            migrationBuilder.RenameColumn(
                name: "LandArea",
                table: "Listings",
                newName: "Area");

            migrationBuilder.AddColumn<string>(
                name: "Bio",
                table: "Architects",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Architects",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");
        }
    }
}
