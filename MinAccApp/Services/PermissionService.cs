using Microsoft.Data.SqlClient;

using MinAccApp.Models;
using System.Data;
using System.Security.Claims;


public class PermissionService : IPermissionService
{
    private readonly IConfiguration _configuration;

    public PermissionService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<RoleModulePermissionDto> GetPermissionsAsync(ClaimsPrincipal user, string moduleName)
    {
        // Get user ID from ClaimsPrincipal (assuming string, adapt if int)
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return null; // or empty permissions

        using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        using var command = new SqlCommand("sp_GetPermissionsForUserAndModule", connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        command.Parameters.AddWithValue("@UserId", userId);
        command.Parameters.AddWithValue("@ModuleName", moduleName);

        await connection.OpenAsync();

        using var reader = await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return new RoleModulePermissionDto
            {
                RoleId = reader["RoleId"]?.ToString(), // optional
                ModuleName = moduleName,
                CanView = reader["CanView"] != DBNull.Value && Convert.ToBoolean(reader["CanView"]),
                CanCreate = reader["CanCreate"] != DBNull.Value && Convert.ToBoolean(reader["CanCreate"]),
                CanEdit = reader["CanEdit"] != DBNull.Value && Convert.ToBoolean(reader["CanEdit"]),
                CanDelete = reader["CanDelete"] != DBNull.Value && Convert.ToBoolean(reader["CanDelete"]),
            };

        }

        // No permission found - deny all
        return new RoleModulePermissionDto
        {
            RoleId = null,
            ModuleName = moduleName,
            CanView = false,
            CanCreate = false,
            CanEdit = false,
            CanDelete = false,
        };
    }

    public async Task<bool> HasPermissionAsync(ClaimsPrincipal user, string moduleName, string action)
    {
        var permissions = await GetPermissionsAsync(user, moduleName);
        if (permissions == null) return false;

        return action.ToLower() switch
        {
            "view" => permissions.CanView,
            "create" => permissions.CanCreate,
            "edit" => permissions.CanEdit,
            "delete" => permissions.CanDelete,
            _ => false
        };
    }
}
