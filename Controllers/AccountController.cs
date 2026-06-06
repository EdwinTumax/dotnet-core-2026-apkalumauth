using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApiKalumAuth.DTOs;
using ApiKalumAuth.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ApiKalumAuth.Controllers
{
    [ApiController]
    [Route("kalum-auth/v1/account")]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public AccountController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            this._userManager = userManager;
            this._roleManager = roleManager;
        }

        [HttpPost("user/{id}")]
        public async Task<ActionResult> AddRoleByUser(string id, [FromBody] List<RoleCreatedDTO> roles )
        {
            var user = await this._userManager.FindByIdAsync(id);
            if(user == null)
            {
                return NotFound(ApiResponseDTO<Object>.Fail("Error en asignación de roles", new List<string>() { $"No existe el usuario con el id {id}"}));                
            }
            var result =  await this._userManager.AddToRolesAsync(user,roles.Select(r => r.Name).ToArray());
            if(result.Succeeded)
            {
                return StatusCode(201, ApiResponseDTO<Object>.Ok(await this._userManager.GetRolesAsync(user),"Roles creados exitosamente"));
            }
            return BadRequest(ApiResponseDTO<object>.Fail("Error al momento de asignar roles", new List<string>() { $"No fue posible asignar los roles al usuario con el id {id}"}));
        }
        
        [HttpDelete("user/{id}")]
        public async Task<ActionResult> RemoveRoleByUser(string id, [FromBody] List<RoleCreatedDTO> roles)
        {
            var user = await this._userManager.FindByIdAsync(id);
            if(user == null)
            {
                return NotFound(ApiResponseDTO<Object>.Fail("Error en eliminación de roles", new List<string>() { $"No existe el usuario con el id {id}"}));                
            }
            var result =  await this._userManager.RemoveFromRolesAsync(user,roles.Select(r => r.Name).ToArray());
            if(result.Succeeded)
            {
                return Ok(ApiResponseDTO<Object>.Ok(await this._userManager.GetRolesAsync(user),"Roles actuales"));
            }
            return BadRequest(ApiResponseDTO<object>.Fail("Error al momento de eliminar los roles", new List<string>() { $"No fue posible eliminar los roles al usuario con el id {id}"}));
        }
    }
}