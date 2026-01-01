using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace cima.Migrations
{
    /// <inheritdoc />
    public partial class AddPropertyCatalogReferences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PortfolioProjects_Category",
                table: "PortfolioProjects");

            migrationBuilder.DropIndex(
                name: "IX_Listings_Status_Type_Transaction",
                table: "Listings");

            migrationBuilder.AddColumn<Guid>(
                name: "CategoryId",
                table: "PortfolioProjects",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("c58faa45-339c-4bd5-b469-186d86fb72e0"));

            migrationBuilder.AddColumn<Guid>(
                name: "CategoryId",
                table: "Listings",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("c58faa45-339c-4bd5-b469-186d86fb72e0"));

            migrationBuilder.AddColumn<Guid>(
                name: "TypeId",
                table: "Listings",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("9a49a6e3-4e30-4b32-a40a-f46b98eb9272"));

            migrationBuilder.InsertData(
                table: "PropertyCategories",
                columns: new[] { "Id", "CreationTime", "CreatorId", "DeleterId", "DeletionTime", "Description", "Icon", "IsActive", "LastModificationTime", "LastModifierId", "Name", "SortOrder" },
                values: new object[,]
                {
                    { new Guid("3882f8f0-0b66-45d0-bc3a-41b8772a3067"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, "Terrenos", null, true, null, null, "Terreno", 4 },
                    { new Guid("3d9067c8-6dfd-43aa-b023-14f69829374c"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, "Industrial y manufactura", null, true, null, null, "Industrial", 3 },
                    { new Guid("6988ee29-5554-4590-98af-9aa02215f5eb"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, "Comercio y oficinas", null, true, null, null, "Comercial", 2 },
                    { new Guid("a2469648-4ea4-40a6-a5d4-f3634a1b889f"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, "Vivienda y uso residencial", null, true, null, null, "Residencial", 1 },
                    { new Guid("c58faa45-339c-4bd5-b469-186d86fb72e0"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, "Otros", null, true, null, null, "Otros", 99 },
                    { new Guid("d0dfa188-447f-4c98-89e5-117534bc18c3"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, "Uso mixto", null, true, null, null, "Uso mixto", 5 }
                });

            migrationBuilder.InsertData(
                table: "PropertyTypes",
                columns: new[] { "Id", "CategoryId", "CreationTime", "CreatorId", "DeleterId", "DeletionTime", "Description", "IsActive", "LastModificationTime", "LastModifierId", "Name", "SortOrder" },
                values: new object[,]
                {
                    { new Guid("026f298d-d208-41db-907b-a79781e0a8d5"), new Guid("a2469648-4ea4-40a6-a5d4-f3634a1b889f"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, "Departamento o apartamento", true, null, null, "Departamento", 2 },
                    { new Guid("11381d7b-6329-4c85-957c-0d51d3b1b8c5"), new Guid("6988ee29-5554-4590-98af-9aa02215f5eb"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, "Restaurante", true, null, null, "Restaurante", 4 },
                    { new Guid("13820725-10f6-47b8-9075-ff5add3e46cd"), new Guid("3882f8f0-0b66-45d0-bc3a-41b8772a3067"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, "Terreno residencial", true, null, null, "Terreno residencial", 1 },
                    { new Guid("16512d95-a706-4559-a203-ec8e9eedc389"), new Guid("3882f8f0-0b66-45d0-bc3a-41b8772a3067"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, "Terreno comercial", true, null, null, "Terreno comercial", 2 },
                    { new Guid("2fc13152-ba99-4bb8-afc2-e9ac216b82cf"), new Guid("d0dfa188-447f-4c98-89e5-117534bc18c3"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, "Espacio vive-trabaja", true, null, null, "Espacio vive-trabaja", 2 },
                    { new Guid("30adccd4-c21c-4b34-bda6-92c6f5381ae2"), new Guid("a2469648-4ea4-40a6-a5d4-f3634a1b889f"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, "Condominio", true, null, null, "Condominio", 3 },
                    { new Guid("49e28976-d6f4-49c0-94f2-d4ab242b08ca"), new Guid("a2469648-4ea4-40a6-a5d4-f3634a1b889f"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, "Casa adosada", true, null, null, "Casa adosada", 4 },
                    { new Guid("5b2a7245-7a08-4bfa-b34e-8319b3bab828"), new Guid("d0dfa188-447f-4c98-89e5-117534bc18c3"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, "Edificio de uso mixto", true, null, null, "Edificio de uso mixto", 1 },
                    { new Guid("6dda1f32-1007-4c43-acac-580bbe2e4187"), new Guid("a2469648-4ea4-40a6-a5d4-f3634a1b889f"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, "Villa", true, null, null, "Villa", 5 },
                    { new Guid("70ea2410-b9c4-4bbe-92fa-6d49de1cd5ea"), new Guid("6988ee29-5554-4590-98af-9aa02215f5eb"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, "Bodega o almacen", true, null, null, "Bodega", 2 },
                    { new Guid("9a49a6e3-4e30-4b32-a40a-f46b98eb9272"), new Guid("c58faa45-339c-4bd5-b469-186d86fb72e0"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, "Otro", true, null, null, "Otro", 99 },
                    { new Guid("a065c868-37a0-4589-9cdc-82cb080c545a"), new Guid("a2469648-4ea4-40a6-a5d4-f3634a1b889f"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, "Casa unifamiliar", true, null, null, "Casa", 1 },
                    { new Guid("a7f7351e-217a-49df-9059-c343f584b7f9"), new Guid("6988ee29-5554-4590-98af-9aa02215f5eb"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, "Hotel", true, null, null, "Hotel", 5 },
                    { new Guid("bff51520-df9c-4465-827a-7c1c8d12b5bb"), new Guid("6988ee29-5554-4590-98af-9aa02215f5eb"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, "Local comercial", true, null, null, "Local comercial", 3 },
                    { new Guid("e4d9e3ea-9fbb-4e64-a8fa-6fbbdd6ac52f"), new Guid("3882f8f0-0b66-45d0-bc3a-41b8772a3067"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, "Terreno agricola", true, null, null, "Terreno agricola", 3 },
                    { new Guid("e72bca5f-f955-411b-aa28-95b744e8000a"), new Guid("6988ee29-5554-4590-98af-9aa02215f5eb"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, "Oficina", true, null, null, "Oficina", 1 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_PortfolioProjects_CategoryId",
                table: "PortfolioProjects",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Listings_CategoryId",
                table: "Listings",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Listings_Status_Type_Transaction",
                table: "Listings",
                columns: new[] { "Status", "TypeId", "TransactionType" });

            migrationBuilder.CreateIndex(
                name: "IX_Listings_TypeId",
                table: "Listings",
                column: "TypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Listings_PropertyCategories_CategoryId",
                table: "Listings",
                column: "CategoryId",
                principalTable: "PropertyCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Listings_PropertyTypes_TypeId",
                table: "Listings",
                column: "TypeId",
                principalTable: "PropertyTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PortfolioProjects_PropertyCategories_CategoryId",
                table: "PortfolioProjects",
                column: "CategoryId",
                principalTable: "PropertyCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Listings_PropertyCategories_CategoryId",
                table: "Listings");

            migrationBuilder.DropForeignKey(
                name: "FK_Listings_PropertyTypes_TypeId",
                table: "Listings");

            migrationBuilder.DropForeignKey(
                name: "FK_PortfolioProjects_PropertyCategories_CategoryId",
                table: "PortfolioProjects");

            migrationBuilder.DropIndex(
                name: "IX_PortfolioProjects_CategoryId",
                table: "PortfolioProjects");

            migrationBuilder.DropIndex(
                name: "IX_Listings_CategoryId",
                table: "Listings");

            migrationBuilder.DropIndex(
                name: "IX_Listings_Status_Type_Transaction",
                table: "Listings");

            migrationBuilder.DropIndex(
                name: "IX_Listings_TypeId",
                table: "Listings");

            migrationBuilder.DeleteData(
                table: "PropertyCategories",
                keyColumn: "Id",
                keyValue: new Guid("3d9067c8-6dfd-43aa-b023-14f69829374c"));

            migrationBuilder.DeleteData(
                table: "PropertyTypes",
                keyColumn: "Id",
                keyValue: new Guid("026f298d-d208-41db-907b-a79781e0a8d5"));

            migrationBuilder.DeleteData(
                table: "PropertyTypes",
                keyColumn: "Id",
                keyValue: new Guid("11381d7b-6329-4c85-957c-0d51d3b1b8c5"));

            migrationBuilder.DeleteData(
                table: "PropertyTypes",
                keyColumn: "Id",
                keyValue: new Guid("13820725-10f6-47b8-9075-ff5add3e46cd"));

            migrationBuilder.DeleteData(
                table: "PropertyTypes",
                keyColumn: "Id",
                keyValue: new Guid("16512d95-a706-4559-a203-ec8e9eedc389"));

            migrationBuilder.DeleteData(
                table: "PropertyTypes",
                keyColumn: "Id",
                keyValue: new Guid("2fc13152-ba99-4bb8-afc2-e9ac216b82cf"));

            migrationBuilder.DeleteData(
                table: "PropertyTypes",
                keyColumn: "Id",
                keyValue: new Guid("30adccd4-c21c-4b34-bda6-92c6f5381ae2"));

            migrationBuilder.DeleteData(
                table: "PropertyTypes",
                keyColumn: "Id",
                keyValue: new Guid("49e28976-d6f4-49c0-94f2-d4ab242b08ca"));

            migrationBuilder.DeleteData(
                table: "PropertyTypes",
                keyColumn: "Id",
                keyValue: new Guid("5b2a7245-7a08-4bfa-b34e-8319b3bab828"));

            migrationBuilder.DeleteData(
                table: "PropertyTypes",
                keyColumn: "Id",
                keyValue: new Guid("6dda1f32-1007-4c43-acac-580bbe2e4187"));

            migrationBuilder.DeleteData(
                table: "PropertyTypes",
                keyColumn: "Id",
                keyValue: new Guid("70ea2410-b9c4-4bbe-92fa-6d49de1cd5ea"));

            migrationBuilder.DeleteData(
                table: "PropertyTypes",
                keyColumn: "Id",
                keyValue: new Guid("9a49a6e3-4e30-4b32-a40a-f46b98eb9272"));

            migrationBuilder.DeleteData(
                table: "PropertyTypes",
                keyColumn: "Id",
                keyValue: new Guid("a065c868-37a0-4589-9cdc-82cb080c545a"));

            migrationBuilder.DeleteData(
                table: "PropertyTypes",
                keyColumn: "Id",
                keyValue: new Guid("a7f7351e-217a-49df-9059-c343f584b7f9"));

            migrationBuilder.DeleteData(
                table: "PropertyTypes",
                keyColumn: "Id",
                keyValue: new Guid("bff51520-df9c-4465-827a-7c1c8d12b5bb"));

            migrationBuilder.DeleteData(
                table: "PropertyTypes",
                keyColumn: "Id",
                keyValue: new Guid("e4d9e3ea-9fbb-4e64-a8fa-6fbbdd6ac52f"));

            migrationBuilder.DeleteData(
                table: "PropertyTypes",
                keyColumn: "Id",
                keyValue: new Guid("e72bca5f-f955-411b-aa28-95b744e8000a"));

            migrationBuilder.DeleteData(
                table: "PropertyCategories",
                keyColumn: "Id",
                keyValue: new Guid("3882f8f0-0b66-45d0-bc3a-41b8772a3067"));

            migrationBuilder.DeleteData(
                table: "PropertyCategories",
                keyColumn: "Id",
                keyValue: new Guid("6988ee29-5554-4590-98af-9aa02215f5eb"));

            migrationBuilder.DeleteData(
                table: "PropertyCategories",
                keyColumn: "Id",
                keyValue: new Guid("a2469648-4ea4-40a6-a5d4-f3634a1b889f"));

            migrationBuilder.DeleteData(
                table: "PropertyCategories",
                keyColumn: "Id",
                keyValue: new Guid("c58faa45-339c-4bd5-b469-186d86fb72e0"));

            migrationBuilder.DeleteData(
                table: "PropertyCategories",
                keyColumn: "Id",
                keyValue: new Guid("d0dfa188-447f-4c98-89e5-117534bc18c3"));

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "PortfolioProjects");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "Listings");

            migrationBuilder.DropColumn(
                name: "TypeId",
                table: "Listings");

            migrationBuilder.CreateIndex(
                name: "IX_PortfolioProjects_Category",
                table: "PortfolioProjects",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_Listings_Status_Type_Transaction",
                table: "Listings",
                columns: new[] { "Status", "Type", "TransactionType" });
        }
    }
}
