namespace UserService.Services.ServiceSendEmail
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string body);
    }

}
