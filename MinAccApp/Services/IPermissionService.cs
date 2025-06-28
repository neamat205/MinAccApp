using MinAccApp.Models;
using System.Security.Claims;
using System.Threading.Tasks;

public interface IPermissionService
{
    Task<RoleModulePermissionDto> GetPermissionsAsync(ClaimsPrincipal user, string moduleName);
    Task<bool> HasPermissionAsync(ClaimsPrincipal user, string moduleName, string action); // e.g. "View", "Edit"
}
