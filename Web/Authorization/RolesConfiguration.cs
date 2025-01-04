namespace Web.Authorization;

class RolesConfiguration
{
    public const string ConfigurationSection = "Authentication:Roles";
    public IEnumerable<string> ApplicationRoles { get; set; } = [];
}
