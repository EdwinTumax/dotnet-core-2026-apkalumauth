using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using ApiKalumAuth.DTOs;
using ApiKalumAuth.Entities;
using ApiKalumAuth.Enums;
using ApiKalumAuth.Repositories.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace ApiKalumAuth.Controllers
{
    [ApiController]
    [Route("kalum-auth/v1/account")]
    [Authorize]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IUtils _utils;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        public AccountController(IMapper mapper, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, 
            IUtils utils, IConfiguration configuration)
        {
            this._userManager = userManager;
            this._signInManager = signInManager;
            this._utils = utils;
            this._mapper = mapper;
            this._configuration = configuration;
        }

        [HttpGet("user/search")]
        public async Task<ActionResult> GetByEmail([FromQuery] string value, [FromQuery] string type)
        {
            long initialTime = DateTime.Now.Ticks;
            
            if(String.IsNullOrEmpty(value) || String.IsNullOrEmpty(type))
            {
                ModelState.AddModelError("BAD REQUEST", "Parametro Email o Username invalidos"); 
                this._utils.Log(initialTime,"Parametros Email o Username invalidos",400, TypeLog.ERROR, HttpContext, MethodLog.GET);               
                return BadRequest(ApiResponseDTO<Object>.Fail("Error en criterio de busqueda",ModelState.Values.SelectMany(e => e.Errors).Select(e => e.ErrorMessage).ToList()));
            }
            ApplicationUser user = null;
            if(type.Equals("email",StringComparison.OrdinalIgnoreCase))
            {
                user = await this._userManager.Users.FirstOrDefaultAsync(u => u.Email == value);
            }
            else if (type.Equals("username", StringComparison.OrdinalIgnoreCase))
            {
                user = await this._userManager.Users.FirstOrDefaultAsync(u => u.UserName == value);
            }
            else
            {
                ModelState.AddModelError("BAD REQUEST", $"Los criterios de busquedas [{type}] & [{value}] no son validos");
                this._utils.Log(initialTime,$"Los criterios de busquedas [{type}] & [{value}] no son validos",404, TypeLog.ERROR, HttpContext, MethodLog.GET);               
                return StatusCode(404,ApiResponseDTO<Object>
                    .Fail($"Creterios no validos [{type}] &  [{value}]", ModelState.Values.SelectMany(e => e.Errors).Select(e => e.ErrorMessage).ToList()));
            }
            if(user == null)
            {
                ModelState.AddModelError("NOT_FOUND", $"Usuario no encontrado con el {type} {value}");
                this._utils.Log(initialTime,$"Usuario no encontrado con el {type} {value}",404, TypeLog.ERROR, HttpContext, MethodLog.GET);               
                return StatusCode(404,ApiResponseDTO<Object>
                    .Fail($"No se encontro ningún usuario con el {type} {value}", ModelState.Values.SelectMany(e => e.Errors).Select(e => e.ErrorMessage).ToList()));
            }
            List<string> roles = (List<string>)await this._userManager.GetRolesAsync(user);
            UserListDTO userListDTO = this._mapper.Map<UserListDTO>(user);
            if(roles.Count > 0)
            {
                userListDTO.Roles = roles;                
            }
            this._utils.Log(initialTime,$"Usuario encontrado con el {type} {value}",200, TypeLog.INFORMATION, HttpContext, MethodLog.GET);
            return StatusCode(200, ApiResponseDTO<Object>.Ok(userListDTO));
        }

        [HttpPost("user/{id}")]
        public async Task<ActionResult> AddRoleByUser(string id, [FromBody] List<RoleCreatedDTO> roles )
        {
            long initialTime = DateTime.Now.Ticks;
            var user = await this._userManager.FindByIdAsync(id);
            if(user == null)
            {
                this._utils.Log(initialTime, $"No existe el usuario con el id {id}",404, TypeLog.ERROR, HttpContext, MethodLog.POST);
                return NotFound(ApiResponseDTO<Object>.Fail("Error en asignación de roles", new List<string>() { $"No existe el usuario con el id {id}"}));                
            }
            var result =  await this._userManager.AddToRolesAsync(user,roles.Select(r => r.Name).ToArray());
            if(result.Succeeded)
            {
                this._utils.Log(initialTime,"Roles creados exitosamente",201, TypeLog.INFORMATION, HttpContext, MethodLog.POST);
                return StatusCode(201, ApiResponseDTO<Object>.Ok(await this._userManager.GetRolesAsync(user),"Roles creados exitosamente"));
            }
            this._utils.Log(initialTime,$"No fue posible asignar los roles al usuario con el id {id}", 400, TypeLog.ERROR, HttpContext, MethodLog.POST);
            return BadRequest(ApiResponseDTO<object>.Fail("Error al momento de asignar roles", new List<string>() { $"No fue posible asignar los roles al usuario con el id {id}"}));
        }
        
        [HttpDelete("user/{id}")]
        public async Task<ActionResult> RemoveRoleByUser(string id, [FromBody] List<RoleCreatedDTO> roles)
        {
            long initialTime = DateTime.Now.Ticks;
            var user = await this._userManager.FindByIdAsync(id);
            if(user == null)
            {
                this._utils.Log(initialTime,"Error en eliminación de roles", 404, TypeLog.ERROR, HttpContext, MethodLog.DELETE);
                return NotFound(ApiResponseDTO<Object>.Fail("Error en eliminación de roles", new List<string>() { $"No existe el usuario con el id {id}"}));                
            }
            var result =  await this._userManager.RemoveFromRolesAsync(user,roles.Select(r => r.Name).ToArray());
            if(result.Succeeded)
            {
                this._utils.Log(initialTime,"Roles actuales", 200, TypeLog.INFORMATION, HttpContext, MethodLog.DELETE);
                return Ok(ApiResponseDTO<Object>.Ok(await this._userManager.GetRolesAsync(user),"Roles actuales"));
            }
            this._utils.Log(initialTime,"Error al momento de eliminar los roles", 400, TypeLog.ERROR, HttpContext, MethodLog.DELETE);
            return BadRequest(ApiResponseDTO<object>.Fail("Error al momento de eliminar los roles", new List<string>() { $"No fue posible eliminar los roles al usuario con el id {id}"}));
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult> Login([FromBody] UserLoginDTO userLogin)
        {
            long initialTime = DateTime.Now.Ticks;
            // Authentication
            Microsoft.AspNetCore.Identity.SignInResult login = await this._signInManager.PasswordSignInAsync(userLogin.UserName, userLogin.Password, isPersistent: false, lockoutOnFailure: false);
            if(login.Succeeded)
            {                
                ApplicationUser applicationUser = await this._userManager.FindByNameAsync(userLogin.UserName);
                // Authorization
                List<string> roles = (List<string>) await  this._userManager.GetRolesAsync(applicationUser);
                if(roles.Count == 0)
                {
                    this._utils.Log(initialTime,"El login fue exitoso y el usuario no tiene roles asignados",201, TypeLog.INFORMATION, HttpContext, MethodLog.POST);
                    return StatusCode(201, ApiResponseDTO<Object>.Ok(this._utils.BuildToken(applicationUser,new List<string>())));
                }
                else
                {
                    this._utils.Log(initialTime,"El login fue exitoso y fueron asignados los roles correspondientes",201, TypeLog.INFORMATION, HttpContext, MethodLog.POST);
                    return StatusCode(201, ApiResponseDTO<Object>.Ok(this._utils.BuildToken(applicationUser,roles)));                
                }
            } 
            else
            {
                ModelState.AddModelError(string.Empty, "Error en inicio de sesión.");
                List<string> errores = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                this._utils.Log(initialTime,"El inicio de sesión es incorrecto, favor de validar sus credenciales.", 401, TypeLog.ERROR, HttpContext, MethodLog.POST);
                return BadRequest(ApiResponseDTO<Object>.Fail("El inicio de sesión es incorrecto, favor de validar sus credenciales.", errores));
            }
        }

        [AllowAnonymous]
        [HttpPost("create")]
        public async Task<ActionResult<UserTokenDTO>> Create([FromBody] UserCreatedDTO userCreatedDTO)
        {
            long initialTime = DateTime.Now.Ticks;
            ApplicationUser applicationUser = new ApplicationUser()
            {
              UserName = userCreatedDTO.UserName,
              Email = userCreatedDTO.Email  
            };
            var user = await this._userManager.CreateAsync(applicationUser, userCreatedDTO.Password);
            if(user.Succeeded)
            {
                await this._userManager.AddToRoleAsync(applicationUser, this._configuration.GetValue<string>("LocalServer:Auth:Role"));
                this._utils.Log(initialTime,"El usuario fue creado de forma exitosa", 201, TypeLog.INFORMATION, HttpContext, MethodLog.POST);
                return StatusCode(201, ApiResponseDTO<Object>.Ok(this._utils.BuildToken(applicationUser, new List<string>() {this._configuration.GetValue<string>("LocalServer:Auth:Role")})));   
            } 
            else
            {
                List<string> errores = user.Errors.Select(e => e.Description).ToList();
                this._utils.Log(initialTime,"Error al momento de crear el usuario", 400, TypeLog.ERROR, HttpContext, MethodLog.POST);
                return BadRequest(ApiResponseDTO<Object>.Fail("Error en registro de usuario", errores));
            }
        }        


    }
}