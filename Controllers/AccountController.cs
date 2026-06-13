using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using ApiKalumAuth.DTOs;
using ApiKalumAuth.Entities;
using ApiKalumAuth.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ApiKalumAuth.Controllers
{
    [ApiController]
    [Route("kalum-auth/v1/account")]
    [Authorize]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IUtils _utils;

        public AccountController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, SignInManager<ApplicationUser> signInManager, IUtils utils)
        {
            this._userManager = userManager;
            this._roleManager = roleManager;
            this._signInManager = signInManager;
            this._utils = utils;
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

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult> Login([FromBody] UserLoginDTO userLogin)
        {
            // Authentication
            Microsoft.AspNetCore.Identity.SignInResult login = await this._signInManager.PasswordSignInAsync(userLogin.UserName, userLogin.Password, isPersistent: false, lockoutOnFailure: false);
            if(login.Succeeded)
            {                
                ApplicationUser applicationUser = await this._userManager.FindByNameAsync(userLogin.UserName);
                // Authorization
                List<string> roles = (List<string>) await  this._userManager.GetRolesAsync(applicationUser);
                if(roles.Count == 0)
                {
                    return StatusCode(201, ApiResponseDTO<Object>.Ok(this._utils.BuildToken(applicationUser,new List<string>())));
                }
                else
                {
                    return StatusCode(201, ApiResponseDTO<Object>.Ok(this._utils.BuildToken(applicationUser,roles)));                
                }
            } 
            else
            {
                ModelState.AddModelError(string.Empty, "Error en inicio de sesión.");
                List<string> errores = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponseDTO<Object>.Fail("El inicio de sesión es incorrecto, favor de validar sus credenciales.", errores));
            }
        }

        [AllowAnonymous]
        [HttpPost("create")]
        public async Task<ActionResult<UserTokenDTO>> Create([FromBody] UserCreatedDTO userCreatedDTO)
        {
            ApplicationUser applicationUser = new ApplicationUser()
            {
              UserName = userCreatedDTO.UserName,
              Email = userCreatedDTO.Email  
            };
            var user = await this._userManager.CreateAsync(applicationUser, userCreatedDTO.Password);
            if(user.Succeeded)
            {
                await this._userManager.AddToRoleAsync(applicationUser, "ROLE_USER");
                return StatusCode(201, ApiResponseDTO<Object>.Ok(this._utils.BuildToken(applicationUser, new List<string>() {"ROLE_USER"})));   
            } 
            else
            {
                List<string> errores = user.Errors.Select(e => e.Description).ToList();
                return BadRequest(ApiResponseDTO<Object>.Fail("Error en registro de usuario", errores));
            }
        }        


    }
}