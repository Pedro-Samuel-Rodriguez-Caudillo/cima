namespace cima.Permissions;


/// <summary>
/// Se tienen los nombres de los permisos, recordatorio de que es un sistema basado en Roles
/// </summary>
/// 
/// Para usarse tendrá que tener una estructura tipo [Authorize (cimaPermissions.Properties.Create)] en los controladores o servicios
/// 
public static class cimaPermissions
{
    public const string GroupName = "cima";

   // Permisos para gestionar las casas

    public static class Properties
    {
        public const string Default = GroupName + ".Properties";
        public const string Create = Default + ".Create";      // Crear propiedad
        public const string Edit = Default + ".Edit";          // Editar propiedad
        public const string Delete = Default + ".Delete";      // Eliminar propiedad
        public const string Publish = Default + ".Publish";    // Publicar (Draft->Published)
        public const string Archive = Default + ".Archive";
    }

    // Permisos para gestionar los arquitectos

    public static class Architects
    {
        public const string Default = GroupName + ".Architects";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }

    // Permisos para gestionar las solicitudes de contacto

    public static class ContactRequests
    {

        public const string Default = GroupName + ".ContactRequests";
        public const string View = Default + ".View";          // Ver solicitudes
        public const string Manage = Default + ".Manage";      // Gestionar todas
        public const string Reply = Default + ".Reply";        // Marcar como respondida (solo si en un futuro se quiere implementar chat interno)
        public const string Close = Default + ".Close";        // Cerrar solicitud
    
    }

    //Add your own permission names. Example:
    //public const string MyPermission1 = GroupName + ".MyPermission1";
}
