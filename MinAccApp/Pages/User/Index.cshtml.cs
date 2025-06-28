using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using MinAccApp.Models;
using System.Data;

namespace MinAccApp.Pages.User
{
    public class IndexModel : PageModel
    {
        private readonly IConfiguration _config;
        private readonly IPermissionService _permissionService;
        public List<UserWithRoleDto> Users { get; set; } = new();

        public IndexModel(IConfiguration config, IPermissionService permissionService)
        {
            _config = config;
            _permissionService = permissionService;
        }

        public async Task<IActionResult> OnGetAsync()
        { 
            var canView = await _permissionService.HasPermissionAsync(User, "UserManagement", "View");

            if (!canView)
            {
              
                ModelState.AddModelError(string.Empty, "You do not have permission to access this page.");

                return Redirect("/Index");
            }



            await GetAllUsersAsync();
            return Page();
        }

        private async Task GetAllUsersAsync()
        {
            using var conn = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            using var cmd = new SqlCommand("sp_GetAllUsersWithRoles", conn);
            cmd.CommandType = CommandType.StoredProcedure;

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
    }

}
