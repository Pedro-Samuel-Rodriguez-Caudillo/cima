namespace cima.Settings
{
    /// <summary>
    /// Define las claves de configuración del sistema CIMA
    /// Configurables desde el panel de administración
    /// </summary>
    public static class cimaSettings
    {
        /// <summary>
        /// Configuración de contacto administrativo
        /// </summary>
        public static class Contact
        {
            private const string Prefix = "Cima.Contact";

            /// <summary>
            /// Correo electrónico administrativo donde llegan todas las solicitudes de contacto
            /// </summary>
            public const string AdminEmail = Prefix + ".AdminEmail";

            /// <summary>
            /// Número telefónico administrativo para contacto
            /// </summary>
            public const string AdminPhone = Prefix + ".AdminPhone";
        }
    }
}
