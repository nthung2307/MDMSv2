namespace Nobisoft.Core.Constants;

public static class Permission
{
    public const string SysAdmin = "root:root:sysadmin";

    public static ICollection<string> ListPermissions = new List<string>
    {
        "root:root:sysadmin", // System Admin

        // User Management
        "user:user:list", // View/Search list User
        "user:user:view_detail", // View Detailed information
        "user:user:change_status", // Deactivate/Activate User
        "user:user:update_profile", // Assign Title, Assign Role, Assign Permission
        "user:user:import_predefined_permission", // Import User
        "user:module_group:list", // View List Module Group
        "user:module_group:users", // View List User in Module Group
        "user:module_group:update_members", // Add, Remove members to a group

        // Role & Permission Management
        "user:role:add", // Add Role
        "user:role:update", // Update Role, Add/Remove permission vào 1 Role
        "user:role:list", // View list Roles trong hệ thống
        "user:role:view_detail", // View Detail
        "user:role:delete", // Delete Role
    };

    public static class User
    {
        public const string List = "user:user:list";
        public const string View = "user:user:view_detail";
        public const string ChangeStatus = "user:user:change_status";
        public const string UpdateProfile = "user:user:update_profile";
        public const string ImportPredefinedPermission = "user:user:import_predefined_permission";
        
        public static class Role
        {
            public const string Add = "user:role:add";
            public const string Update = "user:role:update";
            public const string List = "user:role:list";
            public const string View = "user:role:view_detail";
            public const string Delete = "user:role:delete";
        }
    }
}