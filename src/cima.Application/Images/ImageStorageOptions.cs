using System;

namespace cima.Images;

public class ImageStorageOptions
{
    /// <summary>
    /// Provider identifier. Values: "AzureBlob" (default), "Local"
    /// </summary>
    public string Provider { get; set; } = "AzureBlob";

    public AzureOptions Azure { get; set; } = new();

    public class AzureOptions
    {
        public string ConnectionString { get; set; } =
            Environment.GetEnvironmentVariable("AZURE_STORAGE_CONNECTION_STRING") ??
            "UseDevelopmentStorage=true";

        public string ContainerName { get; set; } = "listings";

        /// <summary>
        /// Optional public base url. If empty, BlobClient.Uri is used.
        /// Example for azurite: http://127.0.0.1:10000/devstoreaccount1
        /// </summary>
        public string? PublicBaseUrl { get; set; }
        
        /// <summary>
        /// Optional proxy base URL for serving images via HTTPS when PublicBaseUrl uses HTTP.
        /// Example: https://localhost:44350
        /// </summary>
        public string? ProxyBaseUrl { get; set; }
    }
}

