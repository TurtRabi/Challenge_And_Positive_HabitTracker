using UserService.Common;
using UserService.Dto.Role;

namespace UserService.Services.ServiceRole
{
    public interface IRoleService
    {
        Task<ServiceResult> CreateRole(RoleCreateDto dto);

        Task<ServiceResult> UpdateRole(Guid id, RoleUpdateDto dto);

        Task<ServiceResult> DeleteRole(String Name);
        Task<ServiceResult> GetRoleByName(String Name);

        Task<ServiceResult> GetListRole();

        Task<ServiceResult> AddRoleForUser(Guid userId, Guid roleId);

        Task<ServiceResult> DeleteRoleForUser(Guid userId, Guid roleId);
    }
}
