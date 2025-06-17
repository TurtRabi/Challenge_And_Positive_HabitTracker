using UserService.Dto.Role;

namespace UserService.Dto.Response
{
    public class UserWithRolesRequest
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public string Username { get; set; }
        public string Phone { get; set; }
        public List<RoleDto> Roles { get; set; }
    }
}
