using NETCORE.Data.Entities;
using NETCORE.Utils.Constants;

namespace NETCORE.Models.Systems;

public class EditRoleViewModel
{
    public Role? Role { get; set; } // Holds the role being edited

    public List<PermissionViewModel>? Permissions { get; set; } // List of all permissions to display

    // Holds the selected permissions for the role
    public IEnumerable<string>? SelectedPermissions { get; set; }
}


public class PermissionViewModel
{
    public string? FunctionId { get; set; }
    public string? FunctionParentId { get; set; }
    public string? FunctionName { get; set; }  // Tên của Function
    public string? FunctionIcon { get; set; }  // Biểu tượng của Function
    public string? RoleId { get; set; }
    public string? CommandId { get; set; }
    public string? CommandName { get; set; }
    public string? CommandIcon { get; set; }
    public bool IsAssigned { get; set; }

    public void AssignCommandIcon()
    {
        if (IconMapping.ActionIcons.TryGetValue(CommandId ?? string.Empty, out var icon))
        {
            CommandIcon = icon;
        }
        else
        {
            CommandIcon = "bi bi-plus-circle"; // Default icon if not found
        }
    }
}