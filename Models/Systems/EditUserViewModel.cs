using NETCORE.Data.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace NETCORE.Models.Systems
{
    public class EditUserViewModel
    {
        public User? User { get; set; }

        public IList<SelectListItem>? Roles { get; set; }
    }
}