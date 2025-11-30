using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using cima.Localization;
using cima.Permissions;
using cima.MultiTenancy;
using Volo.Abp.Account.Localization;
using Volo.Abp.UI.Navigation;
using Localization.Resources.AbpUi;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.SettingManagement.Blazor.Menus;
using Volo.Abp.Users;
using Volo.Abp.TenantManagement.Blazor.Navigation;
using Volo.Abp.Identity.Blazor;

namespace cima.Blazor.Client.Navigation;

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
        
        // Administration menu
        var administration = context.Menu.GetAdministration();
        administration.Order = 6;

        if (MultiTenancyConsts.IsEnabled)
        {
            administration.SetSubItemOrder(TenantManagementMenuNames.GroupName, 1);
        }
        else
        {
            administration.TryRemoveMenuItem(TenantManagementMenuNames.GroupName);
        }

        administration.SetSubItemOrder(IdentityMenuNames.GroupName, 2);
        administration.SetSubItemOrder(SettingManagementMenus.GroupName, 3);

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
