using UserService.Common;

namespace UserService.Services.UserProviderService
{
    public interface IUserProviderService
    {
        Task<ServiceResult> LinkExternalProviderAsync(Guid userId, string provider, string providerUserId);
        Task<ServiceResult> UnlinkExternalProviderAsync(Guid userId, string provider);
        Task<ServiceResult> GetLinkedProvidersAsync(Guid userId);
        Task<ServiceResult> SocialLoginAsync(string provider, string accessToken, string clientType);

        Task<ServiceResult> StartMfaAsync(Guid userId);
        Task<ServiceResult> VerifyMfaAsync(Guid userId, string otpCode);
        Task<ServiceResult> SearchUsersAsync(string? keyword, int page, int pageSize);
    }
}
