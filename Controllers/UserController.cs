using NETCORE.Authorization;
using NETCORE.Data.Entities;
using NETCORE.Models.Systems;
using NETCORE.Repositories;
using NETCORE.Utils.Constants;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace NETCORE.Controllers;

public class UserController : Controller
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly SignInManager<User> _signInManager;
    public UserController(IUserRepository userRepository, IRoleRepository roleRepository, SignInManager<User> signInManager)
    {
        _roleRepository = roleRepository;
        _userRepository = userRepository;
        _signInManager = signInManager;
    }

    // [ClaimRequirement(FunctionCode.SYSTEM_USER, CommandCode.VIEW)]
    public async Task<IActionResult> Index(int page = 1, int pageSize = 10, string searchName = "")
    {
        var users = await _userRepository.GetUsersPagedAsync(page, pageSize, searchName);
        ViewBag.SearchName = searchName;
        return View(users);
    }

    // [ClaimRequirement(FunctionCode.SYSTEM_USER, CommandCode.VIEW)]
    public async Task<IActionResult> Edit(string id)
    {
        var user = await _userRepository.GetUserByIdAsync(id);
        var roles = await _roleRepository.GetAllRolesAsync();

        var userRoles = await _signInManager.UserManager.GetRolesAsync(user!);

        var roleItems = roles.Select(role =>
            new SelectListItem(
                role.Name,
                role.Id,
                userRoles.Any(ur => ur.Contains(role.Name!)))).ToList();

        var vm = new EditUserViewModel
        {
            User = user,
            Roles = roleItems
        };

        return View(vm);
    }

    [HttpPost]
    // [ClaimRequirement(FunctionCode.SYSTEM_USER, CommandCode.UPDATE)]
    public async Task<IActionResult> OnPostAsync(EditUserViewModel data)
    {
        var user = await _userRepository.GetUserByIdAsync(data.User!.Id);
        if (user == null)
        {
            return NotFound();
        }

        var userRolesInDb = await _signInManager.UserManager.GetRolesAsync(user);
        var currentUser = await _signInManager.UserManager.GetUserAsync(User);

        // Kiểm tra nếu người dùng hiện tại là admin
        bool isCurrentUserAdmin = currentUser != null && await _signInManager.UserManager.IsInRoleAsync(currentUser, "Administrator");

        // Nếu người dùng hiện tại là admin, đảm bảo rằng họ không thể vô hiệu hóa quyền admin của mình
        if (isCurrentUserAdmin)
        {
            // Nếu người dùng hiện tại đang cố gắng bỏ qua vai trò admin của mình, không cho phép thao tác
            if (user.Id == currentUser.Id && !data.Roles.Any(r => r.Text == "Administrator" && r.Selected))
            {
                // Trả lại lỗi hoặc thông báo không cho phép bỏ quyền admin của chính mình
                ModelState.AddModelError(string.Empty, "You cannot remove the 'Administrator' role from yourself.");
                return View(data); // Trả lại view hiện tại với thông báo lỗi
            }
        }
        //Loop through the roles in ViewModel
        //Check if the Role is Assigned In DB
        //If Assigned -> Do Nothing
        //If Not Assigned -> Add Role

        var rolesToAdd = new List<string>();
        var rolesToDelete = new List<string>();

        foreach (var role in data.Roles!)
        {
            var assignedInDb = userRolesInDb.FirstOrDefault(ur => ur == role.Text);
            if (role.Selected)
            {
                if (assignedInDb == null)
                {
                    rolesToAdd.Add(role.Text);
                }
            }
            else
            {
                if (assignedInDb != null)
                {
                    rolesToDelete.Add(role.Text);
                }
            }
        }

        if (rolesToAdd.Any())
        {
            await _signInManager.UserManager.AddToRolesAsync(user, rolesToAdd);
        }

        if (rolesToDelete.Any())
        {
            await _signInManager.UserManager.RemoveFromRolesAsync(user, rolesToDelete);
        }

        user.Email = data.User.Email;

        await _userRepository.UpdateUserAsync(user);

        return RedirectToAction(nameof(Index));
    }
}