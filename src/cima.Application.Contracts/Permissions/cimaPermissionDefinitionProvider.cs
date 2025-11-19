using cima.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;
using Volo.Abp.MultiTenancy;

namespace cima.Permissions;

public class cimaPermissionDefinitionProvider : PermissionDefinitionProvider
{
    /// <summary>
    /// Define una serie de permisos en el sistema
    /// </summary>
    /// <remarks>Override this method to specify custom permissions for your application. Use the provided
    /// context to add groups and permissions as needed. This method is typically called during application startup to
    /// configure authorization.</remarks>
    /// <param name="context">The context used to register permission groups and permissions. Cannot be null.</param>
    ///
    /// Flujo
    /// 1. Añadir grupo
    /// 2. Añadir permiso padre (para listar)
    /// 3. Añadir los permisos que ese grupo tendrá (crear, editar, borrar, etc)


    public override void Define(IPermissionDefinitionContext context)
    {
        var cimaGroup = context.AddGroup(cimaPermissions.GroupName, L("Permission:cima"));

        // Propiedades (casas)
        var listingsPermission = cimaGroup.AddPermission(
            cimaPermissions.Listings.Default, L("Permission:Listings"));

        listingsPermission.AddChild(
            cimaPermissions.Listings.Create, L("Permission:Listings.Create"));
        listingsPermission.AddChild(
            cimaPermissions.Listings.Edit, L("Permission:Listings.Edit"));
        listingsPermission.AddChild(
            cimaPermissions.Listings.Delete, L("Permission:Listings.Delete"));
        listingsPermission.AddChild(
            cimaPermissions.Listings.Publish, L("Permission:Listings.Publish"));
        listingsPermission.AddChild(
            cimaPermissions.Listings.Archive, L("Permission:Listings.Archive"));


        // Arquitectos
        var architectsPermission = cimaGroup.AddPermission(
            cimaPermissions.Architects.Default, L("Permission:Architects"));
        architectsPermission.AddChild(
            cimaPermissions.Architects.Create, L("Permission:Architects.Create"));
        architectsPermission.AddChild(
          cimaPermissions.Architects.Edit, L("Permission:Architects.Edit"));
        architectsPermission.AddChild(
          cimaPermissions.Architects.Delete, L("Permission:Architects.Delete"));

        // Peticiones de contacto
        var contactRequestsPermission = cimaGroup.AddPermission(
            cimaPermissions.ContactRequests.Default, L("Permission:ContactRequests"));
        contactRequestsPermission.AddChild(
            cimaPermissions.ContactRequests.View, L("Permission:ContactRequests.View"));
        contactRequestsPermission.AddChild(
            cimaPermissions.ContactRequests.Reply, L("Permission:ContactRequests.Reply"));
        contactRequestsPermission.AddChild(
            cimaPermissions.ContactRequests.Close, L("Permission:ContactRequests.Close"));

        //Define your own permissions here. Example:
        //myGroup.AddPermission(cimaPermissions.MyPermission1, L("Permission:MyPermission1"));
    }


    private static LocalizableString L(string name) // ni idea de que sea esto, pero ya estaba desde antes de que llegara
    {
        return LocalizableString.Create<cimaResource>(name);
    }
}
