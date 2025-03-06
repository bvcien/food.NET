using NETCORE.Authorization;
using NETCORE.Models;
using NETCORE.Repositories;
using NETCORE.Utils.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NETCORE.Models.Systems;

namespace NETCORE.Controllers
{
    [Authorize]
    public class RoleController : Controller
    {
        private readonly IRoleRepository _roleRepository;
        private readonly IPermissionRepository _permissionRepository;
        private readonly IFunctionRepository _functionRepository;

        public RoleController(IRoleRepository roleRepository, IPermissionRepository permissionRepository, IFunctionRepository functionRepository)
        {
            _roleRepository = roleRepository;
            _permissionRepository = permissionRepository;
            _functionRepository = functionRepository;
        }

        // [ClaimRequirement(FunctionCode.SYSTEM_ROLE, CommandCode.VIEW)]
        public async Task<IActionResult> Index(int page = 1, int pageSize = 10, string searchName = "")
        {
            var users = await _roleRepository.GetRolesPagedAsync(page, pageSize, searchName);
            ViewBag.SearchName = searchName;
            return View(users);
        }

        // GET: Edit Role
        [HttpGet]
        // [ClaimRequirement(FunctionCode.SYSTEM_ROLE, CommandCode.UPDATE)]
        public async Task<IActionResult> Edit(string id)
        {
            // Tìm kiếm role theo ID
            var role = await _roleRepository.GetRoleByIdAsync(id);
            if (role == null)
            {
                return NotFound();
            }

            // Load permissions cho role
            var commandInFunction = await _permissionRepository.GetAllCommandInFunctionsAsync();

            var assignedPermissions = await _permissionRepository.GetPermissionsByRoleIdAsync(role.Id);

            // Tạo HashSet để tra cứu nhanh các permission đã được gán
            var assignedPermissionSet = new HashSet<string>(
                assignedPermissions.Select(ap => $"{ap.FunctionId} - {ap.CommandId}")
            );

            // Tạo danh sách PermissionViewModel với tên và biểu tượng của Function
            var viewModel = new EditRoleViewModel
            {
                Role = role,
                Permissions = commandInFunction.Select(p => new PermissionViewModel
                {
                    FunctionId = p.Function!.Id,
                    RoleId = role.Id,
                    CommandId = p.Command!.Id,
                    // Kiểm tra xem permission này có được gán hay không
                    IsAssigned = assignedPermissionSet.Contains($"{p.Function.Id} - {p.Command.Id}"),
                    FunctionName = p.Function?.Name,
                    CommandName = p.Command?.Name,
                    FunctionIcon = p.Function?.Icon,
                    FunctionParentId = p.Function?.ParentId,
                }).ToList()
            };

            foreach (var permission in viewModel.Permissions)
            {
                permission.AssignCommandIcon();
            }

            return View(viewModel);
        }



        // POST: Edit Role
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditRoleViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var role = await _roleRepository.GetRoleByIdAsync(model.Role!.Id);
            if (role == null)
            {
                return NotFound();
            }

            // Update role properties
            role.Name = model.Role.Name;
            await _roleRepository.UpdateRoleAsync(role);

            // Update permissions for the role
            await _permissionRepository.UpdateRolePermissionsAsync(model.Role.Id, model.SelectedPermissions!);

            return RedirectToAction(nameof(Index));
        }
    }
}