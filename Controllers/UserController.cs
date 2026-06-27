using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApiKalumAuth.DBContext;
using ApiKalumAuth.DTOs;
using ApiKalumAuth.Entities;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiKalumAuth.Controllers
{
    [ApiController]
    [Route("kalum-auth/v1/user")]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IMapper _mapper;
        public UserController(UserManager<ApplicationUser> userManager, IMapper mapper, RoleManager<IdentityRole> roleManager)
        {
            this._userManager = userManager;
            this._mapper = mapper;
            this._roleManager = roleManager;
        }

        [HttpGet]
        public async Task<ActionResult<List<UserListDTO>>> Get()
        {
            List<ApplicationUser> users = await this._userManager.Users.ToListAsync();
            if(users == null || users.Count == 0)
            {
                return NoContent();
            }
            List<UserListDTO> usersListDTO = new List<UserListDTO>();
            foreach(var user in users)
            {
                List<string> roles = (List<string>)await this._userManager.GetRolesAsync(user);
                UserListDTO userListDTO = this._mapper.Map<UserListDTO>(user);
                if(roles.Count > 0)
                {
                    userListDTO.Roles = roles;                
                }
                usersListDTO.Add(userListDTO);
            }            
            return Ok(ApiResponseDTO<Object>.Ok(usersListDTO));
        }
        
        [HttpGet("{id}", Name ="GetUserById")]
        public async Task<ActionResult<UserListDTO>> GetById(String id)
        {
            IdentityUser user = await this._userManager.Users.FirstOrDefaultAsync(U => U.Id == id);
            if(user == null)
            {
                return NotFound(ApiResponseDTO<Object>.Fail($"No existe el usuario con el id {id}"));
            }
            ApplicationUser applicationUser = await this._userManager.FindByIdAsync(user.Id);
            List<string> roles = (List<string>)await this._userManager.GetRolesAsync(applicationUser);
            UserListDTO userListDTO = this._mapper.Map<UserListDTO>(user);
            if(roles.Count > 0)
            {
                userListDTO.Roles = roles;                
            }
            return Ok(ApiResponseDTO<Object>.Ok(userListDTO));

        }

        [HttpPost]
        public async Task<ActionResult> Create([FromBody] UserCreatedDTO userCreatedDTO)
        {     
            ApplicationUser applicationUser = this._mapper.Map<ApplicationUser>(userCreatedDTO);
            var userNew = await this._userManager.CreateAsync(applicationUser,userCreatedDTO.Password);
            if(userNew.Succeeded)
            {
                var user = await this._userManager.FindByEmailAsync(userCreatedDTO.Email);
                return new CreatedAtRouteResult("GetUserById", new {id = user.Id});
            }
            List<string> errores =  userNew.Errors.Select(e => e.Description).ToList();
            return BadRequest(ApiResponseDTO<Object>.Fail("Error en el registro del usuario", errores));            
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(string id)
        {
            ApplicationUser applicationUser = await this._userManager.FindByIdAsync(id);
            if(applicationUser == null)
            {
                return NotFound(ApiResponseDTO<Object>.Fail($"Error al eliminar el registro", new List<string>() {$"No existe el usuario con el id {id}"}));
            }
            var result = await this._userManager.DeleteAsync(applicationUser);
            if(result.Succeeded)
            {
                return NoContent();
            }
            List<string> errores = result.Errors.Select(e => e.Description).ToList();
            return BadRequest(ApiResponseDTO<Object>.Fail("No se logro eliminar el registro",errores));
        }
        [HttpPut("{id}")]
        public async Task<ActionResult> Update(string id, [FromBody] UserUpdateDTO userUpdatedDTO)
        {
            ApplicationUser applicationUser = await this._userManager.FindByIdAsync(id);
            if(applicationUser == null)
            {
                return NotFound(ApiResponseDTO<Object>.Fail("Error al actualizar la información", new List<string>() {$"No existe el usuario con el id {id}"}));
            }
            applicationUser.UserName = userUpdatedDTO.UserName ?? applicationUser.UserName;
            applicationUser.FirstName = userUpdatedDTO.FirstName ?? applicationUser.FirstName;
            applicationUser.LastName = userUpdatedDTO.LastName ?? applicationUser.LastName;
            applicationUser.PhoneNumber = userUpdatedDTO.PhoneNumber ?? applicationUser.PhoneNumber;
            var result = await this._userManager.UpdateAsync(applicationUser);
            if(result.Succeeded)
            {
                return NoContent();
            }
            List<string> errores = result.Errors.Select(e => e.Description).ToList();
            return BadRequest(ApiResponseDTO<Object>.Fail("Error al momento de actualizar la información",errores));
        }

    }
}