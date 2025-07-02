using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.Dto.Request.Role;
using UserService.Dto.Role;
using UserService.Services.ServiceRole;

namespace UserService.Controllers
{
    [AllowAnonymous]
    [ApiController]
    [Route("api/[controller]")]
    public class RoleController : ControllerBase
    {
        private readonly IRoleService roleService;
        public RoleController(IRoleService roleService)
        {
            this.roleService = roleService;
        }
        [HttpPost]
        public async Task<IActionResult> CreateRole([FromBody] RoleCreateRequest request)
        {
            var roleDtoCreate = new RoleCreateDto
            {
                Description = request.Description,
                Name = request.Name
            };

            var result = await roleService.CreateRole(roleDtoCreate);
            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRole([FromRoute] Guid id, [FromBody] RoleUpdateRequest request)
        {
            var roleDtoUpdate = new RoleUpdateDto
            {
                Name = request.Name,
                Description = request.Description,
            };

            var result = await roleService.UpdateRole(id, roleDtoUpdate);
            return Ok(result);
        }

        [HttpDelete("{nameRole}")]
        public async Task<IActionResult> DeleteRole([FromRoute] String nameRole)
        {
            var result = await roleService.DeleteRole(nameRole);
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetListRole()
        {
            var result = await roleService.GetListRole();
            return Ok(result);
        }

        [HttpPost("{userId}/role/{roleId}")]
        public async Task<IActionResult> AddRoleForUser([FromRoute] Guid userId, [FromRoute] Guid roleId)
        {
            var result = await roleService.AddRoleForUser(userId, roleId);
            return Ok(result);
        }

        [HttpDelete("{userId}/role/{roleId}")]
        public async Task<IActionResult> DeleteRoleForUser([FromRoute] Guid userId, [FromRoute] Guid roleId)
        {
            var result = await roleService.DeleteRoleForUser(userId, roleId);
            return Ok(result);
        }
    }
}
