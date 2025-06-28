namespace MinAccApp.Models
{
    public class RoleModulePermission
    {
        public bool CanView { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
        public bool CanCreate { get; set; }
        public string RoleId { get; set; }
        public string ModuleName { get; set; }
    }
}
