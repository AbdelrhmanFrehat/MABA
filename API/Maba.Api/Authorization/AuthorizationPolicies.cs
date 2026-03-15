using Microsoft.AspNetCore.Authorization;

namespace Maba.Api.Authorization;

public static class AuthorizationPolicies
{
    // User Management
    public const string ViewUsers = "ViewUsers";
    public const string ManageUsers = "ManageUsers";

    // Catalog Management
    public const string ViewCatalog = "ViewCatalog";
    public const string ManageCatalog = "ManageCatalog";

    // Orders Management
    public const string ViewOrders = "ViewOrders";
    public const string ManageOrders = "ManageOrders";

    // CMS Management
    public const string ViewCms = "ViewCms";
    public const string ManageCms = "ManageCms";

    // Finance
    public const string ViewFinance = "ViewFinance";
    public const string ManageFinance = "ManageFinance";

    // Inventory
    public const string ViewInventory = "ViewInventory";
    public const string ManageInventory = "ManageInventory";

    // Machines
    public const string ViewMachines = "ViewMachines";
    public const string ManageMachines = "ManageMachines";

    // Printing
    public const string ViewPrinting = "ViewPrinting";
    public const string ManagePrinting = "ManagePrinting";

    // Media
    public const string ViewMedia = "ViewMedia";
    public const string ManageMedia = "ManageMedia";

    // Settings
    public const string ViewSettings = "ViewSettings";
    public const string ManageSettings = "ManageSettings";

    // Audit
    public const string ViewAuditLogs = "ViewAuditLogs";

    public static void ConfigurePolicies(AuthorizationOptions options)
    {
        // User Management Policies
        options.AddPolicy(ViewUsers, policy =>
            policy.Requirements.Add(new PermissionRequirement("users.view")));
        
        options.AddPolicy(ManageUsers, policy =>
            policy.Requirements.Add(new PermissionRequirement("users.manage")));

        // Catalog Policies
        options.AddPolicy(ViewCatalog, policy =>
            policy.Requirements.Add(new PermissionRequirement("catalog.view")));
        
        options.AddPolicy(ManageCatalog, policy =>
            policy.Requirements.Add(new PermissionRequirement("catalog.manage")));

        // Orders Policies
        options.AddPolicy(ViewOrders, policy =>
            policy.Requirements.Add(new PermissionRequirement("orders.view")));
        
        options.AddPolicy(ManageOrders, policy =>
            policy.Requirements.Add(new PermissionRequirement("orders.manage")));

        // CMS Policies
        options.AddPolicy(ViewCms, policy =>
            policy.Requirements.Add(new PermissionRequirement("cms.view")));
        
        options.AddPolicy(ManageCms, policy =>
            policy.Requirements.Add(new PermissionRequirement("cms.manage")));

        // Finance Policies
        options.AddPolicy(ViewFinance, policy =>
            policy.Requirements.Add(new PermissionRequirement("finance.view")));
        
        options.AddPolicy(ManageFinance, policy =>
            policy.Requirements.Add(new PermissionRequirement("finance.manage")));

        // Inventory Policies
        options.AddPolicy(ViewInventory, policy =>
            policy.Requirements.Add(new PermissionRequirement("inventory.view")));
        
        options.AddPolicy(ManageInventory, policy =>
            policy.Requirements.Add(new PermissionRequirement("inventory.manage")));

        // Machines Policies
        options.AddPolicy(ViewMachines, policy =>
            policy.Requirements.Add(new PermissionRequirement("machines.view")));
        
        options.AddPolicy(ManageMachines, policy =>
            policy.Requirements.Add(new PermissionRequirement("machines.manage")));

        // Printing Policies
        options.AddPolicy(ViewPrinting, policy =>
            policy.Requirements.Add(new PermissionRequirement("printing.view")));
        
        options.AddPolicy(ManagePrinting, policy =>
            policy.Requirements.Add(new PermissionRequirement("printing.manage")));

        // Media Policies
        options.AddPolicy(ViewMedia, policy =>
            policy.Requirements.Add(new PermissionRequirement("media.view")));
        
        options.AddPolicy(ManageMedia, policy =>
            policy.Requirements.Add(new PermissionRequirement("media.manage")));

        // Settings Policies
        options.AddPolicy(ViewSettings, policy =>
            policy.Requirements.Add(new PermissionRequirement("settings.view")));
        
        options.AddPolicy(ManageSettings, policy =>
            policy.Requirements.Add(new PermissionRequirement("settings.manage")));

        // Audit Policies
        options.AddPolicy(ViewAuditLogs, policy =>
            policy.Requirements.Add(new PermissionRequirement("audit.view")));
    }
}

