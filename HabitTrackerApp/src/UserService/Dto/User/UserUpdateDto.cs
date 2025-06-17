namespace UserService.Dto.User
{
    public class UserUpdateDto
    {
        public string? FullName { get; set; }
        public string? AvatarUrl { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Gender { get; set; }
        public DateOnly? DateOfBirth { get; set; }
    }
}
