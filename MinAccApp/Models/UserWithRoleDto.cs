namespace MinAccApp.Models
{
    public class UserWithRoleDto
    {
        public string UserId { get; set; }    // User unique ID
        public string UserName { get; set; }  // Username
        public string Email { get; set; }     // User email
        public string RoleName { get; set; }  // Assigned role name (e.g., Admin)
    }
}
