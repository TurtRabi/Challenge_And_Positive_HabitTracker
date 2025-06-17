namespace UserService.Dto.User
{
    public class ChangePasswordDto
    {
        public Guid UserId { get; set; }              
        public string OldPassword { get; set; }       
        public string NewPassword { get; set; }       
        public string? ConfirmPassword { get; set; } 
    }
}
