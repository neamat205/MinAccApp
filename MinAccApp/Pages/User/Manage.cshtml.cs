using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using MinAccApp.Models;
using System.Data;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace MinAccApp.Pages.User
{
    public class ManageModel : PageModel
    {
        private readonly IConfiguration _config;
        private readonly IPermissionService _permissionService;

        public ManageModel(IConfiguration config, IPermissionService permissionService)
        {
            _config = config;
            _permissionService = permissionService;
        }

        // --- Properties ---
        public List<RoleModulePermissionDto> RolePermissions { get; set; } = new();

        public List<string> AvailableModules { get; set; } = new()
        {
            "Voucher", "ChartOfAccount", "UserManagement", "Report"
        };

        public List<UserWithRoleDto> Users { get; set; } = new();

        public List<RoleDto> Roles { get; set; } = new();

        [BindProperty]
        public string SelectedRoleId { get; set; } = "";

        [BindProperty]
        public List<RoleModulePermissionDto> UpdatedPermissions { get; set; } = new();

        // --- Permission Check Helpers ---
        private Task<bool> HasViewPermissionAsync() =>
            _permissionService.HasPermissionAsync(User, "UserManagement", "View");

        private Task<bool> HasCreatePermissionAsync() =>
            _permissionService.HasPermissionAsync(User, "UserManagement", "Create");

        private Task<bool> HasUpdatePermissionAsync() =>
            _permissionService.HasPermissionAsync(User, "UserManagement", "Edit");

        private Task<bool> HasDeletePermissionAsync() =>
            _permissionService.HasPermissionAsync(User, "UserManagement", "Delete");

        private IActionResult RedirectWithError()
        {
            TempData["ErrorMessage"] = "You are not allowed to perform this operation.";
            return Redirect("/Index");
        }

        // --- Page Handlers ---

        public async Task<IActionResult> OnGetAsync()
        {
            if (!await HasViewPermissionAsync())
                return RedirectWithError();

            await ReloadDataAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAddRoleAsync(string newRoleName)
        {
            if (!await HasCreatePermissionAsync())
                return RedirectWithError();

            if (string.IsNullOrWhiteSpace(newRoleName))
            {
                ModelState.AddModelError(string.Empty, "Role name cannot be empty.");
                await ReloadDataAsync();
                return Page();
            }

            string resultMessage = await AddRoleAsync(newRoleName);
            TempData["StatusMessage"] = resultMessage;

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostAssignRoleAsync(string userId, string roleId)
        {
            if (!await HasUpdatePermissionAsync())
                return RedirectWithError();

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(roleId))
            {
                ModelState.AddModelError(string.Empty, "User ID and Role ID are required.");
                await ReloadDataAsync();
                return Page();
            }

            await AssignRoleAsync(userId, roleId);
            await ReloadDataAsync();
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostLoadPermissionsAsync()
        {
            if (string.IsNullOrEmpty(SelectedRoleId))
            {
                ModelState.AddModelError(string.Empty, "Please select a role.");
                await ReloadDataAsync();
                return Page();
            }

            await LoadPermissionsByRoleAsync(SelectedRoleId);

            UpdatedPermissions = RolePermissions.Select(r => new RoleModulePermissionDto
            {
                RoleId = r.RoleId,
                ModuleName = r.ModuleName,
                CanView = r.CanView,
                CanCreate = r.CanCreate,
                CanEdit = r.CanEdit,
                CanDelete = r.CanDelete
            }).ToList();

            await ReloadDataAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostSavePermissionsAsync()
        {
            if (!await HasUpdatePermissionAsync())
                return RedirectWithError();

            if (UpdatedPermissions == null || UpdatedPermissions.Count == 0)
            {
                ModelState.AddModelError(string.Empty, "No permissions to update.");
                await ReloadDataAsync();
                return Page();
            }

            foreach (var perm in UpdatedPermissions)
            {
                await SavePermissionAsync(perm);
            }

            TempData["SuccessMessage"] = "Permissions updated successfully.";
            await ReloadDataAsync();

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteUserAsync(string userId)
        {
            if (!await HasDeletePermissionAsync())
                return RedirectWithError();

            if (string.IsNullOrEmpty(userId))
            {
                ModelState.AddModelError(string.Empty, "User ID is required.");
                await ReloadDataAsync();
                return Page();
            }

            await DeleteUserAsync(userId);
            TempData["SuccessMessage"] = "User deleted successfully.";
            await ReloadDataAsync();

            return RedirectToPage();
        }

        // --- Business Logic Helpers ---

        private async Task ReloadDataAsync()
        {
            await LoadUsersAsync();
            await LoadRolesAsync();
        }

        // --- Database Calls ---

        private async Task LoadUsersAsync()
        {
            Users.Clear();

            using var conn = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            using var cmd = new SqlCommand("sp_GetAllUsersWithRoles", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                Users.Add(new UserWithRoleDto
                {
                    UserId = reader["UserId"].ToString(),
                    UserName = reader["UserName"].ToString(),
                    Email = reader["Email"].ToString(),
                    RoleName = reader["RoleName"].ToString()
                });
            }
        }

        private async Task LoadRolesAsync()
        {
            Roles.Clear();

            using var conn = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            using var cmd = new SqlCommand("sp_GetAllRoles", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                Roles.Add(new RoleDto
                {
                    RoleId = reader["RoleId"].ToString(),
                    RoleName = reader["RoleName"].ToString()
                });
            }
        }

        public async Task<string> AddRoleAsync(string roleName)
        {
            try
            {
                using var conn = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
                using var cmd = new SqlCommand("sp_AddAspNetRole", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@RoleName", roleName);

                await conn.OpenAsync();
                await cmd.ExecuteNonQueryAsync();

                return "✅ Role created successfully.";
            }
            catch (SqlException ex) when (ex.Number == 51000)
            {
                return "⚠️ Role already exists.";
            }
            catch (Exception ex)
            {
                return $"❌ An unexpected error occurred: {ex.Message}";
            }
        }

        private async Task AssignRoleAsync(string userId, string roleId)
        {
            using var conn = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            using var cmd = new SqlCommand("sp_AssignRoleToUser", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("@UserId", userId);
            cmd.Parameters.AddWithValue("@RoleId", roleId);

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        private async Task<List<RoleModulePermissionDto>> LoadPermissionsByRoleAsync(string roleId, string? moduleName = null)
        {
            RolePermissions.Clear();

            using var conn = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            using var cmd = new SqlCommand("sp_GetPermissionByRole", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@RoleId", roleId);
            cmd.Parameters.AddWithValue("@ModuleName", string.IsNullOrEmpty(moduleName) ? DBNull.Value : (object)moduleName);

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            var permissions = new List<RoleModulePermissionDto>();

            while (await reader.ReadAsync())
            {
                permissions.Add(new RoleModulePermissionDto
                {
                    RoleId = roleId,
                    ModuleName = reader["ModuleName"].ToString() ?? string.Empty,
                    CanView = Convert.ToBoolean(reader["CanView"]),
                    CanCreate = Convert.ToBoolean(reader["CanCreate"]),
                    CanEdit = Convert.ToBoolean(reader["CanEdit"]),
                    CanDelete = Convert.ToBoolean(reader["CanDelete"])
                });
            }

            RolePermissions.AddRange(permissions);

            // Add missing modules with all permissions false
            if (string.IsNullOrEmpty(moduleName))
            {
                var existingModules = new HashSet<string>(permissions.Select(p => p.ModuleName), StringComparer.OrdinalIgnoreCase);
                foreach (var module in AvailableModules)
                {
                    if (!existingModules.Contains(module))
                    {
                        RolePermissions.Add(new RoleModulePermissionDto
                        {
                            RoleId = roleId,
                            ModuleName = module,
                            CanView = false,
                            CanCreate = false,
                            CanEdit = false,
                            CanDelete = false
                        });
                    }
                }
            }

            return permissions;
        }

        private async Task SavePermissionAsync(RoleModulePermissionDto dto)
        {
            using var conn = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            using var cmd = new SqlCommand("sp_SetPermissionForRole", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@RoleId", dto.RoleId);
            cmd.Parameters.AddWithValue("@ModuleName", dto.ModuleName);
            cmd.Parameters.AddWithValue("@CanView", dto.CanView);
            cmd.Parameters.AddWithValue("@CanCreate", dto.CanCreate);
            cmd.Parameters.AddWithValue("@CanEdit", dto.CanEdit);
            cmd.Parameters.AddWithValue("@CanDelete", dto.CanDelete);

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        private async Task DeleteUserAsync(string userId)
        {
            using var conn = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            using var cmd = new SqlCommand("sp_DeleteUser", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@UserId", userId);

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }
    }
}
