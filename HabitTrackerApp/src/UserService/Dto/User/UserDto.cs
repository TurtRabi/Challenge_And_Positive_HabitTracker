using UserService.Dto.Role;

namespace UserService.Dto.User
{
    public class UserDto
    {
        public Guid? Id { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? FullName { get; set; }
        public string? AvatarUrl { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Gender { get; set; }
        public DateOnly? DateOfBirth { get; set; }
        public string? Status { get; set; }
        public DateTime? LastLogin { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool? EmailVerified { get; set; }
        public bool? PhoneVerified { get; set; }
        public List<RoleDto> Roles { get; set; }
    }
}
