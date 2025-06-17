namespace UserService.Dto.User
{
    public class VerifyDto
    {
        public Guid UserId { get; set; }
        public string Type { get; set; }
        public string Code { get; set; }
    }
}
