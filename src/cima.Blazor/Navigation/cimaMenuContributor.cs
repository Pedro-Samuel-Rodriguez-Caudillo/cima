using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using cima.Localization;
using cima.Permissions;
using cima.MultiTenancy;
using cima.Blazor.Client.Navigation;
using Volo.Abp.Account.Localization;
using Volo.Abp.UI.Navigation;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Identity;

namespace cima.Blazor.Navigation;

public class cimaMenuContributor : IMenuContributor
{
    private readonly IConfiguration _configuration;

    public cimaMenuContributor(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public Task ConfigureMenuAsync(MenuConfigurationContext context)
    {
        if (context.Menu.Name == StandardMenus.Main)
        {
            return ConfigureMainMenuAsync(context);
        }
        else if (context.Menu.Name == StandardMenus.User)
        {
            return ConfigureUserMenuAsync(context);
        }

        return Task.CompletedTask;
    }

    private static Task ConfigureMainMenuAsync(MenuConfigurationContext context)
    {
        var l = context.GetLocalizer<cimaResource>();
        
        // Public menu items (always visible)
        context.Menu.AddItem(new ApplicationMenuItem(
            cimaMenus.Home,
            l["Menu:Home"],
            "/",
            icon: "fas fa-home",
            order: 1
        ));

        context.Menu.AddItem(new ApplicationMenuItem(
            cimaMenus.Properties,
            l["Menu:Properties"],
            PublicRoutes.Properties,
            icon: "fas fa-building",
            order: 2
        ));

        // Admin menu items (only for authenticated users with permissions)
        context.Menu.AddItem(new ApplicationMenuItem(
            cimaMenus.AdminListings,
            l["Menu:AdminListings"],
            "/admin/listings",
            icon: "fas fa-list",
            order: 3,
            requiredPermissionName: cimaPermissions.Listings.Default
        ));

        context.Menu.AddItem(new ApplicationMenuItem(
            cimaMenus.Architects,
            l["Menu:Architects"],
            "/admin/architects",
            icon: "fas fa-user-tie",
            order: 4,
            requiredPermissionName: cimaPermissions.Architects.Default
        ));

        context.Menu.AddItem(new ApplicationMenuItem(
            cimaMenus.ContactRequests,
            l["Menu:ContactRequests"],
            "/admin/contact-requests",
            icon: "fas fa-envelope",
            order: 5,
            requiredPermissionName: cimaPermissions.ContactRequests.Default
        ));
        
        // Administration menu - configuración básica
        // NOTA: En Blazor Web App, los menús de Identity, TenantManagement, Settings
        // se configuran automáticamente en el servidor (cimaBlazorModule)
        var administration = context.Menu.GetAdministration();
        administration.Order = 6;
        
        // Manualmente agregar Identity si no aparece
        var identityMenu = administration.GetMenuItem("AbpIdentity");
        if (identityMenu == null)
        {
            identityMenu = new ApplicationMenuItem(
                "AbpIdentity",
                l["Menu:IdentityManagement"],
                icon: "far fa-id-card",
                order: 1
            );
            administration.AddItem(identityMenu);
        }

        if (identityMenu.GetMenuItem("AbpIdentity.Users") == null)
        {
            identityMenu.AddItem(new ApplicationMenuItem(
                "AbpIdentity.Users",
                l["Menu:IdentityManagement:Users"],
                "/identity/users",
                icon: "fa fa-users",
                order: 1,
                requiredPermissionName: IdentityPermissions.Users.Default
            ));
        }

        if (identityMenu.GetMenuItem("AbpIdentity.Roles") == null)
        {
            identityMenu.AddItem(new ApplicationMenuItem(
                "AbpIdentity.Roles",
                l["Menu:IdentityManagement:Roles"],
                "/identity/roles",
                icon: "fa fa-id-badge",
                order: 2,
                requiredPermissionName: IdentityPermissions.Roles.Default
            ));
        }

        return Task.CompletedTask;
    }
    
    private Task ConfigureUserMenuAsync(MenuConfigurationContext context)
    {
        if (OperatingSystem.IsBrowser())
        {
            // Blazor wasm menu items
            var authServerUrl = _configuration["AuthServer:Authority"] ?? "";
            var accountResource = context.GetLocalizer<AccountResource>();

            context.Menu.AddItem(new ApplicationMenuItem(
                "Account.Manage", 
                accountResource["MyAccount"], 
                $"{authServerUrl.EnsureEndsWith('/')}Account/Manage", 
                icon: "fa fa-cog", 
                order: 900,  
                target: "_blank"
            ).RequireAuthenticated());
        }

        return Task.CompletedTask;
    }
}
