using UserService.Common;
using UserService.Dto.Role;
using UserService.Models;
using UserService.Repositories.UOW;

namespace UserService.Services.ServiceRole
{
    public class RoleService : IRoleService
    {
        public readonly IUnitOfWork _unitOfWork;
        public RoleService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public Task<ServiceResult> AddRoleForUser(Guid userId, Guid roleId)
        {
            throw new NotImplementedException();
        }

        public async Task<ServiceResult> CreateRole(RoleCreateDto dto)
        {
            var result = new ServiceResult();
            var roleExisting = (await _unitOfWork.role.FindAnsyc(v => v.Name.ToLower() == dto.Name.ToLower())).FirstOrDefault();
            if (roleExisting != null)
            {
                result.Success = false;
                result.Message = "Role name already exists.";
                return result;
            }
            var role = new Role
            {
                Name = dto.Name,
                Description = dto.Description,
            };

            await _unitOfWork.role.AddAnsync(role);
            await _unitOfWork.CommitAsync();

            result.Success = true;
            result.Data = new { role.Id, role.Name,role.Description };
            result.Message = "Role created successfully.";
            return result;
        }

        public async Task<ServiceResult> DeleteRole(String name)
        {
            var result = new ServiceResult();
            var getRoleByName  = (await _unitOfWork.role.FindAnsyc(v=>v.Name.ToLower() == name.ToLower())).FirstOrDefault();
            if(getRoleByName == null)
            {
                result.Success = false;
                result.Message = "role is not exist!";
            }

            await _unitOfWork.role.DeleteAnsync(getRoleByName);
            await _unitOfWork.CommitAsync();

            result.Success = true;
            result.Message = "Delete role success!";

            return result;

        }

        public Task<ServiceResult> DeleteRoleForUser(Guid userId, Guid roleId)
        {
            throw new NotImplementedException();
        }

        public async Task<ServiceResult> GetListRole()
        {
            var result = new ServiceResult();
            var listResult = await _unitOfWork.role.GetAllAsync();
            if(listResult == null)
            {
                result.Success = true;
                result.Message = "list is empty";
            }

            result.Success = true;
            result.Message = "list role";
            result.Data = listResult;
            return result;
        }

        public async Task<ServiceResult> GetRoleByName(string Name)
        {
            var result = new ServiceResult();
            var getRoleByName = await _unitOfWork.role.FindAnsyc(v=>v.Name.ToLower() == Name.ToLower());
            if(getRoleByName == null)
            {
                result.Success = false;
                result.Message = "Not found role";
            }

            result.Success = true;
            result.Message = "role";
            result.Data = getRoleByName;

            return result;
        }

        public async Task<ServiceResult> UpdateRole(Guid id, RoleUpdateDto dto)
        {
            var result = new ServiceResult();
            var getRoleById = (await _unitOfWork.role.FindAnsyc(r => r.Id == id)).FirstOrDefault();

            if (getRoleById == null)
            {
                result.Success = false;
                result.Message = "Not found role";
            }

            getRoleById.Name = dto.Name;
            getRoleById.Description= dto.Description;

            _unitOfWork.role.UpdateAnsync(getRoleById);
            _unitOfWork.CommitAsync();


            result.Success = true;
            result.Message = "Update role success";
            result.Data= dto;
            return result;
        }
    }
}
