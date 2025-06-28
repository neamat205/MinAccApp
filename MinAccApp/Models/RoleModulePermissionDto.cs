namespace MinAccApp.Models
{
    public class RoleModulePermissionDto
    {
        public string? RoleId { get; set; }          // ✅ Now nullable
        public string? ModuleName { get; set; }      // (also mark this nullable if needed)
        public bool CanView { get; set; }
        public bool CanCreate { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
    }
}
