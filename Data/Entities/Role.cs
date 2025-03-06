using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace NETCORE.Data.Entities;

public class Role : IdentityRole
{
    public ICollection<Permission>? Permissions { get; set; }
}