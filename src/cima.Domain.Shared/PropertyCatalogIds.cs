using System;

namespace cima.Domain.Shared;

public static class PropertyCatalogIds
{
    public static class Categories
    {
        public static readonly Guid Residential = Guid.Parse("a2469648-4ea4-40a6-a5d4-f3634a1b889f");
        public static readonly Guid Commercial = Guid.Parse("6988ee29-5554-4590-98af-9aa02215f5eb");
        public static readonly Guid Industrial = Guid.Parse("3d9067c8-6dfd-43aa-b023-14f69829374c");
        public static readonly Guid Land = Guid.Parse("3882f8f0-0b66-45d0-bc3a-41b8772a3067");
        public static readonly Guid Mixed = Guid.Parse("d0dfa188-447f-4c98-89e5-117534bc18c3");
        public static readonly Guid Other = Guid.Parse("c58faa45-339c-4bd5-b469-186d86fb72e0");
    }

    public static class Types
    {
        public static readonly Guid House = Guid.Parse("a065c868-37a0-4589-9cdc-82cb080c545a");
        public static readonly Guid Apartment = Guid.Parse("026f298d-d208-41db-907b-a79781e0a8d5");
        public static readonly Guid Condo = Guid.Parse("30adccd4-c21c-4b34-bda6-92c6f5381ae2");
        public static readonly Guid Townhouse = Guid.Parse("49e28976-d6f4-49c0-94f2-d4ab242b08ca");
        public static readonly Guid Villa = Guid.Parse("6dda1f32-1007-4c43-acac-580bbe2e4187");
        public static readonly Guid Office = Guid.Parse("e72bca5f-f955-411b-aa28-95b744e8000a");
        public static readonly Guid Warehouse = Guid.Parse("70ea2410-b9c4-4bbe-92fa-6d49de1cd5ea");
        public static readonly Guid RetailSpace = Guid.Parse("bff51520-df9c-4465-827a-7c1c8d12b5bb");
        public static readonly Guid Restaurant = Guid.Parse("11381d7b-6329-4c85-957c-0d51d3b1b8c5");
        public static readonly Guid Hotel = Guid.Parse("a7f7351e-217a-49df-9059-c343f584b7f9");
        public static readonly Guid MixedUseBuilding = Guid.Parse("5b2a7245-7a08-4bfa-b34e-8319b3bab828");
        public static readonly Guid LiveWorkSpace = Guid.Parse("2fc13152-ba99-4bb8-afc2-e9ac216b82cf");
        public static readonly Guid ResidentialLand = Guid.Parse("13820725-10f6-47b8-9075-ff5add3e46cd");
        public static readonly Guid CommercialLand = Guid.Parse("16512d95-a706-4559-a203-ec8e9eedc389");
        public static readonly Guid AgriculturalLand = Guid.Parse("e4d9e3ea-9fbb-4e64-a8fa-6fbbdd6ac52f");
        public static readonly Guid Other = Guid.Parse("9a49a6e3-4e30-4b32-a40a-f46b98eb9272");
    }
}
